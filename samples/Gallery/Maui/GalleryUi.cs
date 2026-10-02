using Microsoft.Maui.Graphics;

namespace SkiaSharpSample;

internal static class GalleryUi
{
    public static readonly Color Accent = Color.FromArgb("#3F70DC");

    public static string StableId(string prefix, string text) =>
        prefix + string.Concat(text.ToLowerInvariant().Select(ch => char.IsAsciiLetterOrDigit(ch) ? ch : '-'));
}
