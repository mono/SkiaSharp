# API diff: SkiaSharp.dll

## SkiaSharp.dll

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.GRContext

Added methods:

```csharp
public void ResetContext (GRBackendState state);
public void ResetContext (GRGlBackendState state);
public void ResetContext (uint state);
```


#### Type Changed: SkiaSharp.SKBitmap

Removed method:

```csharp
public SKColor GetIndex8Color (int x, int y);
```

Added methods:

```csharp
public static SKBitmap Decode (System.IO.Stream stream);
public static SKBitmap Decode (System.IO.Stream stream, SKImageInfo bitmapInfo);
public static SKImageInfo DecodeBounds (System.IO.Stream stream);
public static SKBitmap FromImage (SKImage image);
public SKPMColor GetIndex8Color (int x, int y);
```


#### Type Changed: SkiaSharp.SKCanvas

Added methods:

```csharp
public void DrawVertices (SKVertexMode vmode, SKPoint[] vertices, SKColor[] colors, SKPaint paint);
public void DrawVertices (SKVertexMode vmode, SKPoint[] vertices, SKPoint[] texs, SKColor[] colors, SKPaint paint);
public void DrawVertices (SKVertexMode vmode, SKPoint[] vertices, SKPoint[] texs, SKColor[] colors, ushort[] indices, SKPaint paint);
public void DrawVertices (SKVertexMode vmode, SKPoint[] vertices, SKPoint[] texs, SKColor[] colors, SKBlendMode mode, ushort[] indices, SKPaint paint);
```


#### Type Changed: SkiaSharp.SKColorTable

Added constructors:

```csharp
public SKColorTable (SKPMColor[] colors);
public SKColorTable (SKPMColor[] colors, int count);
```

Modified properties:

```diff
-public SKColor[] Colors { get; }
+public SKPMColor[] Colors { get; }
-public SKColor this [int index] { get; }
+public SKPMColor this [int index] { get; }
```

Added property:

```csharp
public SKColor[] UnPreMultipledColors { get; }
```

Added method:

```csharp
public SKColor GetUnPreMultipliedColor (int index);
```


#### Type Changed: SkiaSharp.SKData

Obsoleted constructors:

```diff
 [Obsolete ()]
 public SKData ();
 [Obsolete ()]
 public SKData (byte[] bytes);
 [Obsolete ()]
 public SKData (byte[] bytes, ulong length);
 [Obsolete ()]
 public SKData (IntPtr bytes, ulong length);
```

Added property:

```csharp
public static SKData Empty { get; }
```

Obsoleted methods:

```diff
 [Obsolete ()]
 public static SKData FromMallocMemory (IntPtr bytes, ulong length);
```

Added methods:

```csharp
public static SKData Create (IntPtr address, int length);
public static SKData Create (IntPtr address, int length, SKDataReleaseDelegate releaseProc);
public static SKData Create (IntPtr address, int length, SKDataReleaseDelegate releaseProc, object context);
public static SKData CreateCopy (byte[] bytes);
public static SKData CreateCopy (byte[] bytes, ulong length);
public static SKData CreateCopy (IntPtr bytes, ulong length);
```


#### Type Changed: SkiaSharp.SKImage

Added properties:

```csharp
public SKAlphaType AlphaType { get; }
public bool IsAlphaOnly { get; }
public bool IsTextureBacked { get; }
```

Obsoleted methods:

```diff
 [Obsolete ()]
 public static SKImage FromData (SKData data);
 [Obsolete ()]
 public static SKImage FromData (SKData data, SKRectI subset);
```

Added methods:

```csharp
public SKImage ApplyImageFilter (SKImageFilter filter, SKRectI subset, SKRectI clipBounds, out SKRectI outSubset, out SKPoint outOffset);
public static SKImage Create (SKImageInfo info);
public static SKImage FromAdoptedTexture (GRContext context, GRBackendTextureDesc desc);
public static SKImage FromAdoptedTexture (GRContext context, GRBackendTextureDesc desc, SKAlphaType alpha);
public static SKImage FromEncodedData (SKData data);
public static SKImage FromEncodedData (SKData data, SKRectI subset);
public static SKImage FromPicture (SKPicture picture, SKSizeI dimensions);
public static SKImage FromPicture (SKPicture picture, SKSizeI dimensions, SKMatrix matrix);
public static SKImage FromPicture (SKPicture picture, SKSizeI dimensions, SKPaint paint);
public static SKImage FromPicture (SKPicture picture, SKSizeI dimensions, SKMatrix matrix, SKPaint paint);
public static SKImage FromPixelCopy (SKPixmap pixmap);
public static SKImage FromPixelCopy (SKImageInfo info, IntPtr pixels);
public static SKImage FromPixelCopy (SKImageInfo info, IntPtr pixels, int rowBytes);
public static SKImage FromPixelCopy (SKImageInfo info, IntPtr pixels, int rowBytes, SKColorTable ctable);
public static SKImage FromPixelData (SKImageInfo info, SKData data, int rowBytes);
public static SKImage FromPixels (SKPixmap pixmap);
public static SKImage FromPixels (SKImageInfo info, IntPtr pixels);
public static SKImage FromPixels (SKPixmap pixmap, SKImageRasterReleaseDelegate releaseProc);
public static SKImage FromPixels (SKPixmap pixmap, SKImageRasterReleaseDelegate releaseProc, object releaseContext);
public static SKImage FromTexture (GRContext context, GRBackendTextureDesc desc);
public static SKImage FromTexture (GRContext context, GRBackendTextureDesc desc, SKAlphaType alpha);
public static SKImage FromTexture (GRContext context, GRBackendTextureDesc desc, SKAlphaType alpha, SKImageTextureReleaseDelegate releaseProc);
public static SKImage FromTexture (GRContext context, GRBackendTextureDesc desc, SKAlphaType alpha, SKImageTextureReleaseDelegate releaseProc, object releaseContext);
public SKPixmap PeekPixels ();
public bool PeekPixels (SKPixmap pixmap);
public bool ReadPixels (SKPixmap pixmap, int srcX, int srcY);
public bool ReadPixels (SKPixmap pixmap, int srcX, int srcY, SKImageCachingHint cachingHint);
public bool ReadPixels (SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY);
public bool ReadPixels (SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY, SKImageCachingHint cachingHint);
public bool ScalePixels (SKPixmap dst, SKFilterQuality quality);
public bool ScalePixels (SKPixmap dst, SKFilterQuality quality, SKImageCachingHint cachingHint);
public SKImage Subset (SKRectI subset);
public SKImage ToRasterImage ();
public SKShader ToShader (SKShaderTileMode tileX, SKShaderTileMode tileY);
public SKShader ToShader (SKShaderTileMode tileX, SKShaderTileMode tileY, SKMatrix localMatrix);
public SKImage ToTextureImage (GRContext context);
```


#### Type Changed: SkiaSharp.SKImageInfo

Modified fields:

```diff
-public readonly SKImageInfo Empty;
+public SKImageInfo Empty;
-public readonly SKColorType PlatformColorType;
+public SKColorType PlatformColorType;
```

Added fields:

```csharp
public static int PlatformColorAlphaShift;
public static int PlatformColorBlueShift;
public static int PlatformColorGreenShift;
public static int PlatformColorRedShift;
```

Added properties:

```csharp
public int BitsPerPixel { get; }
public long BytesSize64 { get; }
public long RowBytes64 { get; }
```


#### Type Changed: SkiaSharp.SKPath

Obsoleted methods:

```diff
 [Obsolete ()]
 public void AddPath (SKPath other, SKPath.AddMode mode);
 [Obsolete ()]
 public void AddPath (SKPath other, ref SKMatrix matrix, SKPath.AddMode mode);
 [Obsolete ()]
 public void AddPath (SKPath other, float dx, float dy, SKPath.AddMode mode);
```

Modified methods:

```diff
 public void AddPath (SKPath other, SKPath.AddMode mode--- = 0---)
 public void AddPath (SKPath other, ref SKMatrix matrix, SKPath.AddMode mode--- = 0---)
 public void AddPath (SKPath other, float dx, float dy, SKPath.AddMode mode--- = 0---)
```

Added methods:

```csharp
public void AddPath (SKPath other, SKPathAddMode mode);
public void AddPath (SKPath other, ref SKMatrix matrix, SKPathAddMode mode);
public void AddPath (SKPath other, float dx, float dy, SKPathAddMode mode);
```

#### Type Changed: SkiaSharp.SKPath.Iterator

Removed method:

```csharp
public SKPath.Verb Next (SKPoint[] points, bool doConsumeDegenerates, bool exact);
```

Added method:

```csharp
public SKPathVerb Next (SKPoint[] points, bool doConsumeDegenerates, bool exact);
```


#### Type Changed: SkiaSharp.SKPath.RawIterator

Removed methods:

```csharp
public SKPath.Verb Next (SKPoint[] points);
public SKPath.Verb Peek ();
```

Added methods:

```csharp
public SKPathVerb Next (SKPoint[] points);
public SKPathVerb Peek ();
```



#### Type Changed: SkiaSharp.SKPathMeasure

Obsoleted methods:

```diff
 [Obsolete ()]
 public bool GetMatrix (float distance, out SKMatrix matrix, SKPathMeasure.MatrixFlags flags);
```

Added method:

```csharp
public bool GetMatrix (float distance, out SKMatrix matrix, SKPathMeasureMatrixFlags flags);
```


#### Type Changed: SkiaSharp.SKPixmap

Added constructor:

```csharp
public SKPixmap (SKImageInfo info, IntPtr addr);
```


#### Type Changed: SkiaSharp.SKTypeface

Added method:

```csharp
public static SKTypeface FromStream (System.IO.Stream stream, int index);
```


#### New Type: SkiaSharp.GRBackendState

```csharp
[Serializable]
[Flags]
public enum GRBackendState {
	All = 4294967295,
}
```

#### New Type: SkiaSharp.GRGlBackendState

```csharp
[Serializable]
[Flags]
public enum GRGlBackendState {
	All = 65535,
	Blend = 8,
	FixedFunction = 512,
	MSAAEnable = 16,
	Misc = 1024,
	PathRendering = 2048,
	PixelStore = 128,
	Program = 256,
	RenderTarget = 1,
	Stencil = 64,
	TextureBinding = 2,
	Vertex = 32,
	View = 4,
}
```

#### New Type: SkiaSharp.SKDataReleaseDelegate

```csharp
public sealed delegate SKDataReleaseDelegate : System.MulticastDelegate, System.ICloneable, System.Runtime.Serialization.ISerializable {
	// constructors
	public SKDataReleaseDelegate (object object, IntPtr method);
	// methods
	public virtual System.IAsyncResult BeginInvoke (IntPtr address, object context, System.AsyncCallback callback, object object);
	public virtual void EndInvoke (System.IAsyncResult result);
	public virtual void Invoke (IntPtr address, object context);
}
```

#### New Type: SkiaSharp.SKFrontBufferedStream

```csharp
public class SKFrontBufferedStream : System.IO.Stream, System.IAsyncDisposable, System.IDisposable {
	// constructors
	public SKFrontBufferedStream (System.IO.Stream stream);
	public SKFrontBufferedStream (System.IO.Stream stream, bool disposeUnderlyingStream);
	public SKFrontBufferedStream (System.IO.Stream stream, long bufferSize);
	public SKFrontBufferedStream (System.IO.Stream stream, long bufferSize, bool disposeUnderlyingStream);
	// fields
	public static const int DefaultBufferSize;
	// properties
	public override bool CanRead { get; }
	public override bool CanSeek { get; }
	public override bool CanWrite { get; }
	public override long Length { get; }
	public override long Position { get; set; }
	// methods
	protected override void Dispose (bool disposing);
	public override void Flush ();
	public override int Read (byte[] buffer, int offset, int count);
	public override long Seek (long offset, System.IO.SeekOrigin origin);
	public override void SetLength (long value);
	public override void Write (byte[] buffer, int offset, int count);
}
```

#### New Type: SkiaSharp.SKImageCachingHint

```csharp
[Serializable]
public enum SKImageCachingHint {
	Allow = 0,
	Disallow = 1,
}
```

#### New Type: SkiaSharp.SKImageRasterReleaseDelegate

```csharp
public sealed delegate SKImageRasterReleaseDelegate : System.MulticastDelegate, System.ICloneable, System.Runtime.Serialization.ISerializable {
	// constructors
	public SKImageRasterReleaseDelegate (object object, IntPtr method);
	// methods
	public virtual System.IAsyncResult BeginInvoke (IntPtr pixels, object context, System.AsyncCallback callback, object object);
	public virtual void EndInvoke (System.IAsyncResult result);
	public virtual void Invoke (IntPtr pixels, object context);
}
```

#### New Type: SkiaSharp.SKImageTextureReleaseDelegate

```csharp
public sealed delegate SKImageTextureReleaseDelegate : System.MulticastDelegate, System.ICloneable, System.Runtime.Serialization.ISerializable {
	// constructors
	public SKImageTextureReleaseDelegate (object object, IntPtr method);
	// methods
	public virtual System.IAsyncResult BeginInvoke (object context, System.AsyncCallback callback, object object);
	public virtual void EndInvoke (System.IAsyncResult result);
	public virtual void Invoke (object context);
}
```

#### New Type: SkiaSharp.SKMatrix44

```csharp
public class SKMatrix44 : SkiaSharp.SKObject, System.IDisposable {
	// constructors
	public SKMatrix44 ();
	public SKMatrix44 (SKMatrix src);
	public SKMatrix44 (SKMatrix44 src);
	public SKMatrix44 (SKMatrix44 a, SKMatrix44 b);
	// properties
	public float Item { get; set; }
	public SKMatrix Matrix { get; }
	public SKMatrix44TypeMask Type { get; }
	// methods
	public static SKMatrix44 CreateIdentity ();
	public static SKMatrix44 CreateRotation (float x, float y, float z, float radians);
	public static SKMatrix44 CreateRotationDegrees (float x, float y, float z, float degrees);
	public static SKMatrix44 CreateScale (float x, float y, float z);
	public static SKMatrix44 CreateTranslate (float x, float y, float z);
	public double Determinant ();
	protected override void Dispose (bool disposing);
	public static bool Equal (SKMatrix44 left, SKMatrix44 right);
	public static SKMatrix44 FromColumnMajor (float[] src);
	public static SKMatrix44 FromRowMajor (float[] src);
	public SKMatrix44 Invert ();
	public bool Invert (SKMatrix44 inverse);
	public SKPoint MapPoint (SKPoint src);
	public SKPoint[] MapPoints (SKPoint[] src);
	public float[] MapScalars (float[] srcVector4);
	public void MapScalars (float[] srcVector4, float[] dstVector4);
	public float[] MapScalars (float x, float y, float z, float w);
	public float[] MapVector2 (float[] src2);
	public void MapVector2 (float[] src2, float[] dst4);
	public void PostConcat (SKMatrix44 m);
	public void PostScale (float sx, float sy, float sz);
	public void PostTranslate (float dx, float dy, float dz);
	public void PreConcat (SKMatrix44 m);
	public void PreScale (float sx, float sy, float sz);
	public void PreTranslate (float dx, float dy, float dz);
	public bool Preserves2DAxisAlignment (float epsilon);
	public void SetColumnMajor (float[] src);
	public void SetConcat (SKMatrix44 a, SKMatrix44 b);
	public void SetIdentity ();
	public void SetRotationAbout (float x, float y, float z, float radians);
	public void SetRotationAboutDegrees (float x, float y, float z, float degrees);
	public void SetRotationAboutUnit (float x, float y, float z, float radians);
	public void SetRowMajor (float[] src);
	public void SetScale (float sx, float sy, float sz);
	public void SetTranslate (float dx, float dy, float dz);
	public float[] ToColumnMajor ();
	public void ToColumnMajor (float[] dst);
	public float[] ToRowMajor ();
	public void ToRowMajor (float[] dst);
	public void Transpose ();
}
```

#### New Type: SkiaSharp.SKMatrix44TypeMask

```csharp
[Serializable]
[Flags]
public enum SKMatrix44TypeMask {
	Affine = 4,
	Identity = 0,
	Perspective = 8,
	Scale = 2,
	Translate = 1,
}
```

#### New Type: SkiaSharp.SKPMColor

```csharp
public struct SKPMColor {
	// constructors
	public SKPMColor (uint value);
	// properties
	public byte Alpha { get; }
	public byte Blue { get; }
	public byte Green { get; }
	public byte Red { get; }
	// methods
	public override bool Equals (object other);
	public override int GetHashCode ();
	public static SKPMColor PreMultiply (SKColor color);
	public static SKPMColor[] PreMultiply (SKColor[] colors);
	public override string ToString ();
	public static SKColor UnPreMultiply (SKPMColor pmcolor);
	public static SKColor[] UnPreMultiply (SKPMColor[] pmcolors);
	public static bool op_Equality (SKPMColor left, SKPMColor right);
	public static SKPMColor op_Explicit (SKColor color);
	public static SKColor op_Explicit (SKPMColor color);
	public static uint op_Explicit (SKPMColor color);
	public static SKPMColor op_Implicit (uint color);
	public static bool op_Inequality (SKPMColor left, SKPMColor right);
}
```

#### New Type: SkiaSharp.SKPathAddMode

```csharp
[Serializable]
public enum SKPathAddMode {
	Append = 0,
	Extend = 1,
}
```

#### New Type: SkiaSharp.SKPathMeasureMatrixFlags

```csharp
[Serializable]
[Flags]
public enum SKPathMeasureMatrixFlags {
	GetPosition = 1,
	GetPositionAndTangent = 3,
	GetTangent = 2,
}
```

#### New Type: SkiaSharp.SKPathVerb

```csharp
[Serializable]
public enum SKPathVerb {
	Close = 5,
	Conic = 3,
	Cubic = 4,
	Done = 6,
	Line = 1,
	Move = 0,
	Quad = 2,
}
```

#### New Type: SkiaSharp.SKVertexMode

```csharp
[Serializable]
public enum SKVertexMode {
	TriangleFan = 2,
	TriangleStrip = 1,
	Triangles = 0,
}
```


