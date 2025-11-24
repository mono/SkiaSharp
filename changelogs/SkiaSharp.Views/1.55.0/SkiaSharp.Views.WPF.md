# API diff: SkiaSharp.Views.WPF.dll

## SkiaSharp.Views.WPF.dll

> Assembly Version Changed: 1.55.0.0 vs 0.0.0.0

### New Namespace SkiaSharp.Views.WPF

#### New Type: SkiaSharp.Views.WPF.SKElement

```csharp
public class SKElement : System.Windows.FrameworkElement {
	// constructors
	public SKElement ();
	// properties
	public SkiaSharp.SKSize CanvasSize { get; }
	// events
	public event System.EventHandler<SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs> PaintSurface;
	// methods
	protected virtual void OnPaintSurface (SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e);
	protected override void OnRender (System.Windows.Media.DrawingContext drawingContext);
	protected override void OnRenderSizeChanged (System.Windows.SizeChangedInfo sizeInfo);
}
```

#### New Type: SkiaSharp.Views.WPF.WPFExtensions

```csharp
public static class WPFExtensions {
	// methods
	public static System.Windows.Media.Color ToColor (this SkiaSharp.SKColor color);
	public static System.Windows.Point ToPoint (this SkiaSharp.SKPoint point);
	public static System.Windows.Rect ToRect (this SkiaSharp.SKRect rect);
	public static SkiaSharp.SKColor ToSKColor (this System.Windows.Media.Color color);
	public static SkiaSharp.SKPoint ToSKPoint (this System.Windows.Point point);
	public static SkiaSharp.SKRect ToSKRect (this System.Windows.Rect rect);
	public static SkiaSharp.SKSize ToSKSize (this System.Windows.Size size);
	public static System.Windows.Size ToSize (this SkiaSharp.SKSize size);
}
```

