# API diff: SkiaSharp.HarfBuzz.dll

## SkiaSharp.HarfBuzz.dll

### Namespace SkiaSharp.HarfBuzz

#### Type Changed: SkiaSharp.HarfBuzz.SKShaper+Result

Added constructor:

```csharp
public SKShaper.Result (uint[] codepoints, uint[] clusters, SkiaSharp.SKPoint[] points, float width);
```

Added property:

```csharp
public float Width { get; }
```

