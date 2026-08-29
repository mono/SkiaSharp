namespace SkiaSharp.ReleaseTool.Contracts
{
	public sealed record FinishCloseoutPlan(
		int SchemaVersion,
		FinishCloseoutOperation Operation,
		Guid PlanId,
		DateTimeOffset GeneratedAt,
		string ToolingSha,
		FinishCloseoutNextAction NextAction,
		FinishReleaseInfo Release,
		string SourceCommit,
		string SourceBranch,
		string Tag,
		IReadOnlyList<FinishScheduleOperation> ScheduleOperations,
		IReadOnlyList<FinishReconcileOperation> ReconcileOperations,
		IReadOnlyList<FinishClosureOperation> ClosureOperations,
		IReadOnlyList<FinishWorkflowDispatch> Dispatches,
		IReadOnlyList<string> Warnings);

	public sealed record FinishCloseoutResult(
		int SchemaVersion,
		FinishCloseoutOperation Operation,
		Guid PlanId,
		DateTimeOffset GeneratedAt,
		string ToolingSha,
		FinishCloseoutNextAction NextAction,
		FinishReleaseInfo Release,
		string SourceCommit,
		string SourceBranch,
		string Tag,
		IReadOnlyList<FinishScheduleResult> ScheduleResults,
		IReadOnlyList<FinishReconcileResult> ReconcileResults,
		IReadOnlyList<FinishClosureResult> ClosureResults,
		IReadOnlyList<FinishWorkflowDispatch> Dispatches,
		IReadOnlyList<string> Warnings);

	public sealed record FinishScheduleOperation(
		string Title,
		int? Number,
		FinishCloseoutStatus Status,
		FinishScheduleAction Action,
		DateTimeOffset DueOn,
		string Description,
		IReadOnlyList<FinishScheduleChange> Changes);

	public sealed record FinishScheduleChange(
		string Field,
		string? From,
		string To);

	public sealed record FinishScheduleResult(
		string Title,
		int? Number,
		FinishScheduleAction Action,
		FinishCloseoutStatus Status);

	public sealed record FinishReconcileOperation(
		FinishReconcileKind Kind,
		int Number,
		int? ViaPullRequest,
		string? FromMilestone,
		string ToMilestone,
		int ToMilestoneNumber,
		FinishCloseoutStatus Status);

	public sealed record FinishReconcileResult(
		FinishReconcileKind Kind,
		int Number,
		int? ViaPullRequest,
		string? FromMilestone,
		string ToMilestone,
		FinishCloseoutStatus Status);

	public sealed record FinishClosureOperation(
		string Milestone,
		int MilestoneNumber,
		string Tag,
		FinishCloseoutStatus Status,
		int OpenItemCount,
		string? MoveTo,
		int? MoveToNumber,
		string? Detail);

	public sealed record FinishClosureResult(
		string Milestone,
		FinishCloseoutStatus Status,
		string? MovedTo,
		string? Detail);

	public sealed record FinishWorkflowDispatch(
		string Workflow,
		string Ref,
		IReadOnlyDictionary<string, string> Inputs,
		FinishDispatchStatus Status);
}
