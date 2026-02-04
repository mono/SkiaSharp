namespace SkiaSharp.Collector.Models;

// Community Stats
public record CommunityStats(
    DateTime GeneratedAt,
    int TotalContributors,
    int MicrosoftContributors,
    int CommunityContributors,
    List<ContributorInfo> TopContributors,
    List<CommitInfo> RecentCommits,
    List<MonthlyCount> ContributorGrowth
);

public record ContributorInfo(
    string Login,
    string AvatarUrl,
    int Contributions,
    bool IsMicrosoft
);

public record MonthlyCount(string Date, int Count);
