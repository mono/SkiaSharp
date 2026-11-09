using Microsoft.Maui.Controls;

namespace SkiaSharpSample.Controls;

internal sealed class ThemeMenuView : OptionMenuView
{
    public ThemeMenuView(GalleryWindowSettings settings)
        : base("Appearance", "gallery-theme-menu", 202,
        [
            new MenuOptionItem("System", "gallery-theme-system",
                settings.Theme == AppTheme.Unspecified,
                () => settings.SetTheme(AppTheme.Unspecified)),
            new MenuOptionItem("Light", "gallery-theme-light",
                settings.Theme == AppTheme.Light,
                () => settings.SetTheme(AppTheme.Light)),
            new MenuOptionItem("Dark", "gallery-theme-dark",
                settings.Theme == AppTheme.Dark,
                () => settings.SetTheme(AppTheme.Dark))
        ])
    {
    }
}
