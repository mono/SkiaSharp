# API diff: HarfBuzzSharp.dll

## HarfBuzzSharp.dll

### Namespace HarfBuzzSharp

#### Type Changed: HarfBuzzSharp.Face

Added properties:

```csharp
public bool HasColorLayers { get; }
public bool HasColorPng { get; }
public bool HasColorSvg { get; }
public bool HasPalettes { get; }
public bool HasVariationData { get; }
public int NamedInstanceCount { get; }
public int PaletteCount { get; }
public int VariationAxisCount { get; }
public OpenTypeVarAxisInfo[] VariationAxisInfos { get; }
```

Added methods:

```csharp
public float[] GetNamedInstanceDesignCoords (int instanceIndex);
public int GetNamedInstanceDesignCoords (int instanceIndex, System.Span<float> coords);
public int GetNamedInstanceDesignCoordsCount (int instanceIndex);
public OpenTypeNameId GetNamedInstancePostScriptNameId (int instanceIndex);
public OpenTypeNameId GetNamedInstanceSubfamilyNameId (int instanceIndex);
public OpenTypeNameId GetPaletteColorNameId (int colorIndex);
public HBColor[] GetPaletteColors (int paletteIndex);
public int GetPaletteColors (int paletteIndex, System.Span<HBColor> colors);
public OpenTypeColorPaletteFlags GetPaletteFlags (int paletteIndex);
public OpenTypeNameId GetPaletteNameId (int paletteIndex);
public int GetVariationAxisInfos (System.Span<OpenTypeVarAxisInfo> axes);
public bool TryFindVariationAxis (Tag tag, out OpenTypeVarAxisInfo axisInfo);
```


#### Type Changed: HarfBuzzSharp.Font

Added property:

```csharp
public int[] VariationCoordsNormalized { get; }
```

Added methods:

```csharp
public int GetVariationCoordsNormalized (System.Span<int> coords);
public void SetVariationCoordsDesign (System.ReadOnlySpan<float> coords);
public void SetVariationCoordsNormalized (System.ReadOnlySpan<int> coords);
public void SetVariationNamedInstance (int instanceIndex);
public void SetVariations (System.ReadOnlySpan<Variation> variations);
```


#### New Type: HarfBuzzSharp.HBColor

```csharp
public struct HBColor, System.IEquatable<HBColor> {
	// constructors
	public HBColor (uint value);
	public HBColor (byte red, byte green, byte blue, byte alpha);
	// properties
	public byte Alpha { get; }
	public byte Blue { get; }
	public byte Green { get; }
	public byte Red { get; }
	public uint Value { get; }
	// methods
	public virtual bool Equals (HBColor other);
	public override bool Equals (object obj);
	public override int GetHashCode ();
	public override string ToString ();
	public static bool op_Equality (HBColor left, HBColor right);
	public static HBColor op_Explicit (uint value);
	public static uint op_Implicit (HBColor color);
	public static bool op_Inequality (HBColor left, HBColor right);
}
```


