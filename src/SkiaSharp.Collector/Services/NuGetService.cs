using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Spectre.Console;

namespace SkiaSharp.Collector.Services;

/// <summary>
/// NuGet API client for fetching package statistics.
/// Uses Registration API for full version history with publish dates,
/// and Search API for download counts.
/// </summary>
public sealed class NuGetService : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly int _minSupportedMajorVersion;
    private readonly bool _verbose;

    private const string RegistrationApiUrl = "https://api.nuget.org/v3/registration5-gz-semver2";
    private const string SearchApiUrl = "https://azuresearch-usnc.nuget.org/query";
    private const string MainVersionsUrl = "https://raw.githubusercontent.com/mono/SkiaSharp/main/scripts/VERSIONS.txt";
    private const string ReleaseVersionsUrl = "https://raw.githubusercontent.com/mono/SkiaSharp/release/2.x/VERSIONS.txt";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public NuGetService(int minSupportedMajorVersion = 3, bool verbose = false)
    {
        var handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };
        _httpClient = new HttpClient(handler);
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
    /// Get full version history for a package using both Registration and Search APIs.
    /// Registration API: All versions with publish dates
    /// Search API: Download counts
    /// </summary>
    public async Task<PackageSearchResult> GetPackageStatsAsync(string packageId)
    {
        try
        {
            // Get download counts from Search API
            var searchUrl = $"{SearchApiUrl}?q=packageid:{packageId}&prerelease=true&take=1";
            var searchResponse = await _httpClient.GetFromJsonAsync<NuGetSearchResponse>(searchUrl, JsonOptions);
            
            long totalDownloads = 0;
            var downloadsByVersion = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
            var isLegacy = true;

            if (searchResponse?.Data?.Count > 0)
            {
                var pkg = searchResponse.Data[0];
                totalDownloads = pkg.TotalDownloads;
                
                // Build downloads lookup from search results
                if (pkg.Versions != null)
                {
                    foreach (var v in pkg.Versions)
                    {
                        downloadsByVersion[v.Version] = v.Downloads;
                        
                        // Check legacy status
                        if (!v.Version.Contains('-'))
                        {
                            var parts = v.Version.Split('.');
                            if (parts.Length > 0 && int.TryParse(parts[0], out var major))
                            {
                                if (major >= _minSupportedMajorVersion)
                                    isLegacy = false;
                            }
                        }
                    }
                }
            }

            // Get all versions with publish dates from Registration API
            var regUrl = $"{RegistrationApiUrl}/{packageId.ToLowerInvariant()}/index.json";
            var regResponse = await _httpClient.GetFromJsonAsync<RegistrationIndex>(regUrl, JsonOptions);

            var versions = new List<VersionStats>();

            if (regResponse?.Items != null)
            {
                foreach (var page in regResponse.Items)
                {
                    // If page has items inline, use them; otherwise fetch the page
                    var pageItems = page.Items;
                    if (pageItems == null && page.Id != null)
                    {
                        try
                        {
                            var pageResponse = await _httpClient.GetFromJsonAsync<RegistrationPage>(page.Id, JsonOptions);
                            pageItems = pageResponse?.Items;
                        }
                        catch
                        {
                            continue;
                        }
                    }

                    if (pageItems == null) continue;

                    foreach (var item in pageItems)
                    {
                        var entry = item.CatalogEntry;
                        if (entry == null) continue;

                        var version = entry.Version ?? "";
                        var published = entry.Published;
                        
                        // Get downloads from search API lookup, default to 0
                        var downloads = downloadsByVersion.GetValueOrDefault(version, 0);

                        versions.Add(new VersionStats(version, downloads, published));
                    }
                }
            }

            // Sort by publish date (oldest first for chart data)
            versions = [.. versions.OrderBy(v => v.Published ?? DateTime.MinValue)];

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
        _httpClient.Dispose();
    }
}

// JSON models for NuGet Search API response
public record NuGetSearchResponse(List<NuGetPackageResult>? Data);
public record NuGetPackageResult(string Id, long TotalDownloads, List<NuGetVersionResult>? Versions);
public record NuGetVersionResult(string Version, long Downloads);

// JSON models for NuGet Registration API response
public record RegistrationIndex(
    [property: JsonPropertyName("items")] List<RegistrationPage>? Items
);

public record RegistrationPage(
    [property: JsonPropertyName("@id")] string? Id,
    [property: JsonPropertyName("items")] List<RegistrationLeaf>? Items
);

public record RegistrationLeaf(
    [property: JsonPropertyName("catalogEntry")] CatalogEntry? CatalogEntry
);

public record CatalogEntry(
    [property: JsonPropertyName("version")] string? Version,
    [property: JsonPropertyName("downloads")] long? Downloads,
    [property: JsonPropertyName("published")] DateTime? Published,
    [property: JsonPropertyName("description")] string? Description
);

// Our result model
public record PackageSearchResult(string Id, long TotalDownloads, List<VersionStats> Versions, bool IsLegacy);
public record VersionStats(string Version, long Downloads, DateTime? Published);
