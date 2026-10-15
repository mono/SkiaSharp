# API diff: SkiaSharp.Views.Gtk4.dll

## SkiaSharp.Views.Gtk4.dll

> Assembly Version Changed: 4.156.0.0 vs 4.155.0.0

### Namespace SkiaSharp.Views.Gtk

#### Type Changed: SkiaSharp.Views.Gtk.SKDrawingArea

Added property:

```csharp
public bool IgnorePixelScaling { get; set; }
```


#### New Type: SkiaSharp.Views.Gtk.SKGLArea

```csharp
public class SKGLArea : Gtk.GLArea, GObject.GTypeProvider, GObject.InstanceFactory, GObject.NativeObject, Gtk.Accessible, Gtk.Buildable, Gtk.ConstraintTarget, System.IDisposable {
	// constructors
	public SKGLArea ();
	// properties
	public SkiaSharp.SKSize CanvasSize { get; }
	public bool EnableRenderLoop { get; set; }
	public SkiaSharp.GRContext GRContext { get; }
	public bool IgnorePixelScaling { get; set; }
	// events
	public event System.EventHandler<SkiaSharp.Views.Desktop.SKPaintGLSurfaceEventArgs> PaintSurface;
	// methods
	public override void Dispose ();
	protected virtual void OnPaintSurface (SkiaSharp.Views.Desktop.SKPaintGLSurfaceEventArgs e);
}
```
