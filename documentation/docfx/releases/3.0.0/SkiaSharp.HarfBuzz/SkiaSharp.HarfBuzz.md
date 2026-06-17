# API diff: SkiaSharp.HarfBuzz.dll

## SkiaSharp.HarfBuzz.dll

> Assembly Version Changed: 3.0.0.0 vs 2.88.0.0

### Namespace SkiaSharp.HarfBuzz

#### Type Changed: SkiaSharp.HarfBuzz.CanvasExtensions

Obsoleted methods:

```diff
 [Obsolete ()]
 public static void DrawShapedText (this SkiaSharp.SKCanvas canvas, string text, SkiaSharp.SKPoint p, SkiaSharp.SKPaint paint);
 [Obsolete ()]
 public static void DrawShapedText (this SkiaSharp.SKCanvas canvas, SKShaper shaper, string text, SkiaSharp.SKPoint p, SkiaSharp.SKPaint paint);
 [Obsolete ()]
 public static void DrawShapedText (this SkiaSharp.SKCanvas canvas, string text, float x, float y, SkiaSharp.SKPaint paint);
 [Obsolete ()]
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
 [Obsolete ()]
 public SKShaper.Result Shape (HarfBuzzSharp.Buffer buffer, SkiaSharp.SKPaint paint);
 [Obsolete ()]
 public SKShaper.Result Shape (string text, SkiaSharp.SKPaint paint);
 [Obsolete ()]
 public SKShaper.Result Shape (HarfBuzzSharp.Buffer buffer, float xOffset, float yOffset, SkiaSharp.SKPaint paint);
 [Obsolete ()]
 public SKShaper.Result Shape (string text, float xOffset, float yOffset, SkiaSharp.SKPaint paint);
```

Added methods:

```csharp
public SKShaper.Result Shape (HarfBuzzSharp.Buffer buffer, SkiaSharp.SKFont font);
public SKShaper.Result Shape (string text, SkiaSharp.SKFont font);
public SKShaper.Result Shape (HarfBuzzSharp.Buffer buffer, float xOffset, float yOffset, SkiaSharp.SKFont font);
public SKShaper.Result Shape (string text, float xOffset, float yOffset, SkiaSharp.SKFont font);
```



