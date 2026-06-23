# API diff: SkiaSharp.Views.Tizen.dll

## SkiaSharp.Views.Tizen.dll

### New Namespace SkiaSharp.Views.Tizen.NUI

#### New Type: SkiaSharp.Views.Tizen.NUI.CustomRenderingView

```csharp
public abstract class CustomRenderingView : Tizen.NUI.BaseComponents.ImageView, System.ComponentModel.INotifyPropertyChanged, System.IDisposable, Tizen.NUI.Binding.IResourcesProvider, Tizen.NUI.Binding.Internals.IDynamicResourceHandler, Tizen.NUI.Binding.Internals.INameScope {
	// constructors
	protected CustomRenderingView ();
	// properties
	protected System.Threading.SynchronizationContext MainloopContext { get; }
	// events
	public event System.EventHandler<SkiaSharp.Views.Tizen.SKPaintSurfaceEventArgs> PaintSurface;
	// methods
	public void Invalidate ();
	protected virtual void OnDrawFrame ();
	protected virtual void OnResized ();
	protected void SendPaintSurface (SkiaSharp.Views.Tizen.SKPaintSurfaceEventArgs e);
}
```

#### New Type: SkiaSharp.Views.Tizen.NUI.SKCanvasView

```csharp
public class SKCanvasView : SkiaSharp.Views.Tizen.NUI.CustomRenderingView, System.ComponentModel.INotifyPropertyChanged, System.IDisposable, Tizen.NUI.Binding.IResourcesProvider, Tizen.NUI.Binding.Internals.IDynamicResourceHandler, Tizen.NUI.Binding.Internals.INameScope {
	// constructors
	public SKCanvasView ();
	// properties
	public bool IgnorePixelScaling { get; set; }
	// methods
	protected override void OnDrawFrame ();
	protected override void OnResized ();
}
```

#### New Type: SkiaSharp.Views.Tizen.NUI.SKGLSurfaceView

```csharp
public class SKGLSurfaceView : SkiaSharp.Views.Tizen.NUI.CustomRenderingView, System.ComponentModel.INotifyPropertyChanged, System.IDisposable, Tizen.NUI.Binding.IResourcesProvider, Tizen.NUI.Binding.Internals.IDynamicResourceHandler, Tizen.NUI.Binding.Internals.INameScope {
	// constructors
	public SKGLSurfaceView ();
	// methods
	protected override void Dispose (bool disposing);
	protected override void OnDrawFrame ();
	protected override void OnResized ();
}
```

