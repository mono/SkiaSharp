# API diff: SkiaSharp.HarfBuzz.dll

## SkiaSharp.HarfBuzz.dll

> Assembly Version Changed: 4.147.0.0 vs 3.119.0.0

### Namespace SkiaSharp.HarfBuzz

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


