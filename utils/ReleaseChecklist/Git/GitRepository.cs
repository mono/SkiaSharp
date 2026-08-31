namespace ReleaseChecklist.Git;

/// <summary>Provides noninteractive Git operations used by reusable checklist primitives.</summary>
public sealed class GitRepository
{
	private readonly ProcessRunner runner;

	/// <summary>Initializes a new instance of the <see cref="GitRepository" /> class.</summary>
	/// <param name="root">The repository worktree root.</param>
	/// <param name="remote">The remote name used for authoritative branch operations.</param>
	/// <param name="repositoryIdentity">The stable repository name shown in observations, or <see langword="null" /> to use the root path.</param>
	public GitRepository(
		string root,
		string remote = "origin",
		string? repositoryIdentity = null)
		: this(root, remote, repositoryIdentity, new ProcessRunner())
	{
	}

	private GitRepository(
		string root,
		string remote,
		string? repositoryIdentity,
		ProcessRunner runner)
	{
		Root = Path.GetFullPath(root);
		Remote = RequireSimpleName(remote, nameof(remote));
		RepositoryIdentity = string.IsNullOrWhiteSpace(repositoryIdentity)
			? Root
			: repositoryIdentity;
		this.runner = runner;
	}

	/// <summary>Gets the absolute worktree root.</summary>
	/// <value>The worktree root.</value>
	public string Root { get; }
	/// <summary>Gets the authoritative remote name.</summary>
	/// <value>The remote name.</value>
	public string Remote { get; }
	/// <summary>Gets the stable repository identity used in observations.</summary>
	/// <value>The repository identity.</value>
	public string RepositoryIdentity { get; }

	/// <summary>Gets the configured URL for the authoritative remote.</summary>
	/// <param name="cancellationToken">A token that cancels the lookup.</param>
	/// <returns>The configured remote URL.</returns>
	public async Task<string> RemoteUrlAsync(CancellationToken cancellationToken = default)
	{
		var result = await GitAsync(
			["remote", "get-url", Remote],
			cancellationToken: cancellationToken).ConfigureAwait(false);
		return result.StandardOutput.Trim();
	}

	/// <summary>Discovers the containing Git worktree.</summary>
	/// <param name="start">A path inside the worktree.</param>
	/// <param name="remote">The authoritative remote name.</param>
	/// <param name="repositoryIdentity">The stable repository name shown in observations, or <see langword="null" /> to use the root path.</param>
	/// <param name="cancellationToken">A token that cancels discovery.</param>
	/// <returns>The discovered repository.</returns>
	public static async Task<GitRepository> DiscoverAsync(
		string start,
		string remote = "origin",
		string? repositoryIdentity = null,
		CancellationToken cancellationToken = default)
	{
		var runner = new ProcessRunner();
		var result = await runner.RunAsync(
			"git",
			["rev-parse", "--show-toplevel"],
			start,
			cancellationToken: cancellationToken).ConfigureAwait(false);
		return new GitRepository(
			SingleLine(result.StandardOutput, "repository root"),
			remote,
			repositoryIdentity,
			runner);
	}

	/// <summary>Runs a Git command in the repository worktree.</summary>
	/// <param name="arguments">The ordered Git arguments.</param>
	/// <param name="checkExitCode"><see langword="true" /> to throw on a nonzero exit code; otherwise, <see langword="false" />.</param>
	/// <param name="cancellationToken">A token that cancels the command.</param>
	/// <returns>The captured command result.</returns>
	public Task<ProcessResult> GitAsync(
		IReadOnlyList<string> arguments,
		bool checkExitCode = true,
		CancellationToken cancellationToken = default) =>
		runner.RunAsync(
			"git",
			arguments,
			Root,
			checkExitCode,
			cancellationToken);

	/// <summary>Fetches and prunes authoritative remote branches and tags.</summary>
	/// <param name="cancellationToken">A token that cancels the fetch.</param>
	/// <returns>A task that represents the fetch.</returns>
	public async Task FetchAsync(CancellationToken cancellationToken = default) =>
		_ = await GitAsync(["fetch", Remote, "--prune", "--tags"], cancellationToken: cancellationToken)
			.ConfigureAwait(false);

	/// <summary>Determines whether a fully qualified local ref exists.</summary>
	/// <param name="reference">The fully qualified <c>refs/...</c> name.</param>
	/// <param name="cancellationToken">A token that cancels the lookup.</param>
	/// <returns><see langword="true" /> if the ref exists; otherwise, <see langword="false" />.</returns>
	public async Task<bool> RefExistsAsync(
		string reference,
		CancellationToken cancellationToken = default)
	{
		RequireFullRef(reference);
		var result = await GitAsync(
			["show-ref", "--verify", "--quiet", reference],
			checkExitCode: false,
			cancellationToken).ConfigureAwait(false);
		return BooleanExit(result, "show-ref");
	}

	/// <summary>Resolves a Git revision to its commit SHA.</summary>
	/// <param name="reference">The revision or ref to resolve.</param>
	/// <param name="cancellationToken">A token that cancels the lookup.</param>
	/// <returns>The lowercase 40-character commit SHA.</returns>
	public async Task<string> ResolveAsync(
		string reference,
		CancellationToken cancellationToken = default)
	{
		var result = await GitAsync(
			["rev-parse", "--verify", $"{reference}^{{commit}}"],
			cancellationToken: cancellationToken).ConfigureAwait(false);
		return Sha(SingleLine(result.StandardOutput, "resolved ref"));
	}

	/// <summary>Reads the exact target of a remote branch.</summary>
	/// <param name="branch">The short branch name.</param>
	/// <param name="cancellationToken">A token that cancels the lookup.</param>
	/// <returns>The branch target SHA, or <see langword="null" /> if the branch is absent.</returns>
	public async Task<string?> RemoteBranchShaAsync(
		string branch,
		CancellationToken cancellationToken = default)
	{
		var fullRef = FullBranchRef(branch);
		var result = await GitAsync(
			["ls-remote", "--heads", Remote, fullRef],
			cancellationToken: cancellationToken).ConfigureAwait(false);
		var lines = Lines(result.StandardOutput);
		if (lines.Count == 0)
			return null;
		if (lines.Count != 1)
			throw new GitException($"Remote returned multiple rows for '{fullRef}'.");
		var columns = lines[0].Split('\t');
		if (columns.Length != 2 || columns[1] != fullRef)
			throw new GitException($"Malformed remote row for '{fullRef}'.");
		return Sha(columns[0]);
	}

	/// <summary>Lists remote branches below <c>refs/heads/release/</c>.</summary>
	/// <param name="cancellationToken">A token that cancels the lookup.</param>
	/// <returns>The short release branch names in ordinal order.</returns>
	public async Task<IReadOnlyList<string>> ReleaseBranchesAsync(
		CancellationToken cancellationToken = default)
	{
		var result = await GitAsync(
			["ls-remote", "--heads", Remote, "refs/heads/release/*"],
			cancellationToken: cancellationToken).ConfigureAwait(false);
		return Lines(result.StandardOutput)
			.Select(static line => line.Split('\t'))
			.Select(static columns =>
				columns.Length == 2 && columns[1].StartsWith("refs/heads/", StringComparison.Ordinal)
					? columns[1]["refs/heads/".Length..]
					: throw new GitException("Malformed release branch row."))
			.Order(StringComparer.Ordinal)
			.ToArray();
	}

	/// <summary>Reads a text file from a Git revision without changing the worktree.</summary>
	/// <param name="reference">The revision containing the file.</param>
	/// <param name="path">The repository-relative file path.</param>
	/// <param name="cancellationToken">A token that cancels the read.</param>
	/// <returns>The file content.</returns>
	public async Task<string> ReadRefFileAsync(
		string reference,
		string path,
		CancellationToken cancellationToken = default)
	{
		RequireRelativePath(path);
		var result = await GitAsync(
			["show", $"{reference}:{path}"],
			cancellationToken: cancellationToken).ConfigureAwait(false);
		return result.StandardOutput;
	}

	/// <summary>Reads the commit recorded by a gitlink entry.</summary>
	/// <param name="reference">The revision containing the gitlink.</param>
	/// <param name="path">The repository-relative gitlink path.</param>
	/// <param name="cancellationToken">A token that cancels the read.</param>
	/// <returns>The lowercase 40-character gitlink SHA.</returns>
	public async Task<string> ReadGitlinkAsync(
		string reference,
		string path,
		CancellationToken cancellationToken = default)
	{
		RequireRelativePath(path);
		var result = await GitAsync(
			["ls-tree", reference, "--", path],
			cancellationToken: cancellationToken).ConfigureAwait(false);
		var line = SingleLine(result.StandardOutput, "gitlink");
		var tab = line.IndexOf('\t');
		var fields = tab > 0
			? line[..tab].Split(' ', StringSplitOptions.RemoveEmptyEntries)
			: [];
		if (fields.Length != 3 || fields[0] != "160000" || fields[1] != "commit" || line[(tab + 1)..] != path)
			throw new GitException($"'{path}' is not a gitlink at '{reference}'.");
		return Sha(fields[2]);
	}

	/// <summary>Determines whether one commit is an ancestor of another.</summary>
	/// <param name="ancestor">The proposed ancestor revision.</param>
	/// <param name="descendant">The proposed descendant revision.</param>
	/// <param name="cancellationToken">A token that cancels the check.</param>
	/// <returns><see langword="true" /> if the relationship exists; otherwise, <see langword="false" />.</returns>
	public async Task<bool> IsAncestorAsync(
		string ancestor,
		string descendant,
		CancellationToken cancellationToken = default)
	{
		var result = await GitAsync(
			["merge-base", "--is-ancestor", ancestor, descendant],
			checkExitCode: false,
			cancellationToken).ConfigureAwait(false);
		return BooleanExit(result, "merge-base --is-ancestor");
	}

	internal async Task EnsureRemoteBranchObjectAsync(
		string branch,
		string sha,
		CancellationToken cancellationToken = default)
	{
		var exists = await GitAsync(
			["cat-file", "-e", $"{sha}^{{commit}}"],
			checkExitCode: false,
			cancellationToken).ConfigureAwait(false);
		if (exists.ExitCode == 0)
			return;

		await GitAsync(
			["fetch", "--no-tags", Remote, FullBranchRef(branch)],
			cancellationToken: cancellationToken).ConfigureAwait(false);
		var fetched = await GitAsync(
			["cat-file", "-e", $"{sha}^{{commit}}"],
			checkExitCode: false,
			cancellationToken).ConfigureAwait(false);
		if (fetched.ExitCode != 0)
			throw new GitException($"Remote branch '{branch}' advertised unreachable commit {sha}.");
	}

	/// <summary>Determines whether a commit is contained by at least one remote-tracking branch.</summary>
	/// <param name="sha">The commit SHA.</param>
	/// <param name="cancellationToken">A token that cancels the lookup.</param>
	/// <returns><see langword="true" /> if a branch contains the commit; otherwise, <see langword="false" />.</returns>
	public async Task<bool> IsContainedInRemoteBranchAsync(
		string sha,
		CancellationToken cancellationToken = default)
	{
		var result = await GitAsync(
			["branch", "--remotes", "--contains", sha, "--format=%(refname)"],
			cancellationToken: cancellationToken).ConfigureAwait(false);
		var prefix = $"refs/remotes/{Remote}/";
		return result.StandardOutput
			.Split('\n', StringSplitOptions.RemoveEmptyEntries)
			.Any(line => line.Trim().StartsWith(prefix, StringComparison.Ordinal));
	}

	/// <summary>Gets the currently checked-out branch.</summary>
	/// <param name="cancellationToken">A token that cancels the lookup.</param>
	/// <returns>The short branch name.</returns>
	public async Task<string> CurrentBranchAsync(CancellationToken cancellationToken = default)
	{
		var result = await GitAsync(
			["branch", "--show-current"],
			cancellationToken: cancellationToken).ConfigureAwait(false);
		return SingleLine(result.StandardOutput, "current branch");
	}

	internal async Task CreateAndPushBranchAsync(
		string branch,
		string startPoint,
		Func<GitRepository, CancellationToken, ValueTask>? configureCommit,
		string commitMessage,
		Func<string, CancellationToken, ValueTask<bool>> acceptExisting,
		CancellationToken cancellationToken)
	{
		var remote = await RemoteBranchShaAsync(branch, cancellationToken).ConfigureAwait(false);
		if (remote is not null)
		{
			await EnsureRemoteBranchObjectAsync(branch, remote, cancellationToken).ConfigureAwait(false);
			if (await acceptExisting(remote, cancellationToken).ConfigureAwait(false))
				return;
			throw new GitException($"Remote branch '{branch}' appeared at conflicting SHA {remote}.");
		}

		var temporaryRoot = Path.Combine(
			Path.GetTempPath(),
			$"release-checklist-{Guid.NewGuid():N}");
		await GitAsync(
			["worktree", "add", "--detach", temporaryRoot, startPoint],
			cancellationToken: cancellationToken).ConfigureAwait(false);
		Exception? operationError = null;
		try
		{
			var isolated = new GitRepository(
				temporaryRoot,
				Remote,
				RepositoryIdentity,
				runner);
			if (configureCommit is not null)
			{
				await configureCommit(isolated, cancellationToken).ConfigureAwait(false);
				var dirty = await isolated.GitAsync(
					["status", "--porcelain=v1", "--untracked-files=all"],
					cancellationToken: cancellationToken).ConfigureAwait(false);
				if (!string.IsNullOrWhiteSpace(dirty.StandardOutput))
				{
					await isolated.GitAsync(["add", "-A"], cancellationToken: cancellationToken)
						.ConfigureAwait(false);
					await isolated.GitAsync(["commit", "-m", commitMessage], cancellationToken: cancellationToken)
						.ConfigureAwait(false);
				}
			}

			var localSha = await isolated.ResolveAsync("HEAD", cancellationToken).ConfigureAwait(false);
			var push = await isolated.GitAsync(
				["push", Remote, $"HEAD:{FullBranchRef(branch)}"],
				checkExitCode: false,
				cancellationToken).ConfigureAwait(false);
			if (push.ExitCode == 0)
				return;

			var raced = await RemoteBranchShaAsync(branch, cancellationToken).ConfigureAwait(false);
			if (raced == localSha)
				return;
			if (raced is not null)
			{
				await EnsureRemoteBranchObjectAsync(branch, raced, cancellationToken).ConfigureAwait(false);
				if (await acceptExisting(raced, cancellationToken).ConfigureAwait(false))
					return;
			}
			throw new GitException($"Non-force push of '{branch}' failed: {push.StandardError.Trim()}");
		}
		catch (Exception ex)
		{
			operationError = ex;
			throw;
		}
		finally
		{
			var remove = await GitAsync(
				["worktree", "remove", "--force", temporaryRoot],
				checkExitCode: false,
				CancellationToken.None).ConfigureAwait(false);
			if (remove.ExitCode != 0 && operationError is null)
				throw new GitException($"Unable to remove temporary worktree '{temporaryRoot}'.");
		}
	}

	/// <summary>Writes a text file in the worktree.</summary>
	/// <param name="path">The repository-relative file path.</param>
	/// <param name="content">The text to write.</param>
	/// <param name="cancellationToken">A token that cancels the write.</param>
	/// <returns>A task that represents the write.</returns>
	public Task WriteWorktreeFileAsync(
		string path,
		string content,
		CancellationToken cancellationToken = default)
	{
		RequireRelativePath(path);
		return File.WriteAllTextAsync(Path.Combine(Root, path), content, cancellationToken);
	}

	/// <summary>Reads a text file from the worktree.</summary>
	/// <param name="path">The repository-relative file path.</param>
	/// <param name="cancellationToken">A token that cancels the read.</param>
	/// <returns>The file content.</returns>
	public Task<string> ReadWorktreeFileAsync(
		string path,
		CancellationToken cancellationToken = default)
	{
		RequireRelativePath(path);
		return File.ReadAllTextAsync(Path.Combine(Root, path), cancellationToken);
	}

	/// <summary>Creates a fully qualified branch ref from a short branch name.</summary>
	/// <param name="branch">The short branch name.</param>
	/// <returns>The corresponding <c>refs/heads/...</c> name.</returns>
	public static string FullBranchRef(string branch)
	{
		RequireBranch(branch);
		return $"refs/heads/{branch}";
	}

	private static bool BooleanExit(ProcessResult result, string command) => result.ExitCode switch
	{
		0 => true,
		1 => false,
		_ => throw new GitException($"git {command} failed with exit code {result.ExitCode}."),
	};

	private static string RequireSimpleName(string value, string parameter) =>
		!string.IsNullOrWhiteSpace(value) && value.All(static c => !char.IsWhiteSpace(c))
			? value
			: throw new ArgumentException("Name must be nonempty and contain no whitespace.", parameter);

	private static void RequireFullRef(string reference)
	{
		if (!reference.StartsWith("refs/", StringComparison.Ordinal) ||
			reference.Any(char.IsWhiteSpace) ||
			reference.Contains("..", StringComparison.Ordinal))
			throw new GitException($"Invalid full ref '{reference}'.");
	}

	private static void RequireBranch(string branch)
	{
		if (string.IsNullOrWhiteSpace(branch) ||
			branch.StartsWith('/') ||
			branch.EndsWith('/') ||
			branch.Contains("..", StringComparison.Ordinal) ||
			branch.Any(char.IsWhiteSpace))
			throw new GitException($"Invalid branch '{branch}'.");
	}

	private static void RequireRelativePath(string path)
	{
		if (string.IsNullOrWhiteSpace(path) ||
			Path.IsPathRooted(path) ||
			path.Split(['/', '\\']).Contains("..", StringComparer.Ordinal))
			throw new GitException($"Invalid repository-relative path '{path}'.");
	}

	private static string Sha(string value)
	{
		value = value.TrimEnd('\r', '\n');
		if (value.Length != 40 || value.Any(static c => c is not (>= '0' and <= '9' or >= 'a' and <= 'f')))
			throw new GitException($"Invalid Git SHA '{value}'.");
		return value;
	}

	private static string SingleLine(string text, string description)
	{
		var lines = Lines(text);
		return lines.Count == 1
			? lines[0]
			: throw new GitException($"Expected one {description}, found {lines.Count}.");
	}

	private static IReadOnlyList<string> Lines(string text) =>
		text.Split('\n', StringSplitOptions.RemoveEmptyEntries)
			.Select(static line => line.TrimEnd('\r'))
			.ToArray();
}
