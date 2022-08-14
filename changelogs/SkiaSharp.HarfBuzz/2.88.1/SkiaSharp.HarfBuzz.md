# API diff: SkiaSharp.HarfBuzz.dll

## SkiaSharp.HarfBuzz.dll

> Assembly Version Changed: 2.88.0.0 vs 0.0.0.0

### New Namespace SkiaSharp.HarfBuzz

#### New Type: SkiaSharp.HarfBuzz.BlobExtensions

```csharp
public static class BlobExtensions {
	// methods
	public static HarfBuzzSharp.Blob ToHarfBuzzBlob (this SkiaSharp.SKStreamAsset asset);
}
```

#### New Type: SkiaSharp.HarfBuzz.CanvasExtensions

```csharp
public static class CanvasExtensions {
	// methods
	public static void DrawShapedText (this SkiaSharp.SKCanvas canvas, string text, SkiaSharp.SKPoint p, SkiaSharp.SKPaint paint);
	public static void DrawShapedText (this SkiaSharp.SKCanvas canvas, SKShaper shaper, string text, SkiaSharp.SKPoint p, SkiaSharp.SKPaint paint);
	public static void DrawShapedText (this SkiaSharp.SKCanvas canvas, string text, float x, float y, SkiaSharp.SKPaint paint);
	public static void DrawShapedText (this SkiaSharp.SKCanvas canvas, SKShaper shaper, string text, float x, float y, SkiaSharp.SKPaint paint);
}
```

#### New Type: SkiaSharp.HarfBuzz.FontExtensions

```csharp
public static class FontExtensions {
	// methods
	public static SkiaSharp.SKSizeI GetScale (this HarfBuzzSharp.Font font);
	public static void SetScale (this HarfBuzzSharp.Font font, SkiaSharp.SKSizeI scale);
}
```

#### New Type: SkiaSharp.HarfBuzz.SKShaper

```csharp
public class SKShaper : System.IDisposable {
	// constructors
	public SKShaper (SkiaSharp.SKTypeface typeface);
	// properties
	public SkiaSharp.SKTypeface Typeface { get; }
	// methods
	public virtual void Dispose ();
	public SKShaper.Result Shape (HarfBuzzSharp.Buffer buffer, SkiaSharp.SKPaint paint);
	public SKShaper.Result Shape (string text, SkiaSharp.SKPaint paint);
	public SKShaper.Result Shape (HarfBuzzSharp.Buffer buffer, float xOffset, float yOffset, SkiaSharp.SKPaint paint);
	public SKShaper.Result Shape (string text, float xOffset, float yOffset, SkiaSharp.SKPaint paint);

	// inner types
	public class Result {
		// constructors
		public SKShaper.Result ();
		public SKShaper.Result (uint[] codepoints, uint[] clusters, SkiaSharp.SKPoint[] points);
		public SKShaper.Result (uint[] codepoints, uint[] clusters, SkiaSharp.SKPoint[] points, float width);
		// properties
		public uint[] Clusters { get; }
		public uint[] Codepoints { get; }
		public SkiaSharp.SKPoint[] Points { get; }
		public float Width { get; }
	}
}
```

