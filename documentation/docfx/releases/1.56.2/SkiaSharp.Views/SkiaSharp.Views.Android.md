# API diff: SkiaSharp.Views.Android.dll

## SkiaSharp.Views.Android.dll

### Namespace SkiaSharp.Views.Android

#### Type Changed: SkiaSharp.Views.Android.AndroidExtensions

Added methods:

```csharp
public static Android.Graphics.Bitmap ToBitmap (this SkiaSharp.SKBitmap skiaBitmap);
public static Android.Graphics.Bitmap ToBitmap (this SkiaSharp.SKImage skiaImage);
public static Android.Graphics.Bitmap ToBitmap (this SkiaSharp.SKPixmap skiaPixmap);
public static Android.Graphics.Bitmap ToBitmap (this SkiaSharp.SKPicture skiaPicture, SkiaSharp.SKSizeI dimensions);
public static SkiaSharp.SKBitmap ToSKBitmap (this Android.Graphics.Bitmap bitmap);
public static SkiaSharp.SKImage ToSKImage (this Android.Graphics.Bitmap bitmap);
public static void ToSKPixmap (this Android.Graphics.Bitmap bitmap, SkiaSharp.SKPixmap pixmap);
```



