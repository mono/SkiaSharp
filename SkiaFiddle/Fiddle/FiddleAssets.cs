using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SkiaSharp;

namespace SkiaFiddle.Fiddle;

/// <summary>
/// Catalog of images and fonts that are embedded in the app and decoded once at
/// startup. The currently-selected <see cref="Image"/> and <see cref="Typeface"/>
/// are injected into every snippet's Draw block as the <c>image</c> and
/// <c>typeface</c> variables — mirroring the "Optional source image" feature on
/// fiddle.skia.org. Runtime download is impossible (the trimmed WASM build strips
/// HttpClient's download methods), so the assets are bundled instead.
/// </summary>
public static class FiddleAssets
{
    public sealed record ImageAsset(string Name, SKImage Image);

    public sealed record FontAsset(string Name, SKTypeface Typeface);

    private static readonly Assembly Assembly = typeof(FiddleAssets).Assembly;
    private static readonly string[] ResourceNames = Assembly.GetManifestResourceNames();

    private static int _imageIndex;
    private static int _fontIndex;

    static FiddleAssets()
    {
        // The six canonical Skia fiddle source images (1.png..6.png). The friendly
        // names are only used for tooltips/accessibility — the thumbnails are the
        // real label in the picker.
        var imageFiles = new (string File, string Name)[]
        {
            ("1.png", "Test 1"),
            ("2.png", "Test 2"),
            ("3.png", "Mandrill"),
            ("4.png", "Soccer ball"),
            ("5.png", "Color wheel"),
            ("6.png", "Checkerboard"),
        };
        var images = new List<ImageAsset>();
        foreach (var (file, name) in imageFiles)
        {
            var img = LoadImage(file);
            if (img is not null)
                images.Add(new ImageAsset(name, img));
        }
        Images = images;

        var fontFiles = new (string File, string Name)[]
        {
            ("InterVariable.ttf", "Inter (variable)"),
            ("Nabla.ttf", "Nabla (color)"),
            ("DejaVuSerif.ttf", "DejaVu Serif"),
            ("DejaVuSans.ttf", "DejaVu Sans"),
        };
        var fonts = new List<FontAsset>();
        foreach (var (file, name) in fontFiles)
        {
            var tf = LoadTypeface(file);
            if (tf is not null)
                fonts.Add(new FontAsset(name, tf));
        }
        Fonts = fonts;

        // Defaults: Mandrill image + Inter variable font.
        _imageIndex = Clamp(2, Images.Count);
        _fontIndex = 0;
    }

    public static IReadOnlyList<ImageAsset> Images { get; }

    public static IReadOnlyList<FontAsset> Fonts { get; }

    public static int SelectedImageIndex
    {
        get => _imageIndex;
        set => _imageIndex = Clamp(value, Images.Count);
    }

    public static int SelectedFontIndex
    {
        get => _fontIndex;
        set => _fontIndex = Clamp(value, Fonts.Count);
    }

    /// <summary>The currently-selected source image (injected into snippets as <c>image</c>).</summary>
    public static SKImage Image => Images[_imageIndex].Image;

    /// <summary>The currently-selected typeface (injected into snippets as <c>typeface</c>).</summary>
    public static SKTypeface Typeface => Fonts[_fontIndex].Typeface;

    public static int IndexOfFont(string name)
    {
        for (var i = 0; i < Fonts.Count; i++)
            if (string.Equals(Fonts[i].Name, name, StringComparison.OrdinalIgnoreCase))
                return i;
        return -1;
    }

    private static int Clamp(int value, int count)
    {
        if (count <= 0) return 0;
        if (value < 0) return 0;
        if (value >= count) return count - 1;
        return value;
    }

    private static SKImage? LoadImage(string file)
    {
        // FromEncodedData copies into an owned SKData, so the stream can close here.
        using var stream = OpenResource(file);
        return stream is null ? null : SKImage.FromEncodedData(stream);
    }

    private static SKTypeface? LoadTypeface(string file)
    {
        using var stream = OpenResource(file);
        if (stream is null)
            return null;

        // SKTypeface.FromStream reads glyph data lazily and would fault once the
        // resource stream is disposed, so back the typeface with an owned SKData copy.
        using var data = SKData.Create(stream);
        return SKTypeface.FromData(data);
    }

    private static Stream? OpenResource(string file)
    {
        var suffix = ".Media." + file;
        var name = ResourceNames.FirstOrDefault(n => n.EndsWith(suffix, StringComparison.Ordinal));
        return name is null ? null : Assembly.GetManifestResourceStream(name);
    }
}
