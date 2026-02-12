using SkiaSharp.Triage.Cli.Commands;
using Spectre.Console.Cli;

namespace SkiaSharp.Triage.Cli;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var app = new CommandApp();

        app.Configure(config =>
        {
            config.SetApplicationName("skiasharp-triage");
            config.SetApplicationVersion("1.0.0");

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

                sync.AddCommand<SyncCommunityCommand>("community")
                    .WithDescription("Sync community data (contributors) to cache")
                    .WithExample("sync", "community", "-c", "./.data-cache");
            });

            config.AddCommand<GenerateCommand>("generate")
                .WithDescription("Generate dashboard JSON from cache")
                .WithExample("generate", "-c", "./.data-cache", "-o", "./data");
        });

        return await app.RunAsync(args);
    }
}
