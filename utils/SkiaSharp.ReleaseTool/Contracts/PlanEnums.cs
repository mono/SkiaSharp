using System.Text.Json.Serialization;

namespace SkiaSharp.ReleaseTool.Contracts
{
	public enum ReleaseOperation
	{
		[JsonStringEnumMemberName("prepare")]
		Prepare,

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
