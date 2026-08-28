namespace SkiaSharp.ReleaseTool.Contracts
{
	public sealed record PrepareApplyResult(
		int SchemaVersion,
		Guid PlanId,
		string ToolingSha,
		PrepareNextAction NextAction,
		PrepareReleaseInfo Release,
		IReadOnlyList<PrepareApplyOperationResult> Operations,
		Uri? StableBumpPullRequestUrl,
		IReadOnlyList<string> Warnings);

	public sealed record PrepareApplyOperationResult(
		PlanOperationId Id,
		ApplyOperationStatus Status,
		Uri? PullRequestUrl);
}
