# API diff: SkiaSharp.Resources.dll

## SkiaSharp.Resources.dll

> Assembly Version Changed: 3.116.0.0 vs 0.0.0.0

### New Namespace SkiaSharp.Resources

#### New Type: SkiaSharp.Resources.CachingResourceProvider

```csharp
public sealed class CachingResourceProvider : SkiaSharp.Resources.ResourceProvider, System.IDisposable {
	// constructors
	public CachingResourceProvider (ResourceProvider resourceProvider);
}
```

#### New Type: SkiaSharp.Resources.DataUriResourceProvider

```csharp
public sealed class DataUriResourceProvider : SkiaSharp.Resources.ResourceProvider, System.IDisposable {
	// constructors
	public DataUriResourceProvider (bool preDecode);
	public DataUriResourceProvider (ResourceProvider fallbackProvider, bool preDecode);
}
```

#### New Type: SkiaSharp.Resources.FileResourceProvider

```csharp
public sealed class FileResourceProvider : SkiaSharp.Resources.ResourceProvider, System.IDisposable {
	// constructors
	public FileResourceProvider (string baseDirectory, bool preDecode);
}
```

#### New Type: SkiaSharp.Resources.ResourceProvider

```csharp
public abstract class ResourceProvider : SkiaSharp.SKObject, System.IDisposable {
	// methods
	public SkiaSharp.SKData Load (string resourceName);
	public SkiaSharp.SKData Load (string resourcePath, string resourceName);
}
```

