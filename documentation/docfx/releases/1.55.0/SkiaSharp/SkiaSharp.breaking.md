# API diff: SkiaSharp.dll

## SkiaSharp.dll

> Assembly Version Changed: 1.55.0.0 vs 1.54.0.0

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.GRContext

Modified methods:

```diff
 public void Flush (GRContextFlushBits flagsBitfield--- = 0---)
```


#### Type Changed: SkiaSharp.SKCanvas

Modified methods:

```diff
 public void ClipPath (SKPath path, SKRegionOperation operation--- = 1---, bool antialias = false)
 public void ClipRect (SKRect rect, SKRegionOperation operation--- = 1---, bool antialias = false)
 public void DrawColor (SKColor color, SKXferMode mode--- = 1---)
```


#### Type Changed: SkiaSharp.SKCodec

Removed method:

```csharp
public void GetValidSubset (ref SKRectI desiredSubset);
```


#### Type Changed: SkiaSharp.SKCodecOptions

Modified properties:

```diff
 public bool HasSubset { get; ---set;--- }
-public SKRectI Subset { get; set; }
+public SKRectI? Subset { get; set; }
```


#### Type Changed: SkiaSharp.SKDocument

Removed method:

```csharp
public bool Close ();
```


#### Type Changed: SkiaSharp.SKMatrix

Modified methods:

```diff
-static public void MapRect (ref SKMatrix matrix, out SKRect dest, ref SKRect source)
+ public void MapRect (ref SKMatrix matrix, out SKRect dest, ref SKRect source)
```


#### Type Changed: SkiaSharp.SKPictureRecorder

Removed constructor:

```csharp
public SKPictureRecorder (IntPtr handle, bool owns);
```



