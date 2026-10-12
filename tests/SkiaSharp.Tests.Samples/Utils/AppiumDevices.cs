using System.Text.RegularExpressions;

namespace SkiaSharp.Tests.Samples.Utils;

// Explicit sample-compatibility candidates, not claims about installed runtimes or driver readiness.
internal sealed record AndroidDevice(string Name, string Version, int Api, string Serial, int PageSize)
{
    internal static AndroidDevice PixelApi24 { get; } =
        new("Pixel_API_24", "7.0", 24, "emulator-5558", 4096);
    internal static AndroidDevice PixelApi37 { get; } =
        new("Pixel_API_37", "17", 37, "emulator-5556", 16384);

    internal int Validate()
    {
        if (string.IsNullOrWhiteSpace(Name) || !Regex.IsMatch(Name, @"^[A-Za-z0-9_.-]+$"))
            throw new ArgumentException("Declare the exact AVD name, not a path or wildcard.", nameof(Name));
        if (string.IsNullOrWhiteSpace(Version) || !Regex.IsMatch(Version, @"^[1-9][0-9]*(\.[0-9]+)*$") || Api <= 0)
            throw new ArgumentException("Declare the exact Android release and a positive API level.", nameof(Version));
        if (PageSize is < 4096 or > 65536 || (PageSize & (PageSize - 1)) != 0)
            throw new ArgumentOutOfRangeException(nameof(PageSize), "Declare the expected power-of-two page size in bytes.");
        var selected = Regex.Match(Serial ?? "", @"^emulator-([0-9]+)$");
        var port = 0;
        if (!selected.Success || !int.TryParse(selected.Groups[1].Value, out port) ||
            port is < 5554 or > 5682 || port % 2 != 0 || Serial != $"emulator-{port}")
            throw new ArgumentException("Declare an exact emulator serial with an even console port from 5554 through 5682.", nameof(Serial));
        return port;
    }
}

internal sealed record IosDevice(string Name, string Version)
{
    internal static IosDevice Iphone16ProIos18_6 { get; } = new("iPhone 16 Pro", "18.6");
    internal static IosDevice Iphone16Pro { get; } = new("iPhone 16 Pro", "26.2");

    internal void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name) || Name != Name.Trim() || Name.Any(char.IsControl))
            throw new ArgumentException("Declare the exact available simulator name.", nameof(Name));
        if (string.IsNullOrWhiteSpace(Version) || !Regex.IsMatch(Version, @"^[1-9][0-9]*(\.[0-9]+){1,2}$"))
            throw new ArgumentException("Declare an exact iOS runtime version, including major and minor components.", nameof(Version));
    }
}
