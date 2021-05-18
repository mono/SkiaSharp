# API diff: SkiaSharp.Views.UWP.dll

## SkiaSharp.Views.UWP.dll

### Namespace SkiaSharp.Views.UWP

#### Type Changed: SkiaSharp.Views.UWP.GlobalStaticResources

Obsoleted methods:

```diff
 [Obsolete ("This method is provided for binary backward compatibility. It will always return null.")]
 public static object FindResource (string name);
```

Added methods:

```csharp
public static void RegisterDefaultStyles ();
public static void RegisterResourceDictionariesBySource ();
```



