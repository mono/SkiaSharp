using System.Text.Json;
using System.Text.Json.Serialization;
using SkiaSharp.Triage.Cli.Models;

namespace SkiaSharp.Triage.Cli.Services;

/// <summary>
/// Service for reading and writing cache files.
/// Supports per-repo cache structure: repos/{repoKey}/github/, repos/{repoKey}/nuget/
/// </summary>
public class CacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly string _cachePath;
    private readonly string? _repoKey;

    /// <summary>
    /// Create a cache service for the root cache path (for discovery/migration).
    /// </summary>
    public CacheService(string cachePath) : this(cachePath, null) { }

    /// <summary>
    /// Create a cache service for a specific repo.
    /// </summary>
    public CacheService(string cachePath, string? repoKey)
    {
        _cachePath = Path.GetFullPath(cachePath);
        _repoKey = repoKey;
        ValidateCachePath();
    }

    /// <summary>
    /// Get a cache service scoped to a specific repo.
    /// </summary>
    public CacheService ForRepo(string repoKey) => new(_cachePath, repoKey);

    /// <summary>
    /// Get a cache service scoped to a specific repo config.
    /// </summary>
    public CacheService ForRepo(RepoConfig repo) => new(_cachePath, repo.Key);

    private void ValidateCachePath()
    {
        // Prevent path traversal attacks
        if (_cachePath.Contains(".."))
            throw new ArgumentException("Cache path cannot contain '..'");
    }

    /// <summary>
    /// Get the base path for a repo's data.
    /// New structure: repos/{repoKey}/
    /// Legacy structure: (root) - used when no repoKey specified
    /// </summary>
    private string RepoBasePath => _repoKey is not null
        ? Path.Combine(_cachePath, "repos", _repoKey)
        : _cachePath;

    /// <summary>
    /// Discover all repos in the cache by looking for repos/*/ directories.
    /// </summary>
    public List<string> DiscoverRepoKeys()
    {
        var reposDir = Path.Combine(_cachePath, "repos");
        if (!Directory.Exists(reposDir))
            return [];

        return Directory.GetDirectories(reposDir)
            .Select(Path.GetFileName)
            .Where(name => name is not null)
            .Cast<string>()
            .ToList();
    }

    #region GitHub Cache

    public string GitHubPath => Path.Combine(RepoBasePath, "github");
    public string GitHubSyncMetaPath => Path.Combine(GitHubPath, "sync-meta.json");
    public string GitHubIndexPath => Path.Combine(GitHubPath, "index.json");
    public string GitHubItemsPath => Path.Combine(GitHubPath, "items");
    public string ContributorsPath => Path.Combine(GitHubPath, "contributors.json");
    public string AiTriagePath => Path.Combine(RepoBasePath, "ai-triage");

    public async Task<SyncMeta> LoadGitHubSyncMetaAsync()
    {
        if (!File.Exists(GitHubSyncMetaPath))
            return CreateDefaultSyncMeta();

        var json = await File.ReadAllTextAsync(GitHubSyncMetaPath);
        return JsonSerializer.Deserialize<SyncMeta>(json, JsonOptions) ?? CreateDefaultSyncMeta();
    }

    public async Task SaveGitHubSyncMetaAsync(SyncMeta meta)
    {
        EnsureDirectoryExists(GitHubPath);
        var json = JsonSerializer.Serialize(meta, JsonOptions);
        await WriteAtomicAsync(GitHubSyncMetaPath, json);
    }

    public async Task<CacheIndex> LoadGitHubIndexAsync()
    {
        if (!File.Exists(GitHubIndexPath))
            return new CacheIndex(null, []);

        var json = await File.ReadAllTextAsync(GitHubIndexPath);
        return JsonSerializer.Deserialize<CacheIndex>(json, JsonOptions) ?? new CacheIndex(null, []);
    }

    public async Task SaveGitHubIndexAsync(CacheIndex index)
    {
        EnsureDirectoryExists(GitHubPath);
        var json = JsonSerializer.Serialize(index, JsonOptions);
        await WriteAtomicAsync(GitHubIndexPath, json);
    }

    public async Task<CachedItem?> LoadItemAsync(int number)
    {
        var path = GetItemPath(number);
        if (!File.Exists(path))
            return null;

        var json = await File.ReadAllTextAsync(path);
        return JsonSerializer.Deserialize<CachedItem>(json, JsonOptions);
    }

    public async Task SaveItemAsync(CachedItem item)
    {
        EnsureDirectoryExists(GitHubItemsPath);
        var path = GetItemPath(item.Number);
        var json = JsonSerializer.Serialize(item, JsonOptions);
        await WriteAtomicAsync(path, json);
    }

    /// <summary>
    /// Delete all cached GitHub items (for --reset).
    /// </summary>
    public Task ClearGitHubItemsAsync()
    {
        if (Directory.Exists(GitHubItemsPath))
        {
            Directory.Delete(GitHubItemsPath, recursive: true);
        }
        return Task.CompletedTask;
    }

    public string GetItemPath(int number) => Path.Combine(GitHubItemsPath, $"{number}.json");

    #endregion

    #region Repository Stats

    public string RepoStatsPath => Path.Combine(GitHubPath, "repo.json");

    public async Task<RepoStats?> LoadRepoStatsAsync()
    {
        if (!File.Exists(RepoStatsPath))
            return null;

        var json = await File.ReadAllTextAsync(RepoStatsPath);
        return JsonSerializer.Deserialize<RepoStats>(json, JsonOptions);
    }

    public async Task SaveRepoStatsAsync(RepoStats stats)
    {
        EnsureDirectoryExists(GitHubPath);
        var json = JsonSerializer.Serialize(stats, JsonOptions);
        await WriteAtomicAsync(RepoStatsPath, json);
    }

    #endregion

    #region Contributors (now in github/ folder)

    public string ContributorsSyncMetaPath => Path.Combine(GitHubPath, "contributors-sync-meta.json");

    public async Task<CommunitySyncMeta> LoadCommunitySyncMetaAsync()
    {
        if (!File.Exists(ContributorsSyncMetaPath))
            return new CommunitySyncMeta(1, null, 0, 0);

        var json = await File.ReadAllTextAsync(ContributorsSyncMetaPath);
        return JsonSerializer.Deserialize<CommunitySyncMeta>(json, JsonOptions) ?? new CommunitySyncMeta(1, null, 0, 0);
    }

    public async Task SaveCommunitySyncMetaAsync(CommunitySyncMeta meta)
    {
        EnsureDirectoryExists(GitHubPath);
        var json = JsonSerializer.Serialize(meta, JsonOptions);
        await WriteAtomicAsync(ContributorsSyncMetaPath, json);
    }

    public async Task<List<CachedContributor>> LoadContributorsAsync()
    {
        if (!File.Exists(ContributorsPath))
            return [];

        var json = await File.ReadAllTextAsync(ContributorsPath);
        return JsonSerializer.Deserialize<List<CachedContributor>>(json, JsonOptions) ?? [];
    }

    public async Task SaveContributorsAsync(List<CachedContributor> contributors)
    {
        EnsureDirectoryExists(GitHubPath);
        var json = JsonSerializer.Serialize(contributors, JsonOptions);
        await WriteAtomicAsync(ContributorsPath, json);
    }

    #endregion

    #region NuGet Cache

    public string NuGetPath => Path.Combine(RepoBasePath, "nuget");
    public string NuGetSyncMetaPath => Path.Combine(NuGetPath, "sync-meta.json");
    public string NuGetIndexPath => Path.Combine(NuGetPath, "index.json");
    public string NuGetPackagesPath => Path.Combine(NuGetPath, "packages");

    public async Task<NuGetSyncMeta> LoadNuGetSyncMetaAsync()
    {
        if (!File.Exists(NuGetSyncMetaPath))
            return new NuGetSyncMeta(1, null, 0);

        var json = await File.ReadAllTextAsync(NuGetSyncMetaPath);
        return JsonSerializer.Deserialize<NuGetSyncMeta>(json, JsonOptions) ?? new NuGetSyncMeta(1, null, 0);
    }

    public async Task SaveNuGetSyncMetaAsync(NuGetSyncMeta meta)
    {
        EnsureDirectoryExists(NuGetPath);
        var json = JsonSerializer.Serialize(meta, JsonOptions);
        await WriteAtomicAsync(NuGetSyncMetaPath, json);
    }

    public async Task<NuGetIndex> LoadNuGetIndexAsync()
    {
        if (!File.Exists(NuGetIndexPath))
            return new NuGetIndex(null, []);

        var json = await File.ReadAllTextAsync(NuGetIndexPath);
        return JsonSerializer.Deserialize<NuGetIndex>(json, JsonOptions) ?? new NuGetIndex(null, []);
    }

    public async Task SaveNuGetIndexAsync(NuGetIndex index)
    {
        EnsureDirectoryExists(NuGetPath);
        var json = JsonSerializer.Serialize(index, JsonOptions);
        await WriteAtomicAsync(NuGetIndexPath, json);
    }

    public async Task<CachedPackage?> LoadPackageAsync(string packageId)
    {
        var path = GetPackagePath(packageId);
        if (!File.Exists(path))
            return null;

        var json = await File.ReadAllTextAsync(path);
        return JsonSerializer.Deserialize<CachedPackage>(json, JsonOptions);
    }

    public async Task SavePackageAsync(CachedPackage package)
    {
        EnsureDirectoryExists(NuGetPackagesPath);
        var path = GetPackagePath(package.Id);
        var json = JsonSerializer.Serialize(package, JsonOptions);
        await WriteAtomicAsync(path, json);
    }

    public string GetPackagePath(string packageId) => 
        Path.Combine(NuGetPackagesPath, $"{packageId.ToLowerInvariant()}.json");

    #endregion

    #region Skip List Management

    public List<int> GetItemsToRetry(SyncMeta meta, DateTime now)
    {
        return meta.Failures
            .Where(f => now >= f.Value.RetryAfter)
            .Select(f => int.Parse(f.Key))
            .ToList();
    }

    public List<int> GetItemsToRemove(SyncMeta meta, DateTime now)
    {
        return meta.Failures
            .Where(f => f.Value.RemoveAfter.HasValue && now >= f.Value.RemoveAfter.Value)
            .Select(f => int.Parse(f.Key))
            .ToList();
    }

    public SyncMeta AddFailure(SyncMeta meta, int itemNumber, string layer, string errorType, int? statusCode, string message)
    {
        var key = itemNumber.ToString();
        var now = DateTime.UtcNow;
        
        var existingFailure = meta.Failures.GetValueOrDefault(key);
        var failCount = (existingFailure?.FailCount ?? 0) + 1;
        
        // Calculate retry time based on error type and fail count
        var retryAfter = CalculateRetryTime(errorType, failCount, now);
        var removeAfter = CalculateRemoveTime(errorType, failCount, now);

        var newFailure = new FailureEntry(
            layer,
            now,
            failCount,
            new ErrorInfo(errorType, statusCode, message),
            retryAfter,
            removeAfter
        );

        var newFailures = new Dictionary<string, FailureEntry>(meta.Failures)
        {
            [key] = newFailure
        };

        return meta with { Failures = newFailures };
    }

    public SyncMeta RemoveFailure(SyncMeta meta, int itemNumber)
    {
        var key = itemNumber.ToString();
        var newFailures = new Dictionary<string, FailureEntry>(meta.Failures);
        newFailures.Remove(key);
        return meta with { Failures = newFailures };
    }

    private static DateTime CalculateRetryTime(string errorType, int failCount, DateTime now)
    {
        return errorType switch
        {
            "rate_limit" => now.AddHours(1),
            "server_error" => now.AddHours(Math.Min(failCount, 24)),
            "timeout" => now.AddMinutes(30 * Math.Min(failCount, 4)),
            "not_found" => now.AddHours(24),
            _ => now.AddHours(1)
        };
    }

    private static DateTime? CalculateRemoveTime(string errorType, int failCount, DateTime now)
    {
        // Only set remove time for not_found errors or after many failures
        if (errorType == "not_found")
            return now.AddDays(3);
        if (failCount >= 10)
            return now.AddDays(1);
        return null;
    }

    #endregion

    #region Helpers

    private static SyncMeta CreateDefaultSyncMeta() => new(
        Version: 1,
        LastRun: null,
        LastRunStatus: null,
        Layers: new SyncLayers(
            Items: new LayerStatus(null, null, 0, 0),
            Engagement: new EngagementLayerStatus(null, null, null, 0, 50, 0)
        ),
        RateLimit: null,
        Failures: []
    );

    private static void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }

    private static async Task WriteAtomicAsync(string path, string content)
    {
        var tempPath = path + ".tmp";
        await File.WriteAllTextAsync(tempPath, content);
        File.Move(tempPath, path, overwrite: true);
    }

    #endregion

    #region Legacy Community Cache (for migration)

    public string LegacyCommunityPath => Path.Combine(_cachePath, "community");
    public string LegacyContributorsPath => Path.Combine(LegacyCommunityPath, "contributors.json");

    public bool HasLegacyStructure()
    {
        // Check if old structure exists (github/ at root level, not under repos/)
        return Directory.Exists(Path.Combine(_cachePath, "github")) &&
               !Directory.Exists(Path.Combine(_cachePath, "repos"));
    }

    #endregion
}
