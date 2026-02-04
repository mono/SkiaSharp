using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Spectre.Console;

namespace SkiaSharp.Collector.Services;

/// <summary>
/// NuGet API client for fetching package statistics.
/// </summary>
public sealed class NuGetService : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly int _minSupportedMajorVersion;
    private readonly bool _verbose;

    private const string SearchApiUrl = "https://azuresearch-usnc.nuget.org/query";
    private const string MainVersionsUrl = "https://raw.githubusercontent.com/mono/SkiaSharp/main/scripts/VERSIONS.txt";
    private const string ReleaseVersionsUrl = "https://raw.githubusercontent.com/mono/SkiaSharp/release/2.x/VERSIONS.txt";

    public NuGetService(int minSupportedMajorVersion = 3, bool verbose = false)
    {
        _httpClient = new HttpClient();
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
    /// Get download statistics for a package from NuGet Search API.
    /// </summary>
    public async Task<PackageSearchResult> GetPackageStatsAsync(string packageId)
    {
        try
        {
            var url = $"{SearchApiUrl}?q=packageid:{packageId}&prerelease=true&take=1";
            var response = await _httpClient.GetFromJsonAsync<NuGetSearchResponse>(url);

            if (response?.Data == null || response.Data.Count == 0)
            {
                return new PackageSearchResult(packageId, 0, [], true);
            }

            var pkg = response.Data[0];
            var versions = pkg.Versions?
                .TakeLast(5)
                .Select(v => new VersionStats(v.Version, v.Downloads))
                .Reverse()
                .ToList() ?? [];

            // Check if package has a stable version >= minSupportedMajorVersion
            var isLegacy = true;
            if (pkg.Versions != null)
            {
                foreach (var v in pkg.Versions)
                {
                    // Skip prerelease versions
                    if (v.Version.Contains('-')) continue;

                    // Extract major version
                    var parts = v.Version.Split('.');
                    if (parts.Length > 0 && int.TryParse(parts[0], out var major))
                    {
                        if (major >= _minSupportedMajorVersion)
                        {
                            isLegacy = false;
                            break;
                        }
                    }
                }
            }

            return new PackageSearchResult(packageId, pkg.TotalDownloads, versions, isLegacy);
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
public record NuGetSearchResponse(List<NuGetPackageResult> Data);
public record NuGetPackageResult(string Id, long TotalDownloads, List<NuGetVersionResult>? Versions);
public record NuGetVersionResult(string Version, long Downloads);

// Our result model
public record PackageSearchResult(string Id, long TotalDownloads, List<VersionStats> Versions, bool IsLegacy);
public record VersionStats(string Version, long Downloads);
