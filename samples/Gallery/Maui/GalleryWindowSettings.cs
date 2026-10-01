using Microsoft.Maui.Controls;

namespace SkiaSharpSample;

internal sealed class GalleryWindowSettings
{
    private bool useGpu;

    public event EventHandler? BackendChanged;

    public bool UseGpu => useGpu;

    public void SetGpu(bool enabled)
    {
        if (useGpu == enabled) return;
        useGpu = enabled;
        BackendChanged?.Invoke(this, EventArgs.Empty);
    }

    public AppTheme Theme => Application.Current?.UserAppTheme ?? AppTheme.Unspecified;

    public void SetTheme(AppTheme theme)
    {
        if (Application.Current is { } app && app.UserAppTheme != theme)
            app.UserAppTheme = theme;
    }
}
