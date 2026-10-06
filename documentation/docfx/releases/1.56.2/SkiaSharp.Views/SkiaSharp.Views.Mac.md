# API diff: SkiaSharp.Views.Mac.dll

## SkiaSharp.Views.Mac.dll

### Namespace SkiaSharp.Views.Mac

#### Type Changed: SkiaSharp.Views.Mac.AppleExtensions

Added methods:

```csharp
public static CoreGraphics.CGImage ToCGImage (this SkiaSharp.SKBitmap skiaBitmap);
public static CoreGraphics.CGImage ToCGImage (this SkiaSharp.SKImage skiaImage);
public static CoreGraphics.CGImage ToCGImage (this SkiaSharp.SKPixmap skiaPixmap);
public static CoreGraphics.CGImage ToCGImage (this SkiaSharp.SKPicture skiaPicture, SkiaSharp.SKSizeI dimensions);
public static CoreImage.CIImage ToCIImage (this SkiaSharp.SKBitmap skiaBitmap);
public static CoreImage.CIImage ToCIImage (this SkiaSharp.SKImage skiaImage);
public static CoreImage.CIImage ToCIImage (this SkiaSharp.SKPixmap skiaPixmap);
public static CoreImage.CIImage ToCIImage (this SkiaSharp.SKPicture skiaPicture, SkiaSharp.SKSizeI dimensions);
public static Foundation.NSData ToNSData (this SkiaSharp.SKData skiaData);
public static SkiaSharp.SKBitmap ToSKBitmap (this CoreGraphics.CGImage cgImage);
public static SkiaSharp.SKBitmap ToSKBitmap (this CoreImage.CIImage ciImage);
public static SkiaSharp.SKData ToSKData (this Foundation.NSData nsData);
public static SkiaSharp.SKImage ToSKImage (this CoreGraphics.CGImage cgImage);
public static SkiaSharp.SKImage ToSKImage (this CoreImage.CIImage ciImage);
public static void ToSKPixmap (this CoreGraphics.CGImage cgImage, SkiaSharp.SKPixmap pixmap);
public static void ToSKPixmap (this CoreImage.CIImage ciImage, SkiaSharp.SKPixmap pixmap);
```


#### Type Changed: SkiaSharp.Views.Mac.MacExtensions

Added methods:

```csharp
public static AppKit.NSImage ToNSImage (this SkiaSharp.SKBitmap skiaBitmap);
public static AppKit.NSImage ToNSImage (this SkiaSharp.SKImage skiaImage);
public static AppKit.NSImage ToNSImage (this SkiaSharp.SKPixmap skiaPixmap);
public static AppKit.NSImage ToNSImage (this SkiaSharp.SKPicture skiaPicture, SkiaSharp.SKSizeI dimensions);
public static SkiaSharp.SKBitmap ToSKBitmap (this AppKit.NSImage nsImage);
public static SkiaSharp.SKImage ToSKImage (this AppKit.NSImage nsImage);
public static bool ToSKPixmap (this AppKit.NSImage nsImage, SkiaSharp.SKPixmap pixmap);
```



