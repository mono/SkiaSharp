# API diff: SkiaSharp.dll

## SkiaSharp.dll

> Assembly Version Changed: 1.59.0.0 vs 1.58.0.0

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.GRBackendState

Added value:

```csharp
None = 0,
```


#### Type Changed: SkiaSharp.GRContextOptions

Removed property:

```csharp
public bool ClipDrawOpsToBounds { get; set; }
```

Added property:

```csharp
public bool AvoidStencilBuffers { get; set; }
```


#### Type Changed: SkiaSharp.GRGlBackendState

Added value:

```csharp
None = 0,
```


#### Type Changed: SkiaSharp.SKBitmap

Added constructor:

```csharp
public SKBitmap (SKImageInfo info, SKColorTable ctable, SKBitmapAllocFlags flags);
```

Obsoleted methods:

```diff
 [Obsolete ()]
 public bool CopyPixelsTo (IntPtr dst, int dstSize, int dstRowBytes, bool preserveDstPad);
```


#### Type Changed: SkiaSharp.SKCanvas

Added method:

```csharp
public void DrawVertices (SKVertices vertices, SKBlendMode mode, SKPaint paint);
```


#### Type Changed: SkiaSharp.SKCodecFrameInfo

Added property:

```csharp
public SKAlphaType AlphaType { get; set; }
```


#### Type Changed: SkiaSharp.SKCodecOptions

Added property:

```csharp
public SKTransferFunctionBehavior PremulBehavior { get; set; }
```


#### Type Changed: SkiaSharp.SKColorSpace

Added property:

```csharp
public bool IsSrgb { get; }
```

Obsoleted methods:

```diff
 [Obsolete ()]
 public static SKMatrix44 ConvertPrimariesToXyzD50 (SKColorSpacePrimaries primaries);
 [Obsolete ()]
 public static bool ConvertPrimariesToXyzD50 (SKColorSpacePrimaries primaries, SKMatrix44 toXyzD50);
```


#### Type Changed: SkiaSharp.SKColorSpaceFlags

Added value:

```csharp
None = 0,
```


#### Type Changed: SkiaSharp.SKColorSpacePrimaries

Added methods:

```csharp
public SKMatrix44 ToXyzD50 ();
public bool ToXyzD50 (SKMatrix44 toXyzD50);
```


#### Type Changed: SkiaSharp.SKColorSpaceTransferFn

Added method:

```csharp
public SKColorSpaceTransferFn Invert ();
```


#### Type Changed: SkiaSharp.SKCropRectFlags

Added value:

```csharp
HasNone = 0,
```


#### Type Changed: SkiaSharp.SKPixmap

Added methods:

```csharp
public bool ReadPixels (SKPixmap pixmap);
public bool ReadPixels (SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes);
public bool ReadPixels (SKPixmap pixmap, int srcX, int srcY);
public bool ReadPixels (SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY);
```


#### Type Changed: SkiaSharp.SKSurfacePropsFlags

Added value:

```csharp
None = 0,
```


#### New Type: SkiaSharp.SKBitmapAllocFlags

```csharp
[Serializable]
[Flags]
public enum SKBitmapAllocFlags {
	None = 0,
	ZeroPixels = 1,
}
```

#### New Type: SkiaSharp.SKTransferFunctionBehavior

```csharp
[Serializable]
public enum SKTransferFunctionBehavior {
	Ignore = 1,
	Respect = 0,
}
```

#### New Type: SkiaSharp.SKVertices

```csharp
public class SKVertices : SkiaSharp.SKObject, System.IDisposable {
	// methods
	public static SKVertices CreateCopy (SKVertexMode vmode, SKPoint[] positions, SKColor[] colors);
	public static SKVertices CreateCopy (SKVertexMode vmode, SKPoint[] positions, SKPoint[] texs, SKColor[] colors);
	public static SKVertices CreateCopy (SKVertexMode vmode, SKPoint[] positions, SKPoint[] texs, SKColor[] colors, ushort[] indices);
	protected override void Dispose (bool disposing);
}
```


