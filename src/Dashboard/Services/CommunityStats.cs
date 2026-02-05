namespace SkiaSharp.Dashboard.Services;

public record CommunityStats(
    DateTime GeneratedAt,
    int TotalContributors,
    int MicrosoftContributors,
    int CommunityContributors,
    List<ContributorInfo> TopContributors,
    List<CommitInfo> RecentCommits,
    List<GrowthPoint> ContributorGrowth,
    List<RepoSummary>? Repos = null
);

public record ContributorInfo(
    string Login,
    string AvatarUrl,
    int Contributions,
    bool IsMicrosoft,
    Dictionary<string, int>? ContributionsByRepo = null
);

public record GrowthPoint(
    DateTime Date,
    int Count
);
