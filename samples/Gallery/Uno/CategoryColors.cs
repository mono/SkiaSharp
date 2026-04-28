using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace SkiaSharpSample;

// Mirrors the lookup tables in the Blazor host's Home.razor for visual parity.
internal static class CategoryColors
{
    public static Color Of(string category) => category switch
    {
        SampleCategories.Documents => Color.FromArgb(0xFF, 0x60, 0x7D, 0x8B),
        SampleCategories.General => Color.FromArgb(0xFF, 0x1A, 0x23, 0x7E),
        SampleCategories.ImageFilters => Color.FromArgb(0xFF, 0xE6, 0x51, 0x00),
        SampleCategories.BitmapDecoding => Color.FromArgb(0xFF, 0x00, 0x89, 0x7B),
        SampleCategories.PathEffects => Color.FromArgb(0xFF, 0x6A, 0x1B, 0x9A),
        SampleCategories.Paths => Color.FromArgb(0xFF, 0x2E, 0x7D, 0x32),
        SampleCategories.Shaders => Color.FromArgb(0xFF, 0x15, 0x65, 0xC0),
        SampleCategories.Text => Color.FromArgb(0xFF, 0xAD, 0x14, 0x57),
        _ => Color.FromArgb(0xFF, 0x54, 0x6E, 0x7A),
    };

    public static SolidColorBrush Brush(string category) => new(Of(category));

    public static string Icon(string title) => title switch
    {
        "Blend Modes"          => "", // bi-layers-half
        "Blur Image Filter"    => "", // bi-droplet-half
        "2D Transforms"        => "", // bi-arrows-move
        "3D Transforms"        => "", // bi-box
        "Create XPS Document"  => "", // bi-file-earmark-richtext
        "GIF Player"           => "", // bi-film
        "Gradient"             => "", // bi-rainbow
        "Image Decoder"        => "", // bi-file-image
        "Lottie Player"        => "", // bi-play-circle
        "Nine-Patch Scaler"    => "", // bi-grid-3x3
        "Noise Generator"      => "", // bi-soundwave
        "Path Builder"         => "", // bi-pentagon
        "Path Effects Lab"     => "", // bi-bezier2
        "PDF Composer"         => "", // bi-file-pdf
        "Photo Lab"            => "", // bi-camera
        "Shader Playground"    => "", // bi-lightning
        "Text Lab"             => "", // bi-fonts
        "Text on Path"         => "", // bi-type
        "Vector Art"           => "", // bi-vector-pen
        "Vertex Mesh"          => "", // bi-diagram-3
        "World Text"           => "", // bi-globe2
        _                      => "", // bi-brush  (default, used for newer samples)
    };

    public const string IconMoonStars = ""; // bi-moon-stars (light mode)
    public const string IconSun        = ""; // bi-sun        (dark  mode)

    public const string IconFontFamily = "ms-appx:///Assets/Fonts/bootstrap-icons.ttf#bootstrap-icons";
}
