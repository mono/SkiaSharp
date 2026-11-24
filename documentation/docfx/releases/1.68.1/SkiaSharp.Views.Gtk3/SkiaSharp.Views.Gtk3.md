# API diff: SkiaSharp.Views.Gtk3.dll

## SkiaSharp.Views.Gtk3.dll

> Assembly Version Changed: 1.68.0.0 vs 0.0.0.0

### New Namespace SkiaSharp.Views.Gtk

#### New Type: SkiaSharp.Views.Gtk.SKDrawingArea

```csharp
public class SKDrawingArea : Gtk.DrawingArea, Atk.IImplementor, GLib.IWrapper, System.IDisposable {
	// constructors
	public SKDrawingArea ();
	// properties
	public SkiaSharp.SKSize CanvasSize { get; }
	// events
	public event System.EventHandler<SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs> PaintSurface;
	// methods
	protected override void Dispose (bool disposing);
	protected override bool OnDrawn (Cairo.Context cr);
	protected virtual void OnPaintSurface (SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e);
}
```

