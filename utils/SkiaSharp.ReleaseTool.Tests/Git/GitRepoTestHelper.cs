using SkiaSharp.ReleaseTool.Processes;

namespace SkiaSharp.ReleaseTool.Tests.Git
{
	internal static class GitRepoTestHelper
	{
		private static readonly ProcessRunner Runner = new();

		public static async Task<(string Bare, string Worktree)> CreateBareAndWorktreeAsync(
			string root,
			string name,
			CancellationToken cancellationToken)
		{
			var bare = Path.Combine(root, $"{name}-origin.git");
			var worktree = Path.Combine(root, name);
			Directory.CreateDirectory(bare);
			Directory.CreateDirectory(worktree);
			await RunGitAsync(bare, cancellationToken, "init", "--bare", "-b", "main");
			await RunGitAsync(worktree, cancellationToken, "init", "-b", "main");
			await RunGitAsync(worktree, cancellationToken, "config", "user.email", "release-bot@example.com");
			await RunGitAsync(worktree, cancellationToken, "config", "user.name", "Release Bot");
			await RunGitAsync(worktree, cancellationToken, "remote", "add", "origin", bare);
			return (bare, worktree);
		}

		public static async Task<string> CommitAllAsync(
			string root,
			string message,
			CancellationToken cancellationToken)
		{
			await RunGitAsync(root, cancellationToken, "add", "-A");
			await RunGitAsync(root, cancellationToken, "commit", "-m", message);
			return (await RunGitAsync(root, cancellationToken, "rev-parse", "HEAD")).Trim();
		}

		public static async Task<string> CommitStagedAsync(
			string root,
			string message,
			CancellationToken cancellationToken)
		{
			await RunGitAsync(root, cancellationToken, "commit", "-m", message);
			return (await RunGitAsync(root, cancellationToken, "rev-parse", "HEAD")).Trim();
		}

		public static Task StageAsync(
			string root,
			CancellationToken cancellationToken,
			params string[] paths) =>
			RunGitAsync(root, cancellationToken, ["add", "--", .. paths]);

		public static Task PushAsync(
			string root,
			CancellationToken cancellationToken,
			string branch = "main") =>
			RunGitAsync(root, cancellationToken, "push", "-u", "origin", branch);

		public static async Task AddGitlinkAsync(
			string root,
			string submodulePath,
			string sha,
			CancellationToken cancellationToken)
		{
			await RunGitAsync(
				root,
				cancellationToken,
				"update-index",
				"--add",
				"--cacheinfo",
				$"160000,{sha},{submodulePath}");
			File.WriteAllText(
				Path.Combine(root, ".gitmodules"),
				$"[submodule \"{submodulePath}\"]\n\tpath = {submodulePath}\n\turl = https://example.invalid/skia.git\n");
			await RunGitAsync(root, cancellationToken, "add", ".gitmodules");
		}

		private static async Task<string> RunGitAsync(
			string workingDirectory,
			CancellationToken cancellationToken,
			params string[] arguments) =>
			(await Runner.RunAsync(
				["git", .. arguments],
				workingDirectory,
				cancellationToken: cancellationToken)).StandardOutput;
	}
}
