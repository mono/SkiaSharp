# API diff: SkiaSharp.Views.WPF.dll

## SkiaSharp.Views.WPF.dll

> Assembly Version Changed: 3.116.0.0 vs 2.88.0.0

### Namespace SkiaSharp.Views.WPF

#### New Type: SkiaSharp.Views.WPF.SKGLElement

```csharp
public class SKGLElement : OpenTK.Wpf.GLWpfControl, System.IDisposable {
	// constructors
	public SKGLElement ();
	// properties
	public SkiaSharp.SKSize CanvasSize { get; }
	public SkiaSharp.GRContext GRContext { get; }
	// events
	public event System.EventHandler<SkiaSharp.Views.Desktop.SKPaintGLSurfaceEventArgs> PaintSurface;
	// methods
	public virtual void Dispose ();
	protected virtual void Dispose (bool disposing);
	protected virtual void OnPaint (System.TimeSpan e);
	protected virtual void OnPaintSurface (SkiaSharp.Views.Desktop.SKPaintGLSurfaceEventArgs e);
	protected override void OnRender (System.Windows.Media.DrawingContext drawingContext);
}
```


