using System.Text.RegularExpressions;

namespace SkiaSharp.Tests.Samples.Utils;

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
