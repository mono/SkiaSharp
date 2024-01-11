# API diff: SkiaSharp.Views.Gtk.dll

## SkiaSharp.Views.Gtk.dll

> Assembly Version Changed: 1.60.0.0 vs 0.0.0.0

### New Namespace SkiaSharp.Views.Gtk

#### New Type: SkiaSharp.Views.Gtk.SKWidget

```csharp
public class SKWidget : Gtk.DrawingArea {
	// constructors
	public SKWidget ();
	// properties
	public SkiaSharp.SKSize CanvasSize { get; }
	// events
	public event System.EventHandler<SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs> PaintSurface;
	// methods
	public override void Destroy ();
	public override void Dispose ();
	public virtual void Dispose (bool disposing);
	protected override bool OnExposeEvent (Gdk.EventExpose evnt);
	protected virtual void OnPaintSurface (SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e);
	protected override void ~SKWidget ();
}
```

