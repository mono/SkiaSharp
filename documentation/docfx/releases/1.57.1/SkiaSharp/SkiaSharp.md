# API diff: SkiaSharp.dll

## SkiaSharp.dll

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.SKStream

Added methods:

```csharp
public IntPtr GetMemoryBase ();
public int Read (IntPtr buffer, int size);
```


#### Type Changed: SkiaSharp.SKTypeface

Added property:

```csharp
public int UnitsPerEm { get; }
```

Added methods:

```csharp
public SKStreamAsset OpenStream ();
public SKStreamAsset OpenStream (out int ttcIndex);
```



