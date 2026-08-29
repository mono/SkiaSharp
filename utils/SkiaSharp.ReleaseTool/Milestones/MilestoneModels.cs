namespace SkiaSharp.ReleaseTool.Milestones
{
	internal sealed record GitHubMilestone(
		int Number,
		string Title,
		bool IsOpen,
		DateTimeOffset? DueOn,
		string? Description);

	internal sealed record GitHubMilestoneItem(
		int Number,
		string Title,
		Uri Url,
		bool IsPullRequest);
}
