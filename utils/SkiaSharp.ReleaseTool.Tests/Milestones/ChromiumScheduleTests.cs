using SkiaSharp.ReleaseTool.Contracts;
using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Milestones;
using System.Net;
using System.Text;
using Xunit;

namespace SkiaSharp.ReleaseTool.Tests.Milestones
{
	public sealed class ChromiumScheduleTests
	{
		[Fact]
		public void Desired_dates_titles_and_descriptions_match_release_policy()
		{
			var desired = ChromiumSchedulePlanner.Desired(Schedule(), 152, 4);

			Assert.Collection(
				desired,
				value =>
				{
					Assert.Equal("4.152.0-preview.1", value.Title);
					Assert.Equal(new DateTimeOffset(2026, 8, 12, 0, 0, 0, TimeSpan.Zero), value.DueOn);
					Assert.Equal(
						"Skia m152 preview.1 · Start Wed, Aug 05, 2026 · Merge Skia sync PR and ship preview.",
						value.Description);
				},
				value => Assert.Equal("4.152.0-preview.2", value.Title),
				value => Assert.Equal("4.152.0-rc.1", value.Title),
				value =>
				{
					Assert.Equal("4.152.0", value.Title);
					Assert.Equal(new DateTimeOffset(2026, 9, 9, 0, 0, 0, TimeSpan.Zero), value.DueOn);
				});
		}

		[Fact]
		public void Planner_creates_updates_skips_and_noops_deterministically()
		{
			var desired = ChromiumSchedulePlanner.Desired(Schedule(), 152, 4);
			var matching = desired[0];
			var existing = new Dictionary<string, GitHubMilestone>(StringComparer.Ordinal)
			{
				[matching.Title] = new(1, matching.Title, true, matching.DueOn, matching.Description),
				[desired[1].Title] = new(2, desired[1].Title, true, desired[1].DueOn.AddDays(-1), "old"),
			};

			var operations = ChromiumSchedulePlanner.Plan(
				[(152, 4, Schedule())],
				existing,
				new DateOnly(2026, 9, 30));

			Assert.Equal(FinishScheduleAction.None, operations[0].Action);
			Assert.Equal(FinishCloseoutStatus.Done, operations[0].Status);
			Assert.Equal(FinishScheduleAction.Update, operations[1].Action);
			Assert.Equal(["dueOn", "description"], operations[1].Changes.Select(change => change.Field));
			Assert.Equal(FinishScheduleAction.Create, operations[2].Action);
			Assert.Equal(FinishCloseoutStatus.Pending, operations[2].Status);
			Assert.Equal(FinishScheduleAction.Create, operations[3].Action);
		}

		[Fact]
		public void Missing_milestone_more_than_thirty_days_stale_is_skipped()
		{
			var operations = ChromiumSchedulePlanner.Plan(
				[(152, 4, Schedule())],
				new Dictionary<string, GitHubMilestone>(),
				new DateOnly(2026, 10, 2));

			Assert.Equal(FinishCloseoutStatus.Skipped, operations[0].Status);
			Assert.Equal(FinishScheduleAction.None, operations[0].Action);
			Assert.Equal(FinishCloseoutStatus.Skipped, operations[1].Status);
			Assert.Equal(FinishScheduleAction.Create, operations[2].Action);
		}

		[Fact]
		public async Task Http_gateway_tolerates_unknown_fields_but_requires_schedule_fields()
		{
			using var validHttp = new HttpClient(new JsonHandler(
				"""
				{"mstones":[{
				  "branch_point":"2026-08-05",
				  "earliest_beta":"2026-08-12",
				  "early_stable_cut":"2026-08-19",
				  "early_stable":"2026-08-26",
				  "stable_cut":"2026-09-02",
				  "stable_date":"2026-09-09",
				  "future_field":true
				}],"future":[]}
				"""))
			{
				BaseAddress = new Uri("https://chromium.test/"),
			};
			var result = await new HttpChromiumScheduleClient(validHttp).FetchAsync(
				152,
				TestContext.Current.CancellationToken);
			Assert.Equal("2026-09-09", result.StableDate);

			using var invalidHttp = new HttpClient(new JsonHandler(
				"""{"mstones":[{"branch_point":"2026-08-05"}]}"""))
			{
				BaseAddress = new Uri("https://chromium.test/"),
			};
			await Assert.ThrowsAsync<MilestoneException>(() =>
				new HttpChromiumScheduleClient(invalidHttp).FetchAsync(
					152,
					TestContext.Current.CancellationToken));
		}

		internal static ChromiumMilestoneSchedule Schedule() =>
			new(
				BranchPoint: "2026-08-05T00:00:00",
				EarliestBeta: "2026-08-12",
				EarlyStableCut: "2026-08-19",
				EarlyStable: "2026-08-26",
				StableCut: "2026-09-02",
				StableDate: "2026-09-09");

		private sealed class JsonHandler(string json) : HttpMessageHandler
		{
			protected override Task<HttpResponseMessage> SendAsync(
				HttpRequestMessage request,
				CancellationToken cancellationToken) =>
				Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(json, Encoding.UTF8, "application/json"),
				});
		}
	}
}
