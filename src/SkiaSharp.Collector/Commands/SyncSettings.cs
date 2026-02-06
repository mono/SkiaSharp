using System.ComponentModel;
using Spectre.Console.Cli;

namespace SkiaSharp.Collector.Commands;

/// <summary>
/// Settings for sync commands.
/// </summary>
public class SyncSettings : CommandSettings
{
    [CommandOption("-c|--cache-path <PATH>")]
    [Description("Path to the data cache directory")]
    [DefaultValue("./.data-cache")]
    public string CachePath { get; set; } = "./.data-cache";

    [CommandOption("-r|--repo <REPO>")]
    [Description("Repository in owner/name format (e.g., mono/SkiaSharp)")]
    public string? Repository { get; set; }

    [CommandOption("-v|--verbose")]
    [Description("Show detailed progress")]
    public bool Verbose { get; set; }

    [CommandOption("-q|--quiet")]
    [Description("Suppress output except errors")]
    public bool Quiet { get; set; }

    [CommandOption("--items-only")]
    [Description("Only sync basic item data (Layer 1)")]
    public bool ItemsOnly { get; set; }

    [CommandOption("--engagement-only")]
    [Description("Only sync engagement data (Layer 2)")]
    public bool EngagementOnly { get; set; }

    /// <summary>
    /// Parse repository into owner and name components.
    /// Falls back to defaults if not specified.
    /// </summary>
    public (string Owner, string Name) ParseRepository()
    {
        if (string.IsNullOrEmpty(Repository))
            return ("mono", "SkiaSharp"); // Default

        var parts = Repository.Split('/');
        if (parts.Length != 2)
            throw new ArgumentException($"Invalid repository format: {Repository}. Expected owner/name.");

        return (parts[0], parts[1]);
    }
}

/// <summary>
/// Settings for GitHub sync command.
/// </summary>
public class SyncGitHubSettings : SyncSettings
{
    [CommandOption("--engagement-count <COUNT>")]
    [Description("Number of items to sync engagement for per batch")]
    [DefaultValue(100)]
    public int EngagementCount { get; set; } = 100;

    [CommandOption("--page-count <COUNT>")]
    [Description("Number of pages to sync for items per batch (100 items per page)")]
    [DefaultValue(0)]
    public int PageCount { get; set; } = 0; // 0 = unlimited

    [CommandOption("--full")]
    [Description("Force full refresh, ignore timestamps")]
    public bool FullRefresh { get; set; }
}

/// <summary>
/// Settings for NuGet sync command.
/// </summary>
public class SyncNuGetSettings : SyncSettings
{
    [CommandOption("--min-version <VERSION>")]
    [Description("Minimum supported major version")]
    [DefaultValue(3)]
    public int MinSupportedVersion { get; set; } = 3;
}

/// <summary>
/// Settings for generate commands.
/// </summary>
public class GenerateSettings : CommandSettings
{
    [CommandOption("-c|--from-cache <PATH>")]
    [Description("Path to the data cache directory")]
    [DefaultValue("./.data-cache")]
    public string CachePath { get; set; } = "./.data-cache";

    [CommandOption("-o|--output <DIR>")]
    [Description("Output directory for JSON files")]
    [DefaultValue("./data")]
    public string OutputDir { get; set; } = "./data";

    [CommandOption("-v|--verbose")]
    [Description("Show detailed progress")]
    public bool Verbose { get; set; }

    [CommandOption("-q|--quiet")]
    [Description("Suppress output except errors")]
    public bool Quiet { get; set; }
}
