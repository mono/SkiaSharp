using System.ComponentModel;
using Spectre.Console.Cli;

namespace SkiaSharp.Collector.Commands;

/// <summary>
/// Common settings shared by all commands.
/// </summary>
public class CommonSettings : CommandSettings
{
    [CommandOption("-o|--output <DIR>")]
    [Description("Output directory for JSON files")]
    [DefaultValue("./data")]
    public string OutputDir { get; set; } = "./data";

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
}

/// <summary>
/// Settings for NuGet command with additional options.
/// </summary>
public class NuGetSettings : CommonSettings
{
    [CommandOption("--min-version <VERSION>")]
    [Description("Minimum supported major version (packages without stable >= this are legacy)")]
    [DefaultValue(3)]
    public int MinSupportedVersion { get; set; } = 3;
}
