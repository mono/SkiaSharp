namespace SkiaSharp.ReleaseTool.Contracts
{
	public sealed record PreparePlan(
		int SchemaVersion,
		ReleaseOperation Operation,
		Guid PlanId,
		DateTimeOffset GeneratedAt,
		string ToolingSha,
		PrepareNextAction NextAction,
		PrepareInput Input,
		PrepareReleaseInfo Release,
		PrepareBaseInfo Base,
		MaintenanceBranchInfo MaintenanceBranch,
		PrepareSkiaInfo Skia,
		RemoteState SkiaSharpRemoteState,
		PrepareVersionsInfo Versions,
		IReadOnlyList<PlanOperation> Operations,
		StableBumpInfo? StableBump,
		IReadOnlyList<string> Warnings);

	public sealed record PrepareInput(
		string IntegrationTarget,
		string? RequestedVersion,
		string? ApprovedBase);

	public sealed record PrepareReleaseInfo(
		string Identity,
		string Version,
		string Branch);

	public sealed record PrepareBaseInfo(
		string Ref,
		string Sha);

	public sealed record MaintenanceBranchInfo(
		string Name,
		bool Exists,
		MaintenanceBranchAction Action,
		string? BaseSha);

	public sealed record PrepareSkiaInfo(
		string Sha,
		RemoteState RemoteState);

	public sealed record PrepareVersionsInfo(
		bool RequiresPackageBump);

	public sealed record PlanOperation(
		PlanOperationId Id,
		PlanOperationKind Kind,
		PlanOperationStatus Status,
		string? Detail);

	public sealed record StableBumpInfo(
		string IntegrationBranch,
		string BumpBranch,
		string SkiaSharpVersion,
		string HarfBuzzSharpVersion,
		PlanOperationStatus Status,
		Uri? PullRequestUrl,
		string Title);
}
