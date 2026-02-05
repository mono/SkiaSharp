namespace SkiaSharp.Collector.Models;

/// <summary>
/// Configuration for multi-repository sync and generation.
/// </summary>
public record DashboardConfig(
    int Version,
    List<RepoConfig> Repos
);

/// <summary>
/// Configuration for a single repository.
/// </summary>
public record RepoConfig(
    string Owner,
    string Name,
    string Key,
    string Slug,
    string DisplayName,
    string Color,
    bool Primary,
    bool SyncEngagement,
    NuGetConfig Nuget
)
{
    /// <summary>
    /// Full repository path (e.g., "mono/SkiaSharp")
    /// </summary>
    public string FullName => $"{Owner}/{Name}";
}

/// <summary>
/// NuGet package discovery configuration.
/// </summary>
public record NuGetConfig(
    string Source,
    List<string>? Urls = null,
    string? Prefix = null,
    string? Author = null,
    List<string>? SupportedPackages = null
);
