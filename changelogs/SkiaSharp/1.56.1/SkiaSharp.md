# API diff: SkiaSharp.dll

## SkiaSharp.dll

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.SKBitmap

Added methods:

```csharp
public bool ExtractAlpha (SKBitmap destination);
public bool ExtractAlpha (SKBitmap destination, SKPaint paint);
public bool ExtractAlpha (SKBitmap destination, out SKPointI offset);
public bool ExtractAlpha (SKBitmap destination, SKPaint paint, out SKPointI offset);
public bool ExtractSubset (SKBitmap destination, SKRectI subset);
public IntPtr GetAddr (int x, int y);
public ushort GetAddr16 (int x, int y);
public uint GetAddr32 (int x, int y);
public byte GetAddr8 (int x, int y);
public bool InstallMaskPixels (SKMask mask);
```


#### Type Changed: SkiaSharp.SKCanvas

Obsoleted methods:

```diff
 [Obsolete ()]
 public void DrawText (string text, SKPath path, float hOffset, float vOffset, SKPaint paint);
 [Obsolete ()]
 public void DrawText (IntPtr buffer, int length, SKPath path, float hOffset, float vOffset, SKPaint paint);
```

Added methods:

```csharp
public void DrawAnnotation (SKRect rect, string key, SKData value);
public void DrawLinkDestinationAnnotation (SKRect rect, SKData value);
public SKData DrawLinkDestinationAnnotation (SKRect rect, string value);
public void DrawNamedDestinationAnnotation (SKPoint point, SKData value);
public SKData DrawNamedDestinationAnnotation (SKPoint point, string value);
public void DrawPositionedText (byte[] text, SKPoint[] points, SKPaint paint);
public void DrawText (byte[] text, float x, float y, SKPaint paint);

[Obsolete]
public void DrawText (byte[] text, SKPath path, float hOffset, float vOffset, SKPaint paint);
public void DrawTextOnPath (byte[] text, SKPath path, float hOffset, float vOffset, SKPaint paint);
public void DrawTextOnPath (string text, SKPath path, float hOffset, float vOffset, SKPaint paint);
public void DrawTextOnPath (IntPtr buffer, int length, SKPath path, float hOffset, float vOffset, SKPaint paint);
public void DrawUrlAnnotation (SKRect rect, SKData value);
public SKData DrawUrlAnnotation (SKRect rect, string value);
```


#### Type Changed: SkiaSharp.SKCodec

Added properties:

```csharp
public int NextScanline { get; }
public SKCodecScanlineOrder ScanlineOrder { get; }
```

Added methods:

```csharp
public int GetOutputScanline (int inputScanline);
public int GetScanlines (IntPtr dst, int countLines, int rowBytes);
public bool SkipScanlines (int countLines);
public SKCodecResult StartScanlineDecode (SKImageInfo info);
public SKCodecResult StartScanlineDecode (SKImageInfo info, SKCodecOptions options);
public SKCodecResult StartScanlineDecode (SKImageInfo info, SKCodecOptions options, SKColorTable colorTable, ref int colorTableCount);
public SKCodecResult StartScanlineDecode (SKImageInfo info, SKCodecOptions options, IntPtr colorTable, ref int colorTableCount);
```


#### Type Changed: SkiaSharp.SKData

Added constructor:

```csharp
public SKData (byte[] bytes, ulong length);
```


#### Type Changed: SkiaSharp.SKDocument

Added method:

```csharp
public static SKDocument CreatePdf (SKWStream stream, SKDocumentPdfMetadata metadata, float dpi);
```


#### Type Changed: SkiaSharp.SKMaskFilter

Added methods:

```csharp
public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma, SKBlurMaskFilterFlags flags);
public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma, SKRect occluder);
public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma, SKRect occluder, SKBlurMaskFilterFlags flags);
```


#### Type Changed: SkiaSharp.SKPaint

Added methods:

```csharp
public long BreakText (byte[] text, float maxWidth, out float measuredWidth);
public SKPath GetTextPath (byte[] text, SKPoint[] points);
public SKPath GetTextPath (byte[] text, float x, float y);
public float MeasureText (byte[] text);
public float MeasureText (byte[] text, ref SKRect bounds);
```


#### Type Changed: SkiaSharp.SKPath

Added property:

```csharp
public SKPathSegmentMask SegmentMasks { get; }
```

Added method:

```csharp
public void AddPoly (SKPoint[] points, bool close);
```


#### Type Changed: SkiaSharp.SKStrokeJoin

Obsoleted fields:

```diff
 [Obsolete ()]
 Mitter = 0,
```

Added value:

```csharp
Miter = 0,
```


#### Type Changed: SkiaSharp.SKWStream

Modified properties:

```diff
 public ---virtual--- int BytesWritten { get; }
```

Modified methods:

```diff
 public ---virtual--- void Flush ()
 public ---virtual--- bool Write (byte[] buffer, int size)
```


#### New Type: SkiaSharp.SK3dView

```csharp
public class SK3dView : SkiaSharp.SKObject, System.IDisposable {
	// constructors
	public SK3dView ();
	// properties
	public SKMatrix Matrix { get; }
	// methods
	public void ApplyToCanvas (SKCanvas canvas);
	protected override void Dispose (bool disposing);
	public float DotWithNormal (float dx, float dy, float dz);
	public void GetMatrix (ref SKMatrix matrix);
	public void Restore ();
	public void RotateXDegrees (float degrees);
	public void RotateXRadians (float radians);
	public void RotateYDegrees (float degrees);
	public void RotateYRadians (float radians);
	public void RotateZDegrees (float degrees);
	public void RotateZRadians (float radians);
	public void Save ();
	public void Translate (float x, float y, float z);
	public void TranslateX (float x);
	public void TranslateY (float y);
	public void TranslateZ (float z);
}
```

#### New Type: SkiaSharp.SKAutoMaskFreeImage

```csharp
public class SKAutoMaskFreeImage : System.IDisposable {
	// constructors
	public SKAutoMaskFreeImage (IntPtr maskImage);
	// methods
	public virtual void Dispose ();
}
```

#### New Type: SkiaSharp.SKBlurMaskFilterFlags

```csharp
[Serializable]
[Flags]
public enum SKBlurMaskFilterFlags {
	All = 3,
	HighQuality = 2,
	IgnoreTransform = 1,
	None = 0,
}
```

#### New Type: SkiaSharp.SKCodecScanlineOrder

```csharp
[Serializable]
public enum SKCodecScanlineOrder {
	BottomUp = 1,
	TopDown = 0,
}
```

#### New Type: SkiaSharp.SKDocumentPdfMetadata

```csharp
public struct SKDocumentPdfMetadata {
	// properties
	public string Author { get; set; }
	public System.DateTime? Creation { get; set; }
	public string Creator { get; set; }
	public string Keywords { get; set; }
	public System.DateTime? Modified { get; set; }
	public string Producer { get; set; }
	public string Subject { get; set; }
	public string Title { get; set; }
}
```

#### New Type: SkiaSharp.SKFontManager

```csharp
public class SKFontManager : SkiaSharp.SKObject, System.IDisposable {
	// properties
	public static SKFontManager Default { get; }
	public int FontFamilyCount { get; }
	// methods
	protected override void Dispose (bool disposing);
	public string GetFamilyName (int index);
	public string[] GetFontFamilies ();
	public SKTypeface MatchCharacter (char character);
	public SKTypeface MatchCharacter (int character);
	public SKTypeface MatchCharacter (string familyName, char character);
	public SKTypeface MatchCharacter (string familyName, int character);
	public SKTypeface MatchCharacter (string familyName, string[] bcp47, char character);
	public SKTypeface MatchCharacter (string familyName, string[] bcp47, int character);
	public SKTypeface MatchCharacter (string familyName, SKFontStyleWeight weight, SKFontStyleWidth width, SKFontStyleSlant slant, string[] bcp47, char character);
	public SKTypeface MatchCharacter (string familyName, SKFontStyleWeight weight, SKFontStyleWidth width, SKFontStyleSlant slant, string[] bcp47, int character);
	public SKTypeface MatchCharacter (string familyName, int weight, int width, SKFontStyleSlant slant, string[] bcp47, int character);
}
```

#### New Type: SkiaSharp.SKManagedWStream

```csharp
public class SKManagedWStream : SkiaSharp.SKWStream, System.IDisposable {
	// constructors
	public SKManagedWStream (System.IO.Stream managedStream);
	public SKManagedWStream (System.IO.Stream managedStream, bool disposeManagedStream);
	// methods
	protected override void Dispose (bool disposing);
}
```

#### New Type: SkiaSharp.SKMask

```csharp
public struct SKMask {
	// constructors
	public SKMask (SKRectI bounds, uint rowBytes, SKMaskFormat format);
	public SKMask (IntPtr image, SKRectI bounds, uint rowBytes, SKMaskFormat format);
	// properties
	public SKRectI Bounds { get; }
	public SKMaskFormat Format { get; }
	public IntPtr Image { get; }
	public bool IsEmpty { get; }
	public uint RowBytes { get; }
	// methods
	public long AllocateImage ();
	public static IntPtr AllocateImage (long size);
	public long ComputeImageSize ();
	public long ComputeTotalImageSize ();
	public static SKMask Create (byte[] image, SKRectI bounds, uint rowBytes, SKMaskFormat format);
	public void FreeImage ();
	public static void FreeImage (IntPtr image);
	public IntPtr GetAddr (int x, int y);
	public byte GetAddr1 (int x, int y);
	public ushort GetAddr16 (int x, int y);
	public uint GetAddr32 (int x, int y);
	public byte GetAddr8 (int x, int y);
}
```

#### New Type: SkiaSharp.SKMaskFormat

```csharp
[Serializable]
public enum SKMaskFormat {
	A8 = 1,
	Argb32 = 3,
	BW = 0,
	Lcd16 = 4,
	ThreeD = 2,
}
```

#### New Type: SkiaSharp.SKPathSegmentMask

```csharp
[Serializable]
[Flags]
public enum SKPathSegmentMask {
	Conic = 4,
	Cubic = 8,
	Line = 1,
	Quad = 2,
}
```

#### New Type: SkiaSharp.SKSvgCanvas

```csharp
public class SKSvgCanvas {
	// methods
	public static SKCanvas Create (SKRect bounds, SKXmlWriter writer);
}
```

#### New Type: SkiaSharp.SKXmlStreamWriter

```csharp
public class SKXmlStreamWriter : SkiaSharp.SKXmlWriter, System.IDisposable {
	// constructors
	public SKXmlStreamWriter (SKWStream stream);
	// methods
	protected override void Dispose (bool disposing);
}
```

#### New Type: SkiaSharp.SKXmlWriter

```csharp
public abstract class SKXmlWriter : SkiaSharp.SKObject, System.IDisposable {
}
```

#### New Type: SkiaSharp.StringUtilities

```csharp
public static class StringUtilities {
	// methods
	public static byte[] GetEncodedText (string text, SKTextEncoding encoding);
	public static string GetString (byte[] data, SKTextEncoding encoding);
	public static string GetString (IntPtr data, int dataLength, SKTextEncoding encoding);
	public static int GetUnicodeCharacterCode (string character, SKTextEncoding encoding);
}
```


