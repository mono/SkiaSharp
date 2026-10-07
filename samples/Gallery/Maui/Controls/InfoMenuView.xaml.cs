using Microsoft.Maui.Controls;

namespace SkiaSharpSample.Controls;

public partial class InfoMenuView : ContentView
{
    internal InfoMenuView(GalleryWindowSettings settings)
    {
        var catalog = settings.Catalog;
        SkiaSharpVersion = catalog?.SkiaSharpVersion ?? "<unavailable>";
        HarfBuzzSharpVersion = catalog?.HarfBuzzSharpVersion ?? "<unavailable>";
        BuildTimestamp = catalog?.BuildTimestamp?.ToLocalTime().ToString("f") ?? "Not stamped";
        BuildFooter = catalog?.BuildFooter is { Length: > 0 } footer ? footer : "Local gallery build";
        InitializeComponent();
        BindingContext = this;
    }

    public string SkiaSharpVersion { get; }
    public string HarfBuzzSharpVersion { get; }
    public string BuildTimestamp { get; }
    public string BuildFooter { get; }

    private void CloseClicked(object? sender, EventArgs e) { }
}
