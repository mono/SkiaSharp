# API diff: SkiaSharp.dll

## SkiaSharp.dll

> Assembly Version Changed: 4.151.0.0 vs 4.150.0.0

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.SKColor

Added methods:

```csharp
public static SKColor Parse (System.ReadOnlySpan<char> hexString);
public static bool TryParse (System.ReadOnlySpan<char> hexString, out SKColor color);
```


#### Type Changed: SkiaSharp.SKFourByteTag

Added method:

```csharp
public static SKFourByteTag Parse (System.ReadOnlySpan<char> tag);
```
