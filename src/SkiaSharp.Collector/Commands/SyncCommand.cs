using Spectre.Console;
using Spectre.Console.Cli;

namespace SkiaSharp.Collector.Commands;

/// <summary>
/// Main sync command - syncs all data sources.
/// </summary>
public class SyncCommand : AsyncCommand<SyncSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, SyncSettings settings)
    {
        if (!settings.Quiet)
            AnsiConsole.MarkupLine("[bold blue]Syncing all data sources...[/]");

        var result = 0;

        // Sync GitHub
        var githubCmd = new SyncGitHubCommand();
        var githubSettings = new SyncGitHubSettings
        {
            CachePath = settings.CachePath,
            Owner = settings.Owner,
            Repo = settings.Repo,
            Verbose = settings.Verbose,
            Quiet = settings.Quiet,
            ItemsOnly = settings.ItemsOnly,
            EngagementOnly = settings.EngagementOnly
        };
        
        result = await githubCmd.ExecuteAsync(context, githubSettings);
        if (result != 0)
        {
            AnsiConsole.MarkupLine("[yellow]GitHub sync completed with warnings[/]");
            // Don't fail - continue with other sources
        }

        // Sync NuGet (only if not engagement-only, since NuGet has no engagement)
        if (!settings.EngagementOnly)
        {
            var nugetCmd = new SyncNuGetCommand();
            var nugetSettings = new SyncNuGetSettings
            {
                CachePath = settings.CachePath,
                Owner = settings.Owner,
                Repo = settings.Repo,
                Verbose = settings.Verbose,
                Quiet = settings.Quiet
            };

            var nugetResult = await nugetCmd.ExecuteAsync(context, nugetSettings);
            if (nugetResult != 0)
            {
                AnsiConsole.MarkupLine("[yellow]NuGet sync completed with warnings[/]");
            }
        }

        if (!settings.Quiet)
            AnsiConsole.MarkupLine("[green]✓ All sources synced[/]");

        return 0;
    }
}
