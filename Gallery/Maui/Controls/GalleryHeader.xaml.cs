using Microsoft.Maui.Controls;

namespace SkiaSharpSample.Controls;

public partial class GalleryHeader : ContentView
{
    private readonly GalleryWindowSettings settings;
    private readonly Func<Task>? goBack;
    private bool subscribed;
    private string brandText = "SkiaSharp Gallery";
    private bool showSubheading = true;
    private bool themeVisible = true;

    internal GalleryHeader(GalleryWindowSettings settings, Func<Task>? goBack = null)
    {
        this.settings = settings;
        this.goBack = goBack;
        InitializeComponent();
        BindingContext = this;
        GalleryPopup.SetCloseOnAction(ThemeButton, true);
        GalleryPopup.SetContentFactory(ThemeButton, () => new ThemeMenuView(settings));
        GalleryPopup.SetCloseOnAction(InfoButton, true);
        GalleryPopup.SetContentFactory(InfoButton, () => new InfoMenuView(settings));
        SizeChanged += (_, _) =>
        {
            var compact = Width > 0 && Width < 450;
            BrandText = compact ? "SkiaSharp" : "SkiaSharp Gallery";
            ShowSubheading = !compact;
            ThemeVisible = Width >= 380;
        };
        Loaded += (_, _) =>
        {
            if (subscribed) return;
            subscribed = true;
            settings.BackendChanged += BackendChanged;
            NotifyBackend();
        };
        Unloaded += (_, _) =>
        {
            if (!subscribed) return;
            subscribed = false;
            settings.BackendChanged -= BackendChanged;
        };
    }

    public bool HasBack => goBack is not null;
    public bool IsHome => goBack is null;
    public bool CpuSelected => !settings.UseGpu;
    public bool GpuSelected => settings.UseGpu;
    public string CpuDescription => $"CPU renderer, {(CpuSelected ? "selected" : "not selected")}";
    public string GpuDescription => $"GPU renderer, {(GpuSelected ? "selected" : "not selected")}";
    public string BrandText
    {
        get => brandText;
        private set { if (brandText != value) { brandText = value; OnPropertyChanged(); } }
    }
    public bool ShowSubheading
    {
        get => showSubheading;
        private set { if (showSubheading != value) { showSubheading = value; OnPropertyChanged(); } }
    }
    public bool ThemeVisible
    {
        get => themeVisible;
        private set { if (themeVisible != value) { themeVisible = value; OnPropertyChanged(); } }
    }

    private void BackendChanged(object? sender, EventArgs e) => NotifyBackend();
    private void NotifyBackend()
    {
        OnPropertyChanged(nameof(CpuSelected));
        OnPropertyChanged(nameof(GpuSelected));
        OnPropertyChanged(nameof(CpuDescription));
        OnPropertyChanged(nameof(GpuDescription));
    }

    private async void BackClicked(object? sender, EventArgs e)
    {
        if (goBack is not null) await goBack();
    }
    private void CpuClicked(object? sender, EventArgs e) => settings.SetGpu(false);
    private void GpuClicked(object? sender, EventArgs e) => settings.SetGpu(true);
}
