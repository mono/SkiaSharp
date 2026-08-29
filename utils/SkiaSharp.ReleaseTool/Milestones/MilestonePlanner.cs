using NuGet.Versioning;
using SkiaSharp.ReleaseTool.Contracts;
using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Model;

namespace SkiaSharp.ReleaseTool.Milestones
{
	internal static class MilestonePlanner
	{
		public static IReadOnlyDictionary<string, GitHubMilestone> Index(
			IReadOnlyList<GitHubMilestone> milestones)
		{
			var result = new Dictionary<string, GitHubMilestone>(StringComparer.Ordinal);
			foreach (var milestone in milestones)
			{
				if (!result.TryAdd(milestone.Title, milestone))
					throw new ConflictException($"duplicate milestone title '{milestone.Title}'");
			}
			return result;
		}

		public static async Task<IReadOnlyList<FinishClosureOperation>> PlanClosureAsync(
			IReadOnlyList<GitHubMilestone> milestones,
			IReadOnlySet<string> tags,
			ICloseoutGitHubClient github,
			IReadOnlySet<string>? creatableTitles,
			ICollection<string> warnings,
			CancellationToken cancellationToken)
		{
			var parsed = milestones
				.Select(milestone => (Milestone: milestone, Version: TryParseRelease(milestone.Title)))
				.Where(value => value.Version is not null)
				.Select(value => (value.Milestone, Version: value.Version!))
				.OrderBy(value => value.Version, VersionComparer.VersionRelease)
				.ToArray();
			var creatable = (creatableTitles ?? new HashSet<string>())
				.Select(title => (Title: title, Version: TryParseRelease(title)))
				.Where(value => value.Version is not null)
				.Select(value => (value.Title, Version: value.Version!))
				.OrderBy(value => value.Version, VersionComparer.VersionRelease)
				.ToArray();
			var operations = new List<FinishClosureOperation>();
			foreach (var (milestone, version) in parsed)
			{
				if (!milestone.IsOpen || !tags.Contains($"v{milestone.Title}"))
					continue;
				var items = await github.GetOpenMilestoneItemsAsync(
					milestone.Number,
					cancellationToken).ConfigureAwait(false);
				var target = parsed.FirstOrDefault(candidate =>
					VersionComparer.VersionRelease.Compare(candidate.Version, version) > 0 &&
					candidate.Milestone.IsOpen &&
					!tags.Contains($"v{candidate.Milestone.Title}"));
				string? targetTitle = target.Milestone?.Title;
				int? targetNumber = target.Milestone?.Number;
				if (targetTitle is null)
				{
					var planned = creatable.FirstOrDefault(candidate =>
						VersionComparer.VersionRelease.Compare(candidate.Version, version) > 0 &&
						!tags.Contains($"v{candidate.Title}"));
					targetTitle = planned.Title;
				}
				if (items.Count > 0 && targetTitle is null)
				{
					warnings.Add(
						$"milestone '{milestone.Title}' shipped as v{milestone.Title} but has open items and no unshipped milestone to move them to");
					operations.Add(new(
						milestone.Title,
						milestone.Number,
						$"v{milestone.Title}",
						FinishCloseoutStatus.Blocked,
						items.Count,
						null,
						null,
						"no eligible target milestone"));
				}
				else
				{
					operations.Add(new(
						milestone.Title,
						milestone.Number,
						$"v{milestone.Title}",
						FinishCloseoutStatus.Pending,
						items.Count,
						targetTitle,
						targetNumber,
						null));
				}
			}
			return operations;
		}

		public static IReadOnlyList<int> ExtractMergedPullRequests(
			IReadOnlyList<string> commitSubjects)
		{
			var result = new List<int>();
			var seen = new HashSet<int>();
			foreach (var subject in commitSubjects)
			{
				var start = 0;
				while (start < subject.Length)
				{
					var open = subject.IndexOf("(#", start, StringComparison.Ordinal);
					if (open < 0)
						break;
					var close = subject.IndexOf(')', open + 2);
					if (close >= 0 &&
						int.TryParse(subject.AsSpan(open + 2, close - open - 2), out var number) &&
						number > 0)
					{
						if (seen.Add(number))
							result.Add(number);
						break;
					}
					start = open + 2;
				}
			}
			return result;
		}

		public static async Task<IReadOnlyList<FinishReconcileOperation>> PlanReconciliationAsync(
			IReadOnlyList<int> pullRequestNumbers,
			GitHubMilestone target,
			ICloseoutGitHubClient github,
			CancellationToken cancellationToken)
		{
			var operations = new List<FinishReconcileOperation>();
			var seen = new HashSet<(FinishReconcileKind Kind, int Number)>();
			foreach (var pullRequestNumber in pullRequestNumbers)
			{
				var current = await github.GetPullRequestMilestoneAsync(
					pullRequestNumber,
					cancellationToken).ConfigureAwait(false);
				if (current != target.Title && seen.Add((FinishReconcileKind.PullRequest, pullRequestNumber)))
				{
					operations.Add(new(
						FinishReconcileKind.PullRequest,
						pullRequestNumber,
						null,
						current,
						target.Title,
						target.Number,
						FinishCloseoutStatus.Pending));
				}
				foreach (var issueNumber in await github.GetClosingIssuesAsync(
					pullRequestNumber,
					cancellationToken).ConfigureAwait(false))
				{
					if (!seen.Add((FinishReconcileKind.Issue, issueNumber)))
						continue;
					var issueCurrent = await github.GetIssueMilestoneAsync(
						issueNumber,
						cancellationToken).ConfigureAwait(false);
					if (issueCurrent != target.Title)
					{
						operations.Add(new(
							FinishReconcileKind.Issue,
							issueNumber,
							pullRequestNumber,
							issueCurrent,
							target.Title,
							target.Number,
							FinishCloseoutStatus.Pending));
					}
				}
			}
			return operations;
		}

		private static NuGetVersion? TryParseRelease(string title)
		{
			if (!NuGetVersion.TryParse(title, out var version) ||
				!ReleaseVersionPolicy.TryGetNumericParts(title, out var numericParts) ||
				numericParts.Length is not (3 or 4) ||
				version.ToNormalizedString() != title ||
				!string.IsNullOrEmpty(version.Metadata))
			{
				return null;
			}
			if (!version.IsPrerelease)
				return version;
			var labels = version.ReleaseLabels.ToArray();
			if (labels.Length != 2 ||
				labels[0] is not ("preview" or "rc") ||
				!int.TryParse(labels[1], out var iteration) ||
				iteration < 1)
			{
				return null;
			}
			return version;
		}
	}
}
