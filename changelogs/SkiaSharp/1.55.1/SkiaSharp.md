# API diff: SkiaSharp.dll

## SkiaSharp.dll

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.GRBackendRenderTargetDesc

Added properties:

```csharp
public SKRectI Rect { get; }
public SKSizeI Size { get; }
```


#### Type Changed: SkiaSharp.SKAutoCanvasRestore

Added constructor:

```csharp
public SKAutoCanvasRestore (SKCanvas canvas);
```


#### Type Changed: SkiaSharp.SKBitmap

Added property:

```csharp
public bool ReadyToDraw { get; }
```

Added methods:

```csharp
public bool CopyPixelsTo (IntPtr dst, int dstSize, int dstRowBytes, bool preserveDstPad);
public static SKBitmap Decode (SKCodec codec, SKImageInfo bitmapInfo);
public static SKBitmap Decode (SKData data, SKImageInfo bitmapInfo);
public static SKBitmap Decode (SKStream stream, SKImageInfo bitmapInfo);
public static SKBitmap Decode (byte[] buffer, SKImageInfo bitmapInfo);
public static SKBitmap Decode (string filename, SKImageInfo bitmapInfo);
public IntPtr GetPixels ();
public bool InstallPixels (SKImageInfo info, IntPtr pixels);
public bool InstallPixels (SKImageInfo info, IntPtr pixels, int rowBytes);
public bool InstallPixels (SKImageInfo info, IntPtr pixels, int rowBytes, SKColorTable ctable);
public bool InstallPixels (SKImageInfo info, IntPtr pixels, int rowBytes, SKColorTable ctable, SKBitmapReleaseDelegate releaseProc, object context);
public void SetColorTable (SKColorTable ct);
public void SetPixels (IntPtr pixels);
public void SetPixels (IntPtr pixels, SKColorTable ct);
```


#### Type Changed: SkiaSharp.SKCanvas

Obsoleted methods:

```diff
 [Obsolete ()]
 public void DrawText (string text, SKPoint[] points, SKPaint paint);
 [Obsolete ()]
 public void DrawText (IntPtr buffer, int length, SKPoint[] points, SKPaint paint);
```

Added methods:

```csharp
protected override void Dispose (bool disposing);
public void DrawPositionedText (string text, SKPoint[] points, SKPaint paint);
public void DrawPositionedText (IntPtr buffer, int length, SKPoint[] points, SKPaint paint);
public void DrawRegion (SKRegion region, SKPaint paint);
public bool QuickReject (SKPath path);
public bool QuickReject (SKRect rect);
```


#### Type Changed: SkiaSharp.SKCodec

Added methods:

```csharp
public SKCodecResult IncrementalDecode ();
public SKCodecResult IncrementalDecode (out int rowsDecoded);
public SKCodecResult StartIncrementalDecode (SKImageInfo info, IntPtr pixels, int rowBytes);
public SKCodecResult StartIncrementalDecode (SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options);
public SKCodecResult StartIncrementalDecode (SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options, SKColorTable colorTable, ref int colorTableCount);
public SKCodecResult StartIncrementalDecode (SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options, IntPtr colorTable, ref int colorTableCount);
```


#### Type Changed: SkiaSharp.SKColor

Added constructor:

```csharp
public SKColor (uint value);
```


#### Type Changed: SkiaSharp.SKColorFilter

Added method:

```csharp
public static SKColorFilter CreateBlendMode (SKColor c, SKBlendMode mode);
```


#### Type Changed: SkiaSharp.SKColorTable

Added property:

```csharp
public SKColor Item { get; }
```


#### Type Changed: SkiaSharp.SKData

Added property:

```csharp
public bool IsEmpty { get; }
```

Added methods:

```csharp
public System.IO.Stream AsStream (bool streamDisposesData);
public byte[] ToArray ();
```


#### Type Changed: SkiaSharp.SKPath

Added properties:

```csharp
public bool IsEmpty { get; }
public int VerbCount { get; }
```


#### New Type: SkiaSharp.SKAutoLockPixels

```csharp
public class SKAutoLockPixels : System.IDisposable {
	// constructors
	public SKAutoLockPixels (SKBitmap bitmap);
	public SKAutoLockPixels (SKBitmap bitmap, bool doLock);
	// methods
	public virtual void Dispose ();
	public void Unlock ();
}
```

#### New Type: SkiaSharp.SKBitmapReleaseDelegate

```csharp
public sealed delegate SKBitmapReleaseDelegate : System.MulticastDelegate, System.ICloneable, System.Runtime.Serialization.ISerializable {
	// constructors
	public SKBitmapReleaseDelegate (object object, IntPtr method);
	// methods
	public virtual System.IAsyncResult BeginInvoke (IntPtr address, object context, System.AsyncCallback callback, object object);
	public virtual void EndInvoke (System.IAsyncResult result);
	public virtual void Invoke (IntPtr address, object context);
}
```


