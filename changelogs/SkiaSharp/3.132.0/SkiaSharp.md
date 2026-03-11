# API diff: SkiaSharp.dll

## SkiaSharp.dll

> Assembly Version Changed: 3.132.0.0 vs 3.119.0.0

### Type Changed: SkiaSharp.SKColorType

Added values:

```csharp
Bgra10101010Xr = 25,
RgbF16F16F16x = 26,
```

Both are 8 bytes/pixel. `Bgra10101010Xr` accepts any alpha type; `RgbF16F16F16x` is opaque-only.
Neither supports GL backend or pixel color readback.

### Type Changed: SkiaSharp.GrVkYcbcrConversionInfo

Added VkComponentMapping fields:

```csharp
public UInt32 ComponentsR;
public UInt32 ComponentsG;
public UInt32 ComponentsB;
public UInt32 ComponentsA;
```

### Behavioral Changes

#### Gray8 Luma Conversion Precision

m132 uses floating-point luma conversion instead of integer:
- Gray8 pixel values shift by +1 (e.g. `0xFF353535` → `0xFF363636`)

#### SKManagedStream Duplicate/Fork Semantics

- Original stream remains readable after `Duplicate()` / `Fork()` (previously threw `InvalidOperationException`)
- Multiple duplicates can be created from the same parent (previously threw on second call)
- Duplicates are independent copies (previously shared the underlying stream)
- Works on all platforms (previously Windows-only)

#### woff2 Typeface Loading

`SKTypeface.FromFile` on woff2 files may return a non-null typeface with empty `FamilyName` instead of null.
