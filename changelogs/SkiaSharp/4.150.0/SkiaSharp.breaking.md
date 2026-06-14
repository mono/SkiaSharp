# API diff: SkiaSharp.dll

## SkiaSharp.dll

> Assembly Version Changed: 4.150.0.0 vs 3.119.0.0

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.GRVkImageInfo

Modified properties:

```diff
-public GrVkYcbcrConversionInfo YcbcrConversionInfo { get; set; }
+public GRVkYcbcrConversionInfo YcbcrConversionInfo { get; set; }
```


#### Type Changed: SkiaSharp.GrVkYcbcrConversionInfo

Removed interface:

```csharp
System.IEquatable<GrVkYcbcrConversionInfo>
```

Removed methods:

```csharp
public virtual bool Equals (GrVkYcbcrConversionInfo obj);
public static bool op_Equality (GrVkYcbcrConversionInfo left, GrVkYcbcrConversionInfo right);
public static bool op_Inequality (GrVkYcbcrConversionInfo left, GrVkYcbcrConversionInfo right);
```


#### Type Changed: SkiaSharp.SKNativeObject

Modified properties:

```diff
 protected bool IgnorePublicDispose { get; ---set;--- }
```



