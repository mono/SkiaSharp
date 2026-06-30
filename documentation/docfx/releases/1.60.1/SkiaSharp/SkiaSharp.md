# API diff: SkiaSharp.dll

## SkiaSharp.dll

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.GRGlInterface

Added method:

```csharp
public static GRGlInterface CreateNativeEvasInterface (IntPtr evas);
```


#### New Type: SkiaSharp.SKAutoCoInitialize

```csharp
public class SKAutoCoInitialize : System.IDisposable {
	// constructors
	public SKAutoCoInitialize ();
	// properties
	public bool Initialized { get; }
	// methods
	public virtual void Dispose ();
	public void Uninitialize ();
}
```


