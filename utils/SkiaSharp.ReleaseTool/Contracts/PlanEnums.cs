using System.Text.Json.Serialization;

namespace SkiaSharp.ReleaseTool.Contracts
{
	public enum ReleaseOperation
	{
		[JsonStringEnumMemberName("prepare")]
		Prepare,

		[JsonStringEnumMemberName("finish")]
		Finish,
	}

	public enum FinishPendingOperation
	{
		[JsonStringEnumMemberName("finish-plan-pending")]
		FinishPlanPending,
	}

	public enum FinishNextAction
	{
		[JsonStringEnumMemberName("create-draft")]
		CreateDraft,

		[JsonStringEnumMemberName("plan-publication")]
		PlanPublication,

		[JsonStringEnumMemberName("publish")]
		Publish,

		[JsonStringEnumMemberName("closeout")]
		Closeout,
	}

	public enum FinishArtifactOperation
	{
		[JsonStringEnumMemberName("finish-create-draft")]
		CreateDraft,

		[JsonStringEnumMemberName("finish-plan-publication")]
		PlanPublication,

		[JsonStringEnumMemberName("finish-publish")]
		Publish,
	}

	public enum FinishCloseoutOperation
	{
		[JsonStringEnumMemberName("finish-closeout")]
		Apply,
	}

	public enum FinishCloseoutNextAction
	{
		[JsonStringEnumMemberName("closeout")]
		Closeout,

		[JsonStringEnumMemberName("blocked")]
		Blocked,

		[JsonStringEnumMemberName("done")]
		Done,
	}

	public enum FinishCloseoutStatus
	{
		[JsonStringEnumMemberName("pending")]
		Pending,

		[JsonStringEnumMemberName("done")]
		Done,

		[JsonStringEnumMemberName("skipped")]
		Skipped,

		[JsonStringEnumMemberName("blocked")]
		Blocked,
	}

	public enum FinishScheduleAction
	{
		[JsonStringEnumMemberName("create")]
		Create,

		[JsonStringEnumMemberName("update")]
		Update,

		[JsonStringEnumMemberName("none")]
		None,
	}

	public enum FinishReconcileKind
	{
		[JsonStringEnumMemberName("pull-request")]
		PullRequest,

		[JsonStringEnumMemberName("issue")]
		Issue,
	}

	public enum FinishDispatchStatus
	{
		[JsonStringEnumMemberName("pending")]
		Pending,

		[JsonStringEnumMemberName("dispatched")]
		Dispatched,
	}

	public enum FinishWriteStatus
	{
		[JsonStringEnumMemberName("created")]
		Created,

		[JsonStringEnumMemberName("existing")]
		Existing,

		[JsonStringEnumMemberName("migrated")]
		Migrated,

		[JsonStringEnumMemberName("published")]
		Published,

		[JsonStringEnumMemberName("already-published")]
		AlreadyPublished,
	}

	public enum BodyHashAlgorithm
	{
		[JsonStringEnumMemberName("SHA256")]
		Sha256,
	}

	public enum PendingNextAction
	{
		[JsonStringEnumMemberName("pending")]
		Pending,
	}

	public enum FinishState
	{
		[JsonStringEnumMemberName("done")]
		Done,

		[JsonStringEnumMemberName("pending")]
		Pending,
	}

	public enum ManagedMarkerState
	{
		[JsonStringEnumMemberName("none")]
		None,

		[JsonStringEnumMemberName("complete")]
		Complete,
	}

	public enum FinishOperationId
	{
		[JsonStringEnumMemberName("create-tag")]
		CreateTag,

		[JsonStringEnumMemberName("create-draft")]
		CreateDraft,

		[JsonStringEnumMemberName("publish-release")]
		PublishRelease,

		[JsonStringEnumMemberName("closeout")]
		Closeout,
	}

	public enum FinishOperationKind
	{
		[JsonStringEnumMemberName("git-tag")]
		GitTag,

		[JsonStringEnumMemberName("github-release")]
		GitHubRelease,

		[JsonStringEnumMemberName("release-closeout")]
		ReleaseCloseout,
	}

	public enum PrepareNextAction
	{
		[JsonStringEnumMemberName("apply")]
		Apply,

		[JsonStringEnumMemberName("await-merge")]
		AwaitMerge,

		[JsonStringEnumMemberName("done")]
		Done,

		[JsonStringEnumMemberName("blocked")]
		Blocked,
	}

	public enum MaintenanceBranchAction
	{
		[JsonStringEnumMemberName("none")]
		None,

		[JsonStringEnumMemberName("create")]
		Create,
	}

	public enum RemoteState
	{
		[JsonStringEnumMemberName("matching")]
		Matching,

		[JsonStringEnumMemberName("missing")]
		Missing,

		[JsonStringEnumMemberName("conflict")]
		Conflict,
	}

	public enum PlanOperationId
	{
		[JsonStringEnumMemberName("create-maintenance-branch")]
		CreateMaintenanceBranch,

		[JsonStringEnumMemberName("create-skia-ref")]
		CreateSkiaRef,

		[JsonStringEnumMemberName("create-release-branch")]
		CreateReleaseBranch,

		[JsonStringEnumMemberName("open-stable-bump-pr")]
		OpenStableBumpPullRequest,
	}

	public enum PlanOperationKind
	{
		[JsonStringEnumMemberName("git-ref")]
		GitRef,

		[JsonStringEnumMemberName("github-ref")]
		GitHubRef,

		[JsonStringEnumMemberName("github-pull-request")]
		GitHubPullRequest,
	}

	public enum PlanOperationStatus
	{
		[JsonStringEnumMemberName("done")]
		Done,

		[JsonStringEnumMemberName("pending")]
		Pending,

		[JsonStringEnumMemberName("blocked")]
		Blocked,

		[JsonStringEnumMemberName("skipped")]
		Skipped,

		[JsonStringEnumMemberName("awaiting-user")]
		AwaitingUser,
	}

	public enum ApplyOperationStatus
	{
		[JsonStringEnumMemberName("done")]
		Done,

		[JsonStringEnumMemberName("skipped")]
		Skipped,
	}

}
