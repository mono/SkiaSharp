using Microsoft.Maui.Controls;
using SkiaSharpSample.Services;

namespace SkiaSharpSample;

internal sealed class GalleryWindowSettings
{
    private GalleryRenderer renderer;

    public GalleryWindowSettings(SampleService? catalog = null) => Catalog = catalog;

    public SampleService? Catalog { get; }

    public event EventHandler? BackendChanged;

    public GalleryRenderer Renderer => renderer;
    public bool UseGpu => renderer != GalleryRenderer.Cpu;
    public static bool GraphiteAvailable =>
#if ANDROID
        OperatingSystem.IsAndroidVersionAtLeast(24);
#elif IOS
        OperatingSystem.IsIOSVersionAtLeast(13);
#elif MACCATALYST
        true;
#else
        false;
#endif

    public void SetGpu(bool enabled)
    {
        SetRenderer(enabled ? GalleryRenderer.Ganesh : GalleryRenderer.Cpu);
    }

    public void SetRenderer(GalleryRenderer value)
    {
        if (value == GalleryRenderer.Graphite && !GraphiteAvailable)
            throw new PlatformNotSupportedException("Graphite presentation is deferred on Windows.");
        if (renderer == value) return;
        renderer = value;
        BackendChanged?.Invoke(this, EventArgs.Empty);
    }

    public AppTheme Theme => Application.Current?.UserAppTheme ?? AppTheme.Unspecified;

    public void SetTheme(AppTheme theme)
    {
        if (Application.Current is { } app && app.UserAppTheme != theme)
            app.UserAppTheme = theme;
    }
}
