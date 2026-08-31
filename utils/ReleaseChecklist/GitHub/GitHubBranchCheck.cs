using ReleaseChecklist.Core;

namespace ReleaseChecklist.GitHub;

/// <summary>Checks whether a GitHub branch points to an accepted commit.</summary>
public sealed class GitHubBranchCheck : IChecklistCheck
{
	private readonly IGitHubRepositoryClient client;
	private readonly GitHubRepositoryIdentity repository;
	private readonly string branch;
	private readonly string expectedSha;
	private readonly Func<string, bool> acceptExisting;

	/// <summary>Initializes a new instance of the <see cref="GitHubBranchCheck" /> class.</summary>
	/// <param name="options">The branch configuration.</param>
	public GitHubBranchCheck(GitHubBranchOptions options)
	{
		client = options.Client;
		repository = options.Repository;
		branch = options.Branch;
		expectedSha = options.ExpectedSha;
		acceptExisting = options.AcceptExisting ?? (actual => actual == expectedSha);
	}

	/// <inheritdoc />
	public async ValueTask<CheckResult> EvaluateAsync(CancellationToken cancellationToken)
	{
		var actual = await client.GetBranchShaAsync(repository, branch, cancellationToken)
			.ConfigureAwait(false);
		var observation = new ObservationBuilder()
			.Add("repository", repository.ToString())
			.Add("ref", $"refs/heads/{branch}")
			.Add("exists", actual is not null)
			.Add("actual", actual ?? "")
			.Add("expected", expectedSha)
			.Build();
		return actual switch
		{
			null => CheckResult.NotDone($"GitHub branch '{repository}:{branch}' is missing.", observation),
			_ when acceptExisting(actual) =>
				CheckResult.Done($"GitHub branch '{repository}:{branch}' is ready.", observation),
			_ => CheckResult.Blocked(
				$"GitHub branch '{repository}:{branch}' conflicts at {actual}.",
				observation),
		};
	}

	internal bool Accept(string sha) => acceptExisting(sha);
}
