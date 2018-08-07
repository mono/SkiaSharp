# API diff: SkiaSharp.Views.UWP.dll

## SkiaSharp.Views.UWP.dll

### Namespace SkiaSharp.Views.UWP

#### Type Changed: SkiaSharp.Views.UWP.UWPExtensions

Added methods:

```csharp
public static SkiaSharp.SKBitmap ToSKBitmap (this Windows.UI.Xaml.Media.Imaging.WriteableBitmap bitmap);
public static SkiaSharp.SKImage ToSKImage (this Windows.UI.Xaml.Media.Imaging.WriteableBitmap bitmap);
public static bool ToSKPixmap (this Windows.UI.Xaml.Media.Imaging.WriteableBitmap bitmap, SkiaSharp.SKPixmap pixmap);
public static Windows.UI.Xaml.Media.Imaging.WriteableBitmap ToWriteableBitmap (this SkiaSharp.SKBitmap skiaBitmap);
public static Windows.UI.Xaml.Media.Imaging.WriteableBitmap ToWriteableBitmap (this SkiaSharp.SKImage skiaImage);
public static Windows.UI.Xaml.Media.Imaging.WriteableBitmap ToWriteableBitmap (this SkiaSharp.SKPixmap pixmap);
public static Windows.UI.Xaml.Media.Imaging.WriteableBitmap ToWriteableBitmap (this SkiaSharp.SKPicture picture, SkiaSharp.SKSizeI dimensions);
```



