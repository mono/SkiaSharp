# API diff: SkiaSharp.Views.WPF.dll

## SkiaSharp.Views.WPF.dll

### Namespace SkiaSharp.Views.WPF

#### Type Changed: SkiaSharp.Views.WPF.WPFExtensions

Added methods:

```csharp
public static SkiaSharp.SKBitmap ToSKBitmap (this System.Windows.Media.Imaging.BitmapSource bitmap);
public static SkiaSharp.SKImage ToSKImage (this System.Windows.Media.Imaging.BitmapSource bitmap);
public static void ToSKPixmap (this System.Windows.Media.Imaging.BitmapSource bitmap, SkiaSharp.SKPixmap pixmap);
public static System.Windows.Media.Imaging.WriteableBitmap ToWriteableBitmap (this SkiaSharp.SKBitmap skiaBitmap);
public static System.Windows.Media.Imaging.WriteableBitmap ToWriteableBitmap (this SkiaSharp.SKImage skiaImage);
public static System.Windows.Media.Imaging.WriteableBitmap ToWriteableBitmap (this SkiaSharp.SKPixmap pixmap);
public static System.Windows.Media.Imaging.WriteableBitmap ToWriteableBitmap (this SkiaSharp.SKPicture picture, SkiaSharp.SKSizeI dimensions);
```



