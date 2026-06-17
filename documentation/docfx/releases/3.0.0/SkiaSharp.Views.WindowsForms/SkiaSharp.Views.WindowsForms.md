# API diff: SkiaSharp.Views.WindowsForms.dll

## SkiaSharp.Views.WindowsForms.dll

> Assembly Version Changed: 3.0.0.0 vs 2.88.0.0

### Namespace SkiaSharp.Views.Desktop

#### New Type: SkiaSharp.Views.Desktop.Extensions

```csharp
public static class Extensions {
	// methods
	public static System.Drawing.Bitmap ToBitmap (this SkiaSharp.SKBitmap skiaBitmap);
	public static System.Drawing.Bitmap ToBitmap (this SkiaSharp.SKImage skiaImage);
	public static System.Drawing.Bitmap ToBitmap (this SkiaSharp.SKPixmap pixmap);
	public static System.Drawing.Bitmap ToBitmap (this SkiaSharp.SKPicture picture, SkiaSharp.SKSizeI dimensions);
	public static System.Drawing.Color ToDrawingColor (this SkiaSharp.SKColor color);
	public static System.Drawing.PointF ToDrawingPoint (this SkiaSharp.SKPoint point);
	public static System.Drawing.Point ToDrawingPoint (this SkiaSharp.SKPointI point);
	public static System.Drawing.RectangleF ToDrawingRect (this SkiaSharp.SKRect rect);
	public static System.Drawing.Rectangle ToDrawingRect (this SkiaSharp.SKRectI rect);
	public static System.Drawing.SizeF ToDrawingSize (this SkiaSharp.SKSize size);
	public static System.Drawing.Size ToDrawingSize (this SkiaSharp.SKSizeI size);
	public static SkiaSharp.SKBitmap ToSKBitmap (this System.Drawing.Bitmap bitmap);
	public static SkiaSharp.SKColor ToSKColor (this System.Drawing.Color color);
	public static SkiaSharp.SKImage ToSKImage (this System.Drawing.Bitmap bitmap);
	public static void ToSKPixmap (this System.Drawing.Bitmap bitmap, SkiaSharp.SKPixmap pixmap);
	public static SkiaSharp.SKPointI ToSKPoint (this System.Drawing.Point point);
	public static SkiaSharp.SKPoint ToSKPoint (this System.Drawing.PointF point);
	public static SkiaSharp.SKRectI ToSKRect (this System.Drawing.Rectangle rect);
	public static SkiaSharp.SKRect ToSKRect (this System.Drawing.RectangleF rect);
	public static SkiaSharp.SKSizeI ToSKSize (this System.Drawing.Size size);
	public static SkiaSharp.SKSize ToSKSize (this System.Drawing.SizeF size);
}
```


