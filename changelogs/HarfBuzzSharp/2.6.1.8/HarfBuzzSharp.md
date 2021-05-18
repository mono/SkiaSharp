# API diff: HarfBuzzSharp.dll

## HarfBuzzSharp.dll

### Namespace HarfBuzzSharp

#### Type Changed: HarfBuzzSharp.Feature

Added interface:

```csharp
System.IEquatable<Feature>
```

Added methods:

```csharp
public virtual bool Equals (Feature obj);
public override bool Equals (object obj);
public override int GetHashCode ();
public static bool op_Equality (Feature left, Feature right);
public static bool op_Inequality (Feature left, Feature right);
```


#### Type Changed: HarfBuzzSharp.FontExtents

Added interface:

```csharp
System.IEquatable<FontExtents>
```

Added methods:

```csharp
public virtual bool Equals (FontExtents obj);
public override bool Equals (object obj);
public override int GetHashCode ();
public static bool op_Equality (FontExtents left, FontExtents right);
public static bool op_Inequality (FontExtents left, FontExtents right);
```


#### Type Changed: HarfBuzzSharp.GlyphExtents

Added interface:

```csharp
System.IEquatable<GlyphExtents>
```

Added methods:

```csharp
public virtual bool Equals (GlyphExtents obj);
public override bool Equals (object obj);
public override int GetHashCode ();
public static bool op_Equality (GlyphExtents left, GlyphExtents right);
public static bool op_Inequality (GlyphExtents left, GlyphExtents right);
```


#### Type Changed: HarfBuzzSharp.GlyphInfo

Added interface:

```csharp
System.IEquatable<GlyphInfo>
```

Added methods:

```csharp
public virtual bool Equals (GlyphInfo obj);
public override bool Equals (object obj);
public override int GetHashCode ();
public static bool op_Equality (GlyphInfo left, GlyphInfo right);
public static bool op_Inequality (GlyphInfo left, GlyphInfo right);
```


#### Type Changed: HarfBuzzSharp.GlyphPosition

Added interface:

```csharp
System.IEquatable<GlyphPosition>
```

Added methods:

```csharp
public virtual bool Equals (GlyphPosition obj);
public override bool Equals (object obj);
public override int GetHashCode ();
public static bool op_Equality (GlyphPosition left, GlyphPosition right);
public static bool op_Inequality (GlyphPosition left, GlyphPosition right);
```


#### Type Changed: HarfBuzzSharp.OpenTypeMetrics

Modified base type:

```diff
-System.ValueType
+System.Object
```

Removed constructor:

```csharp
public OpenTypeMetrics (IntPtr font);
```

Added constructor:

```csharp
public OpenTypeMetrics (Font font);
```


#### New Type: HarfBuzzSharp.BufferDiffFlags

```csharp
[Serializable]
public enum BufferDiffFlags {
	ClusterMismatch = 32,
	CodepointMismatch = 16,
	ContentTypeMismatch = 1,
	DottedCirclePresent = 8,
	Equal = 0,
	GlyphFlagsMismatch = 64,
	LengthMismatch = 2,
	NotdefPresent = 4,
	PositionMismatch = 128,
}
```

#### New Type: HarfBuzzSharp.OpenTypeColorLayer

```csharp
public struct OpenTypeColorLayer, System.IEquatable<OpenTypeColorLayer> {
	// properties
	public uint ColorIndex { get; set; }
	public uint Glyph { get; set; }
	// methods
	public virtual bool Equals (OpenTypeColorLayer obj);
	public override bool Equals (object obj);
	public override int GetHashCode ();
	public static bool op_Equality (OpenTypeColorLayer left, OpenTypeColorLayer right);
	public static bool op_Inequality (OpenTypeColorLayer left, OpenTypeColorLayer right);
}
```

#### New Type: HarfBuzzSharp.OpenTypeColorPaletteFlags

```csharp
[Serializable]
public enum OpenTypeColorPaletteFlags {
	Default = 0,
	UsableWithDarkBackground = 2,
	UsableWithLightBackground = 1,
}
```

#### New Type: HarfBuzzSharp.OpenTypeLayoutBaselineTag

```csharp
[Serializable]
public enum OpenTypeLayoutBaselineTag {
	Hanging = 1751215719,
	IdeoEmboxBottomOrLeft = 1768187247,
	IdeoEmboxTopOrRight = 1768191088,
	IdeoFaceBottomOrLeft = 1768121954,
	IdeoFaceTopOrRight = 1768121972,
	Math = 1835103336,
	Roman = 1919905134,
}
```

#### New Type: HarfBuzzSharp.OpenTypeLayoutGlyphClass

```csharp
[Serializable]
public enum OpenTypeLayoutGlyphClass {
	BaseGlyph = 1,
	Component = 4,
	Ligature = 2,
	Mark = 3,
	Unclassified = 0,
}
```

#### New Type: HarfBuzzSharp.OpenTypeMathConstant

```csharp
[Serializable]
public enum OpenTypeMathConstant {
	AccentBaseHeight = 6,
	AxisHeight = 5,
	DelimitedSubFormulaMinHeight = 2,
	DisplayOperatorMinHeight = 3,
	FlattenedAccentBaseHeight = 7,
	FractionDenomDisplayStyleGapMin = 40,
	FractionDenominatorDisplayStyleShiftDown = 35,
	FractionDenominatorGapMin = 39,
	FractionDenominatorShiftDown = 34,
	FractionNumDisplayStyleGapMin = 37,
	FractionNumeratorDisplayStyleShiftUp = 33,
	FractionNumeratorGapMin = 36,
	FractionNumeratorShiftUp = 32,
	FractionRuleThickness = 38,
	LowerLimitBaselineDropMin = 21,
	LowerLimitGapMin = 20,
	MathLeading = 4,
	OverbarExtraAscender = 45,
	OverbarRuleThickness = 44,
	OverbarVerticalGap = 43,
	RadicalDegreeBottomRaisePercent = 55,
	RadicalDisplayStyleVerticalGap = 50,
	RadicalExtraAscender = 52,
	RadicalKernAfterDegree = 54,
	RadicalKernBeforeDegree = 53,
	RadicalRuleThickness = 51,
	RadicalVerticalGap = 49,
	ScriptPercentScaleDown = 0,
	ScriptScriptPercentScaleDown = 1,
	SkewedFractionHorizontalGap = 41,
	SkewedFractionVerticalGap = 42,
	SpaceAfterScript = 17,
	StackBottomDisplayStyleShiftDown = 25,
	StackBottomShiftDown = 24,
	StackDisplayStyleGapMin = 27,
	StackGapMin = 26,
	StackTopDisplayStyleShiftUp = 23,
	StackTopShiftUp = 22,
	StretchStackBottomShiftDown = 29,
	StretchStackGapAboveMin = 30,
	StretchStackGapBelowMin = 31,
	StretchStackTopShiftUp = 28,
	SubSuperscriptGapMin = 15,
	SubscriptBaselineDropMin = 10,
	SubscriptShiftDown = 8,
	SubscriptTopMax = 9,
	SuperscriptBaselineDropMax = 14,
	SuperscriptBottomMaxWithSubscript = 16,
	SuperscriptBottomMin = 13,
	SuperscriptShiftUp = 11,
	SuperscriptShiftUpCramped = 12,
	UnderbarExtraDescender = 48,
	UnderbarRuleThickness = 47,
	UnderbarVerticalGap = 46,
	UpperLimitBaselineRiseMin = 19,
	UpperLimitGapMin = 18,
}
```

#### New Type: HarfBuzzSharp.OpenTypeMathGlyphPart

```csharp
public struct OpenTypeMathGlyphPart, System.IEquatable<OpenTypeMathGlyphPart> {
	// properties
	public int EndConnectorLength { get; set; }
	public OpenTypeMathGlyphPartFlags Flags { get; set; }
	public int FullAdvance { get; set; }
	public uint Glyph { get; set; }
	public int StartConnectorLength { get; set; }
	// methods
	public virtual bool Equals (OpenTypeMathGlyphPart obj);
	public override bool Equals (object obj);
	public override int GetHashCode ();
	public static bool op_Equality (OpenTypeMathGlyphPart left, OpenTypeMathGlyphPart right);
	public static bool op_Inequality (OpenTypeMathGlyphPart left, OpenTypeMathGlyphPart right);
}
```

#### New Type: HarfBuzzSharp.OpenTypeMathGlyphPartFlags

```csharp
[Serializable]
public enum OpenTypeMathGlyphPartFlags {
	Extender = 1,
}
```

#### New Type: HarfBuzzSharp.OpenTypeMathGlyphVariant

```csharp
public struct OpenTypeMathGlyphVariant, System.IEquatable<OpenTypeMathGlyphVariant> {
	// properties
	public int Advance { get; set; }
	public uint Glyph { get; set; }
	// methods
	public virtual bool Equals (OpenTypeMathGlyphVariant obj);
	public override bool Equals (object obj);
	public override int GetHashCode ();
	public static bool op_Equality (OpenTypeMathGlyphVariant left, OpenTypeMathGlyphVariant right);
	public static bool op_Inequality (OpenTypeMathGlyphVariant left, OpenTypeMathGlyphVariant right);
}
```

#### New Type: HarfBuzzSharp.OpenTypeMathKern

```csharp
[Serializable]
public enum OpenTypeMathKern {
	BottomLeft = 3,
	BottomRight = 2,
	TopLeft = 1,
	TopRight = 0,
}
```

#### New Type: HarfBuzzSharp.OpenTypeMetaTag

```csharp
[Serializable]
public enum OpenTypeMetaTag {
	DesignLanguages = 1684827751,
	SupportedLanguages = 1936485991,
}
```

#### New Type: HarfBuzzSharp.OpenTypeNameEntry

```csharp
public struct OpenTypeNameEntry, System.IEquatable<OpenTypeNameEntry> {
	// properties
	public IntPtr Language { get; set; }
	public OpenTypeNameId NameId { get; set; }
	public int Var { get; set; }
	// methods
	public virtual bool Equals (OpenTypeNameEntry obj);
	public override bool Equals (object obj);
	public override int GetHashCode ();
	public static bool op_Equality (OpenTypeNameEntry left, OpenTypeNameEntry right);
	public static bool op_Inequality (OpenTypeNameEntry left, OpenTypeNameEntry right);
}
```

#### New Type: HarfBuzzSharp.OpenTypeNameId

```csharp
[Serializable]
public enum OpenTypeNameId {
	CidFindFontName = 20,
	Copyright = 0,
	DarkBackground = 24,
	Description = 10,
	Designer = 9,
	DesignerUrl = 12,
	FontFamily = 1,
	FontSubfamily = 2,
	FullName = 4,
	Invalid = 65535,
	License = 13,
	LicenseUrl = 14,
	LightBackground = 23,
	MacFullName = 18,
	Manufacturer = 8,
	PostscriptName = 6,
	SampleText = 19,
	Trademark = 7,
	TypographicFamily = 16,
	TypographicSubfamily = 17,
	UniqueId = 3,
	VariationsPostscriptPrefix = 25,
	VendorUrl = 11,
	VersionString = 5,
	WwsFamily = 21,
	WwsSubfamily = 22,
}
```

#### New Type: HarfBuzzSharp.OpenTypeVarAxis

```csharp
public struct OpenTypeVarAxis, System.IEquatable<OpenTypeVarAxis> {
	// properties
	public float DefaultValue { get; set; }
	public float MaxValue { get; set; }
	public float MinValue { get; set; }
	public OpenTypeNameId NameId { get; set; }
	public uint Tag { get; set; }
	// methods
	public virtual bool Equals (OpenTypeVarAxis obj);
	public override bool Equals (object obj);
	public override int GetHashCode ();
	public static bool op_Equality (OpenTypeVarAxis left, OpenTypeVarAxis right);
	public static bool op_Inequality (OpenTypeVarAxis left, OpenTypeVarAxis right);
}
```

#### New Type: HarfBuzzSharp.OpenTypeVarAxisFlags

```csharp
[Serializable]
public enum OpenTypeVarAxisFlags {
	Hidden = 1,
}
```

#### New Type: HarfBuzzSharp.OpenTypeVarAxisInfo

```csharp
public struct OpenTypeVarAxisInfo, System.IEquatable<OpenTypeVarAxisInfo> {
	// properties
	public uint AxisIndex { get; set; }
	public float DefaultValue { get; set; }
	public OpenTypeVarAxisFlags Flags { get; set; }
	public float MaxValue { get; set; }
	public float MinValue { get; set; }
	public OpenTypeNameId NameId { get; set; }
	public uint Tag { get; set; }
	// methods
	public virtual bool Equals (OpenTypeVarAxisInfo obj);
	public override bool Equals (object obj);
	public override int GetHashCode ();
	public static bool op_Equality (OpenTypeVarAxisInfo left, OpenTypeVarAxisInfo right);
	public static bool op_Inequality (OpenTypeVarAxisInfo left, OpenTypeVarAxisInfo right);
}
```

#### New Type: HarfBuzzSharp.Variation

```csharp
public struct Variation, System.IEquatable<Variation> {
	// properties
	public uint Tag { get; set; }
	public float Value { get; set; }
	// methods
	public virtual bool Equals (Variation obj);
	public override bool Equals (object obj);
	public override int GetHashCode ();
	public static bool op_Equality (Variation left, Variation right);
	public static bool op_Inequality (Variation left, Variation right);
}
```


