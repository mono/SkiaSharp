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

            config.AddCommand<AllCommand>("all")
                .WithDescription("Run all collectors (default)")
                .WithExample("all", "-o", "./data");

            config.AddCommand<GitHubCommand>("github")
                .WithDescription("Collect GitHub repository statistics")
                .WithExample("github", "-o", "./data");

            config.AddCommand<NuGetCommand>("nuget")
                .WithDescription("Collect NuGet package download statistics")
                .WithExample("nuget", "-o", "./data", "--min-version", "3");

            config.AddCommand<CommunityCommand>("community")
                .WithDescription("Collect contributor and community statistics")
                .WithExample("community", "-o", "./data");

            config.AddCommand<IssuesCommand>("issues")
                .WithDescription("Collect all open issues with labels")
                .WithExample("issues", "-o", "./data");

            config.AddCommand<PrTriageCommand>("pr-triage")
                .WithDescription("Collect and analyze open PRs for triage")
                .WithExample("pr-triage", "-o", "./data");
        });

        // Default to "all" if no command specified
        if (args.Length == 0)
            args = ["all"];

        return await app.RunAsync(args);
    }
}
