using SkiaSharp.ReleaseTool.Git;
using SkiaSharp.ReleaseTool.Milestones;
using Xunit;

namespace SkiaSharp.ReleaseTool.Tests.Git
{
	public sealed class CloseoutGitTests
	{
		[Fact]
		public async Task First_parent_range_excludes_side_branch_pull_request_subjects()
		{
			using var directory = new TestDirectory("closeout-first-parent");
			var (_, worktree) = await GitRepoTestHelper.CreateBareAndWorktreeAsync(
				directory.Path,
				"repo",
				TestContext.Current.CancellationToken);
			File.WriteAllText(Path.Combine(worktree, "base.txt"), "base");
			var lower = await GitRepoTestHelper.CommitAllAsync(
				worktree,
				"Base",
				TestContext.Current.CancellationToken);
			var repository = new GitRepository(worktree);

			_ = await repository.GitAsync(
				["switch", "-c", "feature"],
				cancellationToken: TestContext.Current.CancellationToken);
			File.WriteAllText(Path.Combine(worktree, "feature.txt"), "feature");
			_ = await GitRepoTestHelper.CommitAllAsync(
				worktree,
				"Side branch (#99)",
				TestContext.Current.CancellationToken);
			_ = await repository.GitAsync(
				["switch", "main"],
				cancellationToken: TestContext.Current.CancellationToken);
			File.WriteAllText(Path.Combine(worktree, "main.txt"), "main");
			_ = await GitRepoTestHelper.CommitAllAsync(
				worktree,
				"Main change (#7)",
				TestContext.Current.CancellationToken);
			_ = await repository.GitAsync(
				["merge", "--no-ff", "feature", "-m", "Merge feature (#42)"],
				cancellationToken: TestContext.Current.CancellationToken);
			var source = await repository.ResolveAsync(
				"HEAD",
				TestContext.Current.CancellationToken);

			var subjects = await repository.CommitSubjectsFirstParentAsync(
				lower,
				source,
				TestContext.Current.CancellationToken);
			var pullRequests = MilestonePlanner.ExtractMergedPullRequests(subjects);

			Assert.Equal([42, 7], pullRequests);
			Assert.DoesNotContain(99, pullRequests);
			Assert.True(await repository.IsAncestorAsync(
				lower,
				source,
				TestContext.Current.CancellationToken));
		}

		[Fact]
		public void Pull_request_parser_skips_malformed_decoys_within_a_subject()
		{
			Assert.Equal(
				[1237],
				MilestonePlanner.ExtractMergedPullRequests(
				[
					"Fixed unit tests crashes and a WGL deadlock (#1228 for master) (#1237)",
				]));
		}

		[Fact]
		public void Closing_issue_parser_supports_all_GitHub_keywords_and_qualified_references()
		{
			var body =
				"close #1\ncloses: #2\nclosed mono/SkiaSharp#3\n" +
				"fix #4\nfixes: #5\nfixed #6\n" +
				"resolve #7\nresolves: #8\nresolved #9\nFixes #5\n" +
				"<!-- Fixes #123 -->\nFixes mono/skia#124\n" +
				"`Fixes #125`\n```\nFixes #126\n```\n" +
				"Fixes #2147483648";

			Assert.Equal(
				[1, 2, 3, 4, 5, 6, 7, 8, 9],
				MilestonePlanner.ExtractClosingIssues(body));
		}

		[Fact]
		public async Task Empty_commit_subjects_are_ignored()
		{
			using var directory = new TestDirectory("closeout-empty-subject");
			var (_, worktree) = await GitRepoTestHelper.CreateBareAndWorktreeAsync(
				directory.Path,
				"repo",
				TestContext.Current.CancellationToken);
			File.WriteAllText(Path.Combine(worktree, "base.txt"), "base");
			var lower = await GitRepoTestHelper.CommitAllAsync(
				worktree,
				"Base",
				TestContext.Current.CancellationToken);
			var repository = new GitRepository(worktree);
			_ = await repository.GitAsync(
				["commit", "--allow-empty", "--allow-empty-message", "-m", ""],
				cancellationToken: TestContext.Current.CancellationToken);
			File.WriteAllText(Path.Combine(worktree, "later.txt"), "later");
			var source = await GitRepoTestHelper.CommitAllAsync(
				worktree,
				"Later (#5)",
				TestContext.Current.CancellationToken);

			var subjects = await repository.CommitSubjectsFirstParentAsync(
				lower,
				source,
				TestContext.Current.CancellationToken);

			Assert.Equal(["Later (#5)"], subjects);
			Assert.Equal([5], MilestonePlanner.ExtractMergedPullRequests(subjects));
		}
	}
}
