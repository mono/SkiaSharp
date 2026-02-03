namespace SkiaSharp.Dashboard.Services;

public record CommunityStats(
    DateTime GeneratedAt,
    int TotalContributors,
    List<ContributorInfo> TopContributors,
    List<CommitInfo> RecentCommits,
    List<GrowthPoint> ContributorGrowth
);

public record ContributorInfo(
    string Login,
    string AvatarUrl,
    int Contributions
);

public record GrowthPoint(
    DateTime Date,
    int Count
);
