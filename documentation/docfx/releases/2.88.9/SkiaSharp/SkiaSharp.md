# API diff: SkiaSharp.dll

## SkiaSharp.dll

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.SKTypeface

Added property:

```csharp
public bool HasGetKerningPairAdjustments { get; }
```

Added method:

```csharp
public bool GetKerningPairAdjustments (System.ReadOnlySpan<ushort> glyphs, System.Span<int> adjustments);
```



