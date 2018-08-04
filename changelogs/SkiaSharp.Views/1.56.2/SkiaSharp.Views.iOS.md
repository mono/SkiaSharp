# API diff: SkiaSharp.Views.iOS.dll

## SkiaSharp.Views.iOS.dll

### Namespace SkiaSharp.Views.iOS

#### Type Changed: SkiaSharp.Views.iOS.AppleExtensions

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


#### Type Changed: SkiaSharp.Views.iOS.iOSExtensions

Added methods:

```csharp
public static SkiaSharp.SKBitmap ToSKBitmap (this UIKit.UIImage uiImage);
public static SkiaSharp.SKImage ToSKImage (this UIKit.UIImage uiImage);
public static bool ToSKPixmap (this UIKit.UIImage uiImage, SkiaSharp.SKPixmap pixmap);
public static UIKit.UIImage ToUIImage (this SkiaSharp.SKBitmap skiaBitmap);
public static UIKit.UIImage ToUIImage (this SkiaSharp.SKImage skiaImage);
public static UIKit.UIImage ToUIImage (this SkiaSharp.SKPixmap skiaPixmap);
public static UIKit.UIImage ToUIImage (this SkiaSharp.SKPicture skiaPicture, SkiaSharp.SKSizeI dimensions);
```



