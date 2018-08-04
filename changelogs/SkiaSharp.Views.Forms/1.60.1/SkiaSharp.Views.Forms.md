# API diff: SkiaSharp.Views.Forms.dll

## SkiaSharp.Views.Forms.dll

### Namespace SkiaSharp.Views.Forms

#### Type Changed: SkiaSharp.Views.Forms.SKCanvasView

Added interface:

```csharp
ISKCanvasViewController
```


#### New Type: SkiaSharp.Views.Forms.GetPropertyValueEventArgs`1

```csharp
public class GetPropertyValueEventArgs`1 : System.EventArgs {
	// constructors
	public GetPropertyValueEventArgs`1 ();
	// properties
	public T Value { get; set; }
}
```

#### New Type: SkiaSharp.Views.Forms.ISKCanvasViewController

```csharp
public interface ISKCanvasViewController : Xamarin.Forms.IElementController, Xamarin.Forms.IViewController, Xamarin.Forms.IVisualElementController {
	// events
	public event System.EventHandler<SkiaSharp.Views.Forms.GetPropertyValueEventArgs<SkiaSharp.SKSize>> GetCanvasSize;
	public event System.EventHandler SurfaceInvalidated;
	// methods
	public virtual void OnPaintSurface (SKPaintSurfaceEventArgs e);
	public virtual void OnTouch (SKTouchEventArgs e);
}
```


