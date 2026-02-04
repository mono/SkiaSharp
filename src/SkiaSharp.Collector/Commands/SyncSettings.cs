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

    [CommandOption("--owner <OWNER>")]
    [Description("GitHub repository owner")]
    [DefaultValue("mono")]
    public string Owner { get; set; } = "mono";

    [CommandOption("--repo <REPO>")]
    [Description("GitHub repository name")]
    [DefaultValue("SkiaSharp")]
    public string Repo { get; set; } = "SkiaSharp";

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
}

/// <summary>
/// Settings for GitHub sync command.
/// </summary>
public class SyncGitHubSettings : SyncSettings
{
    [CommandOption("--engagement-count <COUNT>")]
    [Description("Number of items to sync engagement for per batch")]
    [DefaultValue(25)]
    public int EngagementCount { get; set; } = 25;

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
