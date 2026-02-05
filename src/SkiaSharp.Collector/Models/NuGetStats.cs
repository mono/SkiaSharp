namespace SkiaSharp.Collector.Models;

// NuGet Stats - Multi-repo version
public record NuGetStats(
    DateTime GeneratedAt,
    long TotalDownloads,
    List<PackageInfo> Packages,
    List<RepoSummary>? Repos = null,
    Dictionary<string, long>? ByRepo = null
);

public record PackageInfo(
    string Id,
    long TotalDownloads,
    List<VersionInfo> Versions,
    bool IsLegacy,
    string? Repo = null
);

public record VersionInfo(string Version, long Downloads, DateTime? Published = null);

// NuGet Charts Data
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
