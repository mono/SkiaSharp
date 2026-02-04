namespace SkiaSharp.Collector.Services;

/// <summary>
/// Shared label parsing logic for issues and PRs.
/// </summary>
public static class LabelParser
{
    public record ParsedLabels(
        string? Type,
        List<string> Areas,
        List<string> Backends,
        List<string> Oses,
        List<string> Other
    );

    public static ParsedLabels Parse(IEnumerable<string> labelNames)
    {
        string? type = null;
        var areas = new List<string>();
        var backends = new List<string>();
        var oses = new List<string>();
        var other = new List<string>();

        foreach (var name in labelNames)
        {
            if (name.StartsWith("type/"))
                type = name[5..];
            else if (name.StartsWith("area/"))
                areas.Add(name[5..]);
            else if (name.StartsWith("backend/"))
                backends.Add(name[8..]);
            else if (name.StartsWith("os/"))
                oses.Add(name[3..]);
            else
                other.Add(name);
        }

        return new ParsedLabels(type, areas, backends, oses, other);
    }

    public static string GetAgeCategory(int daysOpen) => daysOpen switch
    {
        < 7 => "fresh",
        < 30 => "recent",
        < 90 => "aging",
        < 365 => "stale",
        _ => "ancient"
    };

    public static string GetSizeCategory(int totalChanges) => totalChanges switch
    {
        < 10 => "xs",
        < 50 => "s",
        < 200 => "m",
        < 500 => "l",
        _ => "xl"
    };
}
