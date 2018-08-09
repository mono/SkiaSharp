# API diff: HarfBuzzSharp.dll

## HarfBuzzSharp.dll

> Assembly Version Changed: 1.0.0.0 vs 0.0.0.0

### New Namespace HarfBuzzSharp

#### New Type: HarfBuzzSharp.Blob

```csharp
public class Blob : HarfBuzzSharp.NativeObject, System.IDisposable {
	// constructors
	public Blob (IntPtr data, uint length, MemoryMode mode, object userData, BlobReleaseDelegate releaseDelegate);
	// methods
	protected override void Dispose (bool disposing);
	public void MakeImmutable ();
}
```

#### New Type: HarfBuzzSharp.BlobReleaseDelegate

```csharp
public sealed delegate BlobReleaseDelegate : System.MulticastDelegate, System.ICloneable, System.Runtime.Serialization.ISerializable {
	// constructors
	public BlobReleaseDelegate (object object, IntPtr method);
	// methods
	public virtual System.IAsyncResult BeginInvoke (object context, System.AsyncCallback callback, object object);
	public virtual void EndInvoke (System.IAsyncResult result);
	public virtual void Invoke (object context);
}
```

#### New Type: HarfBuzzSharp.Buffer

```csharp
public class Buffer : HarfBuzzSharp.NativeObject, System.IDisposable {
	// constructors
	public Buffer ();
	// properties
	public Direction Direction { get; set; }
	public GlyphInfo[] GlyphInfos { get; }
	public GlyphPosition[] GlyphPositions { get; }
	public uint Length { get; set; }
	// methods
	public void AddUtf8 (byte[] bytes);
	public void AddUtf8 (string utf8text);
	public void ClearContents ();
	protected override void Dispose (bool disposing);
	public void GuessSegmentProperties ();
}
```

#### New Type: HarfBuzzSharp.Direction

```csharp
[Serializable]
public enum Direction {
	BottomToTop = 7,
	Invalid = 0,
	LeftToRight = 4,
	RightToLeft = 5,
	TopToBottom = 6,
}
```

#### New Type: HarfBuzzSharp.Face

```csharp
public class Face : HarfBuzzSharp.NativeObject, System.IDisposable {
	// constructors
	public Face (Blob blob, uint index);
	// properties
	public uint Index { get; set; }
	public uint UnitsPerEm { get; set; }
	// methods
	protected override void Dispose (bool disposing);
}
```

#### New Type: HarfBuzzSharp.Feature

```csharp
public struct Feature {
	// properties
	public uint End { get; set; }
	public uint Start { get; set; }
	public uint Tag { get; set; }
	public uint Value { get; set; }
}
```

#### New Type: HarfBuzzSharp.Font

```csharp
public class Font : HarfBuzzSharp.NativeObject, System.IDisposable {
	// constructors
	public Font (Face face);
	// methods
	protected override void Dispose (bool disposing);
	public void GetScale (out int xScale, out int yScale);
	public void SetFunctionsOpenType ();
	public void SetScale (int xScale, int yScale);
	public void Shape (Buffer buffer, Feature[] features);
}
```

#### New Type: HarfBuzzSharp.GlyphInfo

```csharp
public struct GlyphInfo {
	// properties
	public uint Cluster { get; set; }
	public uint Codepoint { get; set; }
	public uint Mask { get; set; }
}
```

#### New Type: HarfBuzzSharp.GlyphPosition

```csharp
public struct GlyphPosition {
	// properties
	public int XAdvance { get; set; }
	public int XOffset { get; set; }
	public int YAdvance { get; set; }
	public int YOffset { get; set; }
}
```

#### New Type: HarfBuzzSharp.MemoryMode

```csharp
[Serializable]
public enum MemoryMode {
	Duplicate = 0,
	ReadOnly = 1,
	ReadOnlyMayMakeWriteable = 3,
	Writeable = 2,
}
```

#### New Type: HarfBuzzSharp.NativeObject

```csharp
public class NativeObject : System.IDisposable {
	// properties
	protected virtual IntPtr Handle { get; set; }
	// methods
	public virtual void Dispose ();
	protected virtual void Dispose (bool disposing);
	protected override void ~NativeObject ();
}
```

