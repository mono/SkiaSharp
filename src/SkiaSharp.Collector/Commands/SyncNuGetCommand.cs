using SkiaSharp.Collector.Models;
using SkiaSharp.Collector.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using PackageVersion = SkiaSharp.Collector.Models.PackageVersion;

namespace SkiaSharp.Collector.Commands;

/// <summary>
/// Syncs NuGet data to the cache.
/// </summary>
public class SyncNuGetCommand : AsyncCommand<SyncNuGetSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, SyncNuGetSettings settings)
    {
        if (!settings.Quiet)
            AnsiConsole.MarkupLine("[bold blue]Syncing NuGet data...[/]");

        var cache = new CacheService(settings.CachePath);
        using var nuget = new NuGetService(settings.MinSupportedVersion, settings.Verbose);

        var syncMeta = await cache.LoadNuGetSyncMetaAsync();
        var index = await cache.LoadNuGetIndexAsync();
        var now = DateTime.UtcNow;

        // Get package list from VERSIONS.txt
        var packages = await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Fetching package list...", async _ => await nuget.GetPackageListAsync());

        if (!settings.Quiet)
            AnsiConsole.MarkupLine($"[dim]Found {packages.Count} packages to sync[/]");

        var indexDict = index.Packages.ToDictionary(p => p.Id, StringComparer.OrdinalIgnoreCase);
        var processed = 0;

        await AnsiConsole.Progress()
            .AutoClear(false)
            .HideCompleted(false)
            .Columns(
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new SpinnerColumn())
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask("[green]Syncing packages[/]", maxValue: packages.Count);

                foreach (var packageId in packages)
                {
                    task.Description = $"[dim]{packageId}[/]";

                    try
                    {
                        var stats = await nuget.GetPackageStatsAsync(packageId);
                        
                        var cachedPackage = new CachedPackage(
                            stats.Id,
                            null, // Description not returned by search API
                            stats.Versions.FirstOrDefault()?.Version,
                            stats.TotalDownloads,
                            stats.IsLegacy,
                            [.. stats.Versions.Select(v => new PackageVersion(v.Version, v.Downloads, null))],
                            now
                        );

                        await cache.SavePackageAsync(cachedPackage);

                        indexDict[packageId] = new NuGetIndexPackage(
                            stats.Id,
                            stats.Versions.FirstOrDefault()?.Version,
                            stats.TotalDownloads,
                            stats.IsLegacy,
                            now
                        );

                        processed++;
                    }
                    catch (Exception ex)
                    {
                        if (settings.Verbose)
                            AnsiConsole.MarkupLine($"[yellow]Failed to sync {packageId}: {ex.Message}[/]");
                    }

                    task.Increment(1);
                    await Task.Delay(300); // Rate limiting
                }

                task.Description = "[green]Syncing packages[/]";
            });

        // Save index and metadata
        var newIndex = new NuGetIndex(now, [.. indexDict.Values.OrderBy(p => p.Id)]);
        await cache.SaveNuGetIndexAsync(newIndex);

        var newMeta = new NuGetSyncMeta(1, now, processed);
        await cache.SaveNuGetSyncMetaAsync(newMeta);

        if (!settings.Quiet)
        {
            var totalDownloads = indexDict.Values.Sum(p => p.TotalDownloads);
            var supported = indexDict.Values.Count(p => !p.IsLegacy);
            var legacy = indexDict.Values.Count(p => p.IsLegacy);

            var table = new Table()
                .AddColumn("Metric")
                .AddColumn(new TableColumn("Value").RightAligned());
            
            table.AddRow("Packages Synced", $"[green]{processed}[/]");
            table.AddRow("Total Downloads", $"[bold green]{totalDownloads:N0}[/]");
            table.AddRow("Supported", $"[green]{supported}[/]");
            table.AddRow("Legacy", $"[yellow]{legacy}[/]");
            
            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine($"[green]✓ NuGet sync complete[/]");
        }

        return 0;
    }
}
