# API diff: SkiaSharp.Views.Desktop.Common.dll

## SkiaSharp.Views.Desktop.Common.dll

### Namespace SkiaSharp.Views.Desktop

#### Type Changed: SkiaSharp.Views.Desktop.Extensions

Added methods:

```csharp
public static System.Drawing.Bitmap ToBitmap (this SkiaSharp.SKBitmap skiaBitmap);
public static System.Drawing.Bitmap ToBitmap (this SkiaSharp.SKImage skiaImage);
public static System.Drawing.Bitmap ToBitmap (this SkiaSharp.SKPixmap pixmap);
public static System.Drawing.Bitmap ToBitmap (this SkiaSharp.SKPicture picture, SkiaSharp.SKSizeI dimensions);
public static SkiaSharp.SKBitmap ToSKBitmap (this System.Drawing.Bitmap bitmap);
public static SkiaSharp.SKImage ToSKImage (this System.Drawing.Bitmap bitmap);
public static void ToSKPixmap (this System.Drawing.Bitmap bitmap, SkiaSharp.SKPixmap pixmap);
```



