using SkiaSharp.ReleaseTool.Processes;

namespace SkiaSharp.ReleaseTool.Git
{
	/// <summary>
	/// The real, <c>git</c>-CLI-backed implementation of
	/// <see cref="IGitRepository"/>. Mirrors Python's
	/// <c>release_git.GitRepository</c> method-for-method; tests exercise
	/// it against real temporary repositories created with
	/// <c>git init</c> (see the test project's git repo test helper)
	/// rather than mocking Git, so the same code path is covered as in
	/// production.
	/// </summary>
	public sealed class GitRepository : IGitRepository
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

		public static GitRepository Discover(string start, IProcessRunner? runner = null)
		{
			var effectiveRunner = runner ?? new ProcessRunner();
			var result = effectiveRunner.Run(["git", "rev-parse", "--show-toplevel"], start);
			return new GitRepository(result.StandardOutput.Trim(), effectiveRunner);
		}

		public ProcessRunResult Git(IReadOnlyList<string> args, bool check = true, TimeSpan? timeout = null)
		{
			var argv = new string[args.Count + 1];
			argv[0] = "git";
			for (var i = 0; i < args.Count; i++)
				argv[i + 1] = args[i];
			return runner.Run(argv, Root, checkExitCode: check, timeout: timeout);
		}

		public void Fetch(string remote = "origin") => Git([ "fetch", remote, "--prune", "--tags" ]);

		public bool RefExists(string reference) =>
			Git(["show-ref", "--verify", "--quiet", reference], check: false).Success;

		public string Resolve(string reference) =>
			Git(["rev-parse", "--verify", $"{reference}^{{commit}}"]).StandardOutput.Trim();

		public string ReadRefFile(string reference, string path) =>
			Git(["show", $"{reference}:{path}"]).StandardOutput;

		public string ReadGitlink(string reference, string submodulePath)
		{
			var line = Git(["ls-tree", reference, "--", submodulePath]).StandardOutput.Trim();
			if (line.Length == 0)
				throw new GitException($"{submodulePath} is not a gitlink at {reference}");
			// "<mode> commit <sha>\t<path>"
			var fields = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
			if (fields.Length < 3 || fields[1] != "commit")
				throw new GitException($"{submodulePath} at {reference} is not a submodule gitlink");
			return fields[2];
		}

		public string? RemoteSha(string branch, string remote = "origin")
		{
			var line = Git(["ls-remote", "--heads", remote, $"refs/heads/{branch}"]).StandardOutput.Trim();
			if (line.Length == 0)
				return null;
			return line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)[0];
		}

		public IReadOnlyDictionary<string, string> RemoteTags(string remote = "origin", string pattern = "refs/tags/*")
		{
			var output = Git(["ls-remote", "--tags", remote, pattern]).StandardOutput;
			var tags = new Dictionary<string, string>();
			foreach (var rawLine in output.Split('\n'))
			{
				var line = rawLine.TrimEnd('\r');
				if (line.Length == 0)
					continue;
				var parts = line.Split((char[]?)null, 2, StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length < 2)
					continue;
				var sha = parts[0];
				var reference = parts[1].Trim();
				if (reference.EndsWith(PeeledTagSuffix, StringComparison.Ordinal))
				{
					reference = reference[..^PeeledTagSuffix.Length];
					tags[reference[TagRefPrefix.Length..]] = sha;
				}
				else
				{
					var name = reference[TagRefPrefix.Length..];
					if (!tags.ContainsKey(name))
						tags[name] = sha;
				}
			}
			return tags;
		}

		public IReadOnlyList<string> ReleaseBranches(string remote = "origin") =>
			SplitNonEmptyLines(
				Git(["for-each-ref", "--format=%(refname:strip=3)", $"refs/remotes/{remote}/release/"]).StandardOutput);

		public string MergeBase(string a, string b) => Git(["merge-base", a, b]).StandardOutput.Trim();

		public bool IsAncestor(string ancestor, string descendant) =>
			Git(["merge-base", "--is-ancestor", ancestor, descendant], check: false).Success;

		public IReadOnlyList<string> CommitSubjectsFirstParent(string rangeSpec) =>
			SplitNonEmptyLines(Git(["log", "--first-parent", "--format=%s", rangeSpec]).StandardOutput);

		public void RequireClean()
		{
			var status = Git(["status", "--porcelain", "--ignore-submodules"]).StandardOutput;
			if (status.Trim().Length > 0)
				throw new GitException($"working tree at {Root} is not clean:\n{status}");
		}

		public string CurrentBranch() => Git(["rev-parse", "--abbrev-ref", "HEAD"]).StandardOutput.Trim();

		public void CreateBranch(string branch, string startPoint) => Git(["branch", branch, startPoint]);

		public void Switch(string branch) => Git(["switch", branch]);

		public void SwitchCreate(string branch, string startPoint) => Git(["switch", "-c", branch, startPoint]);

		public string Commit(string message, IReadOnlyList<string>? paths = null)
		{
			if (paths is { Count: > 0 })
				Git(["add", "--", .. paths]);
			Git(["commit", "-m", message]);
			return Resolve("HEAD");
		}

		public void PushBranch(string branch, string remote = "origin", bool setUpstream = true)
		{
			var args = new List<string> { "push" };
			if (setUpstream)
				args.Add("-u");
			args.Add(remote);
			args.Add(branch);
			Git(args);
		}

		public void PushTag(string tag, string sha, string remote = "origin") =>
			Git(["push", remote, $"{sha}:refs/tags/{tag}"]);

		public bool ContainsCommit(string branchRef, string commit) => IsAncestor(commit, branchRef);

		private static IReadOnlyList<string> SplitNonEmptyLines(string text)
		{
			var lines = new List<string>();
			foreach (var rawLine in text.Split('\n'))
			{
				var line = rawLine.TrimEnd('\r');
				if (line.Length > 0)
					lines.Add(line);
			}
			return lines;
		}
	}
}
