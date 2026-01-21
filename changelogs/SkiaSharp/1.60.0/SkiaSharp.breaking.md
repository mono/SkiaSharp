# API diff: SkiaSharp.dll

## SkiaSharp.dll

> Assembly Version Changed: 1.60.0.0 vs 1.59.0.0

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.GRContext

Removed method:

```csharp
[Obsolete ("Use Flush() instead.")]
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
[Obsolete ("Use CreateNativeGlInterface() or CreateDefaultInterface() instead.")]
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
[Obsolete ("Use SKPixmap.ReadPixels instead.")]
public bool CopyPixelsTo (IntPtr dst, int dstSize, int dstRowBytes, bool preserveDstPad);
public void LockPixels ();
public void UnlockPixels ();
```


#### Type Changed: SkiaSharp.SKCanvas

Removed methods:

```csharp
[Obsolete ("Use ClipPath(SKPath, SKClipOperation, bool) instead.")]
public void ClipPath (SKPath path, SKRegionOperation operation, bool antialias);

[Obsolete ("Use ClipRect(SKRect, SKClipOperation, bool) instead.")]
public void ClipRect (SKRect rect, SKRegionOperation operation, bool antialias);

[Obsolete ("Use DrawColor(SKColor, SKBlendMode) instead.")]
public void DrawColor (SKColor color, SKXferMode mode);

[Obsolete ("Use DrawPositionedText instead.")]
public void DrawText (string text, SKPoint[] points, SKPaint paint);

[Obsolete ("Use DrawPositionedText instead.")]
public void DrawText (IntPtr buffer, int length, SKPoint[] points, SKPaint paint);

[Obsolete ("Use DrawTextOnPath instead.")]
public void DrawText (byte[] text, SKPath path, float hOffset, float vOffset, SKPaint paint);

[Obsolete ("Use DrawTextOnPath instead.")]
public void DrawText (string text, SKPath path, float hOffset, float vOffset, SKPaint paint);

[Obsolete ("Use DrawTextOnPath instead.")]
public void DrawText (IntPtr buffer, int length, SKPath path, float hOffset, float vOffset, SKPaint paint);

[Obsolete ("Use GetLocalClipBounds instead.")]
public bool GetClipBounds (ref SKRect bounds);

[Obsolete ("Use GetDeviceClipBounds instead.")]
public bool GetClipDeviceBounds (ref SKRectI bounds);
```


#### Type Changed: SkiaSharp.SKColorFilter

Removed methods:

```csharp
[Obsolete ("Use CreateBlendMode(SKColor, SKBlendMode) instead.")]
public static SKColorFilter CreateBlendMode (SKColor c, SKXferMode mode);

[Obsolete ("Use CreateBlendMode(SKColor, SKBlendMode) instead.")]
public static SKColorFilter CreateXferMode (SKColor c, SKXferMode mode);
```


#### Type Changed: SkiaSharp.SKColorSpace

Removed methods:

```csharp
[Obsolete ("Use SKColorSpacePrimaries.ToXyzD50 instead.")]
public static SKMatrix44 ConvertPrimariesToXyzD50 (SKColorSpacePrimaries primaries);

[Obsolete ("Use SKColorSpacePrimaries.ToXyzD50(SKMatrix44) instead.")]
public static bool ConvertPrimariesToXyzD50 (SKColorSpacePrimaries primaries, SKMatrix44 toXyzD50);
```


#### Type Changed: SkiaSharp.SKData

Removed method:

```csharp
[Obsolete ("Not supported.")]
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
[Obsolete ("Use Encode(SKEncodedImageFormat, int) instead.")]
public SKData Encode (SKImageEncodeFormat format, int quality);

[Obsolete ("Use FromEncodedData instead.")]
public static SKImage FromData (SKData data);

[Obsolete ("Use FromEncodedData instead.")]
public static SKImage FromData (SKData data, SKRectI subset);
```


#### Type Changed: SkiaSharp.SKImageFilter

Removed method:

```csharp
[Obsolete ("Use CreateDisplacementMapEffect instead.")]
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
[Obsolete ("Use MapPoint instead.")]
public SKPoint MapXY (float x, float y);
```


#### Type Changed: SkiaSharp.SKPath

Removed methods:

```csharp
[Obsolete ("Use AddPath(SKPath, SKPathAddMode) instead.")]
public void AddPath (SKPath other, SKPath.AddMode mode);

[Obsolete ("Use AddPath(SKPath, ref SKMatrix, SKPathAddMode) instead.")]
public void AddPath (SKPath other, ref SKMatrix matrix, SKPath.AddMode mode);

[Obsolete ("Use AddPath(SKPath, float, float, SKPathAddMode) instead.")]
public void AddPath (SKPath other, float dx, float dy, SKPath.AddMode mode);
```


#### Type Changed: SkiaSharp.SKPathEffect

Removed method:

```csharp
[Obsolete ("Use Create1DPath(SKPath, float, float, SKPath1DPathEffectStyle) instead.")]
public static SKPathEffect Create1DPath (SKPath path, float advance, float phase, SkPath1DPathEffectStyle style);
```


#### Type Changed: SkiaSharp.SKPathMeasure

Removed method:

```csharp
[Obsolete ("Use GetMatrix(float, out SKMatrix, SKPathMeasureMatrixFlags) instead.")]
public bool GetMatrix (float distance, out SKMatrix matrix, SKPathMeasure.MatrixFlags flags);
```


#### Type Changed: SkiaSharp.SKShader

Removed method:

```csharp
[Obsolete ("Use CreateCompose(SKShader, SKShader, SKBlendMode) instead.")]
public static SKShader CreateCompose (SKShader shaderA, SKShader shaderB, SKXferMode mode);
```


#### Removed Type SkiaSharp.SKAutoLockPixels

