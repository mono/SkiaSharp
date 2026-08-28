using SkiaSharp.ReleaseTool.Git;
using SkiaSharp.ReleaseTool.Processes;
using SkiaSharp.ReleaseTool.Tests.Processes;
using Xunit;

namespace SkiaSharp.ReleaseTool.Tests.Git
{
	public sealed class GitRepositoryTests
	{
		[Fact]
		public async Task Real_repository_operations_round_trip()
		{
			using var root = new TestDirectory("git-round-trip");
			var (_, worktree) = await GitRepoTestHelper.CreateBareAndWorktreeAsync(
				root.Path,
				"repo",
				TestContext.Current.CancellationToken);
			File.WriteAllText(Path.Combine(worktree, "file.txt"), "first\r\nsecond\r\n");
			var first = await GitRepoTestHelper.CommitAllAsync(
				worktree,
				"first",
				TestContext.Current.CancellationToken);
			await GitRepoTestHelper.PushAsync(worktree, TestContext.Current.CancellationToken);
			File.WriteAllText(Path.Combine(worktree, "file.txt"), "updated");
			var second = await GitRepoTestHelper.CommitAllAsync(
				worktree,
				"second",
				TestContext.Current.CancellationToken);

			var repository = await GitRepository.DiscoverAsync(
				worktree,
				cancellationToken: TestContext.Current.CancellationToken);
			await repository.FetchAsync(cancellationToken: TestContext.Current.CancellationToken);

			Assert.True(await repository.RefExistsAsync(
				"refs/remotes/origin/main",
				TestContext.Current.CancellationToken));
			Assert.Equal(first, await repository.RemoteShaAsync(
				"main",
				cancellationToken: TestContext.Current.CancellationToken));
			Assert.True(await repository.IsAncestorAsync(
				first,
				second,
				TestContext.Current.CancellationToken));
			Assert.False(await repository.IsAncestorAsync(
				second,
				first,
				TestContext.Current.CancellationToken));
			Assert.Equal(first, await repository.MergeBaseAsync(
				first,
				second,
				TestContext.Current.CancellationToken));
			Assert.Equal("first\r\nsecond\r\n", await repository.ReadRefFileAsync(
				first,
				"file.txt",
				TestContext.Current.CancellationToken));
			var blob = (await repository.GitAsync(
				["rev-parse", $"{first}:file.txt"],
				cancellationToken: TestContext.Current.CancellationToken)).StandardOutput.Trim();
			Assert.True(await repository.CommitExistsAsync(
				first,
				TestContext.Current.CancellationToken));
			Assert.False(await repository.CommitExistsAsync(
				blob,
				TestContext.Current.CancellationToken));
			Assert.False(await repository.CommitExistsAsync(
				new string('f', 40),
				TestContext.Current.CancellationToken));
		}

		[Fact]
		public async Task Gitlink_and_tag_parsing_round_trip()
		{
			using var root = new TestDirectory("git-tag");
			var (_, worktree) = await GitRepoTestHelper.CreateBareAndWorktreeAsync(
				root.Path,
				"repo",
				TestContext.Current.CancellationToken);
			var skiaSha = new string('a', 40);
			await GitRepoTestHelper.AddGitlinkAsync(
				worktree,
				"externals/skia",
				skiaSha,
				TestContext.Current.CancellationToken);
			File.WriteAllText(Path.Combine(worktree, "root.txt"), "content");
			await GitRepoTestHelper.StageAsync(
				worktree,
				TestContext.Current.CancellationToken,
				"root.txt");
			var commit = await GitRepoTestHelper.CommitStagedAsync(
				worktree,
				"initial",
				TestContext.Current.CancellationToken);
			await GitRepoTestHelper.PushAsync(worktree, TestContext.Current.CancellationToken);

			var repository = new GitRepository(worktree);
			Assert.Equal(
				skiaSha,
				await repository.ReadGitlinkAsync(
					"HEAD",
					"externals/skia",
					TestContext.Current.CancellationToken));

			await repository.PushTagAsync(
				"v3.119.0",
				commit,
				cancellationToken: TestContext.Current.CancellationToken);
			Assert.Equal(
				commit,
				(await repository.RemoteTagsAsync(
					cancellationToken: TestContext.Current.CancellationToken))["v3.119.0"]);
		}

		[Fact]
		public async Task Expected_exit_one_is_false_but_exit_128_is_fatal()
		{
			var runner = new RecordingProcessRunner();
			runner.Enqueue(Result(1));
			runner.Enqueue(Result(128, standardError: "fatal: bad ref"));
			runner.Enqueue(Result(1));
			runner.Enqueue(Result(128, standardError: "fatal: bad commit"));
			var repository = new GitRepository("/repo", runner);

			Assert.False(await repository.RefExistsAsync(
				"refs/heads/missing",
				TestContext.Current.CancellationToken));
			await Assert.ThrowsAsync<GitException>(
				() => repository.RefExistsAsync(
					"refs/heads/bad",
					TestContext.Current.CancellationToken));
			Assert.False(await repository.IsAncestorAsync(
				"a",
				"b",
				TestContext.Current.CancellationToken));
			await Assert.ThrowsAsync<GitException>(
				() => repository.IsAncestorAsync(
					"bad",
					"b",
					TestContext.Current.CancellationToken));
		}

		[Theory]
		[InlineData("main")]
		[InlineData("origin/main")]
		[InlineData("refs/heads/bad ref")]
		[InlineData("refs/heads/../bad")]
		[InlineData("refs/heads/bad^name")]
		[InlineData("refs/heads/topic./child")]
		public async Task Ref_exists_rejects_noncanonical_refs_before_git(string reference)
		{
			var runner = new RecordingProcessRunner();
			var repository = new GitRepository("/repo", runner);

			await Assert.ThrowsAsync<GitException>(
				() => repository.RefExistsAsync(
					reference,
					TestContext.Current.CancellationToken));
			Assert.Empty(runner.Invocations);
		}

		[Fact]
		public async Task Ls_remote_parsing_is_exact_and_CRLF_safe()
		{
			var sha = new string('a', 40);
			var runner = new RecordingProcessRunner();
			runner.Enqueue(Result(0, $"{sha}\trefs/heads/main\r\n"));
			runner.Enqueue(Result(0, $"{sha} refs/heads/main\n"));
			runner.Enqueue(Result(0, $"{sha}\trefs/heads/other\n"));
			var repository = new GitRepository("/repo", runner);

			Assert.Equal(sha, await repository.RemoteShaAsync(
				"main",
				cancellationToken: TestContext.Current.CancellationToken));
			await Assert.ThrowsAsync<GitException>(
				() => repository.RemoteShaAsync(
					"main",
					cancellationToken: TestContext.Current.CancellationToken));
			await Assert.ThrowsAsync<GitException>(
				() => repository.RemoteShaAsync(
					"main",
					cancellationToken: TestContext.Current.CancellationToken));
		}

		[Fact]
		public async Task Annotated_tags_use_peeled_SHA_and_reject_malformed_refs()
		{
			var tagObject = new string('a', 40);
			var commit = new string('b', 40);
			var runner = new RecordingProcessRunner();
			runner.Enqueue(Result(
				0,
				$"{tagObject}\trefs/tags/v3.119.0\r\n" +
				$"{commit}\trefs/tags/v3.119.0^{{}}\r\n"));
			runner.Enqueue(Result(0, $"{commit}\trefs/heads/main\n"));
			var repository = new GitRepository("/repo", runner);

			Assert.Equal(
				commit,
				(await repository.RemoteTagsAsync(
					cancellationToken: TestContext.Current.CancellationToken))["v3.119.0"]);
			await Assert.ThrowsAsync<GitException>(
				() => repository.RemoteTagsAsync(
					cancellationToken: TestContext.Current.CancellationToken));
		}

		[Fact]
		public async Task Malformed_SHA_gitlink_and_branch_output_is_rejected()
		{
			var runner = new RecordingProcessRunner();
			runner.Enqueue(Result(0, "short\n"));
			runner.Enqueue(Result(0, $"{new string('a', 40)} commit externals/skia\n"));
			runner.Enqueue(Result(0, "not-a-release-branch\n"));
			var repository = new GitRepository("/repo", runner);

			await Assert.ThrowsAsync<GitException>(
				() => repository.ResolveAsync(
					"HEAD",
					TestContext.Current.CancellationToken));
			await Assert.ThrowsAsync<GitException>(
				() => repository.ReadGitlinkAsync(
					"HEAD",
					"externals/skia",
					TestContext.Current.CancellationToken));
			await Assert.ThrowsAsync<GitException>(
				() => repository.ReleaseBranchesAsync(
					cancellationToken: TestContext.Current.CancellationToken));
		}

		[Fact]
		public async Task Uppercase_SHA_output_is_rejected()
		{
			var runner = new RecordingProcessRunner();
			runner.Enqueue(Result(0, $"{new string('A', 40)}\n"));
			var repository = new GitRepository("/repo", runner);

			await Assert.ThrowsAsync<GitException>(
				() => repository.ResolveAsync(
					"HEAD",
					TestContext.Current.CancellationToken));
		}

		[Fact]
		public async Task Read_ref_file_preserves_machine_output_exactly()
		{
			const string output = "line one\r\nline two\r\n";
			var runner = new RecordingProcessRunner();
			runner.Enqueue(Result(0, output));
			var repository = new GitRepository("/repo", runner);

			Assert.Equal(
				output,
				await repository.ReadRefFileAsync(
					"HEAD",
					"file.txt",
					TestContext.Current.CancellationToken));
		}

		[Fact]
		public async Task Clean_check_allows_only_exact_untracked_artifacts()
		{
			using var root = new TestDirectory("git-clean-allow");
			var (_, worktree) = await GitRepoTestHelper.CreateBareAndWorktreeAsync(
				root.Path,
				"repo",
				TestContext.Current.CancellationToken);
			File.WriteAllText(Path.Combine(worktree, "tracked.txt"), "tracked");
			_ = await GitRepoTestHelper.CommitAllAsync(
				worktree,
				"initial",
				TestContext.Current.CancellationToken);
			var repository = new GitRepository(worktree);
			var plan = Path.Combine(worktree, "artifacts", "my plan.json");
			Directory.CreateDirectory(Path.GetDirectoryName(plan)!);
			File.WriteAllText(plan, "{}");

			await repository.RequireCleanAsync(
				[plan],
				TestContext.Current.CancellationToken);

			File.WriteAllText(
				Path.Combine(worktree, "artifacts", "unexpected.json"),
				"{}");
			await Assert.ThrowsAsync<GitException>(
				() => repository.RequireCleanAsync(
					[plan],
					TestContext.Current.CancellationToken));
		}

		private static ProcessRunResult Result(
			int exitCode,
			string standardOutput = "",
			string standardError = "") =>
			new(["git"], exitCode, standardOutput, standardError);
	}
}
