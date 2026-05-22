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
public uint[] GetPaletteColors (int paletteIndex);
public int GetPaletteColors (int paletteIndex, System.Span<uint> colors);
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



