# API diff: SkiaSharp.dll

## SkiaSharp.dll

> Assembly Version Changed: 1.60.0.0 vs 1.59.0.0

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.GRContext

Removed method:

```csharp
[Obsolete]
public void Flush (GRContextFlushBits flagsBitfield);
```


#### Type Changed: SkiaSharp.GRContextOptions

Removed properties:

```csharp
public int MaxOpCombineLookahead { get; set; }
public int MaxOpCombineLookback { get; set; }
```


#### Type Changed: SkiaSharp.GRContextOptionsGpuPathRenderers

Removed values:

```csharp
DistanceField = 128,
Pls = 64,
```

Modified fields:

```diff
-All = 1023
+All = 511
-Default = 512
+Default = 256
-Tessellating = 256
+Tessellating = 128
```


#### Type Changed: SkiaSharp.GRGlInterface

Removed method:

```csharp
[Obsolete]
public static GRGlInterface CreateNativeInterface ();
```


#### Type Changed: SkiaSharp.GRPixelConfig

Removed value:

```csharp
Etc1 = 9,
```


#### Type Changed: SkiaSharp.GRSurfaceOrigin

Modified fields:

```diff
-BottomLeft = 1
+BottomLeft = 2
-TopLeft = 0
+TopLeft = 1
```


#### Type Changed: SkiaSharp.SKBitmap

Removed methods:

```csharp
[Obsolete]
public bool CopyPixelsTo (IntPtr dst, int dstSize, int dstRowBytes, bool preserveDstPad);
public void LockPixels ();
public void UnlockPixels ();
```


#### Type Changed: SkiaSharp.SKCanvas

Removed methods:

```csharp
[Obsolete]
public void ClipPath (SKPath path, SKRegionOperation operation, bool antialias);

[Obsolete]
public void ClipRect (SKRect rect, SKRegionOperation operation, bool antialias);

[Obsolete]
public void DrawColor (SKColor color, SKXferMode mode);

[Obsolete]
public void DrawText (string text, SKPoint[] points, SKPaint paint);

[Obsolete]
public void DrawText (IntPtr buffer, int length, SKPoint[] points, SKPaint paint);

[Obsolete]
public void DrawText (byte[] text, SKPath path, float hOffset, float vOffset, SKPaint paint);

[Obsolete]
public void DrawText (string text, SKPath path, float hOffset, float vOffset, SKPaint paint);

[Obsolete]
public void DrawText (IntPtr buffer, int length, SKPath path, float hOffset, float vOffset, SKPaint paint);

[Obsolete]
public bool GetClipBounds (ref SKRect bounds);

[Obsolete]
public bool GetClipDeviceBounds (ref SKRectI bounds);
```


#### Type Changed: SkiaSharp.SKColorFilter

Removed methods:

```csharp
[Obsolete]
public static SKColorFilter CreateBlendMode (SKColor c, SKXferMode mode);

[Obsolete]
public static SKColorFilter CreateXferMode (SKColor c, SKXferMode mode);
```


#### Type Changed: SkiaSharp.SKColorSpace

Removed methods:

```csharp
[Obsolete]
public static SKMatrix44 ConvertPrimariesToXyzD50 (SKColorSpacePrimaries primaries);

[Obsolete]
public static bool ConvertPrimariesToXyzD50 (SKColorSpacePrimaries primaries, SKMatrix44 toXyzD50);
```


#### Type Changed: SkiaSharp.SKData

Removed method:

```csharp
[Obsolete]
public static SKData FromMallocMemory (IntPtr bytes, ulong length);
```


#### Type Changed: SkiaSharp.SKDynamicMemoryWStream

Removed method:

```csharp
public void CopyTo (SKWStream dst);
```


#### Type Changed: SkiaSharp.SKImage

Removed methods:

```csharp
[Obsolete]
public SKData Encode (SKImageEncodeFormat format, int quality);

[Obsolete]
public static SKImage FromData (SKData data);

[Obsolete]
public static SKImage FromData (SKData data, SKRectI subset);
```


#### Type Changed: SkiaSharp.SKImageFilter

Removed method:

```csharp
[Obsolete]
public static SKImageFilter CreateCompose (SKDisplacementMapEffectChannelSelectorType xChannelSelector, SKDisplacementMapEffectChannelSelectorType yChannelSelector, float scale, SKImageFilter displacement, SKImageFilter input, SKImageFilter.CropRect cropRect);
```


#### Type Changed: SkiaSharp.SKManagedStream

Modified base type:

```diff
-SkiaSharp.SKStreamAsset
+SkiaSharp.SKAbstractManagedStream
```


#### Type Changed: SkiaSharp.SKManagedWStream

Modified base type:

```diff
-SkiaSharp.SKWStream
+SkiaSharp.SKAbstractManagedWStream
```


#### Type Changed: SkiaSharp.SKMatrix

Removed method:

```csharp
[Obsolete]
public SKPoint MapXY (float x, float y);
```


#### Type Changed: SkiaSharp.SKPath

Removed methods:

```csharp
[Obsolete]
public void AddPath (SKPath other, SKPath.AddMode mode);

[Obsolete]
public void AddPath (SKPath other, ref SKMatrix matrix, SKPath.AddMode mode);

[Obsolete]
public void AddPath (SKPath other, float dx, float dy, SKPath.AddMode mode);
```


#### Type Changed: SkiaSharp.SKPathEffect

Removed method:

```csharp
[Obsolete]
public static SKPathEffect Create1DPath (SKPath path, float advance, float phase, SkPath1DPathEffectStyle style);
```


#### Type Changed: SkiaSharp.SKPathMeasure

Removed method:

```csharp
[Obsolete]
public bool GetMatrix (float distance, out SKMatrix matrix, SKPathMeasure.MatrixFlags flags);
```


#### Type Changed: SkiaSharp.SKShader

Removed method:

```csharp
[Obsolete]
public static SKShader CreateCompose (SKShader shaderA, SKShader shaderB, SKXferMode mode);
```


#### Removed Type SkiaSharp.SKAutoLockPixels

