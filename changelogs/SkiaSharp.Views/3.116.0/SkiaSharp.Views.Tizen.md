# API diff: SkiaSharp.Views.Tizen.dll

## SkiaSharp.Views.Tizen.dll

> Assembly Version Changed: 3.116.0.0 vs 2.88.0.0

### Namespace SkiaSharp.Views.Tizen

#### Type Changed: SkiaSharp.Views.Tizen.SKPaintGLSurfaceEventArgs

Removed constructors:

```csharp
[Obsolete ("Use SKPaintGLSurfaceEventArgs(SKSurface, GRBackendRenderTarget, SKColorType, GRSurfaceOrigin) instead.")]
public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTargetDesc renderTarget);

[Obsolete ("Use SKPaintGLSurfaceEventArgs(SKSurface, GRBackendRenderTarget, GRSurfaceOrigin, SKColorType) instead.")]
public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTarget renderTarget, SkiaSharp.GRSurfaceOrigin origin, SkiaSharp.SKColorType colorType, SkiaSharp.GRGlFramebufferInfo glInfo);
```

Removed property:

```csharp
[Obsolete ("Use BackendRenderTarget instead.")]
public SkiaSharp.GRBackendRenderTargetDesc RenderTarget { get; }
```


#### Type Changed: SkiaSharp.Views.Tizen.TizenExtensions

Added methods:

```csharp
public static SkiaSharp.SKColorF ToSKColorF (this Tizen.NUI.Color color);
public static SkiaSharp.SKPoint ToSKPoint (this Tizen.NUI.Position point);
public static SkiaSharp.SKPointI ToSKPointI (this Tizen.NUI.Position2D point);
public static SkiaSharp.SKRect ToSKRect (this Tizen.NUI.Rectangle rect);
public static SkiaSharp.SKRectI ToSKRectI (this Tizen.NUI.Rectangle rect);
public static SkiaSharp.SKSize ToSKSize (this Tizen.NUI.Size size);
public static SkiaSharp.SKSizeI ToSKSizeI (this Tizen.NUI.Size2D size);
```


#### Removed Type SkiaSharp.Views.Tizen.Extensions

### New Namespace SkiaSharp.Views.Tizen.NUI

#### New Type: SkiaSharp.Views.Tizen.NUI.CustomRenderingView

```csharp
public abstract class CustomRenderingView : Tizen.NUI.BaseComponents.ImageView, System.ComponentModel.INotifyPropertyChanged, System.IDisposable, Tizen.NUI.Binding.IResourcesProvider, Tizen.NUI.Binding.Internals.IDynamicResourceHandler, Tizen.NUI.Binding.Internals.INameScope {
	// constructors
	protected CustomRenderingView ();
	// properties
	public SkiaSharp.SKSize CanvasSize { get; }
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

