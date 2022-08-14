# API diff: SkiaSharp.Views.Maui.Controls.dll

## SkiaSharp.Views.Maui.Controls.dll

> Assembly Version Changed: 2.88.0.0 vs 0.0.0.0

### New Namespace SkiaSharp.Views.Maui.Controls

#### New Type: SkiaSharp.Views.Maui.Controls.AppHostBuilderExtensions

```csharp
public static class AppHostBuilderExtensions {
	// methods

	[Obsolete]
public static Microsoft.Maui.Hosting.MauiAppBuilder UseSkiaSharpHandlers (this Microsoft.Maui.Hosting.MauiAppBuilder builder);
}
```

#### New Type: SkiaSharp.Views.Maui.Controls.GetPropertyValueEventArgs`1

```csharp
public class GetPropertyValueEventArgs`1 : System.EventArgs {
	// constructors
	public GetPropertyValueEventArgs`1 ();
	// properties
	public T Value { get; set; }
}
```

#### New Type: SkiaSharp.Views.Maui.Controls.ISKCanvasViewController

```csharp
public interface ISKCanvasViewController : Microsoft.Maui.Controls.IElementController, Microsoft.Maui.Controls.IViewController, Microsoft.Maui.Controls.IVisualElementController {
	// events
	public event System.EventHandler<SkiaSharp.Views.Maui.Controls.GetPropertyValueEventArgs<SkiaSharp.SKSize>> GetCanvasSize;
	public event System.EventHandler SurfaceInvalidated;
	// methods
	public virtual void OnPaintSurface (SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs e);
	public virtual void OnTouch (SkiaSharp.Views.Maui.SKTouchEventArgs e);
}
```

#### New Type: SkiaSharp.Views.Maui.Controls.ISKGLViewController

```csharp
public interface ISKGLViewController : Microsoft.Maui.Controls.IElementController, Microsoft.Maui.Controls.IViewController, Microsoft.Maui.Controls.IVisualElementController {
	// events
	public event System.EventHandler<SkiaSharp.Views.Maui.Controls.GetPropertyValueEventArgs<SkiaSharp.SKSize>> GetCanvasSize;
	public event System.EventHandler<SkiaSharp.Views.Maui.Controls.GetPropertyValueEventArgs<SkiaSharp.GRContext>> GetGRContext;
	public event System.EventHandler SurfaceInvalidated;
	// methods
	public virtual void OnPaintSurface (SkiaSharp.Views.Maui.SKPaintGLSurfaceEventArgs e);
	public virtual void OnTouch (SkiaSharp.Views.Maui.SKTouchEventArgs e);
}
```

#### New Type: SkiaSharp.Views.Maui.Controls.SKBitmapImageSource

```csharp
public sealed class SKBitmapImageSource : Microsoft.Maui.Controls.ImageSource, Microsoft.Maui.Controls.IEffectControlProvider, Microsoft.Maui.Controls.IElementController, Microsoft.Maui.Controls.Internals.IDynamicResourceHandler, Microsoft.Maui.Controls.Internals.INameScope, Microsoft.Maui.IElement, Microsoft.Maui.IImageSource, Microsoft.Maui.IVisualTreeElement, SkiaSharp.Views.Maui.ISKBitmapImageSource, System.ComponentModel.INotifyPropertyChanged {
	// constructors
	public SKBitmapImageSource ();
	// fields
	public static Microsoft.Maui.Controls.BindableProperty BitmapProperty;
	// properties
	public override SkiaSharp.SKBitmap Bitmap { get; set; }
	// methods
	public override System.Threading.Tasks.Task<bool> Cancel ();
	protected override void OnPropertyChanged (string propertyName);
	public static SKBitmapImageSource op_Implicit (SkiaSharp.SKBitmap bitmap);
	public static SkiaSharp.SKBitmap op_Implicit (SKBitmapImageSource source);
}
```

#### New Type: SkiaSharp.Views.Maui.Controls.SKCanvasView

```csharp
public class SKCanvasView : Microsoft.Maui.Controls.View, Microsoft.Maui.Controls.IAnimatable, Microsoft.Maui.Controls.IEffectControlProvider, Microsoft.Maui.Controls.IElementController, Microsoft.Maui.Controls.IGestureRecognizers, Microsoft.Maui.Controls.IViewController, Microsoft.Maui.Controls.IVisualElementController, Microsoft.Maui.Controls.Internals.IDynamicResourceHandler, Microsoft.Maui.Controls.Internals.IGestureController, Microsoft.Maui.Controls.Internals.INameScope, Microsoft.Maui.Controls.Internals.INavigationProxy, Microsoft.Maui.HotReload.IHotReloadableView, Microsoft.Maui.IElement, Microsoft.Maui.IPropertyMapperView, Microsoft.Maui.IReplaceableView, Microsoft.Maui.ITransform, Microsoft.Maui.IView, Microsoft.Maui.IVisualTreeElement, ISKCanvasViewController, SkiaSharp.Views.Maui.ISKCanvasView, System.ComponentModel.INotifyPropertyChanged {
	// constructors
	public SKCanvasView ();
	// fields
	public static Microsoft.Maui.Controls.BindableProperty EnableTouchEventsProperty;
	public static Microsoft.Maui.Controls.BindableProperty IgnorePixelScalingProperty;
	// properties
	public virtual SkiaSharp.SKSize CanvasSize { get; }
	public override bool EnableTouchEvents { get; set; }
	public override bool IgnorePixelScaling { get; set; }
	// events
	public event System.EventHandler<SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs> PaintSurface;
	public event System.EventHandler<SkiaSharp.Views.Maui.SKTouchEventArgs> Touch;
	// methods
	public virtual void InvalidateSurface ();
	protected override Microsoft.Maui.SizeRequest OnMeasure (double widthConstraint, double heightConstraint);
	protected virtual void OnPaintSurface (SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs e);
	protected virtual void OnTouch (SkiaSharp.Views.Maui.SKTouchEventArgs e);
}
```

#### New Type: SkiaSharp.Views.Maui.Controls.SKGLView

```csharp
public class SKGLView : Microsoft.Maui.Controls.View, Microsoft.Maui.Controls.IAnimatable, Microsoft.Maui.Controls.IEffectControlProvider, Microsoft.Maui.Controls.IElementController, Microsoft.Maui.Controls.IGestureRecognizers, Microsoft.Maui.Controls.IViewController, Microsoft.Maui.Controls.IVisualElementController, Microsoft.Maui.Controls.Internals.IDynamicResourceHandler, Microsoft.Maui.Controls.Internals.IGestureController, Microsoft.Maui.Controls.Internals.INameScope, Microsoft.Maui.Controls.Internals.INavigationProxy, Microsoft.Maui.HotReload.IHotReloadableView, Microsoft.Maui.IElement, Microsoft.Maui.IPropertyMapperView, Microsoft.Maui.IReplaceableView, Microsoft.Maui.ITransform, Microsoft.Maui.IView, Microsoft.Maui.IVisualTreeElement, ISKGLViewController, System.ComponentModel.INotifyPropertyChanged {
	// constructors
	public SKGLView ();
	// fields
	public static Microsoft.Maui.Controls.BindableProperty EnableTouchEventsProperty;
	public static Microsoft.Maui.Controls.BindableProperty HasRenderLoopProperty;
	// properties
	public SkiaSharp.SKSize CanvasSize { get; }
	public bool EnableTouchEvents { get; set; }
	public SkiaSharp.GRContext GRContext { get; }
	public bool HasRenderLoop { get; set; }
	// events
	public event System.EventHandler<SkiaSharp.Views.Maui.SKPaintGLSurfaceEventArgs> PaintSurface;
	public event System.EventHandler<SkiaSharp.Views.Maui.SKTouchEventArgs> Touch;
	// methods
	public void InvalidateSurface ();
	protected override Microsoft.Maui.SizeRequest OnMeasure (double widthConstraint, double heightConstraint);
	protected virtual void OnPaintSurface (SkiaSharp.Views.Maui.SKPaintGLSurfaceEventArgs e);
	protected virtual void OnTouch (SkiaSharp.Views.Maui.SKTouchEventArgs e);
}
```

#### New Type: SkiaSharp.Views.Maui.Controls.SKImageImageSource

```csharp
public sealed class SKImageImageSource : Microsoft.Maui.Controls.ImageSource, Microsoft.Maui.Controls.IEffectControlProvider, Microsoft.Maui.Controls.IElementController, Microsoft.Maui.Controls.Internals.IDynamicResourceHandler, Microsoft.Maui.Controls.Internals.INameScope, Microsoft.Maui.IElement, Microsoft.Maui.IImageSource, Microsoft.Maui.IVisualTreeElement, SkiaSharp.Views.Maui.ISKImageImageSource, System.ComponentModel.INotifyPropertyChanged {
	// constructors
	public SKImageImageSource ();
	// fields
	public static Microsoft.Maui.Controls.BindableProperty ImageProperty;
	// properties
	public override SkiaSharp.SKImage Image { get; set; }
	// methods
	public override System.Threading.Tasks.Task<bool> Cancel ();
	protected override void OnPropertyChanged (string propertyName);
	public static SKImageImageSource op_Implicit (SkiaSharp.SKImage image);
	public static SkiaSharp.SKImage op_Implicit (SKImageImageSource source);
}
```

#### New Type: SkiaSharp.Views.Maui.Controls.SKPictureImageSource

```csharp
public sealed class SKPictureImageSource : Microsoft.Maui.Controls.ImageSource, Microsoft.Maui.Controls.IEffectControlProvider, Microsoft.Maui.Controls.IElementController, Microsoft.Maui.Controls.Internals.IDynamicResourceHandler, Microsoft.Maui.Controls.Internals.INameScope, Microsoft.Maui.IElement, Microsoft.Maui.IImageSource, Microsoft.Maui.IVisualTreeElement, SkiaSharp.Views.Maui.ISKPictureImageSource, System.ComponentModel.INotifyPropertyChanged {
	// constructors
	public SKPictureImageSource ();
	// fields
	public static Microsoft.Maui.Controls.BindableProperty DimensionsProperty;
	public static Microsoft.Maui.Controls.BindableProperty PictureProperty;
	// properties
	public override SkiaSharp.SKSizeI Dimensions { get; set; }
	public override SkiaSharp.SKPicture Picture { get; set; }
	// methods
	public override System.Threading.Tasks.Task<bool> Cancel ();
	protected override void OnPropertyChanged (string propertyName);
	public static SkiaSharp.SKPicture op_Explicit (SKPictureImageSource source);
}
```

#### New Type: SkiaSharp.Views.Maui.Controls.SKPixmapImageSource

```csharp
public sealed class SKPixmapImageSource : Microsoft.Maui.Controls.ImageSource, Microsoft.Maui.Controls.IEffectControlProvider, Microsoft.Maui.Controls.IElementController, Microsoft.Maui.Controls.Internals.IDynamicResourceHandler, Microsoft.Maui.Controls.Internals.INameScope, Microsoft.Maui.IElement, Microsoft.Maui.IImageSource, Microsoft.Maui.IVisualTreeElement, SkiaSharp.Views.Maui.ISKPixmapImageSource, System.ComponentModel.INotifyPropertyChanged {
	// constructors
	public SKPixmapImageSource ();
	// fields
	public static Microsoft.Maui.Controls.BindableProperty PixmapProperty;
	// properties
	public override SkiaSharp.SKPixmap Pixmap { get; set; }
	// methods
	public override System.Threading.Tasks.Task<bool> Cancel ();
	protected override void OnPropertyChanged (string propertyName);
	public static SKPixmapImageSource op_Implicit (SkiaSharp.SKPixmap pixmap);
	public static SkiaSharp.SKPixmap op_Implicit (SKPixmapImageSource source);
}
```

### New Namespace SkiaSharp.Views.Maui.Controls.Hosting

#### New Type: SkiaSharp.Views.Maui.Controls.Hosting.AppHostBuilderExtensions

```csharp
public static class AppHostBuilderExtensions {
	// methods
	public static Microsoft.Maui.Hosting.MauiAppBuilder UseSkiaSharp (this Microsoft.Maui.Hosting.MauiAppBuilder builder);
}
```

