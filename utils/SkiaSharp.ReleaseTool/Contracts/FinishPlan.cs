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
		ReceiptInfo Receipt,
		FinishReleaseInfo Release,
		TagInfo Tag,
		string? PreviousTag,
		DraftInfo Draft,
		IReadOnlyList<string> Warnings);

	public sealed record FinishInput(string RequestedVersion);

	public sealed record ReceiptInfo(
		string SkiaSharpVersion,
		string Base,
		string Label,
		string? BuildRevision,
		string SourceCommit,
		string SourceBranch,
		string HarfBuzzSharpVersion,
		IReadOnlyList<PackageReceipt> Packages);

	public sealed record PackageReceipt(
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

	public sealed record TagInfo(
		string Name,
		string TargetCommit,
		string? ExistingSha,
		CompletionStatus Status);

	public sealed record DraftInfo(
		bool Exists,
		bool IsPublished,
		CompletionStatus Status,
		bool HasManagedMarkers);
}
