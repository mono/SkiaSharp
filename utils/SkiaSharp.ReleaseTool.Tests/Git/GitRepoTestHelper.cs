using SkiaSharp.ReleaseTool.Processes;

namespace SkiaSharp.ReleaseTool.Tests.Git
{
	/// <summary>
	/// Test-only helpers for building throwaway local Git repositories.
	/// Ported from Python's <c>tests/gitrepo_helpers.py</c>:
	/// <see cref="Git.GitRepositoryTests"/> exercises real <c>git</c>
	/// commands against temporary on-disk repositories created here
	/// rather than mocking Git, so the same code path is exercised as in
	/// production.
	/// </summary>
	internal static class GitRepoTestHelper
	{
		private static readonly ProcessRunner Runner = new();

		public static string InitBare(string path)
		{
			Directory.CreateDirectory(path);
			RunGit(path, "init", "--bare", "-b", "main");
			return path;
		}

		public static string InitWorktree(string path, string? origin = null)
		{
			Directory.CreateDirectory(path);
			RunGit(path, "init", "-b", "main");
			RunGit(path, "config", "user.email", "release-bot@example.com");
			RunGit(path, "config", "user.name", "Release Bot");
			if (origin is not null)
				RunGit(path, "remote", "add", "origin", origin);
			return path;
		}

		public static (string Bare, string Worktree) CreateBareAndWorktree(string root, string name)
		{
			var bare = InitBare(Path.Combine(root, $"{name}-origin.git"));
			var worktree = InitWorktree(Path.Combine(root, name), bare);
			return (bare, worktree);
		}

		public static void WriteVariables(string root, string skiaSharpVersion, string previewLabel)
		{
			var scriptsDir = Path.Combine(root, "scripts");
			Directory.CreateDirectory(scriptsDir);
			File.WriteAllText(
				Path.Combine(scriptsDir, "azure-templates-variables.yml"),
				"variables:\n" +
				$"  SKIASHARP_VERSION: {skiaSharpVersion}\n" +
				$"  PREVIEW_LABEL: '{previewLabel}'\n");
		}

		public static void WriteVersions(string root, string skiaSharpVersion, string harfBuzzSharpVersion)
		{
			var scriptsDir = Path.Combine(root, "scripts");
			Directory.CreateDirectory(scriptsDir);
			var majorMinor = string.Join('.', skiaSharpVersion.Split('.')[..2]);
			var text =
				"# nuget versions\n" +
				$"SkiaSharp                                       nuget       {skiaSharpVersion}\n" +
				$"SkiaSharp.HarfBuzz                               nuget       {skiaSharpVersion}\n" +
				"# HarfBuzzSharp\n" +
				$"HarfBuzzSharp                                   nuget       {harfBuzzSharpVersion}\n" +
				$"SkiaSharp               assembly    {majorMinor}.0.0\n" +
				$"SkiaSharp               file        {skiaSharpVersion}\n" +
				"HarfBuzzSharp           assembly    1.0.0.0\n" +
				$"HarfBuzzSharp           file        {harfBuzzSharpVersion}\n";
			File.WriteAllText(Path.Combine(scriptsDir, "VERSIONS.txt"), text);
		}

		public static void Stage(string root, params string[] files) => RunGit(root, ["add", "--", .. files]);

		public static string CommitStaged(string root, string message)
		{
			RunGit(root, "commit", "-m", message);
			return RunGit(root, "rev-parse", "HEAD").Trim();
		}

		public static string CommitAll(string root, string message)
		{
			RunGit(root, "add", "-A");
			RunGit(root, "commit", "-m", message);
			return RunGit(root, "rev-parse", "HEAD").Trim();
		}

		public static void AddGitlink(
			string root, string submodulePath, string sha, string url = "https://example.invalid/skia.git")
		{
			RunGit(root, "update-index", "--add", "--cacheinfo", $"160000,{sha},{submodulePath}");
			File.WriteAllText(
				Path.Combine(root, ".gitmodules"),
				$"[submodule \"{submodulePath}\"]\n\tpath = {submodulePath}\n\turl = {url}\n");
			RunGit(root, "add", ".gitmodules");
		}

		public static void Push(string root, string branch = "main") => RunGit(root, "push", "-u", "origin", branch);

		private static string RunGit(string cwd, params string[] args) =>
			Runner.Run(["git", .. args], cwd).StandardOutput;
	}
}
