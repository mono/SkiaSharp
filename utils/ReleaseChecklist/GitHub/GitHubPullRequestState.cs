namespace ReleaseChecklist.GitHub;

/// <summary>Describes a GitHub pull request relevant to a checklist step.</summary>
/// <param name="Number">The pull request number.</param>
/// <param name="Head">The source branch name.</param>
/// <param name="Base">The target branch name.</param>
/// <param name="Url">The public pull request URL.</param>
/// <param name="Merged"><see langword="true" /> if the pull request was merged.</param>
/// <param name="Open"><see langword="true" /> if the pull request is open.</param>
public sealed record GitHubPullRequestState(
	int Number,
	string Head,
	string Base,
	string Url,
	bool Merged = false,
	bool Open = true);
