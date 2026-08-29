using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Planning;
using SkiaSharp.ReleaseTool.Processes;

namespace SkiaSharp.ReleaseTool.Git
{
	public sealed class GitRepository : IGitRepository, IReleaseRepository
	{
		private const string TagRefPrefix = "refs/tags/";
		private const string PeeledTagSuffix = "^{}";
		private readonly IProcessRunner runner;

		public GitRepository(string root, IProcessRunner? runner = null)
		{
			Root = root;
			this.runner = runner ?? new ProcessRunner();
		}

		public string Root { get; }

		public static async Task<GitRepository> DiscoverAsync(
			string start,
			IProcessRunner? runner = null,
			CancellationToken cancellationToken = default)
		{
			var effectiveRunner = runner ?? new ProcessRunner();
			var result = await effectiveRunner.RunAsync(
				["git", "rev-parse", "--show-toplevel"],
				start,
				cancellationToken: cancellationToken).ConfigureAwait(false);
			return new GitRepository(ParseSingleLine(result.StandardOutput, "repository root"), effectiveRunner);
		}

		public async Task<ProcessRunResult> GitAsync(
			IReadOnlyList<string> args,
			bool check = true,
			TimeSpan? timeout = null,
			CancellationToken cancellationToken = default)
		{
			var argv = new string[args.Count + 1];
			argv[0] = "git";
			for (var index = 0; index < args.Count; index++)
				argv[index + 1] = args[index];

			try
			{
				return await runner.RunAsync(
					argv,
					Root,
					checkExitCode: check,
					timeout: timeout,
					cancellationToken: cancellationToken).ConfigureAwait(false);
			}
			catch (ReleaseToolException ex) when (ex is not GitException)
			{
				throw new GitException(ex.Message, ex);
			}
		}

		public async Task FetchAsync(string remote = "origin", CancellationToken cancellationToken = default) =>
			_ = await GitAsync(["fetch", remote, "--prune", "--tags"], cancellationToken: cancellationToken).ConfigureAwait(false);

		public async Task<bool> RefExistsAsync(string reference, CancellationToken cancellationToken = default)
		{
			if (!GitReferencePolicy.IsFullyQualified(reference))
			{
				throw new GitException(
					$"ref must be a fully-qualified, well-formed refs/... name: '{reference}'");
			}
			var result = await GitAsync(
				["show-ref", "--verify", "--quiet", reference],
				check: false,
				cancellationToken: cancellationToken).ConfigureAwait(false);
			return ExpectedBooleanExit(result, "show-ref");
		}

		public async Task<string> ResolveAsync(string reference, CancellationToken cancellationToken = default)
		{
			var result = await GitAsync(
				["rev-parse", "--verify", $"{reference}^{{commit}}"],
				cancellationToken: cancellationToken).ConfigureAwait(false);
			return ParseSha(ParseSingleLine(result.StandardOutput, "resolved ref"), "resolved ref");
		}

		public async Task<bool> CommitExistsAsync(
			string commit,
			CancellationToken cancellationToken = default)
		{
			_ = ParseSha(commit, "commit");
			var result = await GitAsync(
				["rev-parse", "--verify", "--quiet", $"{commit}^{{commit}}"],
				check: false,
				cancellationToken: cancellationToken).ConfigureAwait(false);
			return ExpectedBooleanExit(result, "rev-parse --verify --quiet");
		}

		public async Task<string> ReadRefFileAsync(
			string reference,
			string path,
			CancellationToken cancellationToken = default) =>
			(await GitAsync(["show", $"{reference}:{path}"], cancellationToken: cancellationToken).ConfigureAwait(false))
				.StandardOutput;

		public async Task<string> ReadGitlinkAsync(
			string reference,
			string submodulePath,
			CancellationToken cancellationToken = default)
		{
			var result = await GitAsync(
				["ls-tree", reference, "--", submodulePath],
				cancellationToken: cancellationToken).ConfigureAwait(false);
			var line = ParseSingleLine(result.StandardOutput, "gitlink");
			var tab = line.IndexOf('\t');
			if (tab <= 0 || line.IndexOf('\t', tab + 1) >= 0 || line[(tab + 1)..] != submodulePath)
				throw new GitException($"malformed gitlink output for '{submodulePath}'");

			var fields = line[..tab].Split(' ', StringSplitOptions.RemoveEmptyEntries);
			if (fields.Length != 3 || fields[0] != "160000" || fields[1] != "commit")
				throw new GitException($"{submodulePath} at {reference} is not a submodule gitlink");
			return ParseSha(fields[2], "gitlink");
		}

		public async Task<string?> RemoteShaAsync(
			string branch,
			string remote = "origin",
			CancellationToken cancellationToken = default)
		{
			var expectedRef = $"refs/heads/{branch}";
			var result = await GitAsync(
				["ls-remote", "--heads", remote, expectedRef],
				cancellationToken: cancellationToken).ConfigureAwait(false);
			var lines = MachineLines(result.StandardOutput, "ls-remote");
			if (lines.Count == 0)
				return null;
			if (lines.Count != 1)
				throw new GitException($"ls-remote returned multiple rows for '{expectedRef}'");
			var (sha, reference) = ParseRemoteRow(lines[0]);
			if (reference != expectedRef)
				throw new GitException($"ls-remote returned unexpected ref '{reference}'");
			return sha;
		}

		public async Task<IReadOnlyDictionary<string, string>> RemoteTagsAsync(
			string remote = "origin",
			string pattern = "refs/tags/*",
			CancellationToken cancellationToken = default)
		{
			var result = await GitAsync(
				["ls-remote", "--tags", remote, pattern],
				cancellationToken: cancellationToken).ConfigureAwait(false);
			var tags = new Dictionary<string, string>(StringComparer.Ordinal);
			foreach (var line in MachineLines(result.StandardOutput, "ls-remote --tags"))
			{
				var (sha, reference) = ParseRemoteRow(line);
				if (!reference.StartsWith(TagRefPrefix, StringComparison.Ordinal))
					throw new GitException($"ls-remote returned non-tag ref '{reference}'");

				var peeled = reference.EndsWith(PeeledTagSuffix, StringComparison.Ordinal);
				var nameLength = reference.Length - TagRefPrefix.Length - (peeled ? PeeledTagSuffix.Length : 0);
				if (nameLength <= 0)
					throw new GitException("ls-remote returned an empty tag name");
				var name = reference.Substring(TagRefPrefix.Length, nameLength);
				if (peeled || !tags.ContainsKey(name))
					tags[name] = sha;
			}
			return tags;
		}

		public async Task<IReadOnlyList<string>> ReleaseBranchesAsync(
			string remote = "origin",
			CancellationToken cancellationToken = default)
		{
			var result = await GitAsync(
				["for-each-ref", "--format=%(refname:strip=3)", $"refs/remotes/{remote}/release/"],
				cancellationToken: cancellationToken).ConfigureAwait(false);
			var lines = MachineLines(result.StandardOutput, "for-each-ref");
			if (lines.Any(line => !line.StartsWith("release/", StringComparison.Ordinal) || line.Any(char.IsWhiteSpace)))
				throw new GitException("for-each-ref returned a malformed release branch");
			return lines;
		}

		public async Task<string> MergeBaseAsync(
			string a,
			string b,
			CancellationToken cancellationToken = default)
		{
			var result = await GitAsync(["merge-base", a, b], cancellationToken: cancellationToken).ConfigureAwait(false);
			return ParseSha(ParseSingleLine(result.StandardOutput, "merge base"), "merge base");
		}

		public async Task<bool> IsAncestorAsync(
			string ancestor,
			string descendant,
			CancellationToken cancellationToken = default)
		{
			var result = await GitAsync(
				["merge-base", "--is-ancestor", ancestor, descendant],
				check: false,
				cancellationToken: cancellationToken).ConfigureAwait(false);
			return ExpectedBooleanExit(result, "merge-base --is-ancestor");
		}

		public async Task<IReadOnlyList<string>> CommitSubjectsFirstParentAsync(
			string? exclusiveLowerBound,
			string sourceCommit,
			CancellationToken cancellationToken = default)
		{
			_ = ParseSha(sourceCommit, "source commit");
			if (exclusiveLowerBound is not null)
				_ = ParseSha(exclusiveLowerBound, "exclusive lower bound");
			var range = exclusiveLowerBound is null
				? sourceCommit
				: $"{exclusiveLowerBound}..{sourceCommit}";
			var result = await GitAsync(
				["log", "--first-parent", "--format=%s", range],
				cancellationToken: cancellationToken).ConfigureAwait(false);
			var subjects = new List<string>();
			using var reader = new StringReader(result.StandardOutput);
			while (reader.ReadLine() is { } subject)
			{
				if (subject.Length > 0)
					subjects.Add(subject);
			}
			return subjects;
		}

		public async Task RequireCleanAsync(
			IReadOnlyList<string>? allowedUntrackedPaths = null,
			CancellationToken cancellationToken = default)
		{
			var result = await GitAsync(
				[
					"status",
					"--porcelain=v1",
					"-z",
					"--untracked-files=all",
					"--ignore-submodules",
				],
				cancellationToken: cancellationToken).ConfigureAwait(false);
			if (result.StandardOutput.Length == 0)
				return;

			var allowed = (allowedUntrackedPaths ?? [])
				.Select(Path.GetFullPath)
				.Select(path => Path.GetRelativePath(Root, path).Replace('\\', '/'))
				.ToHashSet(StringComparer.Ordinal);
			var disallowed = result.StandardOutput
				.Split('\0', StringSplitOptions.RemoveEmptyEntries)
				.Where(entry =>
					entry.Length < 4 ||
					!entry.StartsWith("?? ", StringComparison.Ordinal) ||
					!allowed.Contains(entry[3..]))
				.ToArray();
			if (disallowed.Length > 0)
				throw new GitException($"working tree at {Root} is not clean:\n{string.Join('\n', disallowed)}");
		}

		public async Task<string> CurrentBranchAsync(CancellationToken cancellationToken = default)
		{
			var result = await GitAsync(
				["rev-parse", "--abbrev-ref", "HEAD"],
				cancellationToken: cancellationToken).ConfigureAwait(false);
			return ParseSingleLine(result.StandardOutput, "current branch");
		}

		public async Task CreateBranchAsync(
			string branch,
			string startPoint,
			CancellationToken cancellationToken = default) =>
			_ = await GitAsync(["branch", branch, startPoint], cancellationToken: cancellationToken).ConfigureAwait(false);

		public async Task UpdateLocalBranchAsync(
			string branch,
			string sha,
			CancellationToken cancellationToken = default) =>
			_ = await GitAsync(
				["update-ref", $"refs/heads/{branch}", sha],
				cancellationToken: cancellationToken).ConfigureAwait(false);

		public async Task SwitchAsync(string branch, CancellationToken cancellationToken = default) =>
			_ = await GitAsync(["switch", branch], cancellationToken: cancellationToken).ConfigureAwait(false);

		public async Task SwitchCreateAsync(
			string branch,
			string startPoint,
			CancellationToken cancellationToken = default) =>
			_ = await GitAsync(["switch", "-c", branch, startPoint], cancellationToken: cancellationToken).ConfigureAwait(false);

		public async Task<string> CommitAsync(
			string message,
			IReadOnlyList<string>? paths = null,
			CancellationToken cancellationToken = default)
		{
			if (paths is { Count: > 0 })
				_ = await GitAsync(["add", "--", .. paths], cancellationToken: cancellationToken).ConfigureAwait(false);
			_ = await GitAsync(["commit", "-m", message], cancellationToken: cancellationToken).ConfigureAwait(false);
			return await ResolveAsync("HEAD", cancellationToken).ConfigureAwait(false);
		}

		public async Task PushBranchAsync(
			string branch,
			string remote = "origin",
			bool setUpstream = true,
			CancellationToken cancellationToken = default)
		{
			var args = new List<string> { "push" };
			if (setUpstream)
				args.Add("-u");
			args.Add(remote);
			args.Add(branch);
			_ = await GitAsync(args, cancellationToken: cancellationToken).ConfigureAwait(false);
		}

		public async Task PushTagAsync(
			string tag,
			string sha,
			string remote = "origin",
			CancellationToken cancellationToken = default) =>
			_ = await GitAsync(
				["push", remote, $"{sha}:refs/tags/{tag}"],
				cancellationToken: cancellationToken).ConfigureAwait(false);

		public Task<bool> ContainsCommitAsync(
			string branchRef,
			string commit,
			CancellationToken cancellationToken = default) =>
			IsAncestorAsync(commit, branchRef, cancellationToken);

		public Task<string> ReadWorktreeFileAsync(
			string path,
			CancellationToken cancellationToken = default) =>
			File.ReadAllTextAsync(Path.Combine(Root, path), cancellationToken);

		public Task WriteWorktreeFileAsync(
			string path,
			string content,
			CancellationToken cancellationToken = default) =>
			File.WriteAllTextAsync(Path.Combine(Root, path), content, cancellationToken);

		private static bool ExpectedBooleanExit(ProcessRunResult result, string operation) =>
			result.ExitCode switch
			{
				0 => true,
				1 => false,
				_ => throw new GitException(
					$"git {operation} failed with exit code {result.ExitCode}: " +
					FirstNonEmpty(result.StandardError, result.StandardOutput)),
			};

		private static (string Sha, string Reference) ParseRemoteRow(string line)
		{
			var tab = line.IndexOf('\t');
			if (tab <= 0 || line.IndexOf('\t', tab + 1) >= 0)
				throw new GitException($"malformed ls-remote row: '{line}'");
			return (ParseSha(line[..tab], "ls-remote"), line[(tab + 1)..]);
		}

		private static string ParseSha(string value, string description)
		{
			if (value.Length != 40 || value.Any(static character =>
				character is not (>= '0' and <= '9' or >= 'a' and <= 'f')))
			{
				throw new GitException($"{description} is not a 40-hex SHA: '{value}'");
			}
			return value;
		}

		private static string ParseSingleLine(string text, string description)
		{
			var lines = MachineLines(text, description);
			if (lines.Count != 1)
				throw new GitException($"expected one {description} row, found {lines.Count}");
			return lines[0];
		}

		private static IReadOnlyList<string> MachineLines(string text, string description)
		{
			if (text.Length == 0)
				return [];

			var rawLines = text.Split('\n');
			var lines = new List<string>(rawLines.Length);
			for (var index = 0; index < rawLines.Length; index++)
			{
				var line = rawLines[index].EndsWith('\r')
					? rawLines[index][..^1]
					: rawLines[index];
				if (line.Length == 0)
				{
					if (index == rawLines.Length - 1)
						continue;
					throw new GitException($"{description} contained an unexpected empty row");
				}
				lines.Add(line);
			}
			return lines;
		}

		private static string FirstNonEmpty(params string[] values)
		{
			foreach (var value in values)
			{
				if (!string.IsNullOrWhiteSpace(value))
					return value.Trim();
			}
			return "no output";
		}
	}
}
