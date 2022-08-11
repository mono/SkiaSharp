# API diff: HarfBuzzSharp.dll

## HarfBuzzSharp.dll

### Namespace HarfBuzzSharp

#### Type Changed: HarfBuzzSharp.Blob

Obsoleted constructors:

```diff
 [Obsolete ()]
 public Blob (IntPtr data, uint length, MemoryMode mode, object userData, BlobReleaseDelegate releaseDelegate);
```

Added constructors:

```csharp
public Blob (IntPtr data, int length, MemoryMode mode);
public Blob (IntPtr data, int length, MemoryMode mode, ReleaseDelegate releaseDelegate);
```

Added properties:

```csharp
public static Blob Empty { get; }
public int FaceCount { get; }
public bool IsImmutable { get; }
public int Length { get; }
```

Added methods:

```csharp
public System.ReadOnlySpan<byte> AsSpan ();
public System.IO.Stream AsStream ();
protected override void DisposeHandler ();
public static Blob FromFile (string fileName);
public static Blob FromStream (System.IO.Stream stream);
```


#### Type Changed: HarfBuzzSharp.Buffer

Added field:

```csharp
public static const int DefaultReplacementCodepoint;
```

Modified properties:

```diff
-public uint Length { get; set; }
+public int Length { get; set; }
```

Added properties:

```csharp
public ClusterLevel ClusterLevel { get; set; }
public ContentType ContentType { get; set; }
public BufferFlags Flags { get; set; }
public uint InvisibleGlyph { get; set; }
public Language Language { get; set; }
public uint ReplacementCodepoint { get; set; }
public Script Script { get; set; }
public UnicodeFunctions UnicodeFunctions { get; set; }
```

Added methods:

```csharp
public void Add (int codepoint, int cluster);
public void Add (uint codepoint, uint cluster);
public void AddCodepoints (System.ReadOnlySpan<int> text);
public void AddCodepoints (System.ReadOnlySpan<uint> text);
public void AddCodepoints (IntPtr text, int textLength);
public void AddCodepoints (System.ReadOnlySpan<int> text, int itemOffset, int itemLength);
public void AddCodepoints (System.ReadOnlySpan<uint> text, int itemOffset, int itemLength);
public void AddCodepoints (IntPtr text, int textLength, int itemOffset, int itemLength);
public void AddUtf16 (System.ReadOnlySpan<byte> text);
public void AddUtf16 (System.ReadOnlySpan<char> text);
public void AddUtf16 (string text);
public void AddUtf16 (IntPtr text, int textLength);
public void AddUtf16 (System.ReadOnlySpan<char> text, int itemOffset, int itemLength);
public void AddUtf16 (string text, int itemOffset, int itemLength);
public void AddUtf16 (IntPtr text, int textLength, int itemOffset, int itemLength);
public void AddUtf32 (System.ReadOnlySpan<byte> text);
public void AddUtf32 (System.ReadOnlySpan<int> text);
public void AddUtf32 (System.ReadOnlySpan<uint> text);
public void AddUtf32 (string text);
public void AddUtf32 (IntPtr text, int textLength);
public void AddUtf32 (System.ReadOnlySpan<int> text, int itemOffset, int itemLength);
public void AddUtf32 (System.ReadOnlySpan<uint> text, int itemOffset, int itemLength);
public void AddUtf32 (IntPtr text, int textLength, int itemOffset, int itemLength);
public void AddUtf8 (System.ReadOnlySpan<byte> text);
public void AddUtf8 (IntPtr text, int textLength);
public void AddUtf8 (System.ReadOnlySpan<byte> text, int itemOffset, int itemLength);
public void AddUtf8 (IntPtr text, int textLength, int itemOffset, int itemLength);
public void Append (Buffer buffer);
public void Append (Buffer buffer, int start, int end);
public void DeserializeGlyphs (string data);
public void DeserializeGlyphs (string data, Font font);
public void DeserializeGlyphs (string data, Font font, SerializeFormat format);
protected override void DisposeHandler ();
public System.ReadOnlySpan<GlyphInfo> GetGlyphInfoSpan ();
public System.ReadOnlySpan<GlyphPosition> GetGlyphPositionSpan ();
public void NormalizeGlyphs ();
public void Reset ();
public void Reverse ();
public void ReverseClusters ();
public void ReverseRange (int start, int end);
public string SerializeGlyphs ();
public string SerializeGlyphs (Font font);
public string SerializeGlyphs (int start, int end);
public string SerializeGlyphs (Font font, SerializeFormat format, SerializeFlag flags);
public string SerializeGlyphs (int start, int end, Font font, SerializeFormat format, SerializeFlag flags);
```


#### Type Changed: HarfBuzzSharp.Face

Added constructors:

```csharp
public Face (GetTableDelegate getTable);
public Face (Blob blob, int index);
public Face (GetTableDelegate getTable, ReleaseDelegate destroy);
```

Modified properties:

```diff
-public uint Index { get; set; }
+public int Index { get; set; }
-public uint UnitsPerEm { get; set; }
+public int UnitsPerEm { get; set; }
```

Added properties:

```csharp
public static Face Empty { get; }
public int GlyphCount { get; set; }
public bool IsImmutable { get; }
public Tag[] Tables { get; }
```

Added methods:

```csharp
protected override void DisposeHandler ();
public void MakeImmutable ();
public Blob ReferenceTable (Tag table);
```


#### Type Changed: HarfBuzzSharp.Feature

Added constructors:

```csharp
public Feature (Tag tag);
public Feature (Tag tag, uint value);
public Feature (Tag tag, uint value, uint start, uint end);
```

Modified properties:

```diff
-public uint Tag { get; set; }
+public Tag Tag { get; set; }
```

Added methods:

```csharp
public static Feature Parse (string s);
public override string ToString ();
public static bool TryParse (string s, out Feature feature);
```


#### Type Changed: HarfBuzzSharp.Font

Added constructor:

```csharp
public Font (Font parent);
```

Added properties:

```csharp
public OpenTypeMetrics OpenTypeMetrics { get; }
public Font Parent { get; }
public string[] SupportedShapers { get; }
```

Added methods:

```csharp
protected override void DisposeHandler ();
public FontExtents GetFontExtentsForDirection (Direction direction);
public void GetGlyphAdvanceForDirection (uint glyph, Direction direction, out int x, out int y);
public int[] GetGlyphAdvancesForDirection (System.ReadOnlySpan<uint> glyphs, Direction direction);
public int[] GetGlyphAdvancesForDirection (IntPtr firstGlyph, int count, Direction direction);
public int GetHorizontalGlyphAdvance (uint glyph);
public int[] GetHorizontalGlyphAdvances (System.ReadOnlySpan<uint> glyphs);
public int[] GetHorizontalGlyphAdvances (IntPtr firstGlyph, int count);
public int GetHorizontalGlyphKerning (uint leftGlyph, uint rightGlyph);
public int GetVerticalGlyphAdvance (uint glyph);
public int[] GetVerticalGlyphAdvances (System.ReadOnlySpan<uint> glyphs);
public int[] GetVerticalGlyphAdvances (IntPtr firstGlyph, int count);
public string GlyphToString (uint glyph);
public void SetFontFunctions (FontFunctions fontFunctions);
public void SetFontFunctions (FontFunctions fontFunctions, object fontData);
public void SetFontFunctions (FontFunctions fontFunctions, object fontData, ReleaseDelegate destroy);
public void Shape (Buffer buffer, System.Collections.Generic.IReadOnlyList<Feature> features, System.Collections.Generic.IReadOnlyList<string> shapers);
public bool TryGetGlyph (int unicode, out uint glyph);
public bool TryGetGlyph (uint unicode, out uint glyph);
public bool TryGetGlyph (int unicode, uint variationSelector, out uint glyph);
public bool TryGetGlyph (uint unicode, uint variationSelector, out uint glyph);
public bool TryGetGlyphContourPoint (uint glyph, uint pointIndex, out int x, out int y);
public bool TryGetGlyphContourPointForOrigin (uint glyph, uint pointIndex, Direction direction, out int x, out int y);
public bool TryGetGlyphExtents (uint glyph, out GlyphExtents extents);
public bool TryGetGlyphFromName (string name, out uint glyph);
public bool TryGetGlyphFromString (string s, out uint glyph);
public bool TryGetGlyphName (uint glyph, out string name);
public bool TryGetHorizontalFontExtents (out FontExtents extents);
public bool TryGetHorizontalGlyphOrigin (uint glyph, out int xOrigin, out int yOrigin);
public bool TryGetNominalGlyph (int unicode, out uint glyph);
public bool TryGetNominalGlyph (uint unicode, out uint glyph);
public bool TryGetVariationGlyph (int unicode, out uint glyph);
public bool TryGetVariationGlyph (uint unicode, out uint glyph);
public bool TryGetVariationGlyph (int unicode, uint variationSelector, out uint glyph);
public bool TryGetVariationGlyph (uint unicode, uint variationSelector, out uint glyph);
public bool TryGetVerticalFontExtents (out FontExtents extents);
public bool TryGetVerticalGlyphOrigin (uint glyph, out int xOrigin, out int yOrigin);
```


#### Type Changed: HarfBuzzSharp.GlyphInfo

Added property:

```csharp
public GlyphFlags GlyphFlags { get; }
```


#### Type Changed: HarfBuzzSharp.NativeObject

Added method:

```csharp
protected virtual void DisposeHandler ();
```


#### New Type: HarfBuzzSharp.BufferFlags

```csharp
[Serializable]
[Flags]
public enum BufferFlags {
	BeginningOfText = 1,
	Default = 0,
	DoNotInsertDottedCircle = 16,
	EndOfText = 2,
	PreserveDefaultIgnorables = 4,
	RemoveDefaultIgnorables = 8,
}
```

#### New Type: HarfBuzzSharp.ClusterLevel

```csharp
[Serializable]
public enum ClusterLevel {
	Characters = 2,
	Default = 0,
	MonotoneCharacters = 1,
	MonotoneGraphemes = 0,
}
```

#### New Type: HarfBuzzSharp.CombiningClassDelegate

```csharp
public sealed delegate CombiningClassDelegate : System.MulticastDelegate, System.ICloneable, System.Runtime.Serialization.ISerializable {
	// constructors
	public CombiningClassDelegate (object object, IntPtr method);
	// methods
	public virtual System.IAsyncResult BeginInvoke (UnicodeFunctions ufuncs, uint unicode, System.AsyncCallback callback, object object);
	public virtual UnicodeCombiningClass EndInvoke (System.IAsyncResult result);
	public virtual UnicodeCombiningClass Invoke (UnicodeFunctions ufuncs, uint unicode);
}
```

#### New Type: HarfBuzzSharp.ComposeDelegate

```csharp
public sealed delegate ComposeDelegate : System.MulticastDelegate, System.ICloneable, System.Runtime.Serialization.ISerializable {
	// constructors
	public ComposeDelegate (object object, IntPtr method);
	// methods
	public virtual System.IAsyncResult BeginInvoke (UnicodeFunctions ufuncs, uint a, uint b, out uint ab, System.AsyncCallback callback, object object);
	public virtual bool EndInvoke (out uint ab, System.IAsyncResult result);
	public virtual bool Invoke (UnicodeFunctions ufuncs, uint a, uint b, out uint ab);
}
```

#### New Type: HarfBuzzSharp.ContentType

```csharp
[Serializable]
public enum ContentType {
	Glyphs = 2,
	Invalid = 0,
	Unicode = 1,
}
```

#### New Type: HarfBuzzSharp.DecomposeDelegate

```csharp
public sealed delegate DecomposeDelegate : System.MulticastDelegate, System.ICloneable, System.Runtime.Serialization.ISerializable {
	// constructors
	public DecomposeDelegate (object object, IntPtr method);
	// methods
	public virtual System.IAsyncResult BeginInvoke (UnicodeFunctions ufuncs, uint ab, out uint a, out uint b, System.AsyncCallback callback, object object);
	public virtual bool EndInvoke (out uint a, out uint b, System.IAsyncResult result);
	public virtual bool Invoke (UnicodeFunctions ufuncs, uint ab, out uint a, out uint b);
}
```

#### New Type: HarfBuzzSharp.FontExtents

```csharp
public struct FontExtents {
	// properties
	public int Ascender { get; set; }
	public int Descender { get; set; }
	public int LineGap { get; set; }
}
```

#### New Type: HarfBuzzSharp.FontExtentsDelegate

```csharp
public sealed delegate FontExtentsDelegate : System.MulticastDelegate, System.ICloneable, System.Runtime.Serialization.ISerializable {
	// constructors
	public FontExtentsDelegate (object object, IntPtr method);
	// methods
	public virtual System.IAsyncResult BeginInvoke (Font font, object fontData, out FontExtents extents, System.AsyncCallback callback, object object);
	public virtual bool EndInvoke (out FontExtents extents, System.IAsyncResult result);
	public virtual bool Invoke (Font font, object fontData, out FontExtents extents);
}
```

#### New Type: HarfBuzzSharp.FontFunctions

```csharp
public class FontFunctions : HarfBuzzSharp.NativeObject, System.IDisposable {
	// constructors
	public FontFunctions ();
	// properties
	public static FontFunctions Empty { get; }
	public bool IsImmutable { get; }
	// methods
	protected override void Dispose (bool disposing);
	protected override void DisposeHandler ();
	public void MakeImmutable ();
	public void SetGlyphContourPointDelegate (GlyphContourPointDelegate del, ReleaseDelegate destroy);
	public void SetGlyphExtentsDelegate (GlyphExtentsDelegate del, ReleaseDelegate destroy);
	public void SetGlyphFromNameDelegate (GlyphFromNameDelegate del, ReleaseDelegate destroy);
	public void SetGlyphNameDelegate (GlyphNameDelegate del, ReleaseDelegate destroy);
	public void SetHorizontalFontExtentsDelegate (FontExtentsDelegate del, ReleaseDelegate destroy);
	public void SetHorizontalGlyphAdvanceDelegate (GlyphAdvanceDelegate del, ReleaseDelegate destroy);
	public void SetHorizontalGlyphAdvancesDelegate (GlyphAdvancesDelegate del, ReleaseDelegate destroy);
	public void SetHorizontalGlyphKerningDelegate (GlyphKerningDelegate del, ReleaseDelegate destroy);
	public void SetHorizontalGlyphOriginDelegate (GlyphOriginDelegate del, ReleaseDelegate destroy);
	public void SetNominalGlyphDelegate (NominalGlyphDelegate del, ReleaseDelegate destroy);
	public void SetNominalGlyphsDelegate (NominalGlyphsDelegate del, ReleaseDelegate destroy);
	public void SetVariationGlyphDelegate (VariationGlyphDelegate del, ReleaseDelegate destroy);
	public void SetVerticalFontExtentsDelegate (FontExtentsDelegate del, ReleaseDelegate destroy);
	public void SetVerticalGlyphAdvanceDelegate (GlyphAdvanceDelegate del, ReleaseDelegate destroy);
	public void SetVerticalGlyphAdvancesDelegate (GlyphAdvancesDelegate del, ReleaseDelegate destroy);
	public void SetVerticalGlyphOriginDelegate (GlyphOriginDelegate del, ReleaseDelegate destroy);
}
```

#### New Type: HarfBuzzSharp.GeneralCategoryDelegate

```csharp
public sealed delegate GeneralCategoryDelegate : System.MulticastDelegate, System.ICloneable, System.Runtime.Serialization.ISerializable {
	// constructors
	public GeneralCategoryDelegate (object object, IntPtr method);
	// methods
	public virtual System.IAsyncResult BeginInvoke (UnicodeFunctions ufuncs, uint unicode, System.AsyncCallback callback, object object);
	public virtual UnicodeGeneralCategory EndInvoke (System.IAsyncResult result);
	public virtual UnicodeGeneralCategory Invoke (UnicodeFunctions ufuncs, uint unicode);
}
```

#### New Type: HarfBuzzSharp.GetTableDelegate

```csharp
public sealed delegate GetTableDelegate : System.MulticastDelegate, System.ICloneable, System.Runtime.Serialization.ISerializable {
	// constructors
	public GetTableDelegate (object object, IntPtr method);
	// methods
	public virtual System.IAsyncResult BeginInvoke (Face face, Tag tag, System.AsyncCallback callback, object object);
	public virtual Blob EndInvoke (System.IAsyncResult result);
	public virtual Blob Invoke (Face face, Tag tag);
}
```

#### New Type: HarfBuzzSharp.GlyphAdvanceDelegate

```csharp
public sealed delegate GlyphAdvanceDelegate : System.MulticastDelegate, System.ICloneable, System.Runtime.Serialization.ISerializable {
	// constructors
	public GlyphAdvanceDelegate (object object, IntPtr method);
	// methods
	public virtual System.IAsyncResult BeginInvoke (Font font, object fontData, uint glyph, System.AsyncCallback callback, object object);
	public virtual int EndInvoke (System.IAsyncResult result);
	public virtual int Invoke (Font font, object fontData, uint glyph);
}
```

#### New Type: HarfBuzzSharp.GlyphAdvancesDelegate

```csharp
public sealed delegate GlyphAdvancesDelegate : System.MulticastDelegate, System.ICloneable, System.Runtime.Serialization.ISerializable {
	// constructors
	public GlyphAdvancesDelegate (object object, IntPtr method);
	// methods
	public virtual System.IAsyncResult BeginInvoke (Font font, object fontData, uint count, System.ReadOnlySpan<uint> glyphs, System.Span<int> advances, System.AsyncCallback callback, object object);
	public virtual void EndInvoke (System.IAsyncResult result);
	public virtual void Invoke (Font font, object fontData, uint count, System.ReadOnlySpan<uint> glyphs, System.Span<int> advances);
}
```

#### New Type: HarfBuzzSharp.GlyphContourPointDelegate

```csharp
public sealed delegate GlyphContourPointDelegate : System.MulticastDelegate, System.ICloneable, System.Runtime.Serialization.ISerializable {
	// constructors
	public GlyphContourPointDelegate (object object, IntPtr method);
	// methods
	public virtual System.IAsyncResult BeginInvoke (Font font, object fontData, uint glyph, uint pointIndex, out int x, out int y, System.AsyncCallback callback, object object);
	public virtual bool EndInvoke (out int x, out int y, System.IAsyncResult result);
	public virtual bool Invoke (Font font, object fontData, uint glyph, uint pointIndex, out int x, out int y);
}
```

#### New Type: HarfBuzzSharp.GlyphExtents

```csharp
public struct GlyphExtents {
	// properties
	public int Height { get; set; }
	public int Width { get; set; }
	public int XBearing { get; set; }
	public int YBearing { get; set; }
}
```

#### New Type: HarfBuzzSharp.GlyphExtentsDelegate

```csharp
public sealed delegate GlyphExtentsDelegate : System.MulticastDelegate, System.ICloneable, System.Runtime.Serialization.ISerializable {
	// constructors
	public GlyphExtentsDelegate (object object, IntPtr method);
	// methods
	public virtual System.IAsyncResult BeginInvoke (Font font, object fontData, uint glyph, out GlyphExtents extents, System.AsyncCallback callback, object object);
	public virtual bool EndInvoke (out GlyphExtents extents, System.IAsyncResult result);
	public virtual bool Invoke (Font font, object fontData, uint glyph, out GlyphExtents extents);
}
```

#### New Type: HarfBuzzSharp.GlyphFlags

```csharp
[Serializable]
[Flags]
public enum GlyphFlags {
	Defined = 1,
	UnsafeToBreak = 1,
}
```

#### New Type: HarfBuzzSharp.GlyphFromNameDelegate

```csharp
public sealed delegate GlyphFromNameDelegate : System.MulticastDelegate, System.ICloneable, System.Runtime.Serialization.ISerializable {
	// constructors
	public GlyphFromNameDelegate (object object, IntPtr method);
	// methods
	public virtual System.IAsyncResult BeginInvoke (Font font, object fontData, string name, out uint glyph, System.AsyncCallback callback, object object);
	public virtual bool EndInvoke (out uint glyph, System.IAsyncResult result);
	public virtual bool Invoke (Font font, object fontData, string name, out uint glyph);
}
```

#### New Type: HarfBuzzSharp.GlyphKerningDelegate

```csharp
public sealed delegate GlyphKerningDelegate : System.MulticastDelegate, System.ICloneable, System.Runtime.Serialization.ISerializable {
	// constructors
	public GlyphKerningDelegate (object object, IntPtr method);
	// methods
	public virtual System.IAsyncResult BeginInvoke (Font font, object fontData, uint firstGlyph, uint secondGlyph, System.AsyncCallback callback, object object);
	public virtual int EndInvoke (System.IAsyncResult result);
	public virtual int Invoke (Font font, object fontData, uint firstGlyph, uint secondGlyph);
}
```

#### New Type: HarfBuzzSharp.GlyphNameDelegate

```csharp
public sealed delegate GlyphNameDelegate : System.MulticastDelegate, System.ICloneable, System.Runtime.Serialization.ISerializable {
	// constructors
	public GlyphNameDelegate (object object, IntPtr method);
	// methods
	public virtual System.IAsyncResult BeginInvoke (Font font, object fontData, uint glyph, out string name, System.AsyncCallback callback, object object);
	public virtual bool EndInvoke (out string name, System.IAsyncResult result);
	public virtual bool Invoke (Font font, object fontData, uint glyph, out string name);
}
```

#### New Type: HarfBuzzSharp.GlyphOriginDelegate

```csharp
public sealed delegate GlyphOriginDelegate : System.MulticastDelegate, System.ICloneable, System.Runtime.Serialization.ISerializable {
	// constructors
	public GlyphOriginDelegate (object object, IntPtr method);
	// methods
	public virtual System.IAsyncResult BeginInvoke (Font font, object fontData, uint glyph, out int x, out int y, System.AsyncCallback callback, object object);
	public virtual bool EndInvoke (out int x, out int y, System.IAsyncResult result);
	public virtual bool Invoke (Font font, object fontData, uint glyph, out int x, out int y);
}
```

#### New Type: HarfBuzzSharp.Language

```csharp
public class Language : HarfBuzzSharp.NativeObject, System.IDisposable {
	// constructors
	public Language (System.Globalization.CultureInfo culture);
	public Language (string name);
	// properties
	public static Language Default { get; }
	public string Name { get; }
	// methods
	public override bool Equals (object obj);
	public override int GetHashCode ();
	public override string ToString ();
}
```

#### New Type: HarfBuzzSharp.MirroringDelegate

```csharp
public sealed delegate MirroringDelegate : System.MulticastDelegate, System.ICloneable, System.Runtime.Serialization.ISerializable {
	// constructors
	public MirroringDelegate (object object, IntPtr method);
	// methods
	public virtual System.IAsyncResult BeginInvoke (UnicodeFunctions ufuncs, uint unicode, System.AsyncCallback callback, object object);
	public virtual uint EndInvoke (System.IAsyncResult result);
	public virtual uint Invoke (UnicodeFunctions ufuncs, uint unicode);
}
```

#### New Type: HarfBuzzSharp.NominalGlyphDelegate

```csharp
public sealed delegate NominalGlyphDelegate : System.MulticastDelegate, System.ICloneable, System.Runtime.Serialization.ISerializable {
	// constructors
	public NominalGlyphDelegate (object object, IntPtr method);
	// methods
	public virtual System.IAsyncResult BeginInvoke (Font font, object fontData, uint unicode, out uint glyph, System.AsyncCallback callback, object object);
	public virtual bool EndInvoke (out uint glyph, System.IAsyncResult result);
	public virtual bool Invoke (Font font, object fontData, uint unicode, out uint glyph);
}
```

#### New Type: HarfBuzzSharp.NominalGlyphsDelegate

```csharp
public sealed delegate NominalGlyphsDelegate : System.MulticastDelegate, System.ICloneable, System.Runtime.Serialization.ISerializable {
	// constructors
	public NominalGlyphsDelegate (object object, IntPtr method);
	// methods
	public virtual System.IAsyncResult BeginInvoke (Font font, object fontData, uint count, System.ReadOnlySpan<uint> codepoints, System.Span<uint> glyphs, System.AsyncCallback callback, object object);
	public virtual uint EndInvoke (System.IAsyncResult result);
	public virtual uint Invoke (Font font, object fontData, uint count, System.ReadOnlySpan<uint> codepoints, System.Span<uint> glyphs);
}
```

#### New Type: HarfBuzzSharp.OpenTypeMetrics

```csharp
public struct OpenTypeMetrics {
	// constructors
	public OpenTypeMetrics (IntPtr font);
	// methods
	public float GetVariation (OpenTypeMetricsTag metricsTag);
	public int GetXVariation (OpenTypeMetricsTag metricsTag);
	public int GetYVariation (OpenTypeMetricsTag metricsTag);
	public bool TryGetPosition (OpenTypeMetricsTag metricsTag, out int position);
}
```

#### New Type: HarfBuzzSharp.OpenTypeMetricsTag

```csharp
[Serializable]
public enum OpenTypeMetricsTag {
	CapHeight = 1668311156,
	HorizontalAscender = 1751216995,
	HorizontalCaretOffset = 1751347046,
	HorizontalCaretRise = 1751347827,
	HorizontalCaretRun = 1751347822,
	HorizontalClippingAscent = 1751346273,
	HorizontalClippingDescent = 1751346276,
	HorizontalDescender = 1751413603,
	HorizontalLineGap = 1751934832,
	StrikeoutOffset = 1937011311,
	StrikeoutSize = 1937011315,
	SubScriptEmXOffset = 1935833199,
	SubScriptEmXSize = 1935833203,
	SubScriptEmYOffset = 1935833455,
	SubScriptEmYSize = 1935833459,
	SuperScriptEmXOffset = 1936750703,
	SuperScriptEmXSize = 1936750707,
	SuperScriptEmYOffset = 1936750959,
	SuperScriptEmYSize = 1936750963,
	UnderlineOffset = 1970168943,
	UnderlineSize = 1970168947,
	VerticalAscender = 1986098019,
	VerticalCaretOffset = 1986228070,
	VerticalCaretRise = 1986228851,
	VerticalCaretRun = 1986228846,
	VerticalDescender = 1986294627,
	VerticalLineGap = 1986815856,
	XHeight = 2020108148,
}
```

#### New Type: HarfBuzzSharp.ReleaseDelegate

```csharp
public sealed delegate ReleaseDelegate : System.MulticastDelegate, System.ICloneable, System.Runtime.Serialization.ISerializable {
	// constructors
	public ReleaseDelegate (object object, IntPtr method);
	// methods
	public virtual System.IAsyncResult BeginInvoke (System.AsyncCallback callback, object object);
	public virtual void EndInvoke (System.IAsyncResult result);
	public virtual void Invoke ();
}
```

#### New Type: HarfBuzzSharp.Script

```csharp
public struct Script, System.IEquatable<Script> {
	// fields
	public static Script Adlam;
	public static Script Ahom;
	public static Script AnatolianHieroglyphs;
	public static Script Arabic;
	public static Script Armenian;
	public static Script Avestan;
	public static Script Balinese;
	public static Script Bamum;
	public static Script BassaVah;
	public static Script Batak;
	public static Script Bengali;
	public static Script Bhaiksuki;
	public static Script Bopomofo;
	public static Script Brahmi;
	public static Script Braille;
	public static Script Buginese;
	public static Script Buhid;
	public static Script CanadianSyllabics;
	public static Script Carian;
	public static Script CaucasianAlbanian;
	public static Script Chakma;
	public static Script Cham;
	public static Script Cherokee;
	public static Script Common;
	public static Script Coptic;
	public static Script Cuneiform;
	public static Script Cypriot;
	public static Script Cyrillic;
	public static Script Deseret;
	public static Script Devanagari;
	public static Script Dogra;
	public static Script Duployan;
	public static Script EgyptianHieroglyphs;
	public static Script Elbasan;
	public static Script Ethiopic;
	public static Script Georgian;
	public static Script Glagolitic;
	public static Script Gothic;
	public static Script Grantha;
	public static Script Greek;
	public static Script Gujarati;
	public static Script GunjalaGondi;
	public static Script Gurmukhi;
	public static Script Han;
	public static Script Hangul;
	public static Script HanifiRohingya;
	public static Script Hanunoo;
	public static Script Hatran;
	public static Script Hebrew;
	public static Script Hiragana;
	public static Script ImperialAramaic;
	public static Script Inherited;
	public static Script InscriptionalPahlavi;
	public static Script InscriptionalParthian;
	public static Script Invalid;
	public static Script Javanese;
	public static Script Kaithi;
	public static Script Kannada;
	public static Script Katakana;
	public static Script KayahLi;
	public static Script Kharoshthi;
	public static Script Khmer;
	public static Script Khojki;
	public static Script Khudawadi;
	public static Script Lao;
	public static Script Latin;
	public static Script Lepcha;
	public static Script Limbu;
	public static Script LinearA;
	public static Script LinearB;
	public static Script Lisu;
	public static Script Lycian;
	public static Script Lydian;
	public static Script Mahajani;
	public static Script Makasar;
	public static Script Malayalam;
	public static Script Mandaic;
	public static Script Manichaean;
	public static Script Marchen;
	public static Script MasaramGondi;
	public static Script MaxValue;
	public static Script MaxValueSigned;
	public static Script Medefaidrin;
	public static Script MeeteiMayek;
	public static Script MendeKikakui;
	public static Script MeroiticCursive;
	public static Script MeroiticHieroglyphs;
	public static Script Miao;
	public static Script Modi;
	public static Script Mongolian;
	public static Script Mro;
	public static Script Multani;
	public static Script Myanmar;
	public static Script Nabataean;
	public static Script NewTaiLue;
	public static Script Newa;
	public static Script Nko;
	public static Script Nushu;
	public static Script Ogham;
	public static Script OlChiki;
	public static Script OldHungarian;
	public static Script OldItalic;
	public static Script OldNorthArabian;
	public static Script OldPermic;
	public static Script OldPersian;
	public static Script OldSogdian;
	public static Script OldSouthArabian;
	public static Script OldTurkic;
	public static Script Oriya;
	public static Script Osage;
	public static Script Osmanya;
	public static Script PahawhHmong;
	public static Script Palmyrene;
	public static Script PauCinHau;
	public static Script PhagsPa;
	public static Script Phoenician;
	public static Script PsalterPahlavi;
	public static Script Rejang;
	public static Script Runic;
	public static Script Samaritan;
	public static Script Saurashtra;
	public static Script Sharada;
	public static Script Shavian;
	public static Script Siddham;
	public static Script Signwriting;
	public static Script Sinhala;
	public static Script Sogdian;
	public static Script SoraSompeng;
	public static Script Soyombo;
	public static Script Sundanese;
	public static Script SylotiNagri;
	public static Script Syriac;
	public static Script Tagalog;
	public static Script Tagbanwa;
	public static Script TaiLe;
	public static Script TaiTham;
	public static Script TaiViet;
	public static Script Takri;
	public static Script Tamil;
	public static Script Tangut;
	public static Script Telugu;
	public static Script Thaana;
	public static Script Thai;
	public static Script Tibetan;
	public static Script Tifinagh;
	public static Script Tirhuta;
	public static Script Ugaritic;
	public static Script Unknown;
	public static Script Vai;
	public static Script WarangCiti;
	public static Script Yi;
	public static Script ZanabazarSquare;
	// properties
	public Direction HorizontalDirection { get; }
	// methods
	public virtual bool Equals (Script other);
	public override bool Equals (object obj);
	public override int GetHashCode ();
	public static Script Parse (string str);
	public override string ToString ();
	public static bool TryParse (string str, out Script script);
	public static uint op_Implicit (Script script);
	public static Script op_Implicit (uint tag);
}
```

#### New Type: HarfBuzzSharp.ScriptDelegate

```csharp
public sealed delegate ScriptDelegate : System.MulticastDelegate, System.ICloneable, System.Runtime.Serialization.ISerializable {
	// constructors
	public ScriptDelegate (object object, IntPtr method);
	// methods
	public virtual System.IAsyncResult BeginInvoke (UnicodeFunctions ufuncs, uint unicode, System.AsyncCallback callback, object object);
	public virtual Script EndInvoke (System.IAsyncResult result);
	public virtual Script Invoke (UnicodeFunctions ufuncs, uint unicode);
}
```

#### New Type: HarfBuzzSharp.SerializeFlag

```csharp
[Serializable]
[Flags]
public enum SerializeFlag {
	Default = 0,
	GlyphExtents = 8,
	GlyphFlags = 16,
	NoAdvances = 32,
	NoClusters = 1,
	NoGlyphNames = 4,
	NoPositions = 2,
}
```

#### New Type: HarfBuzzSharp.SerializeFormat

```csharp
[Serializable]
public enum SerializeFormat {
	Invalid = 0,
	Json = 1246973774,
	Text = 1413830740,
}
```

#### New Type: HarfBuzzSharp.Tag

```csharp
public struct Tag, System.IEquatable<Tag> {
	// constructors
	public Tag (char c1, char c2, char c3, char c4);
	// fields
	public static Tag Max;
	public static Tag MaxSigned;
	public static Tag None;
	// methods
	public virtual bool Equals (Tag other);
	public override bool Equals (object obj);
	public override int GetHashCode ();
	public static Tag Parse (string tag);
	public override string ToString ();
	public static uint op_Implicit (Tag tag);
	public static Tag op_Implicit (uint tag);
}
```

#### New Type: HarfBuzzSharp.UnicodeCombiningClass

```csharp
[Serializable]
public enum UnicodeCombiningClass {
	Above = 230,
	AboveLeft = 228,
	AboveRight = 232,
	AttachedAbove = 214,
	AttachedAboveRight = 216,
	AttachedBelow = 202,
	AttachedBelowLeft = 200,
	Below = 220,
	BelowLeft = 218,
	BelowRight = 222,
	CCC10 = 10,
	CCC103 = 103,
	CCC107 = 107,
	CCC11 = 11,
	CCC118 = 118,
	CCC12 = 12,
	CCC122 = 122,
	CCC129 = 129,
	CCC13 = 13,
	CCC130 = 130,
	CCC133 = 132,
	CCC14 = 14,
	CCC15 = 15,
	CCC16 = 16,
	CCC17 = 17,
	CCC18 = 18,
	CCC19 = 19,
	CCC20 = 20,
	CCC21 = 21,
	CCC22 = 22,
	CCC23 = 23,
	CCC24 = 24,
	CCC25 = 25,
	CCC26 = 26,
	CCC27 = 27,
	CCC28 = 28,
	CCC29 = 29,
	CCC30 = 30,
	CCC31 = 31,
	CCC32 = 32,
	CCC33 = 33,
	CCC34 = 34,
	CCC35 = 35,
	CCC36 = 36,
	CCC84 = 84,
	CCC91 = 91,
	DoubleAbove = 234,
	DoubleBelow = 233,
	Invalid = 255,
	IotaSubscript = 240,
	KanaVoicing = 8,
	Left = 224,
	NotReordered = 0,
	Nukta = 7,
	Overlay = 1,
	Right = 226,
	Virama = 9,
}
```

#### New Type: HarfBuzzSharp.UnicodeFunctions

```csharp
public class UnicodeFunctions : HarfBuzzSharp.NativeObject, System.IDisposable {
	// constructors
	public UnicodeFunctions (UnicodeFunctions parent);
	// properties
	public static UnicodeFunctions Default { get; }
	public static UnicodeFunctions Empty { get; }
	public bool IsImmutable { get; }
	public UnicodeFunctions Parent { get; }
	// methods
	protected override void Dispose (bool disposing);
	protected override void DisposeHandler ();
	public UnicodeCombiningClass GetCombiningClass (int unicode);
	public UnicodeCombiningClass GetCombiningClass (uint unicode);
	public UnicodeGeneralCategory GetGeneralCategory (int unicode);
	public UnicodeGeneralCategory GetGeneralCategory (uint unicode);
	public int GetMirroring (int unicode);
	public uint GetMirroring (uint unicode);
	public Script GetScript (int unicode);
	public Script GetScript (uint unicode);
	public void MakeImmutable ();
	public void SetCombiningClassDelegate (CombiningClassDelegate del, ReleaseDelegate destroy);
	public void SetComposeDelegate (ComposeDelegate del, ReleaseDelegate destroy);
	public void SetDecomposeDelegate (DecomposeDelegate del, ReleaseDelegate destroy);
	public void SetGeneralCategoryDelegate (GeneralCategoryDelegate del, ReleaseDelegate destroy);
	public void SetMirroringDelegate (MirroringDelegate del, ReleaseDelegate destroy);
	public void SetScriptDelegate (ScriptDelegate del, ReleaseDelegate destroy);
	public bool TryCompose (int a, int b, out int ab);
	public bool TryCompose (uint a, uint b, out uint ab);
	public bool TryDecompose (int ab, out int a, out int b);
	public bool TryDecompose (uint ab, out uint a, out uint b);
}
```

#### New Type: HarfBuzzSharp.UnicodeGeneralCategory

```csharp
[Serializable]
public enum UnicodeGeneralCategory {
	ClosePunctuation = 18,
	ConnectPunctuation = 16,
	Control = 0,
	CurrencySymbol = 23,
	DashPunctuation = 17,
	DecimalNumber = 13,
	EnclosingMark = 11,
	FinalPunctuation = 19,
	Format = 1,
	InitialPunctuation = 20,
	LetterNumber = 14,
	LineSeparator = 27,
	LowercaseLetter = 5,
	MathSymbol = 25,
	ModifierLetter = 6,
	ModifierSymbol = 24,
	NonSpacingMark = 12,
	OpenPunctuation = 22,
	OtherLetter = 7,
	OtherNumber = 15,
	OtherPunctuation = 21,
	OtherSymbol = 26,
	ParagraphSeparator = 28,
	PrivateUse = 3,
	SpaceSeparator = 29,
	SpacingMark = 10,
	Surrogate = 4,
	TitlecaseLetter = 8,
	Unassigned = 2,
	UppercaseLetter = 9,
}
```

#### New Type: HarfBuzzSharp.VariationGlyphDelegate

```csharp
public sealed delegate VariationGlyphDelegate : System.MulticastDelegate, System.ICloneable, System.Runtime.Serialization.ISerializable {
	// constructors
	public VariationGlyphDelegate (object object, IntPtr method);
	// methods
	public virtual System.IAsyncResult BeginInvoke (Font font, object fontData, uint unicode, uint variationSelector, out uint glyph, System.AsyncCallback callback, object object);
	public virtual bool EndInvoke (out uint glyph, System.IAsyncResult result);
	public virtual bool Invoke (Font font, object fontData, uint unicode, uint variationSelector, out uint glyph);
}
```


