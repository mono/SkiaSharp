using SkiaSharp.ReleaseTool.Git;
using Xunit;

namespace SkiaSharp.ReleaseTool.Tests.Git
{
	/// <summary>
	/// Ported test-for-test from Python's <c>tests/test_release_git.py</c>:
	/// every test exercises <see cref="GitRepository"/> against a real,
	/// throwaway on-disk repository rather than a fake.
	/// </summary>
	public sealed class GitRepositoryTests : IDisposable
	{
		private readonly DirectoryInfo root = Directory.CreateTempSubdirectory("skiasharp-release-tool-git-tests-");

		public void Dispose()
		{
			try
			{
				root.Delete(recursive: true);
			}
			catch (IOException)
			{
				// Best-effort cleanup only; a stray file handle on a
				// throwaway temp directory must never fail the test.
			}
		}

		[Fact]
		public void Discover_finds_repo_root()
		{
			var (_, worktree) = GitRepoTestHelper.CreateBareAndWorktree(root.FullName, "repo");
			File.WriteAllText(Path.Combine(worktree, "file.txt"), "hi");
			GitRepoTestHelper.CommitAll(worktree, "init");
			var nested = Directory.CreateDirectory(Path.Combine(worktree, "sub"));

			var repo = GitRepository.Discover(nested.FullName);

			// Compare by content rather than by string equality: on macOS
			// a temp directory path routes through a `/tmp` -> `/private/tmp`
			// symlink that `git rev-parse --show-toplevel` resolves but the
			// original `worktree` string does not.
			Assert.True(File.Exists(Path.Combine(repo.Root, "file.txt")));
		}

		[Fact]
		public void RefExists_and_RemoteSha_round_trip()
		{
			var (_, worktree) = GitRepoTestHelper.CreateBareAndWorktree(root.FullName, "repo");
			File.WriteAllText(Path.Combine(worktree, "file.txt"), "hi");
			var sha = GitRepoTestHelper.CommitAll(worktree, "init");
			GitRepoTestHelper.Push(worktree);
			var repo = new GitRepository(worktree);

			repo.Fetch();

			Assert.True(repo.RefExists("refs/remotes/origin/main"));
			Assert.Equal(sha, repo.RemoteSha("main"));
			Assert.Null(repo.RemoteSha("does-not-exist"));
		}

		[Fact]
		public void ReadRefFile_reads_file_content_at_ref()
		{
			var (_, worktree) = GitRepoTestHelper.CreateBareAndWorktree(root.FullName, "repo");
			File.WriteAllText(Path.Combine(worktree, "file.txt"), "hello world");
			GitRepoTestHelper.CommitAll(worktree, "init");
			var repo = new GitRepository(worktree);

			Assert.Equal("hello world", repo.ReadRefFile("HEAD", "file.txt"));
		}

		[Fact]
		public void ReadGitlink_returns_the_submodule_sha()
		{
			var (_, worktree) = GitRepoTestHelper.CreateBareAndWorktree(root.FullName, "repo");
			var skiaSha = new string('a', 40);
			GitRepoTestHelper.AddGitlink(worktree, "externals/skia", skiaSha);
			File.WriteAllText(Path.Combine(worktree, "root.txt"), "x");
			GitRepoTestHelper.Stage(worktree, "root.txt");
			GitRepoTestHelper.CommitStaged(worktree, "init");
			var repo = new GitRepository(worktree);

			Assert.Equal(skiaSha, repo.ReadGitlink("HEAD", "externals/skia"));
		}

		[Fact]
		public void ReadGitlink_rejects_non_gitlink_path()
		{
			var (_, worktree) = GitRepoTestHelper.CreateBareAndWorktree(root.FullName, "repo");
			File.WriteAllText(Path.Combine(worktree, "file.txt"), "hi");
			GitRepoTestHelper.CommitAll(worktree, "init");
			var repo = new GitRepository(worktree);

			Assert.Throws<GitException>(() => repo.ReadGitlink("HEAD", "file.txt"));
		}

		[Fact]
		public void MergeBase_and_IsAncestor()
		{
			var (_, worktree) = GitRepoTestHelper.CreateBareAndWorktree(root.FullName, "repo");
			File.WriteAllText(Path.Combine(worktree, "file.txt"), "v1");
			var first = GitRepoTestHelper.CommitAll(worktree, "first");
			File.WriteAllText(Path.Combine(worktree, "file.txt"), "v2");
			var second = GitRepoTestHelper.CommitAll(worktree, "second");
			var repo = new GitRepository(worktree);

			Assert.Equal(first, repo.MergeBase(first, second));
			Assert.True(repo.IsAncestor(first, second));
			Assert.False(repo.IsAncestor(second, first));
		}

		[Fact]
		public void RequireClean_detects_dirty_worktree()
		{
			var (_, worktree) = GitRepoTestHelper.CreateBareAndWorktree(root.FullName, "repo");
			File.WriteAllText(Path.Combine(worktree, "file.txt"), "v1");
			GitRepoTestHelper.CommitAll(worktree, "first");
			var repo = new GitRepository(worktree);

			repo.RequireClean(); // must not throw

			File.WriteAllText(Path.Combine(worktree, "file.txt"), "dirty");
			Assert.Throws<GitException>(() => repo.RequireClean());
		}

		[Fact]
		public void ReleaseBranches_lists_remote_release_branches()
		{
			var (_, worktree) = GitRepoTestHelper.CreateBareAndWorktree(root.FullName, "repo");
			File.WriteAllText(Path.Combine(worktree, "file.txt"), "v1");
			GitRepoTestHelper.CommitAll(worktree, "first");
			GitRepoTestHelper.Push(worktree);
			var repo = new GitRepository(worktree);
			repo.Git(["branch", "release/3.119.0-preview.1"]);
			repo.PushBranch("release/3.119.0-preview.1");

			repo.Fetch();

			Assert.Equal(["release/3.119.0-preview.1"], repo.ReleaseBranches());
		}

		[Fact]
		public void PushTag_and_RemoteTags_round_trip()
		{
			var (_, worktree) = GitRepoTestHelper.CreateBareAndWorktree(root.FullName, "repo");
			File.WriteAllText(Path.Combine(worktree, "file.txt"), "v1");
			var sha = GitRepoTestHelper.CommitAll(worktree, "first");
			GitRepoTestHelper.Push(worktree);
			var repo = new GitRepository(worktree);

			repo.PushTag("v3.119.0", sha);

			Assert.Equal(sha, repo.RemoteTags()["v3.119.0"]);
		}

		[Fact]
		public void ContainsCommit_reports_reachability_from_a_remote_ref()
		{
			var (_, worktree) = GitRepoTestHelper.CreateBareAndWorktree(root.FullName, "repo");
			File.WriteAllText(Path.Combine(worktree, "file.txt"), "v1");
			var first = GitRepoTestHelper.CommitAll(worktree, "first");
			File.WriteAllText(Path.Combine(worktree, "file.txt"), "v2");
			GitRepoTestHelper.CommitAll(worktree, "second");
			GitRepoTestHelper.Push(worktree);
			var repo = new GitRepository(worktree);

			repo.Fetch();

			Assert.True(repo.ContainsCommit("refs/remotes/origin/main", first));
		}

		[Fact]
		public void CommitSubjectsFirstParent_returns_oldest_first()
		{
			var (_, worktree) = GitRepoTestHelper.CreateBareAndWorktree(root.FullName, "repo");
			File.WriteAllText(Path.Combine(worktree, "file.txt"), "v1");
			GitRepoTestHelper.CommitAll(worktree, "first commit");
			File.WriteAllText(Path.Combine(worktree, "file.txt"), "v2");
			GitRepoTestHelper.CommitAll(worktree, "second commit");
			var repo = new GitRepository(worktree);

			Assert.Equal(["second commit", "first commit"], repo.CommitSubjectsFirstParent("HEAD"));
		}
	}
}
