# API diff: SkiaSharp.Views.Maui.Core.dll

## SkiaSharp.Views.Maui.Core.dll

> Assembly Version Changed: 3.116.0.0 vs 2.88.0.0

### Namespace SkiaSharp.Views.Maui

#### Type Changed: SkiaSharp.Views.Maui.SKPaintGLSurfaceEventArgs

Added constructors:

```csharp
public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTarget renderTarget, SkiaSharp.GRSurfaceOrigin origin, SkiaSharp.SKImageInfo info);
public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTarget renderTarget, SkiaSharp.GRSurfaceOrigin origin, SkiaSharp.SKImageInfo info, SkiaSharp.SKImageInfo rawInfo);
```

Added properties:

```csharp
public SkiaSharp.SKImageInfo Info { get; }
public SkiaSharp.SKImageInfo RawInfo { get; }
```


#### New Type: SkiaSharp.Views.Maui.ISKGLView

```csharp
public interface ISKGLView : Microsoft.Maui.IElement, Microsoft.Maui.ITransform, Microsoft.Maui.IView {
	// properties
	public virtual SkiaSharp.SKSize CanvasSize { get; }
	public virtual bool EnableTouchEvents { get; }
	public virtual SkiaSharp.GRContext GRContext { get; }
	public virtual bool HasRenderLoop { get; }
	public virtual bool IgnorePixelScaling { get; }
	// methods
	public virtual void InvalidateSurface ();
	public virtual void OnCanvasSizeChanged (SkiaSharp.SKSizeI size);
	public virtual void OnGRContextChanged (SkiaSharp.GRContext context);
	public virtual void OnPaintSurface (SKPaintGLSurfaceEventArgs e);
	public virtual void OnTouch (SKTouchEventArgs e);
}
```


### Namespace SkiaSharp.Views.Maui.Handlers

#### New Type: SkiaSharp.Views.Maui.Handlers.SKGLViewHandler

```csharp
public class SKGLViewHandler : Microsoft.Maui.Handlers.ViewHandler`2[SkiaSharp.Views.Maui.ISKGLView,System.Object] {
	// constructors
	public SKGLViewHandler ();
	public SKGLViewHandler (Microsoft.Maui.PropertyMapper mapper, Microsoft.Maui.CommandMapper commands);
	// fields
	public static Microsoft.Maui.CommandMapper<SkiaSharp.Views.Maui.ISKGLView,SkiaSharp.Views.Maui.Handlers.SKGLViewHandler> SKGLViewCommandMapper;
	public static Microsoft.Maui.PropertyMapper<SkiaSharp.Views.Maui.ISKGLView,SkiaSharp.Views.Maui.Handlers.SKGLViewHandler> SKGLViewMapper;
	// methods
	protected override object CreatePlatformView ();
	public static void MapEnableTouchEvents (SKGLViewHandler handler, SkiaSharp.Views.Maui.ISKGLView view);
	public static void MapHasRenderLoop (SKGLViewHandler handler, SkiaSharp.Views.Maui.ISKGLView view);
	public static void MapIgnorePixelScaling (SKGLViewHandler handler, SkiaSharp.Views.Maui.ISKGLView view);
	public static void OnInvalidateSurface (SKGLViewHandler handler, SkiaSharp.Views.Maui.ISKGLView view, object args);
}
```


