using SkiaSharp.Triage.Models;

namespace SkiaSharp.Triage.Dashboard.Services;

// Multi-repo GitHub stats (new format)
public record MultiRepoGitHubStats(
    DateTime GeneratedAt,
    Dictionary<string, RepoGitHubStats> Repos,
    TotalGitHubStats Total
);

public record RepoGitHubStats(
    string DisplayName,
    string Slug,
    string Color,
    int Stars,
    int Forks,
    int Watchers,
    int OpenIssues,
    int ClosedIssues,
    int OpenPRs,
    int ClosedPRs
);

public record TotalGitHubStats(
    int Stars,
    int Forks,
    int Watchers,
    int OpenIssues,
    int ClosedIssues,
    int OpenPRs,
    int ClosedPRs
);

// Legacy single-repo format (kept for compatibility)
public record GitHubStats(
    DateTime GeneratedAt,
    RepositoryInfo Repository,
    IssueStats Issues,
    PullRequestStats PullRequests,
    ActivityStats Activity
);

public record RepositoryInfo(
    int Stars,
    int Forks,
    int Watchers,
    int OpenIssues,
    int ClosedIssues
);

public record IssueStats(
    int Open,
    int Closed,
    int OpenedLast30Days,
    int ClosedLast30Days,
    List<LabelCount> ByLabel
);

public record PullRequestStats(
    int Open,
    int Merged,
    int Closed,
    int OpenedLast30Days,
    int MergedLast30Days
);

public record ActivityStats(
    int CommitsLast30Days,
    List<CommitInfo> RecentCommits
);

public record CommitInfo(
    string Sha,
    string Message,
    string Author,
    DateTime Date
);
