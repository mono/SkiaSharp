using System.Text.RegularExpressions;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using SkiaSharp.Triage.Cli.Models;
using Spectre.Console;

namespace SkiaSharp.Triage.Cli.Services;

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

    public NuGetService(int minSupportedMajorVersion = 3, bool verbose = false)
    {
        _httpClient = new HttpClient();
        _repository = Repository.Factory.GetCoreV3(NuGetV3Url);
        _cache = new SourceCacheContext();
        _minSupportedMajorVersion = minSupportedMajorVersion;
        _verbose = verbose;
    }

    /// <summary>
    /// Discover package IDs using the configured discovery method.
    /// </summary>
    public async Task<List<string>> DiscoverPackagesAsync(NuGetConfig nugetConfig)
    {
        return nugetConfig.Source switch
        {
            "versions-txt" => await GetPackagesFromVersionsTxtAsync(nugetConfig.Urls ?? []),
            "nuget-search" => await SearchPackagesAsync(nugetConfig.Prefix ?? "", nugetConfig.Author),
            _ => throw new ArgumentException($"Unknown NuGet source type: {nugetConfig.Source}")
        };
    }

    /// <summary>
    /// Fetch package IDs from VERSIONS.txt files.
    /// </summary>
    public async Task<List<string>> GetPackagesFromVersionsTxtAsync(IEnumerable<string> urls)
    {
        var packages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var pattern = new Regex(@"^\s*((?:SkiaSharp|HarfBuzzSharp)[^\s]*)\s+nuget\s+", 
            RegexOptions.Multiline);

        foreach (var url in urls)
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
                if (_verbose)
                    AnsiConsole.MarkupLine($"[yellow]Warning: Failed to fetch {url}: {ex.Message}[/]");
            }
        }

        return [.. packages.OrderBy(p => p)];
    }

    /// <summary>
    /// Search NuGet for packages matching a prefix, optionally filtering by author.
    /// </summary>
    public async Task<List<string>> SearchPackagesAsync(string prefix, string? authorFilter)
    {
        var packages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        try
        {
            var searchResource = await _repository.GetResourceAsync<PackageSearchResource>();
            var searchFilter = new SearchFilter(includePrerelease: false);

            // Search for packages starting with prefix
            var results = await searchResource.SearchAsync(
                prefix,
                searchFilter,
                skip: 0,
                take: 100, // Should be enough for Extended packages
                NullLogger.Instance,
                CancellationToken.None);

            foreach (var result in results)
            {
                // Check if package ID matches the prefix
                if (!result.Identity.Id.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    continue;

                // Filter by author if specified (Microsoft and Xamarin are equivalent)
                if (!string.IsNullOrEmpty(authorFilter) && result.Authors != null)
                {
                    var authors = result.Authors.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(a => a.Trim())
                        .ToList();

                    // Accept both Microsoft and Xamarin as valid authors (Xamarin Inc. is part of Microsoft)
                    var validAuthors = new[] { "Microsoft", "Xamarin" };
                    if (!authors.Any(a => validAuthors.Any(v => a.Contains(v, StringComparison.OrdinalIgnoreCase))))
                        continue;
                }

                packages.Add(result.Identity.Id);
                if (_verbose)
                    AnsiConsole.MarkupLine($"[dim]Found package: {result.Identity.Id}[/]");
            }

            if (_verbose)
                AnsiConsole.MarkupLine($"[dim]Found {packages.Count} packages matching '{prefix}' by '{authorFilter}'[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[yellow]Warning: NuGet search failed: {ex.Message}[/]");
        }

        return [.. packages.OrderBy(p => p)];
    }

    /// <summary>
    /// Get full version history for a package using the NuGet.Protocol SDK.
    /// Combines PackageSearchResource (downloads) and PackageMetadataResource (publish dates).
    /// </summary>
    /// <param name="packageId">The package ID to fetch</param>
    /// <param name="supportedPackages">Optional whitelist of supported package IDs. If provided, only these are non-legacy.</param>
    public async Task<PackageSearchResult> GetPackageStatsAsync(string packageId, IReadOnlyCollection<string>? supportedPackages = null)
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
            
            // Determine legacy status
            bool isLegacy;
            if (supportedPackages is { Count: > 0 })
            {
                // Use whitelist: only packages in the list are supported
                isLegacy = !supportedPackages.Contains(packageId, StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                // Use version-based detection (default)
                isLegacy = true;
            }

            if (searchPkg != null)
            {
                totalDownloads = searchPkg.DownloadCount ?? 0;
                
                // Get all versions with download counts
                var searchVersions = await searchPkg.GetVersionsAsync();
                foreach (var v in searchVersions)
                {
                    var versionStr = v.Version.ToNormalizedString();
                    downloadsByVersion[versionStr] = v.DownloadCount ?? 0;
                    
                    // Check legacy status via version (only if not using whitelist)
                    if (supportedPackages is null or { Count: 0 } && !versionStr.Contains('-'))
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
