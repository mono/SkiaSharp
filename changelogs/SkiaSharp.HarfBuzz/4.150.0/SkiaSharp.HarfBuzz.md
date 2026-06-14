# API diff: SkiaSharp.HarfBuzz.dll

## SkiaSharp.HarfBuzz.dll

> Assembly Version Changed: 4.150.0.0 vs 3.119.0.0

### Namespace SkiaSharp.HarfBuzz

#### Type Changed: SkiaSharp.HarfBuzz.CanvasExtensions

Obsoleted methods:

```diff
 [Obsolete ("Use the overload with SKTextAlign parameter instead.")]
 public static void DrawShapedText (this SkiaSharp.SKCanvas canvas, string text, SkiaSharp.SKPoint p, SkiaSharp.SKFont font, SkiaSharp.SKPaint paint);
 [Obsolete ("Use the overload with SKTextAlign parameter instead.")]
 public static void DrawShapedText (this SkiaSharp.SKCanvas canvas, SKShaper shaper, string text, SkiaSharp.SKPoint p, SkiaSharp.SKFont font, SkiaSharp.SKPaint paint);
 [Obsolete ("Use the overload with SKTextAlign parameter instead.")]
 public static void DrawShapedText (this SkiaSharp.SKCanvas canvas, string text, float x, float y, SkiaSharp.SKFont font, SkiaSharp.SKPaint paint);
 [Obsolete ("Use the overload with SKTextAlign parameter instead.")]
 public static void DrawShapedText (this SkiaSharp.SKCanvas canvas, SKShaper shaper, string text, float x, float y, SkiaSharp.SKFont font, SkiaSharp.SKPaint paint);
```


#### New Type: SkiaSharp.HarfBuzz.ColorExtensions

```csharp
public static class ColorExtensions {
	// methods
	public static HarfBuzzSharp.HBColor ToHBColor (this SkiaSharp.SKColor color);
	public static HarfBuzzSharp.HBColor ToHBColor (this SkiaSharp.SKColorF color);
	public static SkiaSharp.SKColor ToSKColor (this HarfBuzzSharp.HBColor hbColor);
	public static SkiaSharp.SKColorF ToSKColorF (this HarfBuzzSharp.HBColor hbColor);
	public static SkiaSharp.SKColor[] ToSKColors (this HarfBuzzSharp.HBColor[] hbColors);
}
```


