using Octokit;
using SkiaSharp.Collector.Models;
using SkiaSharp.Collector.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SkiaSharp.Collector.Commands;

/// <summary>
/// Syncs community data (contributors) to the cache.
/// </summary>
public class SyncCommunityCommand : AsyncCommand<SyncCommunitySettings>
{
    private const int TopContributorsToCheck = 20;
    private const int MaxContributors = 100;
    
    private string _owner = "";
    private string _repoName = "";

    public override async Task<int> ExecuteAsync(CommandContext context, SyncCommunitySettings settings)
    {
        (_owner, _repoName) = settings.ParseRepository();
        
        if (!settings.Quiet)
            AnsiConsole.MarkupLine($"[bold blue]Syncing community data for {_owner}/{_repoName}...[/]");

        // Load config to get repo key for cache path
        var configService = new ConfigService();
        var repoConfig = await configService.GetRepoByFullNameAsync(_owner, _repoName);
        var repoKey = repoConfig?.Key ?? $"{_owner}-{_repoName}";

        // Use per-repo cache path
        var rootCache = new CacheService(settings.CachePath);
        var cache = rootCache.ForRepo(repoKey);

        using var github = new GitHubService(_owner, _repoName, settings.Verbose);
        var client = CreateOctokitClient();

        var syncMeta = await cache.LoadCommunitySyncMetaAsync();
        var existingContributors = await cache.LoadContributorsAsync();
        var existingByLogin = existingContributors.ToDictionary(c => c.Login, StringComparer.OrdinalIgnoreCase);

        var contributors = new List<CachedContributor>();
        var membershipChecked = 0;

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Fetching contributors...", async ctx =>
            {
                // Fetch contributors from GitHub
                ctx.Status("Fetching contributor list...");
                var repoContributors = await client.Repository.GetAllContributors(
                    _owner, _repoName,
                    new ApiOptions { PageSize = MaxContributors });

                if (!settings.Quiet)
                    AnsiConsole.MarkupLine($"  [dim]Found {repoContributors.Count} contributors[/]");

                // Process each contributor
                for (var i = 0; i < repoContributors.Count; i++)
                {
                    var contrib = repoContributors[i];
                    ctx.Status($"Processing contributor {i + 1}/{repoContributors.Count}...");

                    // Check if we already have MS membership info
                    var existing = existingByLogin.GetValueOrDefault(contrib.Login);
                    bool? isMicrosoft = existing?.IsMicrosoft;
                    DateTime? membershipCheckedAt = existing?.MembershipCheckedAt;

                    // Check MS membership for top contributors if not already known
                    if (i < TopContributorsToCheck && isMicrosoft == null)
                    {
                        try
                        {
                            isMicrosoft = await github.IsMicrosoftMemberAsync(contrib.Login);
                            membershipCheckedAt = DateTime.UtcNow;
                            membershipChecked++;

                            if (settings.Verbose)
                                AnsiConsole.MarkupLine($"  [dim]{contrib.Login}: {(isMicrosoft == true ? "Microsoft" : "Community")}[/]");

                            await Task.Delay(100); // Rate limit courtesy
                        }
                        catch
                        {
                            // If check fails, assume community
                            isMicrosoft = false;
                        }
                    }

                    contributors.Add(new CachedContributor(
                        contrib.Login,
                        contrib.AvatarUrl,
                        contrib.Contributions,
                        isMicrosoft,
                        membershipCheckedAt
                    ));
                }
            });

        // Save contributors
        await cache.SaveContributorsAsync(contributors);

        // Update sync meta
        var newMeta = new CommunitySyncMeta(
            1,
            DateTime.UtcNow,
            contributors.Count,
            membershipChecked
        );
        await cache.SaveCommunitySyncMetaAsync(newMeta);

        if (!settings.Quiet)
        {
            var msCount = contributors.Count(c => c.IsMicrosoft == true);
            var communityCount = contributors.Count(c => c.IsMicrosoft == false);
            var unknownCount = contributors.Count(c => c.IsMicrosoft == null);

            var table = new Table()
                .AddColumn("Metric")
                .AddColumn(new TableColumn("Value").RightAligned());

            table.AddRow("Total Contributors", $"[green]{contributors.Count}[/]");
            table.AddRow("Microsoft", $"[blue]{msCount}[/]");
            table.AddRow("Community", $"[cyan]{communityCount}[/]");
            if (unknownCount > 0)
                table.AddRow("Unknown", $"[dim]{unknownCount}[/]");
            table.AddRow("Membership Checked", membershipChecked.ToString());

            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine($"[green]✓ Community sync complete[/]");
        }

        return 0;
    }

    private static GitHubClient CreateOctokitClient()
    {
        var client = new GitHubClient(new ProductHeaderValue("SkiaSharp-Collector"));
        var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
        if (!string.IsNullOrEmpty(token))
            client.Credentials = new Credentials(token);
        return client;
    }
}

/// <summary>
/// Settings for community sync command.
/// </summary>
public class SyncCommunitySettings : SyncSettings
{
}
