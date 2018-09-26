# API diff: SkiaSharp.dll

## SkiaSharp.dll

> Assembly Version Changed: 1.54.0.0 vs 1.53.0.0

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.SKCodecOptions

Removed fields:

```csharp
public bool HasSubset;
public SKRectI Subset;
public SKZeroInitialized ZeroInitialized;
```


#### Type Changed: SkiaSharp.SKImageInfo

Removed fields:

```csharp
public SKAlphaType AlphaType;
public SKColorType ColorType;
public int Height;
public int Width;
```

Modified properties:

```diff
-public SKPointI Size { get; }
+public SKSizeI Size { get; }
```


#### Type Changed: SkiaSharp.SKRect

Removed fields:

```csharp
public float Bottom;
public float Left;
public float Right;
public float Top;
```


#### Type Changed: SkiaSharp.SKRectI

Removed fields:

```csharp
public int Bottom;
public int Left;
public int Right;
public int Top;
```


#### Type Changed: SkiaSharp.SKSize

Removed fields:

```csharp
public float Height;
public float Width;
```


#### Type Changed: SkiaSharp.SKSizeI

Removed fields:

```csharp
public int Height;
public int Width;
```


#### Type Changed: SkiaSharp.SKSurfaceProps

Removed field:

```csharp
public SKPixelGeometry PixelGeometry;
```



