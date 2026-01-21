# API diff: SkiaSharp.Views.WPF.dll

## SkiaSharp.Views.WPF.dll

> Assembly Version Changed: 1.68.0.0 vs 0.0.0.0

### New Namespace SkiaSharp.Views.WPF

#### New Type: SkiaSharp.Views.WPF.SKElement

```csharp
public class SKElement : System.Windows.FrameworkElement, System.ComponentModel.ISupportInitialize, System.Windows.IFrameworkInputElement, System.Windows.IInputElement, System.Windows.Markup.IQueryAmbient, System.Windows.Media.Animation.IAnimatable {
	// constructors
	public SKElement ();
	// properties
	public SkiaSharp.SKSize CanvasSize { get; }
	public bool IgnorePixelScaling { get; set; }
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
	public static SkiaSharp.SKBitmap ToSKBitmap (this System.Windows.Media.Imaging.BitmapSource bitmap);
	public static SkiaSharp.SKColor ToSKColor (this System.Windows.Media.Color color);
	public static SkiaSharp.SKImage ToSKImage (this System.Windows.Media.Imaging.BitmapSource bitmap);
	public static void ToSKPixmap (this System.Windows.Media.Imaging.BitmapSource bitmap, SkiaSharp.SKPixmap pixmap);
	public static SkiaSharp.SKPoint ToSKPoint (this System.Windows.Point point);
	public static SkiaSharp.SKRect ToSKRect (this System.Windows.Rect rect);
	public static SkiaSharp.SKSize ToSKSize (this System.Windows.Size size);
	public static System.Windows.Size ToSize (this SkiaSharp.SKSize size);
	public static System.Windows.Media.Imaging.WriteableBitmap ToWriteableBitmap (this SkiaSharp.SKBitmap skiaBitmap);
	public static System.Windows.Media.Imaging.WriteableBitmap ToWriteableBitmap (this SkiaSharp.SKImage skiaImage);
	public static System.Windows.Media.Imaging.WriteableBitmap ToWriteableBitmap (this SkiaSharp.SKPixmap pixmap);
	public static System.Windows.Media.Imaging.WriteableBitmap ToWriteableBitmap (this SkiaSharp.SKPicture picture, SkiaSharp.SKSizeI dimensions);
}
```

