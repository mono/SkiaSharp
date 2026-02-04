namespace SkiaSharp.Dashboard.Services;

public record NuGetStats(
    DateTime GeneratedAt,
    long TotalDownloads,
    List<PackageStats> Packages
);

public record PackageStats(
    string Id,
    long TotalDownloads,
    List<VersionStats> Versions,
    bool IsLegacy = false
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
