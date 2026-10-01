using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace SkiaSharpSample.Controls;

internal sealed class GalleryHeader : ContentView
{
    private readonly GalleryWindowSettings settings;
    private readonly Button cpu;
    private readonly Button gpu;
    private readonly Label brand;
    private readonly Label subheading;
    private bool subscribed;

    public GalleryHeader(GalleryWindowSettings settings, Func<Task>? goBack = null)
    {
        this.settings = settings;
        AutomationId = "gallery-app-header";
        BackgroundColor = GalleryUi.Navy;
        var root = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new(GridLength.Auto), new(GridLength.Star), new(GridLength.Auto),
                new(GridLength.Auto), new(GridLength.Auto)
            },
            ColumnSpacing = 8, Padding = new Thickness(12, 8)
        };
        if (goBack is not null)
        {
            var back = GalleryUi.IconButton(GalleryUi.IconBack, "sample-back", "Back to gallery", async (_, _) => await goBack());
            back.BackgroundColor = Color.FromArgb("#34438E");
            back.Source = new FontImageSource { Glyph = GalleryUi.IconBack, FontFamily = "BootstrapIcons", Color = Colors.White, Size = 17 };
            root.Add(back);
        }
        else
        {
            root.Add(new Label
            {
                Text = "\uf3fa", FontFamily = "BootstrapIcons", TextColor = Colors.White, FontSize = 23,
                WidthRequest = 38, VerticalTextAlignment = TextAlignment.Center,
                HorizontalTextAlignment = TextAlignment.Center, InputTransparent = true
            });
        }
        var titles = new VerticalStackLayout { Spacing = 0, VerticalOptions = LayoutOptions.Center };
        brand = new Label { Text = "SkiaSharp Gallery", FontSize = 17, FontAttributes = FontAttributes.Bold, TextColor = Colors.White, MaxLines = 1, LineBreakMode = LineBreakMode.TailTruncation };
        subheading = new Label { Text = "Native canvas and API atlas", FontSize = 10, TextColor = Color.FromArgb("#C8D7FF") };
        titles.Children.Add(brand);
        titles.Children.Add(subheading);
        root.Add(titles, 1);

        var backends = new HorizontalStackLayout { Spacing = 3, VerticalOptions = LayoutOptions.Center };
        cpu = BackendButton("CPU", "gallery-header-cpu", () => settings.SetGpu(false));
        gpu = BackendButton("GPU", "gallery-header-gpu", () => settings.SetGpu(true));
        backends.Children.Add(cpu);
        backends.Children.Add(gpu);
        root.Add(backends, 2);
        var theme = GalleryUi.IconButton("\uf496", "gallery-header-theme", "Choose system, light, or dark theme", (_, _) => { });
        theme.Source = new FontImageSource { Glyph = "\uf496", FontFamily = "BootstrapIcons", Color = Colors.White, Size = 16 };
        theme.BackgroundColor = Color.FromArgb("#34438E");
        GalleryPopup.SetCloseOnAction(theme, true);
        GalleryPopup.SetContentFactory(theme, () => new ThemeMenuView(settings));
        root.Add(theme, 3);
        var info = GalleryUi.IconButton("\uf431", "gallery-info-trigger", "About this gallery", (_, _) => { });
        GalleryPopup.SetCloseOnAction(info, true);
        GalleryPopup.SetContentFactory(info, () => new InfoMenuView(settings));
        root.Add(info, 4);
        Content = root;
        SizeChanged += (_, _) =>
        {
            var narrow = Width > 0 && Width < 450;
            brand.Text = narrow ? "SkiaSharp" : "SkiaSharp Gallery";
            brand.FontSize = narrow ? 15 : 17;
            root.ColumnSpacing = narrow ? 4 : 8;
            subheading.IsVisible = !narrow;
        };
        Loaded += (_, _) =>
        {
            if (subscribed) return;
            subscribed = true;
            settings.BackendChanged += SettingsChanged;
            UpdateBackend();
        };
        Unloaded += (_, _) =>
        {
            if (!subscribed) return;
            subscribed = false;
            settings.BackendChanged -= SettingsChanged;
        };
        UpdateBackend();
    }

    private static Button BackendButton(string title, string id, Action select)
    {
        var button = new Button
        {
            Text = title, AutomationId = id, CornerRadius = 9, FontSize = 11,
            FontAttributes = FontAttributes.Bold, Padding = new Thickness(6, 3),
            MinimumHeightRequest = 40, MinimumWidthRequest = 45,
            BorderWidth = 1
        };
        button.Clicked += (_, _) => select();
        return button;
    }

    private void SettingsChanged(object? sender, EventArgs e) => UpdateBackend();

    private void UpdateBackend()
    {
        SetBackendStyle(cpu, !settings.UseGpu);
        SetBackendStyle(gpu, settings.UseGpu);
    }

    private static void SetBackendStyle(Button button, bool selected)
    {
        button.BackgroundColor = selected ? Colors.White : Color.FromArgb("#34438E");
        button.TextColor = selected ? GalleryUi.Navy : Colors.White;
        button.BorderColor = selected ? Colors.White : Color.FromArgb("#6979BD");
        SemanticProperties.SetDescription(button, $"{button.Text} renderer, {(selected ? "selected" : "not selected")}");
    }
}
