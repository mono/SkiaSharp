namespace SkiaSharp.Collector.Models;

// Community Stats - Multi-repo version
public record CommunityStats(
    DateTime GeneratedAt,
    int TotalContributors,
    int MicrosoftContributors,
    int CommunityContributors,
    List<ContributorInfo> TopContributors,
    List<CommitInfo> RecentCommits,
    List<MonthlyCount> ContributorGrowth,
    List<RepoSummary>? Repos = null
);

public record ContributorInfo(
    string Login,
    string AvatarUrl,
    int Contributions,
    bool IsMicrosoft,
    Dictionary<string, int>? ContributionsByRepo = null
);

public record MonthlyCount(string Date, int Count);
