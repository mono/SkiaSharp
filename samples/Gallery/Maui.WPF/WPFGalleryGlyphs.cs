using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers.WPF;
using Microsoft.Maui.Graphics;
using WPFColor = System.Windows.Media.Color;
using WPFFontFamily = System.Windows.Media.FontFamily;
using WPFSolidColorBrush = System.Windows.Media.SolidColorBrush;
using MauiButton = Microsoft.Maui.Controls.Button;
using MauiColor = Microsoft.Maui.Graphics.Color;
using MauiColors = Microsoft.Maui.Graphics.Colors;

namespace SkiaSharpSample;

internal static class WPFGalleryGlyphs
{
    private const string FontAlias = "BootstrapIcons";
    private const string GlyphTag = "SkiaSharpGalleryGlyph";
    private static bool registered;
    private static readonly Lazy<WPFFontFamily> glyphFont = new(() =>
    {
        var directory = Path.Combine(AppContext.BaseDirectory, "Fonts");
        var file = Path.Combine(directory, "bootstrap-icons.ttf");
        if (!File.Exists(file))
            throw new FileNotFoundException("The Gallery glyph font was not copied to the WPF output.", file);
        return new WPFFontFamily(
            new Uri(Path.GetFullPath(directory) + Path.DirectorySeparatorChar, UriKind.Absolute),
            "./bootstrap-icons.ttf#bootstrap-icons");
    });

    public static void Register()
    {
        if (registered)
            return;
        registered = true;

        ImageButtonHandler.Mapper.ModifyMapping(nameof(ImageButton.BorderWidth), (handler, view, mapping) =>
        {
            // MAUI's platform-default sentinel is not a valid WPF thickness.
            if (view.BorderWidth == -1)
                handler.PlatformView.BorderThickness = new System.Windows.Thickness(0);
            else
                mapping?.Invoke(handler, view);
        });

        LabelHandler.Mapper.AppendToMapping(nameof(ILabel.Font), (handler, view) =>
        {
            if (view.Font.Family == FontAlias)
                handler.PlatformView.FontFamily = glyphFont.Value;
        });

        ImageButtonHandler.Mapper.AppendToMapping(nameof(ImageButton.Source), (handler, view) =>
        {
            if (view.Source is FontImageSource { FontFamily: FontAlias } glyph)
                handler.PlatformView.Content = CreateGlyph(glyph);
            else if (handler.PlatformView.Content is TextBlock { Tag: GlyphTag })
                handler.PlatformView.Content = null;
        });

        ButtonHandler.Mapper.AppendToMapping(nameof(IImageSourcePart.Source), MapButtonGlyph);
        ButtonHandler.Mapper.AppendToMapping(nameof(IText.Text), MapButtonGlyph);
    }

    private static void MapButtonGlyph(ButtonHandler handler, IButton button)
    {
        if (button is not MauiButton view)
            return;

        if (view.ImageSource is FontImageSource { FontFamily: FontAlias } glyph)
        {
            handler.PlatformView.Content = new StackPanel
            {
                Tag = GlyphTag,
                Orientation = Orientation.Horizontal,
                Children =
                {
                    CreateGlyph(glyph),
                    new TextBlock { Text = view.Text, VerticalAlignment = System.Windows.VerticalAlignment.Center },
                },
            };
        }
        else if (handler.PlatformView.Content is StackPanel { Tag: GlyphTag })
        {
            handler.PlatformView.Content = view.Text;
        }
    }

    private static TextBlock CreateGlyph(FontImageSource glyph) => new()
    {
        Tag = GlyphTag,
        Text = glyph.Glyph,
        FontFamily = glyphFont.Value,
        FontSize = glyph.Size > 0 ? glyph.Size : 16,
        Foreground = new WPFSolidColorBrush(ToWPFColor(glyph.Color ?? MauiColors.White)),
        Margin = new System.Windows.Thickness(0, 0, 5, 0),
        VerticalAlignment = System.Windows.VerticalAlignment.Center,
    };

    private static WPFColor ToWPFColor(MauiColor color) => WPFColor.FromArgb(
        (byte)Math.Round(color.Alpha * 255), (byte)Math.Round(color.Red * 255),
        (byte)Math.Round(color.Green * 255), (byte)Math.Round(color.Blue * 255));
}
