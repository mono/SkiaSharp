using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using SkiaSharp.Triage.Cli.Models;

namespace SkiaSharp.Triage.Cli.Services;

/// <summary>
/// Service for loading dashboard configuration.
/// </summary>
public class ConfigService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private DashboardConfig? _config;

    /// <summary>
    /// Load configuration from config.json next to the executable.
    /// </summary>
    public async Task<DashboardConfig> LoadConfigAsync()
    {
        if (_config is not null)
            return _config;

        var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var configPath = Path.Combine(assemblyDir, "config.json");

        // Fall back to project directory during development
        if (!File.Exists(configPath))
        {
            var projectDir = FindProjectDirectory();
            if (projectDir is not null)
                configPath = Path.Combine(projectDir, "config.json");
        }

        if (!File.Exists(configPath))
            throw new FileNotFoundException($"Configuration file not found: {configPath}");

        var json = await File.ReadAllTextAsync(configPath);
        _config = JsonSerializer.Deserialize<DashboardConfig>(json, JsonOptions)
            ?? throw new InvalidOperationException("Failed to parse config.json");

        return _config;
    }

    /// <summary>
    /// Get a specific repo configuration by key or slug.
    /// </summary>
    public async Task<RepoConfig?> GetRepoAsync(string keyOrSlug)
    {
        var config = await LoadConfigAsync();
        return config.Repos.FirstOrDefault(r =>
            r.Key.Equals(keyOrSlug, StringComparison.OrdinalIgnoreCase) ||
            r.Slug.Equals(keyOrSlug, StringComparison.OrdinalIgnoreCase) ||
            r.FullName.Equals(keyOrSlug, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Get a repo by owner/name format.
    /// </summary>
    public async Task<RepoConfig?> GetRepoByFullNameAsync(string owner, string name)
    {
        var config = await LoadConfigAsync();
        return config.Repos.FirstOrDefault(r =>
            r.Owner.Equals(owner, StringComparison.OrdinalIgnoreCase) &&
            r.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    private static string? FindProjectDirectory()
    {
        var dir = Directory.GetCurrentDirectory();
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir, "SkiaSharp.Triage.Cli.csproj")))
                return dir;
            dir = Path.GetDirectoryName(dir);
        }
        return null;
    }
}
