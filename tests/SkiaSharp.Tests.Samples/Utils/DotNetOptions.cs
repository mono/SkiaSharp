using System.Collections.ObjectModel;

namespace SkiaSharp.Tests.Samples.Utils;

/// <summary>Optional settings for sample builds, runs, and raw dotnet commands.</summary>
public sealed record DotNetOptions
{
    /// <summary>Gets the build or sample-run configuration, defaulting to Release.</summary>
    public string Configuration { get; init; } = "Release";

    /// <summary>Gets the optional framework selected with --framework, without replacing declared framework lists.</summary>
    public string? Framework { get; init; }

    /// <summary>Gets additional MSBuild properties; configuration, framework, and helper-owned properties cannot be overridden.</summary>
    public IReadOnlyDictionary<string, string> Properties { get; init; } =
        new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());

    /// <summary>
    /// Gets a positive finite timeout supported by .NET timers. Null uses 30 minutes for builds
    /// and one minute for raw commands. Sample runs apply it only to address and exit waits
    /// (normally two and three minutes); callbacks own their browser-operation waits.
    /// Cleanup and output reads retain their one-minute bounds.
    /// </summary>
    public TimeSpan? Timeout { get; init; }

    /// <summary>Gets the optional working directory, normalized to a full path; defaults to the project directory or workspace for raw commands.</summary>
    public string? WorkingDirectory { get; init; }

    internal void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Configuration);
        if (Framework is not null)
            ArgumentException.ThrowIfNullOrWhiteSpace(Framework);
        ArgumentNullException.ThrowIfNull(Properties);
        if (Timeout is { } timeout && (timeout <= TimeSpan.Zero || timeout.TotalMilliseconds > uint.MaxValue - 1))
            throw new ArgumentOutOfRangeException(nameof(Timeout), "Timeout must be positive and supported by .NET timers.");
        if (WorkingDirectory is not null)
            ArgumentException.ThrowIfNullOrWhiteSpace(WorkingDirectory);
        foreach (var name in Properties.Keys)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            if (new[] { "Configuration", "TargetFramework", "TargetFrameworks", "RestoreConfigFile",
                "RestoreNoCache", "UseSharedCompilation" }.Contains(name, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException($"Property {name} is already owned by the sample helper.", nameof(Properties));
        }
    }

    internal string GetWorkingDirectory(string defaultDirectory) =>
        Path.GetFullPath(WorkingDirectory ?? defaultDirectory);
}
