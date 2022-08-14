# API diff: SkiaSharp.Views.Windows.dll

## SkiaSharp.Views.Windows.dll

> Assembly Version Changed: 2.88.0.0 vs 0.0.0.0

### New Namespace SkiaSharp.Views.Windows

#### New Type: SkiaSharp.Views.Windows.Extensions

```csharp
public static class Extensions {
	// methods
	public static System.Drawing.PointF ToDrawingPoint (this SkiaSharp.SKPoint point);
	public static System.Drawing.Point ToDrawingPoint (this SkiaSharp.SKPointI point);
	public static System.Drawing.RectangleF ToDrawingRect (this SkiaSharp.SKRect rect);
	public static System.Drawing.Rectangle ToDrawingRect (this SkiaSharp.SKRectI rect);
	public static System.Drawing.SizeF ToDrawingSize (this SkiaSharp.SKSize size);
	public static System.Drawing.Size ToDrawingSize (this SkiaSharp.SKSizeI size);
	public static SkiaSharp.SKPointI ToSKPoint (this System.Drawing.Point point);
	public static SkiaSharp.SKPoint ToSKPoint (this System.Drawing.PointF point);
	public static SkiaSharp.SKRectI ToSKRect (this System.Drawing.Rectangle rect);
	public static SkiaSharp.SKRect ToSKRect (this System.Drawing.RectangleF rect);
	public static SkiaSharp.SKSizeI ToSKSize (this System.Drawing.Size size);
	public static SkiaSharp.SKSize ToSKSize (this System.Drawing.SizeF size);
}
```

#### New Type: SkiaSharp.Views.Windows.GlobalStaticResources

```csharp
public sealed class GlobalStaticResources {
	// constructors
	public GlobalStaticResources ();
	// methods

	[Obsolete]
public static object FindResource (string name);
	public static void Initialize ();
	public static void RegisterDefaultStyles ();
	public static void RegisterResourceDictionariesBySource ();
}
```

#### New Type: SkiaSharp.Views.Windows.SKPaintGLSurfaceEventArgs

```csharp
public class SKPaintGLSurfaceEventArgs : System.EventArgs {
	// constructors
	public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTarget renderTarget);

	[Obsolete]
public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTargetDesc renderTarget);
	public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTarget renderTarget, SkiaSharp.GRSurfaceOrigin origin, SkiaSharp.SKColorType colorType);
	public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTarget renderTarget, SkiaSharp.GRSurfaceOrigin origin, SkiaSharp.SKImageInfo info);

	[Obsolete]
public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTarget renderTarget, SkiaSharp.GRSurfaceOrigin origin, SkiaSharp.SKColorType colorType, SkiaSharp.GRGlFramebufferInfo glInfo);
	public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTarget renderTarget, SkiaSharp.GRSurfaceOrigin origin, SkiaSharp.SKImageInfo info, SkiaSharp.SKImageInfo rawInfo);
	// properties
	public SkiaSharp.GRBackendRenderTarget BackendRenderTarget { get; }
	public SkiaSharp.SKColorType ColorType { get; }
	public SkiaSharp.SKImageInfo Info { get; }
	public SkiaSharp.GRSurfaceOrigin Origin { get; }
	public SkiaSharp.SKImageInfo RawInfo { get; }

	[Obsolete]
public SkiaSharp.GRBackendRenderTargetDesc RenderTarget { get; }
	public SkiaSharp.SKSurface Surface { get; }
}
```

#### New Type: SkiaSharp.Views.Windows.SKPaintSurfaceEventArgs

```csharp
public class SKPaintSurfaceEventArgs : System.EventArgs {
	// constructors
	public SKPaintSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.SKImageInfo info);
	public SKPaintSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.SKImageInfo info, SkiaSharp.SKImageInfo rawInfo);
	// properties
	public SkiaSharp.SKImageInfo Info { get; }
	public SkiaSharp.SKImageInfo RawInfo { get; }
	public SkiaSharp.SKSurface Surface { get; }
}
```

#### New Type: SkiaSharp.Views.Windows.SKSwapChainPanel

```csharp
public class SKSwapChainPanel : Microsoft.UI.Xaml.FrameworkElement {
	// constructors
	public SKSwapChainPanel ();
	// properties
	public SkiaSharp.SKSize CanvasSize { get; }
	public double ContentsScale { get; }
	public bool DrawInBackground { get; set; }
	public bool EnableRenderLoop { get; set; }
	public SkiaSharp.GRContext GRContext { get; }
	public static bool RaiseOnUnsupported { get; set; }
	// events
	public event System.EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;
	// methods
	public void Invalidate ();
	protected virtual void OnPaintSurface (SKPaintGLSurfaceEventArgs e);
}
```

#### New Type: SkiaSharp.Views.Windows.SKXamlCanvas

```csharp
public class SKXamlCanvas : Microsoft.UI.Xaml.Controls.Canvas {
	// constructors
	public SKXamlCanvas ();
	// properties
	public SkiaSharp.SKSize CanvasSize { get; }
	public double Dpi { get; }
	public bool IgnorePixelScaling { get; set; }
	// events
	public event System.EventHandler<SKPaintSurfaceEventArgs> PaintSurface;
	// methods
	public void Invalidate ();
	protected virtual void OnPaintSurface (SKPaintSurfaceEventArgs e);
}
```

#### New Type: SkiaSharp.Views.Windows.UWPExtensions

```csharp
public static class UWPExtensions {
	// methods
	public static Windows.UI.Color ToColor (this SkiaSharp.SKColor color);
	public static Windows.Foundation.Point ToPoint (this SkiaSharp.SKPoint point);
	public static Windows.Foundation.Rect ToRect (this SkiaSharp.SKRect rect);
	public static SkiaSharp.SKColor ToSKColor (this Windows.UI.Color color);
	public static SkiaSharp.SKPoint ToSKPoint (this Windows.Foundation.Point point);
	public static SkiaSharp.SKRect ToSKRect (this Windows.Foundation.Rect rect);
	public static SkiaSharp.SKSize ToSKSize (this Windows.Foundation.Size size);
	public static Windows.Foundation.Size ToSize (this SkiaSharp.SKSize size);
}
```

