using SkiaSharp.Triage.Models;

namespace SkiaSharp.Triage.Cli.Models;

// GitHub Stats - Multi-repo version
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

// Legacy single-repo types kept for backward compatibility
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


// Repo summary for UI dropdown
public record RepoSummary(
    string FullName,
    string Slug,
    string DisplayName,
    string Color
);
