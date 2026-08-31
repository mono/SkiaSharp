using ReleaseChecklist.Core;

namespace ReleaseChecklist.GitHub;

/// <summary>Creates a missing GitHub branch and accepts a concurrent exact creation.</summary>
public sealed class CreateGitHubBranch : IChecklistAction
{
	private readonly IGitHubRepositoryClient client;
	private readonly GitHubRepositoryIdentity repository;
	private readonly string branch;
	private readonly string expectedSha;
	private readonly GitHubBranchCheck check;

	/// <summary>Initializes a new instance of the <see cref="CreateGitHubBranch" /> class.</summary>
	/// <param name="options">The branch configuration.</param>
	/// <param name="check">The check used to validate concurrent creation.</param>
	public CreateGitHubBranch(GitHubBranchOptions options, GitHubBranchCheck check)
	{
		client = options.Client;
		repository = options.Repository;
		branch = options.Branch;
		expectedSha = options.ExpectedSha;
		this.check = check;
	}

	/// <inheritdoc />
	public async ValueTask ExecuteAsync(CancellationToken cancellationToken)
	{
		try
		{
			await client.CreateBranchAsync(repository, branch, expectedSha, cancellationToken)
				.ConfigureAwait(false);
		}
		catch
		{
			var raced = await client.GetBranchShaAsync(repository, branch, cancellationToken)
				.ConfigureAwait(false);
			if (raced is not null && check.Accept(raced))
				return;
			throw;
		}
	}
}
