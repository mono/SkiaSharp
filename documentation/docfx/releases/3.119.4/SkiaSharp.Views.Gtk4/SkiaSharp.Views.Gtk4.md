# API diff: SkiaSharp.Views.Gtk4.dll

## SkiaSharp.Views.Gtk4.dll

> Assembly Version Changed: 3.119.0.0 vs 0.0.0.0

### New Namespace SkiaSharp.Views.Gtk

#### New Type: SkiaSharp.Views.Gtk.GTKExtensions

```csharp
public static class GTKExtensions {
	// methods
	public static Gdk.RGBA ToGdkRGBA (this SkiaSharp.SKColor color);
	public static Gdk.RGBA ToGdkRGBA (this SkiaSharp.SKColorF color);
	public static Gdk.Rectangle ToGdkRectangle (this SkiaSharp.SKRectI rect);
	public static Graphene.Point ToGraphenePoint (this SkiaSharp.SKPoint point);
	public static Graphene.Point3D ToGraphenePoint3D (this SkiaSharp.SKPoint3 point);
	public static Graphene.Rect ToGrapheneRect (this SkiaSharp.SKRect rect);
	public static Graphene.Size ToGrapheneSize (this SkiaSharp.SKSize size);
	public static SkiaSharp.SKColor ToSKColor (this Gdk.RGBA color);
	public static SkiaSharp.SKColorF ToSKColorF (this Gdk.RGBA color);
	public static SkiaSharp.SKPoint ToSKPoint (this Graphene.Point point);
	public static SkiaSharp.SKPoint3 ToSKPoint3 (this Graphene.Point3D point);
	public static SkiaSharp.SKRect ToSKRect (this Graphene.Rect rect);
	public static SkiaSharp.SKRectI ToSKRectI (this Gdk.Rectangle rect);
	public static SkiaSharp.SKSize ToSKSize (this Graphene.Size size);
}
```

#### New Type: SkiaSharp.Views.Gtk.SKDrawingArea

```csharp
public class SKDrawingArea : Gtk.DrawingArea, GObject.GTypeProvider, GObject.InstanceFactory, GObject.NativeObject, Gtk.Accessible, Gtk.Buildable, Gtk.ConstraintTarget, System.IDisposable {
	// constructors
	public SKDrawingArea ();
	// properties
	public SkiaSharp.SKSize CanvasSize { get; }
	// events
	public event System.EventHandler<SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs> PaintSurface;
	// methods
	public override void Dispose ();
	protected virtual void OnPaintSurface (SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e);
}
```

