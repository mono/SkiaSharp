# API diff: SkiaSharp.Views.WindowsForms.dll

## SkiaSharp.Views.WindowsForms.dll

> Assembly Version Changed: 1.68.0.0 vs 0.0.0.0

### New Namespace SkiaSharp.Views.Desktop

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
	public SKGLControl (OpenTK.Graphics.GraphicsMode mode);
	public SKGLControl (OpenTK.Graphics.GraphicsMode mode, int major, int minor, OpenTK.Graphics.GraphicsContextFlags flags);
	// properties
	public SkiaSharp.SKSize CanvasSize { get; }
	public SkiaSharp.GRContext GRContext { get; }
	// events
	public event System.EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;
	// methods
	protected override void Dispose (bool disposing);
	protected override void OnPaint (System.Windows.Forms.PaintEventArgs e);
	protected virtual void OnPaintSurface (SKPaintGLSurfaceEventArgs e);
}
```

