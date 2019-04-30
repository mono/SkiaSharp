# API diff: HarfBuzzSharp.dll

## HarfBuzzSharp.dll

### Namespace HarfBuzzSharp

#### Type Changed: HarfBuzzSharp.Blob

Added constructors:

```csharp
public Blob (IntPtr data, int length, MemoryMode mode);
public Blob (IntPtr data, int length, MemoryMode mode, object userData, BlobReleaseDelegate releaseDelegate);
```

Added properties:

```csharp
public int FaceCount { get; }
public bool IsImmutable { get; }
public int Length { get; }
```

Removed method:

```csharp
protected override void Dispose (bool disposing);
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
public Flags Flags { get; set; }
public uint InvisibleGlyph { get; set; }
public Language Language { get; set; }
public uint ReplacementCodepoint { get; set; }
public Script Script { get; set; }
public UnicodeFunctions UnicodeFunctions { get; set; }
```

Removed method:

```csharp
protected override void Dispose (bool disposing);
```

Added methods:

```csharp
public void Add (int codepoint, int cluster);
public void Add (uint codepoint, uint cluster);
public void AddCodepoints (int[] text);
public void AddCodepoints (System.ReadOnlySpan<int> text);
public void AddCodepoints (System.ReadOnlySpan<uint> text);
public void AddCodepoints (uint[] text);
public void AddCodepoints (IntPtr text, int textLength);
public void AddCodepoints (int[] text, int itemOffset, int itemLength);
public void AddCodepoints (System.ReadOnlySpan<int> text, int itemOffset, int itemLength);
public void AddCodepoints (System.ReadOnlySpan<uint> text, int itemOffset, int itemLength);
public void AddCodepoints (uint[] text, int itemOffset, int itemLength);
public void AddCodepoints (IntPtr text, int textLength, int itemOffset, int itemLength);
public void AddUtf16 (byte[] text);
public void AddUtf16 (System.ReadOnlySpan<char> text);
public void AddUtf16 (string text);
public void AddUtf16 (IntPtr text, int textLength);
public void AddUtf16 (System.ReadOnlySpan<char> text, int itemOffset, int itemLength);
public void AddUtf16 (string text, int itemOffset, int itemLength);
public void AddUtf16 (IntPtr text, int textLength, int itemOffset, int itemLength);
public void AddUtf32 (byte[] text);
public void AddUtf32 (System.ReadOnlySpan<int> text);
public void AddUtf32 (System.ReadOnlySpan<uint> text);
public void AddUtf32 (string text);
public void AddUtf32 (IntPtr text, int textLength);
public void AddUtf32 (System.ReadOnlySpan<int> text, int itemOffset, int itemLength);
public void AddUtf32 (System.ReadOnlySpan<uint> text, int itemOffset, int itemLength);
public void AddUtf32 (IntPtr text, int textLength, int itemOffset, int itemLength);
public void AddUtf8 (System.ReadOnlySpan<byte> text);
public void AddUtf8 (IntPtr text, int textLength);
public void AddUtf8 (byte[] bytes, int itemOffset, int itemLength);
public void AddUtf8 (System.ReadOnlySpan<byte> text, int itemOffset, int itemLength);
public void AddUtf8 (IntPtr text, int textLength, int itemOffset, int itemLength);
public void Append (Buffer buffer);
public void Append (Buffer buffer, int start, int end);
public void DeserializeGlyphs (string data);
public void DeserializeGlyphs (string data, Font font);
public void DeserializeGlyphs (string data, Font font, SerializeFormat format);
protected override void DisposeHandler ();
public System.ReadOnlySpan<GlyphInfo> GetGlyphInfoReferences ();
public System.ReadOnlySpan<GlyphPosition> GetGlyphPositionReferences ();
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

Added constructor:

```csharp
public Face (Blob blob, int index);
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
public int GlyphCount { get; set; }
public bool IsImmutable { get; }
public Tag[] Tables { get; }
```

Removed method:

```csharp
protected override void Dispose (bool disposing);
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
public Feature (Tag tag, bool isEnabled, uint start, uint end);
```

Modified properties:

```diff
-public uint Tag { get; set; }
+public Tag Tag { get; set; }
```

Obsoleted properties:

```diff
 [Obsolete ("Use IsEnabled instead.")]
 public uint Value { get; set; }
```

Added property:

```csharp
public bool IsEnabled { get; set; }
```

Added methods:

```csharp
public static Feature FromString (string s);
public override string ToString ();
```


#### Type Changed: HarfBuzzSharp.Font

Added properties:

```csharp
public FontExtents HorizontalFontExtents { get; }
public string[] SupportedShapers { get; }
public FontExtents VerticalFontExtents { get; }
```

Removed method:

```csharp
protected override void Dispose (bool disposing);
```

Added methods:

```csharp
protected override void DisposeHandler ();
public uint GetGlyph (int unicode);
public uint GetGlyph (uint unicode);
public uint GetGlyph (int unicode, int variationSelector);
public uint GetGlyph (uint unicode, uint variationSelector);
public GlyphExtents GetGlyphExtents (int glyph);
public GlyphExtents GetGlyphExtents (uint glyph);
public int GetHorizontalGlyphAdvance (int glyph);
public int GetHorizontalGlyphAdvance (uint glyph);
public int[] GetHorizontalGlyphAdvances (int[] glyphs);
public int[] GetHorizontalGlyphAdvances (System.ReadOnlySpan<int> glyphs);
public int[] GetHorizontalGlyphAdvances (System.ReadOnlySpan<uint> glyphs);
public int[] GetHorizontalGlyphAdvances (uint[] glyphs);
public int[] GetHorizontalGlyphAdvances (int count, IntPtr firstGlyph);
public void GetHorizontalGlyphOrigin (int glyph, out int xOrigin, out int yOrigin);
public void GetHorizontalGlyphOrigin (uint glyph, out int xOrigin, out int yOrigin);
public int GetVerticalGlyphAdvance (int glyph);
public int GetVerticalGlyphAdvance (uint glyph);
public int[] GetVerticalGlyphAdvances (int[] glyphs);
public int[] GetVerticalGlyphAdvances (System.ReadOnlySpan<int> glyphs);
public int[] GetVerticalGlyphAdvances (System.ReadOnlySpan<uint> glyphs);
public int[] GetVerticalGlyphAdvances (uint[] glyphs);
public int[] GetVerticalGlyphAdvances (int count, IntPtr firstGlyph);
public void GetVerticalGlyphOrigin (int glyph, out int xOrigin, out int yOrigin);
public void GetVerticalGlyphOrigin (uint glyph, out int xOrigin, out int yOrigin);
public void Shape (Buffer buffer, System.Collections.Generic.IReadOnlyList<Feature> features, System.Collections.Generic.IReadOnlyList<string> shapers);
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

#### New Type: HarfBuzzSharp.ContentType

```csharp
[Serializable]
public enum ContentType {
	Glyphs = 2,
	Invalid = 0,
	Unicode = 1,
}
```

#### New Type: HarfBuzzSharp.Flags

```csharp
[Serializable]
[Flags]
public enum Flags {
	Bot = 1,
	Default = 0,
	Eot = 2,
	PreserveDefaultIgnorables = 4,
	RemoveDefaultIgnorables = 8,
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

#### New Type: HarfBuzzSharp.GlyphFlags

```csharp
[Serializable]
[Flags]
public enum GlyphFlags {
	Defined = 1,
	UnsafeToBreak = 1,
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
	protected bool Equals (Language other);
	public override int GetHashCode ();
	public override string ToString ();
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
	public static Script FromString (string str);
	public override int GetHashCode ();
	public override string ToString ();
	public static uint op_Implicit (Script script);
	public static Script op_Implicit (uint tag);
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
	public Tag (string tag);
	public Tag (char c1, char c2, char c3, char c4);
	// fields
	public static Tag Max;
	public static Tag MaxSigned;
	public static Tag None;
	// methods
	public virtual bool Equals (Tag other);
	public override bool Equals (object obj);
	public override int GetHashCode ();
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
	// properties
	public static UnicodeFunctions Default { get; }
	public static UnicodeFunctions Empty { get; }
	public bool IsImmutable { get; }
	// methods
	protected override void DisposeHandler ();
	public UnicodeCombiningClass GetCombiningClass (int unicode);
	public UnicodeCombiningClass GetCombiningClass (uint unicode);
	public UnicodeGeneralCategory GetGeneralCategory (int unicode);
	public UnicodeGeneralCategory GetGeneralCategory (uint unicode);
	public Script GetScript (int unicode);
	public Script GetScript (uint unicode);
	public void MakeImmutable ();
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


