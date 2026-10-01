using Microsoft.Maui.Controls;

namespace SkiaSharpSample.Controls;

internal sealed class ThemeMenuView : ContentView
{
    public ThemeMenuView(GalleryWindowSettings settings)
    {
        WidthRequest = 202;
        AutomationId = "gallery-theme-menu";
        var layout = new VerticalStackLayout { Spacing = 5 };
        layout.Children.Add(GalleryUi.Text("Appearance", 14, true));
        foreach (var (theme, name, id) in new[]
        {
            (AppTheme.Unspecified, "System", "gallery-theme-system"),
            (AppTheme.Light, "Light", "gallery-theme-light"),
            (AppTheme.Dark, "Dark", "gallery-theme-dark")
        })
        {
            var selected = settings.Theme == theme;
            var button = new Button
            {
                Text = $"{(selected ? "✓  " : "     ")}{name}",
                AutomationId = id,
                Style = (Style)Application.Current!.Resources["GalleryRow"]
            };
            GalleryUi.SelectRow(button, selected);
            SemanticProperties.SetDescription(button, $"{name} appearance, {(selected ? "selected" : "not selected")}");
            button.Clicked += (_, _) => settings.SetTheme(theme);
            layout.Children.Add(button);
        }
        Content = layout;
    }
}
