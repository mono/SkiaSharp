using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using SkiaSharp.ReleaseTool.Contracts;
using SkiaSharp.ReleaseTool.Errors;

namespace SkiaSharp.ReleaseTool.Milestones
{
	internal sealed record ChromiumMilestoneSchedule(
		string BranchPoint,
		string EarliestBeta,
		string EarlyStableCut,
		string EarlyStable,
		string StableCut,
		string StableDate);

	internal interface IChromiumScheduleClient
	{
		Task<ChromiumMilestoneSchedule> FetchAsync(
			int milestone,
			CancellationToken cancellationToken = default);
	}

	internal sealed class HttpChromiumScheduleClient(HttpClient? httpClient = null) : IChromiumScheduleClient
	{
		private readonly HttpClient client = httpClient ?? new HttpClient
		{
			BaseAddress = new Uri("https://chromiumdash.appspot.com/"),
			Timeout = TimeSpan.FromSeconds(30),
		};

		public async Task<ChromiumMilestoneSchedule> FetchAsync(
			int milestone,
			CancellationToken cancellationToken = default)
		{
			using var request = new HttpRequestMessage(
				HttpMethod.Get,
				$"fetch_milestone_schedule?mstone={milestone}");
			request.Headers.UserAgent.ParseAdd("SkiaSharp-release-automation");
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			HttpResponseMessage response;
			try
			{
				response = await client.SendAsync(
					request,
					HttpCompletionOption.ResponseHeadersRead,
					cancellationToken).ConfigureAwait(false);
			}
			catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
			{
				throw new MilestoneException($"failed to fetch Chromium schedule for m{milestone}: timed out", ex);
			}
			catch (HttpRequestException ex)
			{
				throw new MilestoneException($"failed to fetch Chromium schedule for m{milestone}", ex);
			}

			using (response)
			{
				if (!response.IsSuccessStatusCode)
				throw new MilestoneException($"failed to fetch Chromium schedule for m{milestone}: HTTP {(int)response.StatusCode}");

				ChromiumScheduleResponse payload;
				try
				{
					payload = await JsonSerializer.DeserializeAsync(
						await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false),
						ChromiumScheduleJsonContext.Default.ChromiumScheduleResponse,
						cancellationToken).ConfigureAwait(false)
						?? throw new MilestoneException($"Chromium returned no schedule for m{milestone}");
				}
				catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
				{
					throw new MilestoneException($"Chromium schedule response for m{milestone} timed out", ex);
				}
				catch (Exception ex) when (ex is JsonException or IOException or HttpRequestException)
				{
					throw new MilestoneException($"Chromium schedule response for m{milestone} is not valid JSON", ex);
				}

				var schedule = payload.Mstones?.FirstOrDefault()
					?? throw new MilestoneException($"Chromium returned no schedule for m{milestone}");
				var values = new[]
				{
					("branch_point", schedule.BranchPoint),
					("earliest_beta", schedule.EarliestBeta),
					("early_stable_cut", schedule.EarlyStableCut),
					("early_stable", schedule.EarlyStable),
					("stable_cut", schedule.StableCut),
					("stable_date", schedule.StableDate),
				};
				var missing = values.Where(value => string.IsNullOrWhiteSpace(value.Item2))
					.Select(value => value.Item1)
					.ToArray();
				if (missing.Length > 0)
					throw new MilestoneException($"Chromium m{milestone} schedule is missing [{string.Join(", ", missing)}]");
				return new(
					schedule.BranchPoint!,
					schedule.EarliestBeta!,
					schedule.EarlyStableCut!,
					schedule.EarlyStable!,
					schedule.StableCut!,
					schedule.StableDate!);
			}
		}
	}

	internal static class ChromiumSchedulePlanner
	{
		private const int CreateCutoffDays = 30;

		public static IReadOnlyList<FinishScheduleOperation> Plan(
			IReadOnlyList<(int Milestone, int Major, ChromiumMilestoneSchedule Schedule)> schedules,
			IReadOnlyDictionary<string, GitHubMilestone> existing,
			DateOnly today)
		{
			var operations = new List<FinishScheduleOperation>();
			foreach (var (milestone, major, schedule) in schedules)
			{
				foreach (var desired in Desired(schedule, milestone, major))
				{
					existing.TryGetValue(desired.Title, out var found);
					var changes = new List<FinishScheduleChange>();
					FinishScheduleAction action;
					FinishCloseoutStatus status;
					int? number;
					if (found is not null)
					{
						var actualDue = found.DueOn?.UtcDateTime.Date;
						if (actualDue != desired.DueOn.UtcDateTime.Date)
						{
							changes.Add(new(
								"dueOn",
								actualDue is null ? null : DateOnly.FromDateTime(actualDue.Value).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
								DateOnly.FromDateTime(desired.DueOn.UtcDateTime).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));
						}
						if ((found.Description ?? "") != desired.Description)
						{
							changes.Add(new(
								"description",
								string.IsNullOrEmpty(found.Description) ? null : found.Description,
								desired.Description));
						}
						action = changes.Count == 0 ? FinishScheduleAction.None : FinishScheduleAction.Update;
						status = changes.Count == 0 ? FinishCloseoutStatus.Done : FinishCloseoutStatus.Pending;
						number = found.Number;
					}
					else if (DateOnly.FromDateTime(desired.DueOn.UtcDateTime) >= today.AddDays(-CreateCutoffDays))
					{
						action = FinishScheduleAction.Create;
						status = FinishCloseoutStatus.Pending;
						number = null;
					}
					else
					{
						action = FinishScheduleAction.None;
						status = FinishCloseoutStatus.Skipped;
						number = null;
					}
					operations.Add(new(
						desired.Title,
						number,
						status,
						action,
						desired.DueOn,
						desired.Description,
						changes));
				}
			}
			return operations;
		}

		internal static IReadOnlyList<DesiredMilestone> Desired(
			ChromiumMilestoneSchedule schedule,
			int milestone,
			int major)
		{
			var branch = ParseDate(schedule.BranchPoint, milestone, "branch_point");
			var beta = ParseDate(schedule.EarliestBeta, milestone, "earliest_beta");
			var earlyCut = ParseDate(schedule.EarlyStableCut, milestone, "early_stable_cut");
			var earlyStable = ParseDate(schedule.EarlyStable, milestone, "early_stable");
			var stableCut = ParseDate(schedule.StableCut, milestone, "stable_cut");
			var stable = ParseDate(schedule.StableDate, milestone, "stable_date");
			var baseVersion = $"{major}.{milestone}.0";
			const string separator = "\u00b7";
			return
			[
				new(
					$"{baseVersion}-preview.1",
					AtUtc(beta),
					$"Skia m{milestone} preview.1 {separator} Start {Display(branch)} {separator} Merge Skia sync PR and ship preview."),
				new(
					$"{baseVersion}-preview.2",
					AtUtc(earlyStable),
					$"Skia m{milestone} preview.2 {separator} Start {Display(earlyCut)} {separator} Bug fixes and API additions from preview.1 feedback."),
				new(
					$"{baseVersion}-rc.1",
					AtUtc(stableCut),
					$"Skia m{milestone} RC {separator} Start {Display(earlyStable)} {separator} Critical bug fixes only, no new features."),
				new(
					baseVersion,
					AtUtc(stable),
					$"Skia m{milestone} stable {separator} Start {Display(stableCut)} {separator} Ship to NuGet.org, tag and create GitHub Release."),
			];
		}

		private static DateOnly ParseDate(string value, int milestone, string field)
		{
			var date = value.Split('T', 2)[0];
			if (!DateOnly.TryParseExact(
				date,
				"yyyy-MM-dd",
				CultureInfo.InvariantCulture,
				DateTimeStyles.None,
				out var result))
			{
				throw new MilestoneException($"Chromium m{milestone} schedule has invalid {field} date '{value}'");
			}
			return result;
		}

		private static string Display(DateOnly value) =>
			value.ToString("ddd, MMM dd, yyyy", CultureInfo.InvariantCulture);

		private static DateTimeOffset AtUtc(DateOnly value) =>
			new(value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
	}

	internal sealed record DesiredMilestone(
		string Title,
		DateTimeOffset DueOn,
		string Description);

	internal sealed record ChromiumScheduleResponse(
		IReadOnlyList<ChromiumScheduleEntry>? Mstones);

	internal sealed record ChromiumScheduleEntry(
		[property: JsonPropertyName("branch_point")] string? BranchPoint,
		[property: JsonPropertyName("earliest_beta")] string? EarliestBeta,
		[property: JsonPropertyName("early_stable_cut")] string? EarlyStableCut,
		[property: JsonPropertyName("early_stable")] string? EarlyStable,
		[property: JsonPropertyName("stable_cut")] string? StableCut,
		[property: JsonPropertyName("stable_date")] string? StableDate);

	[JsonSourceGenerationOptions(
		GenerationMode = JsonSourceGenerationMode.Metadata,
		PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
		RespectNullableAnnotations = true,
		RespectRequiredConstructorParameters = true,
		UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip)]
	[JsonSerializable(typeof(ChromiumScheduleResponse))]
	internal partial class ChromiumScheduleJsonContext : JsonSerializerContext;
}
