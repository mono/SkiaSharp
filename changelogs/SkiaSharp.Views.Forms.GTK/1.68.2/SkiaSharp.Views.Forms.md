# API diff: SkiaSharp.Views.Forms.dll

## SkiaSharp.Views.Forms.dll

> Assembly Version Changed: 1.68.0.0 vs 0.0.0.0

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

#### New Type: SkiaSharp.Views.Forms.ISKGLViewController

```csharp
public interface ISKGLViewController : Xamarin.Forms.IElementController, Xamarin.Forms.IViewController, Xamarin.Forms.IVisualElementController {
	// events
	public event System.EventHandler<SkiaSharp.Views.Forms.GetPropertyValueEventArgs<SkiaSharp.SKSize>> GetCanvasSize;
	public event System.EventHandler<SkiaSharp.Views.Forms.GetPropertyValueEventArgs<SkiaSharp.GRContext>> GetGRContext;
	public event System.EventHandler SurfaceInvalidated;
	// methods
	public virtual void OnPaintSurface (SKPaintGLSurfaceEventArgs e);
	public virtual void OnTouch (SKTouchEventArgs e);
}
```

#### New Type: SkiaSharp.Views.Forms.SKBitmapImageSource

```csharp
public sealed class SKBitmapImageSource : Xamarin.Forms.ImageSource, System.ComponentModel.INotifyPropertyChanged, Xamarin.Forms.IElementController, Xamarin.Forms.Internals.IDynamicResourceHandler, Xamarin.Forms.Internals.INameScope {
	// constructors
	public SKBitmapImageSource ();
	// fields
	public static Xamarin.Forms.BindableProperty BitmapProperty;
	// properties
	public SkiaSharp.SKBitmap Bitmap { get; set; }
	// methods
	public override System.Threading.Tasks.Task<bool> Cancel ();
	protected override void OnPropertyChanged (string propertyName);
	public static SKBitmapImageSource op_Implicit (SkiaSharp.SKBitmap bitmap);
	public static SkiaSharp.SKBitmap op_Implicit (SKBitmapImageSource source);
}
```

#### New Type: SkiaSharp.Views.Forms.SKCanvasView

```csharp
public class SKCanvasView : Xamarin.Forms.View, ISKCanvasViewController, System.ComponentModel.INotifyPropertyChanged, Xamarin.Forms.IAnimatable, Xamarin.Forms.IElementController, Xamarin.Forms.IGestureRecognizers, Xamarin.Forms.ITabStopElement, Xamarin.Forms.IViewController, Xamarin.Forms.IVisualElementController, Xamarin.Forms.Internals.IDynamicResourceHandler, Xamarin.Forms.Internals.IGestureController, Xamarin.Forms.Internals.INameScope, Xamarin.Forms.Internals.INavigationProxy {
	// constructors
	public SKCanvasView ();
	// fields
	public static Xamarin.Forms.BindableProperty EnableTouchEventsProperty;
	public static Xamarin.Forms.BindableProperty IgnorePixelScalingProperty;
	// properties
	public SkiaSharp.SKSize CanvasSize { get; }
	public bool EnableTouchEvents { get; set; }
	public bool IgnorePixelScaling { get; set; }
	// events
	public event System.EventHandler<SKPaintSurfaceEventArgs> PaintSurface;
	public event System.EventHandler<SKTouchEventArgs> Touch;
	// methods
	public void InvalidateSurface ();
	protected override Xamarin.Forms.SizeRequest OnMeasure (double widthConstraint, double heightConstraint);
	protected virtual void OnPaintSurface (SKPaintSurfaceEventArgs e);
	protected virtual void OnTouch (SKTouchEventArgs e);
}
```

#### New Type: SkiaSharp.Views.Forms.SKCanvasViewRenderer

```csharp
public class SKCanvasViewRenderer : SkiaSharp.Views.Forms.SKCanvasViewRendererBase`2[SkiaSharp.Views.Forms.SKCanvasView,SkiaSharp.Views.Gtk.SKWidget], Atk.Implementor, GLib.IWrapper, System.Collections.IEnumerable, System.IDisposable, Xamarin.Forms.IEffectControlProvider, Xamarin.Forms.IRegisterable, Xamarin.Forms.Platform.GTK.IVisualElementRenderer, Xamarin.Forms.Platform.GTK.IVisualNativeElementRenderer {
	// constructors
	public SKCanvasViewRenderer ();
}
```

#### New Type: SkiaSharp.Views.Forms.SKCanvasViewRendererBase`2

```csharp
public abstract class SKCanvasViewRendererBase`2 : Xamarin.Forms.Platform.GTK.ViewRenderer`2[TFormsView,TNativeView], Atk.Implementor, GLib.IWrapper, System.Collections.IEnumerable, System.IDisposable, Xamarin.Forms.IEffectControlProvider, Xamarin.Forms.IRegisterable, Xamarin.Forms.Platform.GTK.IVisualElementRenderer, Xamarin.Forms.Platform.GTK.IVisualNativeElementRenderer {
	// constructors
	protected SKCanvasViewRendererBase`2 ();
	// methods
	protected virtual TNativeView CreateNativeControl ();
	protected override void Dispose (bool disposing);
	protected override void OnElementChanged (Xamarin.Forms.Platform.GTK.ElementChangedEventArgs<TFormsView> e);
	protected override void OnElementPropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e);
}
```

#### New Type: SkiaSharp.Views.Forms.SKGLView

```csharp
public class SKGLView : Xamarin.Forms.View, ISKGLViewController, System.ComponentModel.INotifyPropertyChanged, Xamarin.Forms.IAnimatable, Xamarin.Forms.IElementController, Xamarin.Forms.IGestureRecognizers, Xamarin.Forms.ITabStopElement, Xamarin.Forms.IViewController, Xamarin.Forms.IVisualElementController, Xamarin.Forms.Internals.IDynamicResourceHandler, Xamarin.Forms.Internals.IGestureController, Xamarin.Forms.Internals.INameScope, Xamarin.Forms.Internals.INavigationProxy {
	// constructors
	public SKGLView ();
	// fields
	public static Xamarin.Forms.BindableProperty EnableTouchEventsProperty;
	public static Xamarin.Forms.BindableProperty HasRenderLoopProperty;
	// properties
	public SkiaSharp.SKSize CanvasSize { get; }
	public bool EnableTouchEvents { get; set; }
	public SkiaSharp.GRContext GRContext { get; }
	public bool HasRenderLoop { get; set; }
	// events
	public event System.EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;
	public event System.EventHandler<SKTouchEventArgs> Touch;
	// methods
	public void InvalidateSurface ();
	protected override Xamarin.Forms.SizeRequest OnMeasure (double widthConstraint, double heightConstraint);
	protected virtual void OnPaintSurface (SKPaintGLSurfaceEventArgs e);
	protected virtual void OnTouch (SKTouchEventArgs e);
}
```

#### New Type: SkiaSharp.Views.Forms.SKImageImageSource

```csharp
public sealed class SKImageImageSource : Xamarin.Forms.ImageSource, System.ComponentModel.INotifyPropertyChanged, Xamarin.Forms.IElementController, Xamarin.Forms.Internals.IDynamicResourceHandler, Xamarin.Forms.Internals.INameScope {
	// constructors
	public SKImageImageSource ();
	// fields
	public static Xamarin.Forms.BindableProperty ImageProperty;
	// properties
	public SkiaSharp.SKImage Image { get; set; }
	// methods
	public override System.Threading.Tasks.Task<bool> Cancel ();
	protected override void OnPropertyChanged (string propertyName);
	public static SKImageImageSource op_Implicit (SkiaSharp.SKImage image);
	public static SkiaSharp.SKImage op_Implicit (SKImageImageSource source);
}
```

#### New Type: SkiaSharp.Views.Forms.SKImageSourceHandler

```csharp
public sealed class SKImageSourceHandler : Xamarin.Forms.IRegisterable, Xamarin.Forms.Platform.GTK.Renderers.IImageSourceHandler {
	// constructors
	public SKImageSourceHandler ();
	// methods
	public virtual System.Threading.Tasks.Task<Gdk.Pixbuf> LoadImageAsync (Xamarin.Forms.ImageSource imagesource, System.Threading.CancellationToken cancelationToken, float scale);
}
```

#### New Type: SkiaSharp.Views.Forms.SKMouseButton

```csharp
[Serializable]
public enum SKMouseButton {
	Left = 1,
	Middle = 2,
	Right = 3,
	Unknown = 0,
}
```

#### New Type: SkiaSharp.Views.Forms.SKPaintGLSurfaceEventArgs

```csharp
public class SKPaintGLSurfaceEventArgs : System.EventArgs {
	// constructors
	public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTarget renderTarget);

	[Obsolete]
public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTargetDesc renderTarget);
	public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTarget renderTarget, SkiaSharp.GRSurfaceOrigin origin, SkiaSharp.SKColorType colorType);
	// properties
	public SkiaSharp.GRBackendRenderTarget BackendRenderTarget { get; }
	public SkiaSharp.SKColorType ColorType { get; }
	public SkiaSharp.GRSurfaceOrigin Origin { get; }

	[Obsolete]
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

#### New Type: SkiaSharp.Views.Forms.SKPictureImageSource

```csharp
public sealed class SKPictureImageSource : Xamarin.Forms.ImageSource, System.ComponentModel.INotifyPropertyChanged, Xamarin.Forms.IElementController, Xamarin.Forms.Internals.IDynamicResourceHandler, Xamarin.Forms.Internals.INameScope {
	// constructors
	public SKPictureImageSource ();
	// fields
	public static Xamarin.Forms.BindableProperty DimensionsProperty;
	public static Xamarin.Forms.BindableProperty PictureProperty;
	// properties
	public SkiaSharp.SKSizeI Dimensions { get; set; }
	public SkiaSharp.SKPicture Picture { get; set; }
	// methods
	public override System.Threading.Tasks.Task<bool> Cancel ();
	protected override void OnPropertyChanged (string propertyName);
	public static SkiaSharp.SKPicture op_Explicit (SKPictureImageSource source);
}
```

#### New Type: SkiaSharp.Views.Forms.SKPixmapImageSource

```csharp
public sealed class SKPixmapImageSource : Xamarin.Forms.ImageSource, System.ComponentModel.INotifyPropertyChanged, Xamarin.Forms.IElementController, Xamarin.Forms.Internals.IDynamicResourceHandler, Xamarin.Forms.Internals.INameScope {
	// constructors
	public SKPixmapImageSource ();
	// fields
	public static Xamarin.Forms.BindableProperty PixmapProperty;
	// properties
	public SkiaSharp.SKPixmap Pixmap { get; set; }
	// methods
	public override System.Threading.Tasks.Task<bool> Cancel ();
	protected override void OnPropertyChanged (string propertyName);
	public static SKPixmapImageSource op_Implicit (SkiaSharp.SKPixmap pixmap);
	public static SkiaSharp.SKPixmap op_Implicit (SKPixmapImageSource source);
}
```

#### New Type: SkiaSharp.Views.Forms.SKTouchAction

```csharp
[Serializable]
public enum SKTouchAction {
	Cancelled = 4,
	Entered = 0,
	Exited = 5,
	Moved = 2,
	Pressed = 1,
	Released = 3,
	WheelChanged = 6,
}
```

#### New Type: SkiaSharp.Views.Forms.SKTouchDeviceType

```csharp
[Serializable]
public enum SKTouchDeviceType {
	Mouse = 1,
	Pen = 2,
	Touch = 0,
}
```

#### New Type: SkiaSharp.Views.Forms.SKTouchEventArgs

```csharp
public class SKTouchEventArgs : System.EventArgs {
	// constructors
	public SKTouchEventArgs (long id, SKTouchAction type, SkiaSharp.SKPoint location, bool inContact);
	public SKTouchEventArgs (long id, SKTouchAction type, SKMouseButton mouseButton, SKTouchDeviceType deviceType, SkiaSharp.SKPoint location, bool inContact);
	public SKTouchEventArgs (long id, SKTouchAction type, SKMouseButton mouseButton, SKTouchDeviceType deviceType, SkiaSharp.SKPoint location, bool inContact, int wheelDelta);
	public SKTouchEventArgs (long id, SKTouchAction type, SKMouseButton mouseButton, SKTouchDeviceType deviceType, SkiaSharp.SKPoint location, bool inContact, int wheelDelta, float pressure);
	// properties
	public SKTouchAction ActionType { get; }
	public SKTouchDeviceType DeviceType { get; }
	public bool Handled { get; set; }
	public long Id { get; }
	public bool InContact { get; }
	public SkiaSharp.SKPoint Location { get; }
	public SKMouseButton MouseButton { get; }
	public float Pressure { get; }
	public int WheelDelta { get; }
	// methods
	public override string ToString ();
}
```

