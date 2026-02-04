using SkiaSharp.Collector.Models;
using SkiaSharp.Collector.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SkiaSharp.Collector.Commands;

public class NuGetCommand : AsyncCommand<NuGetSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, NuGetSettings settings)
    {
        if (!settings.Quiet)
            AnsiConsole.MarkupLine("[bold blue]Collecting NuGet statistics...[/]");

        using var nuget = new NuGetService(settings.MinSupportedVersion, settings.Verbose);

        // Get package list
        var packages = await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Fetching package list from VERSIONS.txt...", async _ =>
            {
                return await nuget.GetPackageListAsync();
            });

        if (!settings.Quiet)
            AnsiConsole.MarkupLine($"[dim]Found {packages.Count} packages to track[/]");

        // Fetch stats for each package
        var packageStats = new List<PackageInfo>();
        long totalDownloads = 0;

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
                var task = ctx.AddTask($"[green]Fetching package stats[/]", maxValue: packages.Count);

                foreach (var packageId in packages)
                {
                    task.Description = $"[dim]{packageId}[/]";
                    
                    var stats = await nuget.GetPackageStatsAsync(packageId);
                    packageStats.Add(new PackageInfo(
                        stats.Id,
                        stats.TotalDownloads,
                        stats.Versions.Select(v => new VersionInfo(v.Version, v.Downloads)).ToList(),
                        stats.IsLegacy
                    ));
                    totalDownloads += stats.TotalDownloads;

                    task.Increment(1);
                    await Task.Delay(300); // Rate limiting
                }

                task.Description = "[green]Fetching package stats[/]";
            });

        var output = new NuGetStats(DateTime.UtcNow, totalDownloads, packageStats);

        // Write output
        var outputPath = OutputService.GetOutputPath(settings.OutputDir, "nuget-stats.json");
        await OutputService.WriteJsonAsync(outputPath, output);

        if (!settings.Quiet)
        {
            var supported = packageStats.Count(p => !p.IsLegacy);
            var legacy = packageStats.Count(p => p.IsLegacy);

            var table = new Table()
                .AddColumn("Metric")
                .AddColumn(new TableColumn("Value").RightAligned());
            
            table.AddRow("Total Downloads", $"[bold green]{totalDownloads:N0}[/]");
            table.AddRow("Packages", packages.Count.ToString());
            table.AddRow("Supported", $"[green]{supported}[/]");
            table.AddRow("Legacy", $"[yellow]{legacy}[/]");
            
            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine($"[green]✓ NuGet stats written to {outputPath}[/]");
        }

        return 0;
    }
}
