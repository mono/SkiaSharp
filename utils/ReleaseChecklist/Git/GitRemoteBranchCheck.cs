using ReleaseChecklist.Core;

namespace ReleaseChecklist.Git;

/// <summary>Checks whether a remote Git branch satisfies configured branch policy.</summary>
public sealed class GitRemoteBranchCheck : IChecklistCheck
{
	private readonly GitRepository repository;
	private readonly string branch;
	private readonly string expectedTarget;
	private readonly Func<GitRemoteBranchState, CancellationToken, ValueTask<bool>> acceptExisting;

	/// <summary>Initializes a new instance of the <see cref="GitRemoteBranchCheck" /> class.</summary>
	/// <param name="options">The branch configuration.</param>
	public GitRemoteBranchCheck(GitRemoteBranchOptions options)
	{
		repository = options.Repository;
		branch = options.Branch;
		expectedTarget = options.ExpectedTarget;
		acceptExisting = options.AcceptExisting ??
			((state, _) => ValueTask.FromResult(state.Sha == expectedTarget));
	}

	/// <inheritdoc />
	public async ValueTask<CheckResult> EvaluateAsync(CancellationToken cancellationToken)
	{
		var sha = await repository.RemoteBranchShaAsync(branch, cancellationToken).ConfigureAwait(false);
		var state = new GitRemoteBranchState(branch, GitRepository.FullBranchRef(branch), sha is not null, sha);
		var observation = new ObservationBuilder()
			.Add("repository", repository.RepositoryIdentity)
			.Add("ref", state.FullRef)
			.Add("exists", state.Exists)
			.Add("actual", sha ?? "")
			.Add("creation-target", expectedTarget)
			.Build();
		if (sha is null)
			return CheckResult.NotDone($"Remote branch '{branch}' is missing.", observation);
		await repository.EnsureRemoteBranchObjectAsync(branch, sha, cancellationToken)
			.ConfigureAwait(false);
		if (await acceptExisting(state, cancellationToken).ConfigureAwait(false))
			return CheckResult.Done($"Remote branch '{branch}' is ready at {sha}.", observation);
		return CheckResult.Blocked(
			$"Remote branch '{branch}' at {sha} does not satisfy the expected state " +
			$"for creation target {expectedTarget}.",
			observation);
	}

	internal ValueTask<bool> AcceptShaAsync(string sha, CancellationToken cancellationToken) =>
		acceptExisting(
			new GitRemoteBranchState(branch, GitRepository.FullBranchRef(branch), true, sha),
			cancellationToken);
}
