using Microsoft.Maui.Controls;

namespace SkiaSharpSample.Controls;

internal sealed class InfoMenuView : ContentView
{
    public InfoMenuView(GalleryWindowSettings settings)
    {
        WidthRequest = 274;
        AutomationId = "gallery-info-menu";
        var content = new VerticalStackLayout { Spacing = 9 };
        var heading = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Auto) }
        };
        var title = GalleryUi.Text("About the gallery", 15, true);
        title.VerticalOptions = LayoutOptions.Center;
        heading.Add(title);
        heading.Add(GalleryUi.IconButton(GalleryUi.IconClose, "gallery-info-close", "Close gallery information", (_, _) => { }), 1);
        content.Children.Add(heading);

        var catalog = settings.Catalog;
        content.Children.Add(InfoLine("SkiaSharp", catalog?.SkiaSharpVersion ?? "<unavailable>", "gallery-info-skiasharp-version"));
        content.Children.Add(InfoLine("HarfBuzzSharp", catalog?.HarfBuzzSharpVersion ?? "<unavailable>", "gallery-info-harfbuzz-version"));
        content.Children.Add(InfoLine("Built", catalog?.BuildTimestamp?.ToLocalTime().ToString("f") ?? "Not stamped", "gallery-info-build-timestamp"));
        content.Children.Add(InfoLine("Build", catalog?.BuildFooter is { Length: > 0 } footer ? footer : "Local gallery build", "gallery-info-build-footer"));
        Content = content;
    }

    private static View InfoLine(string caption, string value, string id)
    {
        var row = new VerticalStackLayout { Spacing = 1 };
        row.Children.Add(GalleryUi.Text(caption.ToUpperInvariant(), 10, true, "SecondaryText"));
        var detail = GalleryUi.Text(value, 12);
        detail.AutomationId = id;
        row.Children.Add(detail);
        return row;
    }
}
