# API diff: SkiaSharp.Views.watchOS.dll

## SkiaSharp.Views.watchOS.dll

> Assembly Version Changed: 1.60.0.0 vs 0.0.0.0

### New Namespace SkiaSharp.Views.watchOS

#### New Type: SkiaSharp.Views.watchOS.AppleExtensions

```csharp
public static class AppleExtensions {
	// methods
	public static CoreGraphics.CGColor ToCGColor (this SkiaSharp.SKColor color);
	public static CoreGraphics.CGImage ToCGImage (this SkiaSharp.SKBitmap skiaBitmap);
	public static CoreGraphics.CGImage ToCGImage (this SkiaSharp.SKImage skiaImage);
	public static CoreGraphics.CGImage ToCGImage (this SkiaSharp.SKPixmap skiaPixmap);
	public static CoreGraphics.CGImage ToCGImage (this SkiaSharp.SKPicture skiaPicture, SkiaSharp.SKSizeI dimensions);
	public static Foundation.NSData ToNSData (this SkiaSharp.SKData skiaData);
	public static CoreGraphics.CGPoint ToPoint (this SkiaSharp.SKPoint point);
	public static CoreGraphics.CGRect ToRect (this SkiaSharp.SKRect rect);
	public static SkiaSharp.SKBitmap ToSKBitmap (this CoreGraphics.CGImage cgImage);
	public static SkiaSharp.SKColor ToSKColor (this CoreGraphics.CGColor color);
	public static SkiaSharp.SKData ToSKData (this Foundation.NSData nsData);
	public static SkiaSharp.SKImage ToSKImage (this CoreGraphics.CGImage cgImage);
	public static void ToSKPixmap (this CoreGraphics.CGImage cgImage, SkiaSharp.SKPixmap pixmap);
	public static SkiaSharp.SKPoint ToSKPoint (this CoreGraphics.CGPoint point);
	public static SkiaSharp.SKRect ToSKRect (this CoreGraphics.CGRect rect);
	public static SkiaSharp.SKSize ToSKSize (this CoreGraphics.CGSize size);
	public static CoreGraphics.CGSize ToSize (this SkiaSharp.SKSize size);
}
```

#### New Type: SkiaSharp.Views.watchOS.Extensions

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

#### New Type: SkiaSharp.Views.watchOS.ISKCanvasLayerDelegate

```csharp
public interface ISKCanvasLayerDelegate {
	// methods
	public virtual void DrawInSurface (SkiaSharp.SKSurface surface, SkiaSharp.SKImageInfo info);
}
```

#### New Type: SkiaSharp.Views.watchOS.SKPaintSurfaceEventArgs

```csharp
public class SKPaintSurfaceEventArgs : System.EventArgs {
	// constructors
	public SKPaintSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.SKImageInfo info);
	// properties
	public SkiaSharp.SKImageInfo Info { get; }
	public SkiaSharp.SKSurface Surface { get; }
}
```

#### New Type: SkiaSharp.Views.watchOS.iOSExtensions

```csharp
public static class iOSExtensions {
	// methods
	public static SkiaSharp.SKBitmap ToSKBitmap (this UIKit.UIImage uiImage);
	public static SkiaSharp.SKColor ToSKColor (this UIKit.UIColor color);
	public static SkiaSharp.SKImage ToSKImage (this UIKit.UIImage uiImage);
	public static bool ToSKPixmap (this UIKit.UIImage uiImage, SkiaSharp.SKPixmap pixmap);
	public static UIKit.UIColor ToUIColor (this SkiaSharp.SKColor color);
	public static UIKit.UIImage ToUIImage (this SkiaSharp.SKBitmap skiaBitmap);
	public static UIKit.UIImage ToUIImage (this SkiaSharp.SKImage skiaImage);
	public static UIKit.UIImage ToUIImage (this SkiaSharp.SKPixmap skiaPixmap);
	public static UIKit.UIImage ToUIImage (this SkiaSharp.SKPicture skiaPicture, SkiaSharp.SKSizeI dimensions);
	public static UIKit.UIImage ToUIImage (this SkiaSharp.SKBitmap skiaBitmap, nfloat scale, UIKit.UIImageOrientation orientation);
	public static UIKit.UIImage ToUIImage (this SkiaSharp.SKPixmap skiaPixmap, nfloat scale, UIKit.UIImageOrientation orientation);
	public static UIKit.UIImage ToUIImage (this SkiaSharp.SKPicture skiaPicture, SkiaSharp.SKSizeI dimensions, nfloat scale, UIKit.UIImageOrientation orientation);
}
```

