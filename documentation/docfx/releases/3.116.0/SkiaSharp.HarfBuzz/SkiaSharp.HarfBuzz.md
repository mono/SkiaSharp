# API diff: SkiaSharp.HarfBuzz.dll

## SkiaSharp.HarfBuzz.dll

> Assembly Version Changed: 3.116.0.0 vs 2.88.0.0

### Namespace SkiaSharp.HarfBuzz

#### Type Changed: SkiaSharp.HarfBuzz.CanvasExtensions

Obsoleted methods:

```diff
 [Obsolete ("Use DrawShapedText(string text, SKPoint p, SKTextAlign textAlign, SKFont font, SKPaint paint) instead.")]
 public static void DrawShapedText (this SkiaSharp.SKCanvas canvas, string text, SkiaSharp.SKPoint p, SkiaSharp.SKPaint paint);
 [Obsolete ("Use DrawShapedText(SKShaper shaper, string text, SKPoint p, SKTextAlign textAlign, SKFont font, SKPaint paint) instead.")]
 public static void DrawShapedText (this SkiaSharp.SKCanvas canvas, SKShaper shaper, string text, SkiaSharp.SKPoint p, SkiaSharp.SKPaint paint);
 [Obsolete ("Use DrawShapedText(string text, float x, float y, SKTextAlign textAlign, SKFont font, SKPaint paint) instead.")]
 public static void DrawShapedText (this SkiaSharp.SKCanvas canvas, string text, float x, float y, SkiaSharp.SKPaint paint);
 [Obsolete ("Use DrawShapedText(SKShaper shaper, string text, float x, float y, SKTextAlign textAlign, SKFont font, SKPaint paint) instead.")]
 public static void DrawShapedText (this SkiaSharp.SKCanvas canvas, SKShaper shaper, string text, float x, float y, SkiaSharp.SKPaint paint);
```

Added methods:

```csharp
public static void DrawShapedText (this SkiaSharp.SKCanvas canvas, string text, SkiaSharp.SKPoint p, SkiaSharp.SKFont font, SkiaSharp.SKPaint paint);
public static void DrawShapedText (this SkiaSharp.SKCanvas canvas, SKShaper shaper, string text, SkiaSharp.SKPoint p, SkiaSharp.SKFont font, SkiaSharp.SKPaint paint);
public static void DrawShapedText (this SkiaSharp.SKCanvas canvas, string text, SkiaSharp.SKPoint p, SkiaSharp.SKTextAlign textAlign, SkiaSharp.SKFont font, SkiaSharp.SKPaint paint);
public static void DrawShapedText (this SkiaSharp.SKCanvas canvas, string text, float x, float y, SkiaSharp.SKFont font, SkiaSharp.SKPaint paint);
public static void DrawShapedText (this SkiaSharp.SKCanvas canvas, SKShaper shaper, string text, SkiaSharp.SKPoint p, SkiaSharp.SKTextAlign textAlign, SkiaSharp.SKFont font, SkiaSharp.SKPaint paint);
public static void DrawShapedText (this SkiaSharp.SKCanvas canvas, SKShaper shaper, string text, float x, float y, SkiaSharp.SKFont font, SkiaSharp.SKPaint paint);
public static void DrawShapedText (this SkiaSharp.SKCanvas canvas, string text, float x, float y, SkiaSharp.SKTextAlign textAlign, SkiaSharp.SKFont font, SkiaSharp.SKPaint paint);
public static void DrawShapedText (this SkiaSharp.SKCanvas canvas, SKShaper shaper, string text, float x, float y, SkiaSharp.SKTextAlign textAlign, SkiaSharp.SKFont font, SkiaSharp.SKPaint paint);
```


#### Type Changed: SkiaSharp.HarfBuzz.SKShaper

Obsoleted methods:

```diff
 [Obsolete ("Use Shape(Buffer buffer, SKFont font) instead.")]
 public SKShaper.Result Shape (HarfBuzzSharp.Buffer buffer, SkiaSharp.SKPaint paint);
 [Obsolete ("Use Shape(string text, SKFont font) instead.")]
 public SKShaper.Result Shape (string text, SkiaSharp.SKPaint paint);
 [Obsolete ("Use Shape(Buffer buffer, float xOffset, float yOffset, SKFont font) instead.")]
 public SKShaper.Result Shape (HarfBuzzSharp.Buffer buffer, float xOffset, float yOffset, SkiaSharp.SKPaint paint);
 [Obsolete ("Use Shape(string text, float xOffset, float yOffset, SKFont font) instead.")]
 public SKShaper.Result Shape (string text, float xOffset, float yOffset, SkiaSharp.SKPaint paint);
```

Added methods:

```csharp
public SKShaper.Result Shape (HarfBuzzSharp.Buffer buffer, SkiaSharp.SKFont font);
public SKShaper.Result Shape (string text, SkiaSharp.SKFont font);
public SKShaper.Result Shape (HarfBuzzSharp.Buffer buffer, float xOffset, float yOffset, SkiaSharp.SKFont font);
public SKShaper.Result Shape (string text, float xOffset, float yOffset, SkiaSharp.SKFont font);
```



