# API diff: SkiaSharp.dll

## SkiaSharp.dll

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.GRContextOptions

Removed properties:

```csharp
public bool DisableDistanceFieldPaths { get; set; }
public bool ForceSoftwarePathMasks { get; set; }
```

Added properties:

```csharp
public bool DisableGpuYuvConversion { get; set; }
public GRContextOptionsGpuPathRenderers GpuPathRenderers { get; set; }
public bool SuppressPathRendering { get; set; }
```


#### Type Changed: SkiaSharp.SKRectI

Added methods:

```csharp
public static SKRectI Ceiling (SKRect value, bool outwards);
public static SKRectI Floor (SKRect value);
public static SKRectI Floor (SKRect value, bool inwards);
```


#### New Type: SkiaSharp.GRContextOptionsGpuPathRenderers

```csharp
[Serializable]
public enum GRContextOptionsGpuPathRenderers {
	AaConvex = 16,
	AaHairline = 8,
	AaLinearizing = 32,
	All = 1023,
	DashLine = 1,
	Default = 512,
	DistanceField = 128,
	Msaa = 4,
	None = 0,
	Pls = 64,
	StencilAndCover = 2,
	Tessellating = 256,
}
```


