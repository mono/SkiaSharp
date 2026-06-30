# API diff: SkiaSharp.dll

## SkiaSharp.dll

> Assembly Version Changed: 4.150.0.0 vs 4.148.0.0

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.SKColorFilter

Added methods:

```csharp
public static SKColorFilter CreateOverdraw (SKColor[] colors);
public static SKColorFilter CreateOverdraw (System.ReadOnlySpan<SKColor> colors);
```


#### Type Changed: SkiaSharp.SKImageFilter

Added methods:

```csharp
public static SKImageFilter CreateCrop (SKRect rect);
public static SKImageFilter CreateCrop (SKRect rect, SKShaderTileMode tileMode);
public static SKImageFilter CreateCrop (SKRect rect, SKShaderTileMode tileMode, SKImageFilter input);
public static SKImageFilter CreateEmpty ();
```


#### Type Changed: SkiaSharp.SKPaint

Added method:

```csharp
public bool GetFastBounds (SKRect bounds, out SKRect fastBounds);
```


#### Type Changed: SkiaSharp.SKSurface

Added method:

```csharp
protected override void DisposeManaged ();
```



