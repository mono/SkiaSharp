namespace SkiaSharp.Tests.Samples.Utils;

public sealed record DotNetBuildOptions
{
    public string Configuration { get; init; } = "Release";
    public string? Framework { get; init; }
    public IReadOnlyDictionary<string, string> Properties { get; init; } = new Dictionary<string, string>();
}

internal sealed record DotNetRunOptions
{
    internal string Configuration { get; init; } = "Release";
    internal string? Framework { get; init; }
    internal IReadOnlyList<string> Arguments { get; init; } = [];
}

internal sealed record DotNetCommandOptions
{
    internal string? WorkingDirectory { get; init; }
    internal TimeSpan Timeout { get; init; } = TimeSpan.FromMinutes(1);
}
