namespace SkiaSharp.Collector.Models;

// NuGet Stats
public record NuGetStats(
    DateTime GeneratedAt,
    long TotalDownloads,
    List<PackageInfo> Packages
);

public record PackageInfo(
    string Id,
    long TotalDownloads,
    List<VersionInfo> Versions,
    bool IsLegacy
);

public record VersionInfo(string Version, long Downloads);
