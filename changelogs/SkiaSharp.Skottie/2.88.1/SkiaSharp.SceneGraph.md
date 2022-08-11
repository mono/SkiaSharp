# API diff: SkiaSharp.SceneGraph.dll

## SkiaSharp.SceneGraph.dll

> Assembly Version Changed: 2.88.0.0 vs 0.0.0.0

### New Namespace SkiaSharp.SceneGraph

#### New Type: SkiaSharp.SceneGraph.InvalidationController

```csharp
public class InvalidationController : SkiaSharp.SKObject, System.IDisposable {
	// constructors
	public InvalidationController ();
	// properties
	public SkiaSharp.SKRect Bounds { get; }
	// methods
	public void Begin ();
	protected override void DisposeNative ();
	public void End ();
	public void Invalidate (SkiaSharp.SKRect rect, SkiaSharp.SKMatrix matrix);
	public void Reset ();
}
```

