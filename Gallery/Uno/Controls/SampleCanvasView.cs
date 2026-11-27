using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SkiaSharp;
using SkiaSharp.Views.Windows;
#if HAS_UNO
using Uno.WinUI.Graphics2DSK;
using Windows.Foundation;
#endif

namespace SkiaSharpSample.Controls;

internal abstract class SampleCanvasView : UserControl
{
    public delegate void RenderCallback(SKCanvas canvas, SKImageInfo info, SKImageInfo rawInfo);

    protected RenderCallback OnRender { get; }

    protected SampleCanvasView(RenderCallback onRender)
    {
        OnRender = onRender;
        HorizontalAlignment = HorizontalAlignment.Stretch;
        VerticalAlignment = VerticalAlignment.Stretch;
    }

    public abstract void InvalidateRender();

    public static SampleCanvasView Create(RenderCallback onRender)
    {
#if HAS_UNO
        if (SKCanvasElement.IsSupportedOnCurrentPlatform())
            return new CanvasElementView(onRender);
#endif
        // SKSwapChainPanel is only GPU-backed on browser/Android in SkiaSharp.Views.Uno.WinUI.
        if (OperatingSystem.IsBrowser() || OperatingSystem.IsAndroid())
            return new SwapChainView(onRender);
        return new XamlCanvasView(onRender);
    }
}

internal sealed class XamlCanvasView : SampleCanvasView
{
    private readonly SKXamlCanvas canvas;

    public XamlCanvasView(RenderCallback onRender) : base(onRender)
    {
        canvas = new SKXamlCanvas { IgnorePixelScaling = true };
        canvas.PaintSurface += (_, e) =>
        {
            e.Surface.Canvas.Clear(SKColors.White);
            OnRender(e.Surface.Canvas, e.Info, e.RawInfo);
        };
        Content = canvas;
    }

    public override void InvalidateRender() => canvas.Invalidate();
}

internal sealed class SwapChainView : SampleCanvasView
{
    private readonly SKSwapChainPanel panel;

    public SwapChainView(RenderCallback onRender) : base(onRender)
    {
        panel = new SKSwapChainPanel();
        panel.PaintSurface += (_, e) =>
        {
            e.Surface.Canvas.Clear(SKColors.White);
            OnRender(e.Surface.Canvas, e.Info, e.RawInfo);
        };
        Content = panel;
    }

    public override void InvalidateRender() => panel.Invalidate();
}

#if HAS_UNO
internal sealed class CanvasElementView : SampleCanvasView
{
    private readonly Inner element;

    public CanvasElementView(RenderCallback onRender) : base(onRender)
    {
        element = new Inner(this);
        Content = element;
    }

    public override void InvalidateRender() => element.Invalidate();

    private sealed class Inner : SKCanvasElement
    {
        private readonly CanvasElementView parent;
        public Inner(CanvasElementView parent) { this.parent = parent; }

        protected override void RenderOverride(SKCanvas canvas, Size area)
        {
            canvas.Clear(SKColors.White);
            // SKCanvasElement renders into Uno's Skia compositor, which has
            // already accounted for pixel scaling — DIP and device px coincide here.
            var info = new SKImageInfo((int)area.Width, (int)area.Height);
            parent.OnRender(canvas, info, info);
        }
    }
}
#endif
