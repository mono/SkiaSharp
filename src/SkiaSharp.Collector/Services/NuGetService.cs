using System.Text.RegularExpressions;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using Spectre.Console;

namespace SkiaSharp.Collector.Services;

/// <summary>
/// NuGet API client for fetching package statistics using the official NuGet.Protocol SDK.
/// Combines PackageSearchResource (downloads) and PackageMetadataResource (publish dates)
/// to get complete version history with all data.
/// </summary>
public sealed class NuGetService : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly SourceRepository _repository;
    private readonly SourceCacheContext _cache;
    private readonly int _minSupportedMajorVersion;
    private readonly bool _verbose;

    private const string NuGetV3Url = "https://api.nuget.org/v3/index.json";
    private const string MainVersionsUrl = "https://raw.githubusercontent.com/mono/SkiaSharp/main/scripts/VERSIONS.txt";
    private const string ReleaseVersionsUrl = "https://raw.githubusercontent.com/mono/SkiaSharp/release/2.x/VERSIONS.txt";

    public NuGetService(int minSupportedMajorVersion = 3, bool verbose = false)
    {
        _httpClient = new HttpClient();
        _repository = Repository.Factory.GetCoreV3(NuGetV3Url);
        _cache = new SourceCacheContext();
        _minSupportedMajorVersion = minSupportedMajorVersion;
        _verbose = verbose;
    }

    /// <summary>
    /// Fetch package IDs from VERSIONS.txt files in the SkiaSharp repo.
    /// </summary>
    public async Task<List<string>> GetPackageListAsync()
    {
        var packages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var pattern = new Regex(@"^\s*((?:SkiaSharp|HarfBuzzSharp)[^\s]*)\s+nuget\s+", 
            RegexOptions.Multiline);

        foreach (var url in new[] { MainVersionsUrl, ReleaseVersionsUrl })
        {
            try
            {
                var content = await _httpClient.GetStringAsync(url);
                foreach (Match match in pattern.Matches(content))
                {
                    packages.Add(match.Groups[1].Value);
                }
                if (_verbose)
                    AnsiConsole.MarkupLine($"[dim]Fetched packages from {url}[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[yellow]Warning: Failed to fetch {url}: {ex.Message}[/]");
            }
        }

        return packages.OrderBy(p => p).ToList();
    }

    /// <summary>
    /// Get full version history for a package using the NuGet.Protocol SDK.
    /// Combines PackageSearchResource (downloads) and PackageMetadataResource (publish dates).
    /// </summary>
    public async Task<PackageSearchResult> GetPackageStatsAsync(string packageId)
    {
        try
        {
            var searchResource = await _repository.GetResourceAsync<PackageSearchResource>();
            var metadataResource = await _repository.GetResourceAsync<PackageMetadataResource>();

            // Get downloads from Search API
            var searchFilter = new SearchFilter(includePrerelease: true);
            var searchResults = await searchResource.SearchAsync(
                $"packageid:{packageId}",
                searchFilter,
                skip: 0,
                take: 1,
                NullLogger.Instance,
                CancellationToken.None);

            var searchPkg = searchResults.FirstOrDefault();
            
            long totalDownloads = 0;
            var downloadsByVersion = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
            var isLegacy = true;

            if (searchPkg != null)
            {
                totalDownloads = searchPkg.DownloadCount ?? 0;
                
                // Get all versions with download counts
                var searchVersions = await searchPkg.GetVersionsAsync();
                foreach (var v in searchVersions)
                {
                    var versionStr = v.Version.ToNormalizedString();
                    downloadsByVersion[versionStr] = v.DownloadCount ?? 0;
                    
                    // Check legacy status
                    if (!versionStr.Contains('-'))
                    {
                        var major = v.Version.Major;
                        if (major >= _minSupportedMajorVersion)
                            isLegacy = false;
                    }
                }
            }

            // Get publish dates from Metadata API
            var metadata = await metadataResource.GetMetadataAsync(
                packageId,
                includePrerelease: true,
                includeUnlisted: false,
                _cache,
                NullLogger.Instance,
                CancellationToken.None);

            var versions = new List<VersionStats>();

            foreach (var pkg in metadata)
            {
                var versionStr = pkg.Identity.Version.ToNormalizedString();
                var published = pkg.Published?.UtcDateTime;
                var downloads = downloadsByVersion.GetValueOrDefault(versionStr, 0);

                versions.Add(new VersionStats(versionStr, downloads, published));
            }

            // Sort by publish date (oldest first for chart data)
            versions = [.. versions.OrderBy(v => v.Published ?? DateTime.MinValue)];

            if (_verbose)
                AnsiConsole.MarkupLine($"[dim]Fetched {versions.Count} versions for {packageId}[/]");

            return new PackageSearchResult(packageId, totalDownloads, versions, isLegacy);
        }
        catch (Exception ex)
        {
            if (_verbose)
                AnsiConsole.MarkupLine($"[yellow]Warning: Failed to get stats for {packageId}: {ex.Message}[/]");
            return new PackageSearchResult(packageId, 0, [], true);
        }
    }

    public void Dispose()
    {
        _cache.Dispose();
        _httpClient.Dispose();
    }
}

// Our result models
public record PackageSearchResult(string Id, long TotalDownloads, List<VersionStats> Versions, bool IsLegacy);
public record VersionStats(string Version, long Downloads, DateTime? Published);
