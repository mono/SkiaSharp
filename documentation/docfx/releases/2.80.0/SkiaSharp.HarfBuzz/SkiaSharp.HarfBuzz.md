# API diff: SkiaSharp.HarfBuzz.dll

## SkiaSharp.HarfBuzz.dll

> Assembly Version Changed: 2.80.0.0 vs 1.68.0.0

### Namespace SkiaSharp.HarfBuzz

#### Type Changed: SkiaSharp.HarfBuzz.CanvasExtensions

Added methods:

```csharp
public static void DrawShapedText (this SkiaSharp.SKCanvas canvas, string text, SkiaSharp.SKPoint p, SkiaSharp.SKPaint paint);
public static void DrawShapedText (this SkiaSharp.SKCanvas canvas, SKShaper shaper, string text, SkiaSharp.SKPoint p, SkiaSharp.SKPaint paint);
public static void DrawShapedText (this SkiaSharp.SKCanvas canvas, string text, float x, float y, SkiaSharp.SKPaint paint);
```



