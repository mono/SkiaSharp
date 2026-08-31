using ReleaseChecklist.Core;

namespace ReleaseChecklist.Git;

/// <summary>Creates and pushes a missing branch without force-updating existing state.</summary>
public sealed class CreateGitRemoteBranch : IChecklistAction
{
	private readonly GitRepository repository;
	private readonly string branch;
	private readonly string startPoint;
	private readonly GitRemoteBranchCheck check;
	private readonly Func<GitRepository, CancellationToken, ValueTask>? configureCommit;
	private readonly string commitMessage;

	/// <summary>Initializes a new instance of the <see cref="CreateGitRemoteBranch" /> class.</summary>
	/// <param name="options">The branch configuration.</param>
	/// <param name="check">The check used to validate concurrent branch creation.</param>
	public CreateGitRemoteBranch(GitRemoteBranchOptions options, GitRemoteBranchCheck check)
	{
		repository = options.Repository;
		branch = options.Branch;
		startPoint = options.StartPoint;
		this.check = check;
		configureCommit = options.ConfigureCommit;
		commitMessage = options.CommitMessage ?? $"Create {options.Branch}";
	}

	/// <inheritdoc />
	public ValueTask ExecuteAsync(CancellationToken cancellationToken) =>
		new(repository.CreateAndPushBranchAsync(
			branch,
			startPoint,
			configureCommit,
			commitMessage,
			check.AcceptShaAsync,
			cancellationToken));
}
