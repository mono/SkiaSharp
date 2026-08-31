using System.CommandLine;
using ReleaseChecklist.Core;
using ReleaseChecklist.Git;
using ReleaseChecklist.GitHub;
using SkiaSharp.ReleaseTool.Errors;

namespace SkiaSharp.ReleaseChecklist;

/// <summary>Hosts the SkiaSharp release checklist command-line application.</summary>
public static class Program
{
	/// <summary>Runs the command-line application.</summary>
	/// <param name="args">The command-line arguments.</param>
	/// <returns>The process exit code.</returns>
	public static int Main(string[] args) =>
		InvokeAsync(args, Console.Out, Console.Error).GetAwaiter().GetResult();

	/// <summary>Asynchronously runs the command-line application with explicit output streams.</summary>
	/// <param name="args">The command-line arguments.</param>
	/// <param name="output">The standard output writer.</param>
	/// <param name="error">The standard error writer.</param>
	/// <returns>The process exit code.</returns>
	public static async Task<int> InvokeAsync(
		string[] args,
		TextWriter output,
		TextWriter error)
	{
		var applyOption = new Option<bool>("--apply")
		{
			Description = "Apply pending actions. Without this option the command is a dry run.",
		};
		var branchOption = new Option<string?>("--branch")
		{
			Description = "main, release/X.Y.x, or release/{identity}; defaults to the current branch.",
		};
		var releaseOption = new Option<string?>("--release")
		{
			Description = "Exact release identity X.Y.Z[.F][-preview.N|-rc.N].",
		};
		var maintenanceBaseOption = new Option<string?>("--maintenance-base")
		{
			Description = "Reviewed ref or SHA at the target version and preview.0 when maintenance is missing.",
		};
		var repoOption = new Option<string>("--repo")
		{
			Description = "Path inside the SkiaSharp worktree.",
			DefaultValueFactory = _ => Directory.GetCurrentDirectory(),
		};
		var prepare = new Command("prepare", "Prepare repository branches for a SkiaSharp release.");
		prepare.Options.Add(applyOption);
		prepare.Options.Add(branchOption);
		prepare.Options.Add(releaseOption);
		prepare.Options.Add(maintenanceBaseOption);
		prepare.Options.Add(repoOption);
		prepare.SetAction(async (parse, cancellationToken) =>
		{
			try
			{
				var apply = parse.GetValue(applyOption);
				var release = parse.GetValue(releaseOption);
				if (apply && release is null)
				{
					throw new ReleasePolicyException(
						"Apply mode requires --release so repository changes cannot select a newly inferred identity.");
				}
				var local = await GitRepository.DiscoverAsync(
					parse.GetRequiredValue(repoOption),
					repositoryIdentity: "mono/SkiaSharp",
					cancellationToken: cancellationToken).ConfigureAwait(false);
				await RequireSkiaSharpRepositoryAsync(local, cancellationToken).ConfigureAwait(false);
				await local.FetchAsync(cancellationToken).ConfigureAwait(false);
				var found = await ReleaseDiscovery.DiscoverAsync(
					new GitReleaseDiscoveryRepository(local),
					new ReleaseDiscoveryOptions
					{
						Branch = parse.GetValue(branchOption),
						Release = release,
						MaintenanceBase = parse.GetValue(maintenanceBaseOption),
					},
					cancellationToken).ConfigureAwait(false);
				var token = apply
					? Environment.GetEnvironmentVariable("GH_TOKEN") ??
						Environment.GetEnvironmentVariable("GITHUB_TOKEN")
					: null;
				var github = new OctokitGitHubRepositoryClient("skiasharp-release-checklist", token);
				var definition = PrepareDefinition.Build(found, local, github);
				var report = await ChecklistRunner.RunAsync(
					definition,
					new ChecklistRunOptions
					{
						Mode = apply ? ChecklistRunMode.Apply : ChecklistRunMode.DryRun,
						CompletionTimeout = TimeSpan.FromMinutes(30),
					},
					cancellationToken).ConfigureAwait(false);
				await output.WriteLineAsync(
					$"{(apply ? "Apply" : "Dry run")} Prepare {found.Identity.Raw} " +
					$"from {found.SourceBranch} ({found.SourceSha})")
					.ConfigureAwait(false);
				await output.WriteAsync(ConsoleTreeRenderer.Render(report)).ConfigureAwait(false);
				if (report.Root.CancellationObserved)
					return 130;
				if (report.Root.HasErrors || report.Root.Status == ChecklistStatus.Blocked)
					return 2;
				return 0;
			}
			catch (OperationCanceledException)
			{
				await error.WriteLineAsync("error: operation canceled").ConfigureAwait(false);
				return 130;
			}
			catch (Exception ex) when (
				ex is ReleasePolicyException or GitException or ProcessException or ChecklistDefinitionException)
			{
				await error.WriteLineAsync($"error: {ex.Message}").ConfigureAwait(false);
				return 1;
			}
		});
		var finishVersionOption = new Option<string>("--version")
		{
			Description = "Exact public NuGet.org SkiaSharp version.",
			Required = true,
		};
		var finishApplyOption = new Option<bool>("--apply")
		{
			Description = "Apply pending actions. Without this option the command is a dry run.",
		};
		var finishRepoOption = new Option<string>("--repo")
		{
			Description = "Path inside the SkiaSharp worktree.",
			DefaultValueFactory = _ => Directory.GetCurrentDirectory(),
		};
		var finish = new Command("finish", "Finish a public SkiaSharp package release.");
		finish.Options.Add(finishVersionOption);
		finish.Options.Add(finishApplyOption);
		finish.Options.Add(finishRepoOption);
		finish.SetAction(async (parse, cancellationToken) =>
		{
			try
			{
				var apply = parse.GetValue(finishApplyOption);
				var version = parse.GetRequiredValue(finishVersionOption);
				var repositoryPath = parse.GetRequiredValue(finishRepoOption);
				var repository = await GitRepository.DiscoverAsync(
					repositoryPath,
					repositoryIdentity: "mono/SkiaSharp",
					cancellationToken: cancellationToken).ConfigureAwait(false);
				await RequireSkiaSharpRepositoryAsync(repository, cancellationToken).ConfigureAwait(false);
				using var runtime = await FinishRuntime.CreateAsync(
					repositoryPath,
					version,
					cancellationToken).ConfigureAwait(false);
				var report = await ChecklistRunner.RunAsync(
					FinishDefinition.Build(runtime),
					new ChecklistRunOptions
					{
						Mode = apply ? ChecklistRunMode.Apply : ChecklistRunMode.DryRun,
						CompletionTimeout = TimeSpan.FromMinutes(30),
					},
					cancellationToken).ConfigureAwait(false);
				await output.WriteLineAsync($"{(apply ? "Apply" : "Dry run")} Finish {version}")
					.ConfigureAwait(false);
				await output.WriteAsync(ConsoleTreeRenderer.Render(report)).ConfigureAwait(false);
				if (report.Root.CancellationObserved)
					return 130;
				if (report.Root.HasErrors || report.Root.Status == ChecklistStatus.Blocked)
					return 2;
				return 0;
			}
			catch (OperationCanceledException)
			{
				await error.WriteLineAsync("error: operation canceled").ConfigureAwait(false);
				return 130;
			}
			catch (ReleaseToolException ex)
			{
				await error.WriteLineAsync($"error: {ex.Message}").ConfigureAwait(false);
				return 1;
			}
			catch (Exception ex) when (
				ex is ReleasePolicyException or GitException or ProcessException)
			{
				await error.WriteLineAsync($"error: {ex.Message}").ConfigureAwait(false);
				return 1;
			}
		});
		var root = new RootCommand("Run the SkiaSharp repository release checklist.");
		root.Subcommands.Add(prepare);
		root.Subcommands.Add(finish);
		return await root.Parse(args).InvokeAsync().ConfigureAwait(false);
	}

	private static async Task RequireSkiaSharpRepositoryAsync(
		GitRepository repository,
		CancellationToken cancellationToken)
	{
		var url = await repository.RemoteUrlAsync(cancellationToken).ConfigureAwait(false);
		var path = url.StartsWith("git@github.com:", StringComparison.OrdinalIgnoreCase)
			? url["git@github.com:".Length..]
			: Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
				string.Equals(uri.Host, "github.com", StringComparison.OrdinalIgnoreCase)
					? uri.AbsolutePath.TrimStart('/')
					: "";
		path = path.EndsWith(".git", StringComparison.OrdinalIgnoreCase)
			? path[..^4]
			: path;
		if (!string.Equals(path.TrimEnd('/'), "mono/SkiaSharp", StringComparison.OrdinalIgnoreCase))
		{
			throw new ReleasePolicyException(
				$"Remote '{repository.Remote}' is '{url}', expected mono/SkiaSharp on github.com.");
		}
	}
}
