namespace SkiaSharp.Triage.Dashboard.Services;

public record NuGetStats(
    DateTime GeneratedAt,
    long TotalDownloads,
    List<PackageStats> Packages,
    List<RepoSummary>? Repos = null,
    Dictionary<string, long>? ByRepo = null
);

public record PackageStats(
    string Id,
    long TotalDownloads,
    List<VersionStats> Versions,
    bool IsLegacy = false,
    string? Repo = null
);

public record VersionStats(
    string Version,
    long? Downloads,
    DateTime? Published = null
);

/// <summary>
/// Chart data for NuGet download trends.
/// </summary>
public record NuGetChartsData(
    DateTime GeneratedAt,
    List<PackageChartData> Charts
);

public record PackageChartData(
    string Title,
    List<PackageSeriesData> Series
);

public record PackageSeriesData(
    string PackageId,
    List<ChartDataPoint> DataPoints
);

public record ChartDataPoint(
    DateTime Date,
    long CumulativeDownloads
);
