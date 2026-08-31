using ReleaseChecklist.Core;

namespace ReleaseChecklist.GitHub;

/// <summary>Adds reusable GitHub steps to checklist containers.</summary>
public static class GitHubChecklistExtensions
{
	/// <summary>Adds a step that creates or verifies a GitHub branch.</summary>
	/// <param name="parent">The container receiving the step.</param>
	/// <param name="options">The branch configuration.</param>
	/// <returns>The added branch step.</returns>
	public static Step GitHubBranch(
		this IChecklistChildren parent,
		GitHubBranchOptions options)
	{
		var check = new GitHubBranchCheck(options);
		return parent.Step(new StepOptions(options.Id, options.Title)
		{
			Check = check,
			Action = new CreateGitHubBranch(options, check),
			When = options.When,
		});
	}

	/// <summary>Adds a step that creates or verifies a GitHub pull request.</summary>
	/// <param name="parent">The container receiving the step.</param>
	/// <param name="options">The pull request configuration.</param>
	/// <returns>The added pull request step.</returns>
	public static Step GitHubPullRequest(
		this IChecklistChildren parent,
		GitHubPullRequestOptions options)
	{
		var check = new GitHubPullRequestCheck(options);
		return parent.Step(new StepOptions(options.Id, options.Title)
		{
			Check = check,
			Action = new CreateGitHubPullRequest(options),
			When = options.When,
		});
	}

}
