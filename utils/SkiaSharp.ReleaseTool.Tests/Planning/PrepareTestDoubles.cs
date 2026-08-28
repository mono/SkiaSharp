using SkiaSharp.ReleaseTool.Planning;

namespace SkiaSharp.ReleaseTool.Tests.Planning
{
	internal sealed record TestVersionState(string Skia, string HarfBuzz, string Label);

	internal sealed class FakePrepareRepository : IReleaseRepository
	{
		private readonly Dictionary<string, string> refs = new(StringComparer.Ordinal);
		private readonly Dictionary<string, TestVersionState> states = new(StringComparer.Ordinal);
		private readonly Dictionary<string, string> gitlinks = new(StringComparer.Ordinal);
		private readonly Dictionary<string, string> remoteBranches = new(StringComparer.Ordinal);
		private readonly HashSet<(string Ancestor, string Descendant)> rejectedAncestry = [];
		private readonly Dictionary<string, IReadOnlyList<string>> subjects = new(StringComparer.Ordinal);
		private readonly Dictionary<string, string> messages = new(StringComparer.Ordinal);
		private readonly Dictionary<string, IReadOnlyList<string>> paths = new(StringComparer.Ordinal);

		public FakePrepareRepository(string root)
		{
			Root = root;
		}

		public string Root { get; }
		public bool FetchCalled { get; private set; }
		public List<string> ReleaseBranchNames { get; } = [];

		public void AddRef(
			string reference,
			string sha,
			TestVersionState state,
			string? skiaSha = null)
		{
			refs[reference] = sha;
			states[reference] = state;
			states[sha] = state;
			gitlinks[reference] = skiaSha ?? new string('b', 40);
			gitlinks[sha] = skiaSha ?? new string('b', 40);
		}

		public void AddRemoteRelease(
			string branch,
			string sha,
			TestVersionState state,
			string baseSha,
			string skiaSha,
			bool packageBump)
		{
			remoteBranches[branch] = sha;
			ReleaseBranchNames.Add(branch);
			states[sha] = state;
			subjects[$"{baseSha}..{sha}"] = [$"Bump the version to {branch["release/".Length..]}"];
			messages[sha] =
				$"Bump the version to {branch["release/".Length..]}\n\n" +
				$"Release-Base: {baseSha}\nRelease-Skia: {skiaSha}";
			paths[$"{baseSha}..{sha}"] = packageBump
				? [PreparePlanBuilder.VariablesPath, PreparePlanBuilder.VersionsPath]
				: [PreparePlanBuilder.VariablesPath];
		}

		public void RejectAncestry(string ancestor, string descendant) =>
			rejectedAncestry.Add((ancestor, descendant));

		public void SetCommitShape(
			string baseSha,
			string remoteSha,
			IReadOnlyList<string> commitSubjects,
			string message,
			IReadOnlyList<string> changedPaths)
		{
			subjects[$"{baseSha}..{remoteSha}"] = commitSubjects;
			messages[remoteSha] = message;
			paths[$"{baseSha}..{remoteSha}"] = changedPaths;
		}

		public Task FetchAsync(
			string remote = "origin",
			CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			FetchCalled = true;
			return Task.CompletedTask;
		}

		public Task<bool> RefExistsAsync(
			string reference,
			CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return Task.FromResult(refs.ContainsKey(reference));
		}

		public Task<string> ResolveAsync(
			string reference,
			CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (reference == "HEAD" && refs.TryGetValue("refs/remotes/origin/main", out var head))
				return Task.FromResult(head);
			return Task.FromResult(refs[reference]);
		}

		public Task<string> ReadRefFileAsync(
			string reference,
			string path,
			CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var state = states[reference];
			return Task.FromResult(path switch
			{
				PreparePlanBuilder.VariablesPath =>
					$"SKIASHARP_VERSION: {state.Skia}\nPREVIEW_LABEL: '{state.Label}'\n",
				PreparePlanBuilder.VersionsPath => VersionsText(state),
				_ => throw new InvalidOperationException(path),
			});
		}

		public Task<string> ReadGitlinkAsync(
			string reference,
			string submodulePath,
			CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return Task.FromResult(gitlinks[reference]);
		}

		public Task<string?> RemoteShaAsync(
			string branch,
			string remote = "origin",
			CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return Task.FromResult(remoteBranches.GetValueOrDefault(branch));
		}

		public Task<IReadOnlyList<string>> ReleaseBranchesAsync(
			string remote = "origin",
			CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return Task.FromResult<IReadOnlyList<string>>([.. ReleaseBranchNames]);
		}

		public Task<bool> IsAncestorAsync(
			string ancestor,
			string descendant,
			CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return Task.FromResult(!rejectedAncestry.Contains((ancestor, descendant)));
		}

		public Task<IReadOnlyList<string>> CommitSubjectsFirstParentAsync(
			string rangeSpec,
			CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return Task.FromResult(subjects.GetValueOrDefault(rangeSpec) ?? []);
		}

		public Task<string> CommitMessageAsync(
			string commit,
			CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return Task.FromResult(messages.GetValueOrDefault(commit) ?? "");
		}

		public Task<IReadOnlyList<string>> ChangedPathsAsync(
			string from,
			string to,
			CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return Task.FromResult(paths.GetValueOrDefault($"{from}..{to}") ?? []);
		}

		private static string VersionsText(TestVersionState state)
		{
			var skiaFile = state.Skia.Split('.').Length == 3 ? $"{state.Skia}.0" : state.Skia;
			var harfBuzzFile = state.HarfBuzz;
			return
				$"SkiaSharp file {skiaFile}\n" +
				$"SkiaSharp nuget {state.Skia}\n" +
				$"HarfBuzzSharp file {harfBuzzFile}\n" +
				$"HarfBuzzSharp nuget {state.HarfBuzz}\n";
		}
	}

	internal sealed class FakePrepareGitHubClient : IPrepareGitHubClient
	{
		public Dictionary<string, string> Refs { get; } = new(StringComparer.Ordinal);
		public Dictionary<(string Head, string Base), PullRequestInfo> PullRequests { get; } = [];
		public List<(string Repository, string Reference)> RefRequests { get; } = [];

		public Task<string?> GetRefShaAsync(
			string repository,
			string reference,
			CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			RefRequests.Add((repository, reference));
			return Task.FromResult(Refs.GetValueOrDefault($"{repository}:{reference}"));
		}

		public Task<PullRequestInfo?> FindOpenPullRequestAsync(
			string head,
			string @base,
			CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return Task.FromResult(PullRequests.GetValueOrDefault((head, @base)));
		}
	}
}
