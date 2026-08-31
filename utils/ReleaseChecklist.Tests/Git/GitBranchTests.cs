using ReleaseChecklist.Core;
using ReleaseChecklist.FileSystem;
using ReleaseChecklist.Git;

namespace ReleaseChecklist.Tests.Git;

public class GitBranchTests
{
	[Fact]
	public async Task SmallGitStepsConvergeAFileChangeAndPush()
	{
		using var fixture = await GitFixture.CreateAsync();
		var start = await fixture.Repository.ResolveAsync("HEAD");
		var file = Path.Combine(fixture.Worktree, "README.md");
		var definition = new ChecklistBuilder().Sequence("root", "Root", root =>
		{
			root.GitBranch(new GitBranchOptions
			{
				Id = "branch",
				Title = "Create branch",
				Repository = fixture.Repository,
				Branch = "release/small-steps",
				StartPoint = start,
			});
			root.FileContents(new FileContentsOptions
			{
				Id = "file",
				Title = "Update file",
				Path = file,
				Transform = _ => "updated\n",
			});
			root.GitCommit(new GitCommitOptions
			{
				Id = "commit",
				Title = "Commit file",
				Repository = fixture.Repository,
				Paths = ["README.md"],
				Message = "Update README",
			});
			root.GitPush(new GitPushOptions
			{
				Id = "push",
				Title = "Push branch",
				Repository = fixture.Repository,
				Branch = "release/small-steps",
			});
		});

		var report = await ChecklistRunner.RunAsync(
			definition,
			new ChecklistRunOptions { Mode = ChecklistRunMode.Apply });

		Assert.True(report.Successful);
		Assert.NotNull(await fixture.Repository.RemoteBranchShaAsync("release/small-steps"));
		Assert.Equal("updated\n", await File.ReadAllTextAsync(file));
		Assert.True((await ChecklistRunner.RunAsync(definition)).Successful);
	}

	[Fact]
	public async Task MissingBranchCanBeCreatedWithoutForceAndRerunIsDone()
	{
		using var fixture = await GitFixture.CreateAsync();
		var start = await fixture.Repository.ResolveAsync("HEAD");
		var originalBranch = await fixture.Repository.CurrentBranchAsync();
		var definition = new ChecklistBuilder().Sequence("root", "Root", root =>
			root.GitRemoteBranch(new GitRemoteBranchOptions
			{
				Id = "branch",
				Title = "Release branch",
				Repository = fixture.Repository,
				Branch = "release/1.0.0-preview.1",
				StartPoint = start,
				ExpectedTarget = start,
			}));

		var preview = await ChecklistRunner.RunAsync(definition);
		Assert.Equal(ChecklistStatus.NotDone, preview.Root.Status);
		Assert.Null(await fixture.Repository.RemoteBranchShaAsync("release/1.0.0-preview.1"));
		var localOnly = Path.Combine(fixture.Worktree, "local-only.txt");
		await File.WriteAllTextAsync(localOnly, "leave untouched");
		var execute = await ChecklistRunner.RunAsync(
			definition,
			new ChecklistRunOptions { Mode = ChecklistRunMode.Apply });

		Assert.True(execute.Successful);
		Assert.Equal("leave untouched", await File.ReadAllTextAsync(localOnly));
		Assert.Equal(start, await fixture.Repository.RemoteBranchShaAsync("release/1.0.0-preview.1"));
		Assert.Equal(originalBranch, await fixture.Repository.CurrentBranchAsync());
		var worktrees = await fixture.Repository.GitAsync(["worktree", "list", "--porcelain"]);
		Assert.Single(
			worktrees.StandardOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries),
			static line => line.StartsWith("worktree ", StringComparison.Ordinal));
		Assert.True((await ChecklistRunner.RunAsync(definition)).Successful);
	}

	[Fact]
	public async Task ExistingConflictingBranchIsBlocked()
	{
		using var fixture = await GitFixture.CreateAsync();
		var expected = await fixture.Repository.ResolveAsync("HEAD");
		await File.WriteAllTextAsync(Path.Combine(fixture.Worktree, "other.txt"), "other");
		await fixture.Repository.GitAsync(["add", "other.txt"]);
		await fixture.Repository.GitAsync(["commit", "-m", "other"]);
		var other = await fixture.Repository.ResolveAsync("HEAD");
		await fixture.Repository.GitAsync(
			["push", "origin", $"HEAD:refs/heads/release/conflict"]);
		var check = new GitRemoteBranchCheck(new GitRemoteBranchOptions
		{
			Id = "branch",
			Title = "Branch",
			Repository = fixture.Repository,
			Branch = "release/conflict",
			StartPoint = expected,
			ExpectedTarget = expected,
		});
		var definition = new ChecklistBuilder().Sequence("root", "Root", root =>
			root.Step(new StepOptions("branch", "Branch") { Check = check }));

		var report = await ChecklistRunner.RunAsync(definition);

		Assert.NotEqual(expected, other);
		Assert.Equal(ChecklistStatus.Blocked, report.Root.Children[0].Status);
	}
}

internal sealed class GitFixture : IDisposable
{
	private GitFixture(TestDirectory directory, string worktree, GitRepository repository)
	{
		Directory = directory;
		Worktree = worktree;
		Repository = repository;
	}

	private TestDirectory Directory { get; }
	public string Worktree { get; }
	public GitRepository Repository { get; }

	public static async Task<GitFixture> CreateAsync()
	{
		var directory = TestDirectory.Create();
		var bare = Path.Combine(directory.Path, "origin.git");
		var worktree = Path.Combine(directory.Path, "worktree");
		var runner = new ProcessRunner();
		await runner.RunAsync("git", ["init", "--bare", bare], directory.Path);
		await runner.RunAsync("git", ["clone", bare, worktree], directory.Path);
		var repository = new GitRepository(worktree, repositoryIdentity: "example/repo");
		await repository.GitAsync(["config", "user.name", "Release Checklist Tests"]);
		await repository.GitAsync(["config", "user.email", "release-checklist@example.invalid"]);
		await File.WriteAllTextAsync(Path.Combine(worktree, "README.md"), "initial\n");
		await repository.GitAsync(["add", "README.md"]);
		await repository.GitAsync(["commit", "-m", "initial"]);
		await repository.GitAsync(["branch", "-M", "main"]);
		await repository.GitAsync(["push", "-u", "origin", "main"]);
		return new GitFixture(directory, worktree, repository);
	}

	public void Dispose() => Directory.Dispose();
}
