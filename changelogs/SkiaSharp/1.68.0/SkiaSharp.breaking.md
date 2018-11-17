# API diff: SkiaSharp.dll

## SkiaSharp.dll

> Assembly Version Changed: 1.68.0.0 vs 1.60.0.0

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.GRBackend

Modified fields:

```diff
-OpenGL = 0
+OpenGL = 1
-Vulkan = 1
+Vulkan = 2
```


#### Type Changed: SkiaSharp.GRContext

Removed methods:

```csharp
public static GRContext Create (GRBackend backend, GRGlInterface backendContext, GRContextOptions options);
public static GRContext Create (GRBackend backend, IntPtr backendContext, GRContextOptions options);
```


#### Type Changed: SkiaSharp.GRGlInterface

Removed method:

```csharp
public GRGlInterface Clone ();
```


#### Type Changed: SkiaSharp.GRPixelConfig

Removed value:

```csharp
Rgba8888SInt = 9,
```

Modified fields:

```diff
-AlphaHalf = 12
+AlphaHalf = 13
-Bgra8888 = 6
+Bgra8888 = 7
-RgFloat = 11
+RgFloat = 12
-RgbaFloat = 10
+RgbaFloat = 11
-RgbaHalf = 13
+RgbaHalf = 14
-Sbgra8888 = 8
+Sbgra8888 = 9
-Srgba8888 = 7
+Srgba8888 = 8
```


#### Type Changed: SkiaSharp.GRSurfaceOrigin

Modified fields:

```diff
-BottomLeft = 2
+BottomLeft = 1
-TopLeft = 1
+TopLeft = 0
```


#### Type Changed: SkiaSharp.SKCodec

Removed property:

```csharp
public SKEncodedInfo EncodedInfo { get; }
```


#### Type Changed: SkiaSharp.SKCodecOptions

Removed constructor:

```csharp
public SKCodecOptions (int frameIndex, bool hasPriorFrame);
```

Removed property:

```csharp
public bool HasPriorFrame { get; set; }
```


#### Type Changed: SkiaSharp.SKCodecResult

Modified fields:

```diff
-CouldNotRewind = 6
+CouldNotRewind = 7
-InvalidConversion = 2
+InvalidConversion = 3
-InvalidInput = 5
+InvalidInput = 6
-InvalidParameters = 4
+InvalidParameters = 5
-InvalidScale = 3
+InvalidScale = 4
-Unimplemented = 7
+Unimplemented = 9
```


#### Type Changed: SkiaSharp.SKColorType

Removed value:

```csharp
Index8 = 6,
```

Modified fields:

```diff
-Bgra8888 = 5
+Bgra8888 = 6
-Gray8 = 7
+Gray8 = 9
-RgbaF16 = 8
+RgbaF16 = 10
```


#### Type Changed: SkiaSharp.SKDocument

Modified methods:

```diff
 public SKDocument CreatePdf (SKWStream stream, float dpi--- = 72---)
 public SKDocument CreatePdf (string path, float dpi--- = 72---)
 public SKDocument CreatePdf (SKWStream stream, SKDocumentPdfMetadata metadata, float dpi--- = 72---)
 public SKDocument CreateXps (SKWStream stream, float dpi--- = 72---)
 public SKDocument CreateXps (string path, float dpi--- = 72---)
```


#### Type Changed: SkiaSharp.SKImageFilter

Removed method:

```csharp
public static SKImageFilter CreatePictureForLocalspace (SKPicture picture, SKRect cropRect, SKFilterQuality filterQuality);
```

Modified methods:

```diff
 public SKImageFilter CreateMerge (SKImageFilter[] filters, SKBlendMode[] modes--- = NULL---, SKImageFilter.CropRect cropRect = NULL)
```


#### Type Changed: SkiaSharp.SKLattice

Removed property:

```csharp
public SKLatticeFlags[] Flags { get; set; }
```


#### Type Changed: SkiaSharp.SKManagedPixelSerializer

Removed methods:

```csharp
protected virtual SKData OnEncode (SKPixmap pixmap);
protected virtual bool OnUseEncodedData (IntPtr data, IntPtr length);
```


#### Type Changed: SkiaSharp.SKPaint

Modified methods:

```diff
 public bool GetFillPath (SKPath src, SKPath dst, float resScale--- = 1---)
 public bool GetFillPath (SKPath src, SKPath dst, SKRect cullRect, float resScale--- = 1---)
```


#### Type Changed: SkiaSharp.SKPathEffect

Removed method:

```csharp
public static SKPathEffect CreateArcTo (float radius);
```


#### Type Changed: SkiaSharp.SKPixmap

Modified constructors:

```diff
 public SKPixmap (SKImageInfo info, IntPtr addr, int rowBytes, SKColorTable ctable--- = NULL---)
```

Modified methods:

```diff
 public void Reset (SKImageInfo info, IntPtr addr, int rowBytes, SKColorTable ctable--- = NULL---)
```


#### Type Changed: SkiaSharp.SKTypeface

Modified methods:

```diff
 public SKTypeface FromFamilyName (string familyName, SKTypefaceStyle style--- = 0---)
 public SKTypeface FromTypeface (SKTypeface typeface, SKTypefaceStyle style--- = 0---)
```


#### Removed Type SkiaSharp.GRContextOptions
#### Removed Type SkiaSharp.GRContextOptionsGpuPathRenderers
#### Removed Type SkiaSharp.SKEncodedInfo
#### Removed Type SkiaSharp.SKEncodedInfoAlpha
#### Removed Type SkiaSharp.SKEncodedInfoColor
#### Removed Type SkiaSharp.SKLatticeFlags

