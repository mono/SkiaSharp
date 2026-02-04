namespace SkiaSharp.Collector.Models;

// GitHub Stats
public record GitHubStats(
    DateTime GeneratedAt,
    RepositoryInfo Repository,
    IssuesInfo Issues,
    PullRequestsInfo PullRequests,
    ActivityInfo Activity
);

public record RepositoryInfo(
    int Stars,
    int Forks,
    int Watchers,
    int OpenIssues,
    int ClosedIssues
);

public record IssuesInfo(
    int Open,
    int Closed,
    int OpenedLast30Days,
    int ClosedLast30Days,
    List<LabelCount> ByLabel
);

public record PullRequestsInfo(
    int Open,
    int Merged,
    int Closed,
    int OpenedLast30Days,
    int MergedLast30Days
);

public record ActivityInfo(
    int CommitsLast30Days,
    List<CommitInfo> RecentCommits
);

public record CommitInfo(
    string Sha,
    string Message,
    string Author,
    DateTime Date
);

public record LabelCount(string Label, int Count);
