namespace SkiaSharp.Collector.Models;

// Issues Data
public record IssuesData(
    DateTime GeneratedAt,
    int TotalCount,
    List<LabelCount> ByType,
    List<LabelCount> ByArea,
    List<LabelCount> ByBackend,
    List<LabelCount> ByOs,
    List<AgeCount> ByAge,
    List<IssueInfo> Issues
);

public record AgeCount(string Label, string Display, int Count);

public record IssueInfo(
    int Number,
    string Title,
    string Author,
    string AuthorAvatarUrl,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int CommentCount,
    double DaysOpen,
    double DaysSinceActivity,
    string AgeCategory,
    string? Type,
    List<string> Areas,
    List<string> Backends,
    List<string> Oses,
    List<string> OtherLabels,
    string Url
);
