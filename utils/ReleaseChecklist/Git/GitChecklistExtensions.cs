using ReleaseChecklist.Core;

namespace ReleaseChecklist.Git;

/// <summary>Adds reusable Git steps to checklist containers.</summary>
public static class GitChecklistExtensions
{
	/// <summary>Adds a step that creates or switches to a local Git branch.</summary>
	/// <param name="parent">The container receiving the step.</param>
	/// <param name="options">The local branch configuration.</param>
	/// <returns>The added branch step.</returns>
	public static Step GitBranch(
		this IChecklistChildren parent,
		GitBranchOptions options)
	{
		var check = Check.From(async token =>
		{
			var current = await options.Repository.CurrentBranchAsync(token).ConfigureAwait(false);
			var fullRef = GitRepository.FullBranchRef(options.Branch);
			var exists = await options.Repository.RefExistsAsync(fullRef, token).ConfigureAwait(false);
			var target = exists
				? await options.Repository.ResolveAsync(fullRef, token).ConfigureAwait(false)
				: "";
			var valid = !exists ||
				await options.Repository.IsAncestorAsync(options.StartPoint, target, token)
					.ConfigureAwait(false);
			var observation = new ObservationBuilder()
				.Add("current", current)
				.Add("branch", options.Branch)
				.Add("exists", exists)
				.Add("target", target)
				.Add("start", options.StartPoint)
				.Add("valid", valid)
				.Build();
			if (!valid)
				return CheckResult.Blocked(
					$"Local branch '{options.Branch}' does not descend from its start point.",
					observation);
			return current == options.Branch
				? CheckResult.Done($"Local branch '{options.Branch}' is checked out.", observation)
				: CheckResult.NotDone($"Local branch '{options.Branch}' is not checked out.", observation);
		});
		var action = ChecklistBuilder.Action(async token =>
		{
			var fullRef = GitRepository.FullBranchRef(options.Branch);
			var exists = await options.Repository.RefExistsAsync(fullRef, token).ConfigureAwait(false);
			var arguments = exists
				? new[] { "switch", options.Branch }
				: new[] { "switch", "-c", options.Branch, options.StartPoint };
			_ = await options.Repository.GitAsync(arguments, cancellationToken: token)
				.ConfigureAwait(false);
		});
		return parent.Step(new StepOptions(options.Id, options.Title)
		{
			Check = check,
			Action = action,
			When = options.When,
		});
	}

	/// <summary>Adds a step that commits selected worktree paths when they differ.</summary>
	/// <param name="parent">The container receiving the step.</param>
	/// <param name="options">The commit configuration.</param>
	/// <returns>The added commit step.</returns>
	public static Step GitCommit(
		this IChecklistChildren parent,
		GitCommitOptions options)
	{
		var check = Check.From(async token =>
		{
			var arguments = new List<string> { "status", "--porcelain=v1", "--" };
			arguments.AddRange(options.Paths);
			var status = await options.Repository.GitAsync(arguments, cancellationToken: token)
				.ConfigureAwait(false);
			var clean = string.IsNullOrWhiteSpace(status.StandardOutput);
			var observation = new ObservationBuilder()
				.Add("clean", clean)
				.Add("paths", string.Join(',', options.Paths))
				.Build();
			return clean
				? CheckResult.Done("The selected paths are committed.", observation)
				: CheckResult.NotDone("The selected paths contain uncommitted changes.", observation);
		});
		var action = ChecklistBuilder.Action(async token =>
		{
			var add = new List<string> { "add", "--" };
			add.AddRange(options.Paths);
			_ = await options.Repository.GitAsync(add, cancellationToken: token).ConfigureAwait(false);
			var commit = new List<string> { "commit", "-m", options.Message, "--" };
			commit.AddRange(options.Paths);
			_ = await options.Repository.GitAsync(commit, cancellationToken: token).ConfigureAwait(false);
		});
		return parent.Step(new StepOptions(options.Id, options.Title)
		{
			Check = check,
			Action = action,
			When = options.When,
		});
	}

	/// <summary>Adds a step that pushes a local branch when it is ahead of the remote branch.</summary>
	/// <param name="parent">The container receiving the step.</param>
	/// <param name="options">The push configuration.</param>
	/// <returns>The added push step.</returns>
	public static Step GitPush(
		this IChecklistChildren parent,
		GitPushOptions options)
	{
		var check = Check.From(async token =>
		{
			var current = await options.Repository.CurrentBranchAsync(token).ConfigureAwait(false);
			var local = await options.Repository.ResolveAsync("HEAD", token).ConfigureAwait(false);
			var remote = await options.Repository.RemoteBranchShaAsync(options.Branch, token)
				.ConfigureAwait(false);
			var observation = new ObservationBuilder()
				.Add("current", current)
				.Add("branch", options.Branch)
				.Add("local", local)
				.Add("remote", remote ?? "")
				.Build();
			if (current != options.Branch)
				return CheckResult.Blocked($"Local branch '{options.Branch}' is not checked out.", observation);
			if (remote is null)
				return CheckResult.NotDone($"Remote branch '{options.Branch}' is missing.", observation);
			if (remote == local)
				return CheckResult.Done($"Remote branch '{options.Branch}' is current.", observation);
			await options.Repository.EnsureRemoteBranchObjectAsync(options.Branch, remote, token)
				.ConfigureAwait(false);
			return await options.Repository.IsAncestorAsync(remote, local, token).ConfigureAwait(false)
				? CheckResult.NotDone($"Local branch '{options.Branch}' has unpushed commits.", observation)
				: CheckResult.Blocked($"Local and remote branch '{options.Branch}' have diverged.", observation);
		});
		var action = ChecklistBuilder.Action(async token =>
		{
			var push = await options.Repository.GitAsync(
				["push", options.Repository.Remote, $"HEAD:{GitRepository.FullBranchRef(options.Branch)}"],
				checkExitCode: false,
				token).ConfigureAwait(false);
			if (push.ExitCode == 0)
				return;
			var local = await options.Repository.ResolveAsync("HEAD", token).ConfigureAwait(false);
			var remote = await options.Repository.RemoteBranchShaAsync(options.Branch, token)
				.ConfigureAwait(false);
			if (remote != local)
				throw new GitException($"Push of '{options.Branch}' failed: {push.StandardError.Trim()}");
		});
		return parent.Step(new StepOptions(options.Id, options.Title)
		{
			Check = check,
			Action = action,
			When = options.When,
		});
	}

	/// <summary>Adds a step that creates or verifies a remote Git branch.</summary>
	/// <param name="parent">The container receiving the step.</param>
	/// <param name="options">The branch configuration.</param>
	/// <returns>The added branch step.</returns>
	public static Step GitRemoteBranch(
		this IChecklistChildren parent,
		GitRemoteBranchOptions options)
	{
		var check = new GitRemoteBranchCheck(options);
		var action = new CreateGitRemoteBranch(options, check);
		return parent.Step(new StepOptions(options.Id, options.Title)
		{
			Check = check,
			Action = action,
			When = options.When,
		});
	}
}
