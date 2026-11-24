# API diff: SkiaSharp.Views.Forms.dll

## SkiaSharp.Views.Forms.dll

> Assembly Version Changed: 1.55.0.0 vs 0.0.0.0

### New Namespace SkiaSharp.Views.Forms

#### New Type: SkiaSharp.Views.Forms.Extensions

```csharp
public static class Extensions {
	// methods
	public static Xamarin.Forms.Color ToFormsColor (this SkiaSharp.SKColor color);
	public static Xamarin.Forms.Point ToFormsPoint (this SkiaSharp.SKPoint point);
	public static Xamarin.Forms.Point ToFormsPoint (this SkiaSharp.SKPointI point);
	public static Xamarin.Forms.Rectangle ToFormsRect (this SkiaSharp.SKRect rect);
	public static Xamarin.Forms.Rectangle ToFormsRect (this SkiaSharp.SKRectI rect);
	public static Xamarin.Forms.Size ToFormsSize (this SkiaSharp.SKSize size);
	public static Xamarin.Forms.Size ToFormsSize (this SkiaSharp.SKSizeI size);
	public static SkiaSharp.SKColor ToSKColor (this Xamarin.Forms.Color color);
	public static SkiaSharp.SKPoint ToSKPoint (this Xamarin.Forms.Point point);
	public static SkiaSharp.SKRect ToSKRect (this Xamarin.Forms.Rectangle rect);
	public static SkiaSharp.SKSize ToSKSize (this Xamarin.Forms.Size size);
}
```

#### New Type: SkiaSharp.Views.Forms.SKCanvasView

```csharp
public class SKCanvasView : Xamarin.Forms.View, System.ComponentModel.INotifyPropertyChanged, Xamarin.Forms.IAnimatable, Xamarin.Forms.IElementController, Xamarin.Forms.IGestureRecognizers, Xamarin.Forms.ITabStopElement, Xamarin.Forms.IViewController, Xamarin.Forms.IVisualElementController, Xamarin.Forms.Internals.IDynamicResourceHandler, Xamarin.Forms.Internals.IGestureController, Xamarin.Forms.Internals.INameScope, Xamarin.Forms.Internals.INavigationProxy {
	// constructors
	public SKCanvasView ();
	// properties
	public SkiaSharp.SKSize CanvasSize { get; }
	// events
	public event System.EventHandler<SKPaintSurfaceEventArgs> PaintSurface;
	// methods
	public void InvalidateSurface ();
	protected virtual void OnPaintSurface (SKPaintSurfaceEventArgs e);
}
```

#### New Type: SkiaSharp.Views.Forms.SKGLView

```csharp
public class SKGLView : Xamarin.Forms.View, System.ComponentModel.INotifyPropertyChanged, Xamarin.Forms.IAnimatable, Xamarin.Forms.IElementController, Xamarin.Forms.IGestureRecognizers, Xamarin.Forms.ITabStopElement, Xamarin.Forms.IViewController, Xamarin.Forms.IVisualElementController, Xamarin.Forms.Internals.IDynamicResourceHandler, Xamarin.Forms.Internals.IGestureController, Xamarin.Forms.Internals.INameScope, Xamarin.Forms.Internals.INavigationProxy {
	// constructors
	public SKGLView ();
	// fields
	public static Xamarin.Forms.BindableProperty HasRenderLoopProperty;
	// properties
	public SkiaSharp.SKSize CanvasSize { get; }
	public bool HasRenderLoop { get; set; }
	// events
	public event System.EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;
	// methods
	public void InvalidateSurface ();
	protected virtual void OnPaintSurface (SKPaintGLSurfaceEventArgs e);
}
```

#### New Type: SkiaSharp.Views.Forms.SKPaintGLSurfaceEventArgs

```csharp
public class SKPaintGLSurfaceEventArgs : System.EventArgs {
	// constructors
	public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTargetDesc renderTarget);
	// properties
	public SkiaSharp.GRBackendRenderTargetDesc RenderTarget { get; }
	public SkiaSharp.SKSurface Surface { get; }
}
```

#### New Type: SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs

```csharp
public class SKPaintSurfaceEventArgs : System.EventArgs {
	// constructors
	public SKPaintSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.SKImageInfo info);
	// properties
	public SkiaSharp.SKImageInfo Info { get; }
	public SkiaSharp.SKSurface Surface { get; }
}
```

