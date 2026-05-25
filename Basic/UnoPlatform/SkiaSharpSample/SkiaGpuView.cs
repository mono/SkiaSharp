#if __UNO_SKIA__
using System;
using Windows.Foundation;
using SkiaSharp;
using Uno.WinUI.Graphics2DSK;

namespace SkiaSharpSample;

/// <summary>
/// A custom control that extends Uno's <see cref="SKCanvasElement"/> to
/// render skia animations.
/// This is the recommended GPU-accelerated approach for Skia-rendered Uno targets.
/// </summary>
public sealed partial class SkiaGpuView : SKCanvasElement
{
    public bool EnableRenderLoop
    {
        get;
        set
        {
            field = value;
            if (value)
            {
                Invalidate();
            }
        }
    }

    public Action<SKCanvas, Size>? RenderAction
    {
        get;
        set
        {
            field = value;
            Invalidate();
        }
    }

    protected override void RenderOverride(SKCanvas canvas, Size size)
    {
        RenderAction?.Invoke(canvas, size);
        if (EnableRenderLoop)
        {
            Invalidate();
        }
    }
}
#endif
