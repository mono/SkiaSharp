# API diff: SkiaSharp.dll

## SkiaSharp.dll

> Assembly Version Changed: 1.57.0.0 vs 1.56.0.0

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.GRPixelConfig

Removed value:

```csharp
Index8 = 2,
```


#### Type Changed: SkiaSharp.SKCodec

Modified properties:

```diff
-public SKEncodedFormat EncodedFormat { get; }
+public SKEncodedImageFormat EncodedFormat { get; }
```


#### Type Changed: SkiaSharp.SKImage

Removed method:

```csharp
public SKImage ToTextureImage (GRContext context);
```


#### Type Changed: SkiaSharp.SKImageEncodeFormat

Removed value:

```csharp
Unknown = 0,
```

Modified fields:

```diff
-Bmp = 1
+Bmp = 0
-Gif = 2
+Gif = 1
-Ico = 3
+Ico = 2
-Jpeg = 4
+Jpeg = 3
-Png = 5
+Png = 4
-Wbmp = 6
+Wbmp = 5
-Webp = 7
+Webp = 6
```


#### Type Changed: SkiaSharp.SKMaskFilter

Removed methods:

```csharp
public static SKMaskFilter CreateEmboss (float blurSigma, SKPoint3 direction, float ambient, float specular);
public static SKMaskFilter CreateEmboss (float blurSigma, float directionX, float directionY, float directionZ, float ambient, float specular);
public static SKMaskFilter CreateShadow (float occluderHeight, SKPoint3 lightPos, float lightRadius, float ambientAlpha, float spotAlpha, SKShadowMaskFilterShadowFlags flags);
```


#### Type Changed: SkiaSharp.SKWStream

Removed method:

```csharp
public void NewLine ();
```


#### Removed Type SkiaSharp.SKEncodedFormat
#### Removed Type SkiaSharp.SKShadowMaskFilterShadowFlags

