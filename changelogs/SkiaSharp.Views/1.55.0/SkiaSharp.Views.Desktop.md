# API diff: SkiaSharp.Views.Desktop.dll

## SkiaSharp.Views.Desktop.dll

> Assembly Version Changed: 1.55.0.0 vs 0.0.0.0

### New Namespace SkiaSharp.Views.Desktop

#### New Type: SkiaSharp.Views.Desktop.Extensions

```csharp
public static class Extensions {
	// methods
	public static System.Drawing.Color ToDrawingColor (this SkiaSharp.SKColor color);
	public static System.Drawing.PointF ToDrawingPoint (this SkiaSharp.SKPoint point);
	public static System.Drawing.Point ToDrawingPoint (this SkiaSharp.SKPointI point);
	public static System.Drawing.RectangleF ToDrawingRect (this SkiaSharp.SKRect rect);
	public static System.Drawing.Rectangle ToDrawingRect (this SkiaSharp.SKRectI rect);
	public static System.Drawing.SizeF ToDrawingSize (this SkiaSharp.SKSize size);
	public static System.Drawing.Size ToDrawingSize (this SkiaSharp.SKSizeI size);
	public static SkiaSharp.SKColor ToSKColor (this System.Drawing.Color color);
	public static SkiaSharp.SKPointI ToSKPoint (this System.Drawing.Point point);
	public static SkiaSharp.SKPoint ToSKPoint (this System.Drawing.PointF point);
	public static SkiaSharp.SKRectI ToSKRect (this System.Drawing.Rectangle rect);
	public static SkiaSharp.SKRect ToSKRect (this System.Drawing.RectangleF rect);
	public static SkiaSharp.SKSizeI ToSKSize (this System.Drawing.Size size);
	public static SkiaSharp.SKSize ToSKSize (this System.Drawing.SizeF size);
}
```

#### New Type: SkiaSharp.Views.Desktop.SKControl

```csharp
public class SKControl : System.Windows.Forms.Control {
	// constructors
	public SKControl ();
	// properties
	public SkiaSharp.SKSize CanvasSize { get; }
	// events
	public event System.EventHandler<SKPaintSurfaceEventArgs> PaintSurface;
	// methods
	protected override void Dispose (bool disposing);
	protected override void OnPaint (System.Windows.Forms.PaintEventArgs e);
	protected virtual void OnPaintSurface (SKPaintSurfaceEventArgs e);
}
```

#### New Type: SkiaSharp.Views.Desktop.SKGLControl

```csharp
public class SKGLControl : OpenTK.GLControl {
	// constructors
	public SKGLControl ();
	// properties
	public SkiaSharp.SKSize CanvasSize { get; }
	// events
	public event System.EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;
	// methods
	protected override void Dispose (bool disposing);
	protected override void OnPaint (System.Windows.Forms.PaintEventArgs e);
	protected virtual void OnPaintSurface (SKPaintGLSurfaceEventArgs e);
}
```

#### New Type: SkiaSharp.Views.Desktop.SKPaintGLSurfaceEventArgs

```csharp
public class SKPaintGLSurfaceEventArgs : System.EventArgs {
	// constructors
	public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTargetDesc renderTarget);
	// properties
	public SkiaSharp.GRBackendRenderTargetDesc RenderTarget { get; }
	public SkiaSharp.SKSurface Surface { get; }
}
```

#### New Type: SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs

```csharp
public class SKPaintSurfaceEventArgs : System.EventArgs {
	// constructors
	public SKPaintSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.SKImageInfo info);
	// properties
	public SkiaSharp.SKImageInfo Info { get; }
	public SkiaSharp.SKSurface Surface { get; }
}
```

