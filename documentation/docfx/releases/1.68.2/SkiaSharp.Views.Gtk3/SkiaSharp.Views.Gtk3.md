# API diff: SkiaSharp.Views.Gtk3.dll

## SkiaSharp.Views.Gtk3.dll

### Namespace SkiaSharp.Views.Gtk

#### New Type: SkiaSharp.Views.Gtk.GTKExtensions

```csharp
public static class GTKExtensions {
	// methods
	public static Gdk.Color ToColor (this SkiaSharp.SKColor color);
	public static Gdk.Pixbuf ToPixbuf (this SkiaSharp.SKBitmap skiaBitmap);
	public static Gdk.Pixbuf ToPixbuf (this SkiaSharp.SKImage skiaImage);
	public static Gdk.Pixbuf ToPixbuf (this SkiaSharp.SKPixmap pixmap);
	public static Gdk.Pixbuf ToPixbuf (this SkiaSharp.SKPicture picture, SkiaSharp.SKSizeI dimensions);
	public static Gdk.Point ToPoint (this SkiaSharp.SKPointI point);
	public static Gdk.Rectangle ToRect (this SkiaSharp.SKRectI rect);
	public static SkiaSharp.SKBitmap ToSKBitmap (this Gdk.Pixbuf pixbuf);
	public static SkiaSharp.SKColor ToSKColor (this Gdk.Color color);
	public static SkiaSharp.SKImage ToSKImage (this Gdk.Pixbuf pixbuf);
	public static void ToSKPixmap (this Gdk.Pixbuf pixbuf, SkiaSharp.SKPixmap pixmap);
	public static SkiaSharp.SKPointI ToSKPointI (this Gdk.Point point);
	public static SkiaSharp.SKRectI ToSKRectI (this Gdk.Rectangle rect);
	public static SkiaSharp.SKSizeI ToSKSizeI (this Gdk.Size size);
	public static Gdk.Size ToSize (this SkiaSharp.SKSizeI size);
}
```


