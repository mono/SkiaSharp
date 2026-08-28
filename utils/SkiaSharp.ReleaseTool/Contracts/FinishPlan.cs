using SkiaSharp.ReleaseTool.Model;

namespace SkiaSharp.ReleaseTool.Contracts
{
	public sealed record FinishPlan(
		int SchemaVersion,
		ReleaseOperation Operation,
		Guid PlanId,
		DateTimeOffset GeneratedAt,
		string ToolingSha,
		FinishNextAction NextAction,
		FinishInput Input,
		FinishReceiptInfo Receipt,
		FinishReleaseInfo Release,
		FinishTagInfo Tag,
		string? PreviousTag,
		FinishDraftInfo Draft,
		IReadOnlyList<FinishOperation> Operations,
		IReadOnlyList<string> Warnings);

	public sealed record FinishInput(string RequestedVersion);

	public sealed record FinishReceiptInfo(
		string SkiaSharpVersion,
		string Base,
		string Label,
		string? BuildRevision,
		string SourceCommit,
		string SourceBranch,
		string HarfBuzzSharpVersion,
		IReadOnlyList<FinishPackageReceipt> Packages);

	public sealed record FinishPackageReceipt(
		string Id,
		string Version,
		string SourceCommit,
		string SourceBranch);

	public sealed record FinishReleaseInfo(
		string Identity,
		string Version,
		string Branch,
		string Raw,
		string Numeric,
		string Label,
		ReleaseKind ReleaseType,
		bool Stable,
		string Title,
		string Tag);

	public sealed record FinishTagInfo(
		string Name,
		string TargetCommit,
		string? ExistingSha,
		FinishState Status);

	public sealed record FinishDraftInfo(
		bool Exists,
		bool IsPublished,
		FinishState Status,
		ManagedMarkerState MarkerState,
		string? TargetCommitish,
		Uri? Url,
		string? Body);

	public sealed record FinishOperation(
		FinishOperationId Id,
		FinishOperationKind Kind,
		PlanOperationStatus Status,
		string? Detail);

	public sealed record FinishPendingReport(
		int SchemaVersion,
		FinishPendingOperation Operation,
		DateTimeOffset GeneratedAt,
		string ToolingSha,
		PendingNextAction NextAction,
		string RequestedVersion,
		IReadOnlyList<PendingPackage> MissingPackages,
		double ElapsedSeconds,
		double DeadlineSeconds,
		string Message);

	public sealed record PendingPackage(string Id, string Version);
}
