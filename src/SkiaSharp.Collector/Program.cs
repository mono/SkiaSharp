using SkiaSharp.Collector.Commands;
using Spectre.Console.Cli;

namespace SkiaSharp.Collector;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var app = new CommandApp();

        app.Configure(config =>
        {
            config.SetApplicationName("skiasharp-collector");
            config.SetApplicationVersion("1.0.0");

            // ===== New Cache-Based Commands =====
            config.AddBranch("sync", sync =>
            {
                sync.SetDescription("Sync data from sources to cache");

                sync.AddCommand<SyncCommand>("all")
                    .WithDescription("Sync all data sources")
                    .WithExample("sync", "all", "-c", "./.data-cache");

                sync.AddCommand<SyncGitHubCommand>("github")
                    .WithDescription("Sync GitHub data to cache")
                    .WithExample("sync", "github", "-c", "./.data-cache");

                sync.AddCommand<SyncNuGetCommand>("nuget")
                    .WithDescription("Sync NuGet data to cache")
                    .WithExample("sync", "nuget", "-c", "./.data-cache");
            });

            config.AddCommand<GenerateCommand>("generate")
                .WithDescription("Generate dashboard JSON from cache")
                .WithExample("generate", "-c", "./.data-cache", "-o", "./data");

            // ===== Legacy Direct-API Commands (still useful for quick testing) =====
            config.AddCommand<AllCommand>("all")
                .WithDescription("Run all collectors directly (legacy)")
                .WithExample("all", "-o", "./data");

            config.AddCommand<GitHubCommand>("github")
                .WithDescription("Collect GitHub stats directly (legacy)")
                .WithExample("github", "-o", "./data");

            config.AddCommand<NuGetCommand>("nuget")
                .WithDescription("Collect NuGet stats directly (legacy)")
                .WithExample("nuget", "-o", "./data", "--min-version", "3");

            config.AddCommand<CommunityCommand>("community")
                .WithDescription("Collect contributor stats directly (legacy)")
                .WithExample("community", "-o", "./data");

            config.AddCommand<IssuesCommand>("issues")
                .WithDescription("Collect all open issues directly (legacy)")
                .WithExample("issues", "-o", "./data");

            config.AddCommand<PrTriageCommand>("pr-triage")
                .WithDescription("Collect and analyze PRs directly (legacy)")
                .WithExample("pr-triage", "-o", "./data");
        });

        // Default to "all" if no command specified
        if (args.Length == 0)
            args = ["all"];

        return await app.RunAsync(args);
    }
}
