using ReleaseChecklist.Core;
using ReleaseChecklist.Git;
using ReleaseChecklist.GitHub;

namespace SkiaSharp.ReleaseChecklist.Tests;

public class PrepareCompositionTests
{
	[Fact]
	public async Task PreviewPerformsNoWrites()
	{
		using var fixture = await PrepareFixture.CreateAsync();
		var found = await fixture.DiscoverAsync("main", null);
		var definition = PrepareDefinition.Build(found, fixture.Repository, fixture.GitHub);

		var preview = await ChecklistRunner.RunAsync(definition);

		Assert.Null(await fixture.Repository.RemoteBranchShaAsync(found.MaintenanceBranch));
		Assert.Null(await fixture.Repository.RemoteBranchShaAsync(found.ReleaseBranch));
		Assert.Empty(fixture.GitHub.Branches);
		Assert.Equal(ChecklistStatus.NotDone, preview.Root.Status);
	}

	[Fact]
	public async Task ExecuteConvergesBranchesAndJoin()
	{
		using var fixture = await PrepareFixture.CreateAsync();
		var found = await fixture.DiscoverAsync("main", null);
		var definition = PrepareDefinition.Build(found, fixture.Repository, fixture.GitHub);
		var branchRun = await ChecklistRunner.RunAsync(definition, ExecuteOptions());

		Assert.True(branchRun.Successful);
		Assert.True(Find(branchRun.Root, "maintenance-branch").ActionCompleted);
		Assert.NotNull(await fixture.Repository.RemoteBranchShaAsync(found.ReleaseBranch));
		Assert.Equal(found.SkiaSha, fixture.GitHub.Branches[found.ReleaseBranch]);
		Assert.Equal(ChecklistStatus.Done, Find(branchRun.Root, "release-source-ready").Status);
		Assert.True((await ChecklistRunner.RunAsync(definition)).Successful);
	}

	[Fact]
	public async Task ParallelConflictDoesNotSuppressIndependentSibling()
	{
		using var fixture = await PrepareFixture.CreateAsync();
		var found = await fixture.DiscoverAsync("main", null);
		await fixture.Repository.GitAsync(
			["push", "origin", $"HEAD:refs/heads/{found.MaintenanceBranch}"]);
		fixture.GitHub.Branches[found.ReleaseBranch] = new string('f', 40);
		var definition = PrepareDefinition.Build(found, fixture.Repository, fixture.GitHub);
		var report = await ChecklistRunner.RunAsync(definition, ExecuteOptions());

		Assert.Equal(ChecklistStatus.Blocked, Find(report.Root, "exact-release-branches").Status);
		Assert.NotNull(await fixture.Repository.RemoteBranchShaAsync(found.ReleaseBranch));
	}

	[Fact]
	public async Task StableCreatesBumpAndPullRequestThenWaitsForHumanMerge()
	{
		using var fixture = await PrepareFixture.CreateAsync();
		var main = await fixture.Repository.ResolveAsync("HEAD");
		await fixture.Repository.GitAsync(
			["push", "origin", $"HEAD:refs/heads/release/4.152.x"]);
		var found = await fixture.DiscoverAsync("main", "4.152.0");
		var definition = PrepareDefinition.Build(found, fixture.Repository, fixture.GitHub);
		var prRun = await ChecklistRunner.RunAsync(definition, ExecuteOptions());

		Assert.NotNull(await fixture.Repository.RemoteBranchShaAsync(found.StableBump.Branch!));
		Assert.Single(fixture.GitHub.PullRequests);
		Assert.Equal(
			ChecklistStatus.NotDone,
			Find(prRun.Root, "stable-bump-pull-request-merged").Status);
		Assert.Equal(main, found.MaintenanceExpectedSha);
	}

	[Fact]
	public async Task StablePrepareIsDoneAfterBumpPullRequestMerges()
	{
		using var fixture = await PrepareFixture.CreateAsync();
		await fixture.Repository.GitAsync(
			["push", "origin", "HEAD:refs/heads/release/4.152.x"]);
		var found = await fixture.DiscoverAsync("main", "4.152.0");
		var definition = PrepareDefinition.Build(found, fixture.Repository, fixture.GitHub);
		_ = await ChecklistRunner.RunAsync(definition, ExecuteOptions());

		var bumpSha = await fixture.Repository.RemoteBranchShaAsync(found.StableBump.Branch!);
		Assert.NotNull(bumpSha);
		await fixture.Repository.GitAsync(
			["push", "origin", $"{bumpSha}:refs/heads/{found.MaintenanceBranch}"]);
		fixture.GitHub.PullRequests[0] = fixture.GitHub.PullRequests[0] with
		{
			Merged = true,
			Open = false,
		};

		var rerun = await fixture.DiscoverAsync(found.ReleaseBranch, found.Identity.Raw);
		var report = await ChecklistRunner.RunAsync(
			PrepareDefinition.Build(rerun, fixture.Repository, fixture.GitHub));

		Assert.True(report.Successful);
		Assert.Equal(ChecklistStatus.Done, Find(report.Root, "maintenance-branch").Status);
		Assert.DoesNotContain(
			report.Root.Children,
			child => child.Id == "stable-non-hotfix");
	}

	[Theory]
	[InlineData("4.152.0-preview.1")]
	[InlineData("4.152.0-rc.1")]
	[InlineData("4.152.0.1-preview.1")]
	public async Task NonStableOrHotfixSkipsStableSubtree(string release)
	{
		using var fixture = await PrepareFixture.CreateAsync();
		var branch = release.Contains(".0.1", StringComparison.Ordinal)
			? "release/4.152.x"
			: "main";
		if (branch != "main")
			await fixture.Repository.GitAsync(
				["push", "origin", $"HEAD:refs/heads/{branch}"]);
		var found = await fixture.DiscoverAsync(branch, release);

		var definition = PrepareDefinition.Build(found, fixture.Repository, fixture.GitHub);

		Assert.DoesNotContain(
			((Sequence)definition.Root).Children,
			node => node.Id == "stable-non-hotfix");
	}

	private static ChecklistRunOptions ExecuteOptions() =>
		new() { Mode = ChecklistRunMode.Apply };

	private static NodeResult Find(NodeResult root, string id)
	{
		if (root.Id == id)
			return root;
		foreach (var child in root.Children)
		{
			var match = FindOrNull(child, id);
			if (match is not null)
				return match;
		}
		throw new InvalidOperationException($"Node '{id}' not found.");
	}

	private static NodeResult? FindOrNull(NodeResult root, string id)
	{
		if (root.Id == id)
			return root;
		foreach (var child in root.Children)
		{
			var match = FindOrNull(child, id);
			if (match is not null)
				return match;
		}
		return null;
	}
}

internal sealed class PrepareFixture : IDisposable
{
	private readonly TestDirectory directory;

	private PrepareFixture(
		TestDirectory directory,
		GitRepository repository,
		FakeGitHubClient github)
	{
		this.directory = directory;
		Repository = repository;
		GitHub = github;
	}

	public GitRepository Repository { get; }
	public FakeGitHubClient GitHub { get; }

	public static async Task<PrepareFixture> CreateAsync()
	{
		var directory = TestDirectory.Create();
		var runner = new ProcessRunner();
		var bare = Path.Combine(directory.Path, "origin.git");
		var worktree = Path.Combine(directory.Path, "worktree");
		await runner.RunAsync("git", ["init", "--bare", bare], directory.Path);
		await runner.RunAsync("git", ["clone", bare, worktree], directory.Path);
		var repository = new GitRepository(
			worktree, repositoryIdentity: "mono/SkiaSharp");
		await repository.GitAsync(["config", "user.name", "Release Checklist Tests"]);
		await repository.GitAsync(["config", "user.email", "release-checklist@example.invalid"]);
		Directory.CreateDirectory(Path.Combine(worktree, "scripts"));
		await File.WriteAllTextAsync(
			Path.Combine(worktree, VersionFiles.VariablesPath),
			"""
			variables:
			  SKIASHARP_VERSION: 4.152.0
			  PREVIEW_LABEL: 'preview.0'

			""");
		await File.WriteAllTextAsync(
			Path.Combine(worktree, VersionFiles.VersionsPath),
			"""
			SkiaSharp file 4.152.0.0
			HarfBuzzSharp file 8.0.0.1
			# nuget versions
			# SkiaSharp
			SkiaSharp nuget 4.152.0
			SkiaSharp.Views nuget 4.152.0
			# HarfBuzzSharp
			HarfBuzzSharp nuget 8.0.0.1

			""");
		await repository.GitAsync(["add", "scripts"]);
		await repository.GitAsync(["commit", "-m", "version state"]);
		var skia = await repository.ResolveAsync("HEAD");
		await repository.GitAsync(
			["update-index", "--add", "--cacheinfo", "160000", skia, "externals/skia"]);
		await repository.GitAsync(["commit", "-m", "add skia gitlink"]);
		await repository.GitAsync(["branch", "-M", "main"]);
		await repository.GitAsync(["push", "-u", "origin", "main"]);
		return new PrepareFixture(directory, repository, new FakeGitHubClient());
	}

	public async Task<ReleaseDiscoveryResult> DiscoverAsync(string branch, string? release)
	{
		await Repository.FetchAsync();
		return await ReleaseDiscovery.DiscoverAsync(
			new GitReleaseDiscoveryRepository(Repository),
			new ReleaseDiscoveryOptions
			{
				Branch = branch,
				Release = release,
			});
	}

	public void Dispose() => directory.Dispose();
}

internal sealed class TestDirectory : IDisposable
{
	private TestDirectory(string path)
	{
		Path = path;
		Directory.CreateDirectory(path);
	}

	public string Path { get; }

	public static TestDirectory Create([System.Runtime.CompilerServices.CallerMemberName] string? name = null) =>
		new(System.IO.Path.Combine(
			Directory.GetCurrentDirectory(),
			".test-artifacts",
			name ?? "test",
			Guid.NewGuid().ToString("N")));

	public void Dispose()
	{
		if (Directory.Exists(Path))
			Directory.Delete(Path, recursive: true);
	}
}

internal sealed class FakeGitHubClient : IGitHubRepositoryClient
{
	public Dictionary<string, string> Branches { get; } = new(StringComparer.Ordinal);
	public List<GitHubPullRequestState> PullRequests { get; } = [];

	public Task<string?> GetBranchShaAsync(
		GitHubRepositoryIdentity repository,
		string branch,
		CancellationToken cancellationToken) =>
		Task.FromResult(Branches.GetValueOrDefault(branch));

	public Task CreateBranchAsync(
		GitHubRepositoryIdentity repository,
		string branch,
		string sha,
		CancellationToken cancellationToken)
	{
		if (!Branches.TryAdd(branch, sha))
			throw new InvalidOperationException("Reference already exists.");
		return Task.CompletedTask;
	}

	public Task<IReadOnlyList<GitHubPullRequestState>> FindPullRequestsAsync(
		GitHubRepositoryIdentity repository,
		string head,
		string @base,
		CancellationToken cancellationToken) =>
		Task.FromResult<IReadOnlyList<GitHubPullRequestState>>(
			PullRequests.Where(pr => pr.Head == head && pr.Base == @base).ToArray());

	public Task<GitHubPullRequestState> CreatePullRequestAsync(
		GitHubRepositoryIdentity repository,
		string head,
		string @base,
		string title,
		string body,
		CancellationToken cancellationToken)
	{
		if (PullRequests.Any(pr => pr.Head == head && pr.Base == @base))
			throw new InvalidOperationException("Pull request already exists.");
		var pr = new GitHubPullRequestState(
			PullRequests.Count + 1,
			head,
			@base,
			$"https://example/{PullRequests.Count + 1}");
		PullRequests.Add(pr);
		return Task.FromResult(pr);
	}
}
