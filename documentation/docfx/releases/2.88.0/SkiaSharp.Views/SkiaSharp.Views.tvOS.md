# API diff: SkiaSharp.Views.tvOS.dll

## SkiaSharp.Views.tvOS.dll

> Assembly Version Changed: 2.88.0.0 vs 2.80.0.0

### Namespace SkiaSharp.Views.tvOS

#### Type Changed: SkiaSharp.Views.tvOS.SKCanvasView

Added interface:

```csharp
UIKit.IUITraitChangeObservable
```


#### Type Changed: SkiaSharp.Views.tvOS.SKGLView

Added interface:

```csharp
UIKit.IUITraitChangeObservable
```


#### Type Changed: SkiaSharp.Views.tvOS.SKPaintGLSurfaceEventArgs

Obsoleted constructors:

```diff
 [Obsolete ("Use SKPaintGLSurfaceEventArgs(SKSurface, GRBackendRenderTarget, GRSurfaceOrigin, SKColorType) instead.")]
 public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTarget renderTarget, SkiaSharp.GRSurfaceOrigin origin, SkiaSharp.SKColorType colorType, SkiaSharp.GRGlFramebufferInfo glInfo);
```

Added constructors:

```csharp
public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTarget renderTarget, SkiaSharp.GRSurfaceOrigin origin, SkiaSharp.SKImageInfo info);
public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTarget renderTarget, SkiaSharp.GRSurfaceOrigin origin, SkiaSharp.SKImageInfo info, SkiaSharp.SKImageInfo rawInfo);
```

Added properties:

```csharp
public SkiaSharp.SKImageInfo Info { get; }
public SkiaSharp.SKImageInfo RawInfo { get; }
```


#### Type Changed: SkiaSharp.Views.tvOS.SKPaintSurfaceEventArgs

Added constructor:

```csharp
public SKPaintSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.SKImageInfo info, SkiaSharp.SKImageInfo rawInfo);
```

Added property:

```csharp
public SkiaSharp.SKImageInfo RawInfo { get; }
```


#### Type Changed: SkiaSharp.Views.tvOS.iOSExtensions

Removed methods:

```csharp
public static UIKit.UIImage ToUIImage (this SkiaSharp.SKBitmap skiaBitmap, nfloat scale, UIKit.UIImageOrientation orientation);
public static UIKit.UIImage ToUIImage (this SkiaSharp.SKPixmap skiaPixmap, nfloat scale, UIKit.UIImageOrientation orientation);
public static UIKit.UIImage ToUIImage (this SkiaSharp.SKPicture skiaPicture, SkiaSharp.SKSizeI dimensions, nfloat scale, UIKit.UIImageOrientation orientation);
```

Added methods:

```csharp
public static UIKit.UIImage ToUIImage (this SkiaSharp.SKBitmap skiaBitmap, System.Runtime.InteropServices.NFloat scale, UIKit.UIImageOrientation orientation);
public static UIKit.UIImage ToUIImage (this SkiaSharp.SKPixmap skiaPixmap, System.Runtime.InteropServices.NFloat scale, UIKit.UIImageOrientation orientation);
public static UIKit.UIImage ToUIImage (this SkiaSharp.SKPicture skiaPicture, SkiaSharp.SKSizeI dimensions, System.Runtime.InteropServices.NFloat scale, UIKit.UIImageOrientation orientation);
```



