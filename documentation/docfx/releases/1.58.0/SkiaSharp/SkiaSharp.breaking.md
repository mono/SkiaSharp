# API diff: SkiaSharp.dll

## SkiaSharp.dll

> Assembly Version Changed: 1.58.0.0 vs 1.57.0.0

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.GRContextOptions

Removed properties:

```csharp
public bool ClipBatchToBounds { get; set; }
public bool DrawBatchBounds { get; set; }
public bool ForceSWPathMasks { get; set; }
public int MaxBatchLookahead { get; set; }
public int MaxBatchLookback { get; set; }
```


#### Type Changed: SkiaSharp.GRPixelConfig

Removed values:

```csharp
Astc12x12 = 12,
Latc = 10,
R11Eac = 11,
```

Modified fields:

```diff
-AlphaHalf = 14
+AlphaHalf = 12
-RgbaFloat = 13
+RgbaFloat = 10
-RgbaHalf = 15
+RgbaHalf = 13
```


#### Type Changed: SkiaSharp.SKColorFilter

Removed fields:

```csharp
public static const int MaxColorCubeDimension;
public static const int MinColorCubeDimension;
```

Removed methods:

```csharp
public static SKColorFilter CreateColorCube (SKData cubeData, int cubeDimension);
public static SKColorFilter CreateColorCube (byte[] cubeData, int cubeDimension);
public static SKColorFilter CreateGamma (float gamma);
public static bool IsValid3DColorCube (SKData cubeData, int cubeDimension);
```


#### Type Changed: SkiaSharp.SKImageEncodeFormat

Modified fields:

```diff
-Bmp = 0
+Bmp = 1
-Gif = 1
+Gif = 2
-Ico = 2
+Ico = 3
-Jpeg = 3
+Jpeg = 4
-Png = 4
+Png = 5
-Wbmp = 5
+Wbmp = 6
-Webp = 6
+Webp = 7
```


#### Type Changed: SkiaSharp.SKImageFilter

Removed method:

```csharp
public static SKImageFilter CreateMagnifier (SKRect src, float inset, SKImageFilter input);
```


#### Type Changed: SkiaSharp.SKPaint

Removed properties:

```csharp
public bool StrikeThruText { get; set; }
public bool UnderlineText { get; set; }
```



