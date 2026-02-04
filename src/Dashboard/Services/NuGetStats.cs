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
    long? Downloads
);
