using SkiaSharp.ReleaseTool.Errors;
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

		public FakePrepareRepository(string root)
		{
			Root = root;
		}

		public string Root { get; }
		public bool FetchCalled { get; private set; }
		public List<string> ReleaseBranchNames { get; } = [];
		public string CurrentBranch { get; private set; } = "main";

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
			TestVersionState state)
		{
			remoteBranches[branch] = sha;
			ReleaseBranchNames.Add(branch);
			states[sha] = state;
		}

		public void RejectAncestry(string ancestor, string descendant) =>
			rejectedAncestry.Add((ancestor, descendant));

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

		public Task<bool> CommitExistsAsync(
			string commit,
			CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return Task.FromResult(states.ContainsKey(commit));
		}

		public Task<IReadOnlyDictionary<string, string>> RemoteTagsAsync(
			string remote = "origin",
			string pattern = "refs/tags/*",
			CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return Task.FromResult<IReadOnlyDictionary<string, string>>(
				new Dictionary<string, string>(StringComparer.Ordinal));
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

		public Task RequireCleanAsync(
			IReadOnlyList<string>? allowedUntrackedPaths = null,
			CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return Task.CompletedTask;
		}

		public Task<string> CurrentBranchAsync(CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return Task.FromResult(CurrentBranch);
		}

		public Task UpdateLocalBranchAsync(
			string branch,
			string sha,
			CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			refs[$"refs/heads/{branch}"] = sha;
			return Task.CompletedTask;
		}

		public Task SwitchAsync(
			string branch,
			CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			CurrentBranch = branch;
			return Task.CompletedTask;
		}

		public Task SwitchCreateAsync(
			string branch,
			string startPoint,
			CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			refs[$"refs/heads/{branch}"] = refs.GetValueOrDefault(startPoint, startPoint);
			CurrentBranch = branch;
			return Task.CompletedTask;
		}

		public Task<string> CommitAsync(
			string message,
			IReadOnlyList<string>? paths = null,
			CancellationToken cancellationToken = default) =>
			throw new NotSupportedException("The planning fake does not create commits.");

		public Task PushBranchAsync(
			string branch,
			string remote = "origin",
			bool setUpstream = true,
			CancellationToken cancellationToken = default) =>
			throw new NotSupportedException("The planning fake does not push branches.");

		public Task PushTagAsync(
			string tag,
			string sha,
			string remote = "origin",
			CancellationToken cancellationToken = default) =>
			throw new NotSupportedException("The planning fake does not push tags.");

		public Task<string> ReadWorktreeFileAsync(
			string path,
			CancellationToken cancellationToken = default) =>
			throw new NotSupportedException("The planning fake has no worktree files.");

		public Task WriteWorktreeFileAsync(
			string path,
			string content,
			CancellationToken cancellationToken = default) =>
			throw new NotSupportedException("The planning fake has no worktree files.");

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
		public List<(string Repository, string Reference, string Sha)> CreatedRefs { get; } = [];
		public List<(string Head, string Base, string Title, string Body)> CreatedPullRequests { get; } = [];
		public Func<string, string, string, Exception?>? CreateRefFailure { get; set; }
		public Func<string, string, Exception?>? CreatePullRequestFailure { get; set; }

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

		public Task CreateRefAsync(
			string repository,
			string reference,
			string sha,
			CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (CreateRefFailure?.Invoke(repository, reference, sha) is { } failure)
				return Task.FromException(failure);
			var key = $"{repository}:{reference}";
			if (Refs.TryGetValue(key, out var existing) && existing != sha)
				return Task.FromException(new GitHubException($"conflicting ref {key}"));
			Refs[key] = sha;
			CreatedRefs.Add((repository, reference, sha));
			return Task.CompletedTask;
		}

		public Task<PullRequestInfo> CreatePullRequestAsync(
			string head,
			string @base,
			string title,
			string body,
			CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (CreatePullRequestFailure?.Invoke(head, @base) is { } failure)
				return Task.FromException<PullRequestInfo>(failure);
			var pullRequest = new PullRequestInfo(
				CreatedPullRequests.Count + 1,
				new Uri($"https://example.invalid/pr/{CreatedPullRequests.Count + 1}"));
			PullRequests[(head, @base)] = pullRequest;
			CreatedPullRequests.Add((head, @base, title, body));
			return Task.FromResult(pullRequest);
		}
	}
}
