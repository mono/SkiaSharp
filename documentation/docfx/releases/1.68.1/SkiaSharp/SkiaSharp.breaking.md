# API diff: SkiaSharp.dll

## SkiaSharp.dll

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.SKColorSpace

Modified methods:

```diff
 public SKColorSpace CreateRgb (SKColorSpaceRenderTargetGamma gamma, SKColorSpaceGamut gamut, SKColorSpaceFlags flags--- = 0---)
 public SKColorSpace CreateRgb (SKColorSpaceRenderTargetGamma gamma, SKMatrix44 toXyzD50, SKColorSpaceFlags flags--- = 0---)
 public SKColorSpace CreateRgb (SKColorSpaceTransferFn coeffs, SKColorSpaceGamut gamut, SKColorSpaceFlags flags--- = 0---)
 public SKColorSpace CreateRgb (SKColorSpaceTransferFn coeffs, SKMatrix44 toXyzD50, SKColorSpaceFlags flags--- = 0---)
```


#### Type Changed: SkiaSharp.SKObject

Removed property:

```csharp
protected bool OwnsHandle { get; }
```


#### Type Changed: SkiaSharp.SKPath

#### Type Changed: SkiaSharp.SKPath.Iterator

Modified base type:

```diff
-SkiaSharp.SKNativeObject
+SkiaSharp.SKObject
```


#### Type Changed: SkiaSharp.SKPath.OpBuilder

Modified base type:

```diff
-SkiaSharp.SKNativeObject
+SkiaSharp.SKObject
```


#### Type Changed: SkiaSharp.SKPath.RawIterator

Modified base type:

```diff
-SkiaSharp.SKNativeObject
+SkiaSharp.SKObject
```



#### Type Changed: SkiaSharp.SKTextBlobBuilder

Modified methods:

```diff
-public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, byte[] utf8Text, uint[] clusters)
+public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, byte[] text, uint[] clusters)
-public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, byte[] utf8Text, uint[] clusters, SKRect bounds)
+public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, byte[] text, uint[] clusters, SKRect bounds)
-public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, byte[] utf8Text, uint[] clusters)
+public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, byte[] text, uint[] clusters)
-public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, byte[] utf8Text, uint[] clusters, SKRect bounds)
+public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, byte[] text, uint[] clusters, SKRect bounds)
-public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, byte[] utf8Text, uint[] clusters)
+public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, byte[] text, uint[] clusters)
-public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, byte[] utf8Text, uint[] clusters, SKRect bounds)
+public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, byte[] text, uint[] clusters, SKRect bounds)
```



