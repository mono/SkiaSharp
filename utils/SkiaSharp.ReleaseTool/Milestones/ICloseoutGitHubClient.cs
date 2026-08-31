using SkiaSharp.ReleaseTool.Finishing;

namespace SkiaSharp.ReleaseTool.Milestones
{
	internal interface ICloseoutGitHubClient : IFinishGitHubClient
	{
		Task<IReadOnlyList<GitHubMilestone>> GetMilestonesAsync(
			CancellationToken cancellationToken = default);

		Task<GitHubMilestone> CreateMilestoneAsync(
			string title,
			DateTimeOffset dueOn,
			string description,
			CancellationToken cancellationToken = default);

		Task UpdateMilestoneAsync(
			int number,
			DateTimeOffset dueOn,
			string description,
			CancellationToken cancellationToken = default);

		Task<IReadOnlyList<GitHubMilestoneItem>> GetOpenMilestoneItemsAsync(
			int milestoneNumber,
			CancellationToken cancellationToken = default);

		Task<string?> GetPullRequestMilestoneAsync(
			int pullRequestNumber,
			CancellationToken cancellationToken = default);

		Task<string?> GetPullRequestBodyAsync(
			int pullRequestNumber,
			CancellationToken cancellationToken = default);

		Task<IReadOnlyList<int>> GetClosingIssuesAsync(
			int pullRequestNumber,
			CancellationToken cancellationToken = default);

		Task<string?> GetIssueMilestoneAsync(
			int issueNumber,
			CancellationToken cancellationToken = default);

		Task UpdateItemMilestoneAsync(
			int itemNumber,
			int milestoneNumber,
			CancellationToken cancellationToken = default);

		Task CloseMilestoneAsync(
			int milestoneNumber,
			CancellationToken cancellationToken = default);

		Task DispatchWorkflowAsync(
			string workflow,
			string reference,
			IReadOnlyDictionary<string, string> inputs,
			CancellationToken cancellationToken = default);

	}
}
