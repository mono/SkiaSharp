namespace SkiaSharp.ReleaseTool.Contracts
{
	public sealed record FinishCreateDraftResult(
		int SchemaVersion,
		FinishArtifactOperation Operation,
		Guid PlanId,
		DateTimeOffset GeneratedAt,
		string ToolingSha,
		FinishNextAction NextAction,
		FinishReleaseInfo Release,
		string SourceCommit,
		long ReleaseId,
		Uri ReleaseUrl,
		BodyHashAlgorithm BodyHashAlgorithm,
		string BodyHash,
		IReadOnlyList<FinishWriteOperationResult> Operations);

	public sealed record FinishPublicationPlan(
		int SchemaVersion,
		FinishArtifactOperation Operation,
		Guid PlanId,
		Guid PublicationPlanId,
		DateTimeOffset GeneratedAt,
		string ToolingSha,
		FinishNextAction NextAction,
		FinishReleaseInfo Release,
		string SourceCommit,
		long ReleaseId,
		Uri ReleaseUrl,
		bool IsDraft,
		bool IsPublished,
		ManagedMarkerState MarkerState,
		bool HasGeneratedNotes,
		bool ReadyToPublish,
		BodyHashAlgorithm BodyHashAlgorithm,
		string BodyHash);

	public sealed record FinishPublishResult(
		int SchemaVersion,
		FinishArtifactOperation Operation,
		Guid PlanId,
		Guid PublicationPlanId,
		DateTimeOffset GeneratedAt,
		string ToolingSha,
		FinishNextAction NextAction,
		FinishReleaseInfo Release,
		string SourceCommit,
		long ReleaseId,
		Uri ReleaseUrl,
		BodyHashAlgorithm BodyHashAlgorithm,
		string BodyHash,
		IReadOnlyList<FinishWriteOperationResult> Operations);

	public sealed record FinishWriteOperationResult(
		FinishOperationId Id,
		FinishWriteStatus Status);
}
