# API diff: SkiaSharp.Views.Tizen.dll

## SkiaSharp.Views.Tizen.dll

> Assembly Version Changed: 3.119.0.0 vs 0.0.0.0

### New Namespace SkiaSharp.Views.Tizen

#### New Type: SkiaSharp.Views.Tizen.CustomRenderingView

```csharp
public abstract class CustomRenderingView : ElmSharp.Widget, ElmSharp.Accessible.IAccessibleObject {
	// constructors
	public CustomRenderingView (ElmSharp.EvasObject parent);
	// fields
	protected IntPtr evasImage;
	// properties
	public SkiaSharp.SKSize CanvasSize { get; }
	public RenderingMode RenderingMode { get; set; }
	// methods
	protected virtual void CreateDrawingSurface ();
	protected override IntPtr CreateHandle (ElmSharp.EvasObject parent);
	protected virtual void CreateNativeResources (ElmSharp.EvasObject parent);
	protected virtual void DestroyDrawingSurface ();
	protected virtual void DestroyNativeResources ();
	protected virtual SkiaSharp.SKSizeI GetRawSurfaceSize ();
	protected virtual SkiaSharp.SKSizeI GetSurfaceSize ();
	public void Invalidate ();
	protected virtual void OnDrawFrame ();
	protected void OnResized ();
	protected override void OnUnrealize ();
	protected virtual bool UpdateSurfaceSize (ElmSharp.Rect geometry);
}
```

#### New Type: SkiaSharp.Views.Tizen.RenderingMode

```csharp
[Serializable]
public enum RenderingMode {
	Continuously = 0,
	WhenDirty = 1,
}
```

#### New Type: SkiaSharp.Views.Tizen.SKCanvasView

```csharp
public class SKCanvasView : SkiaSharp.Views.Tizen.CustomRenderingView, ElmSharp.Accessible.IAccessibleObject {
	// constructors
	public SKCanvasView (ElmSharp.EvasObject parent);
	// properties
	public bool IgnorePixelScaling { get; set; }
	// events
	public event System.EventHandler<SKPaintSurfaceEventArgs> PaintSurface;
	// methods
	protected override SkiaSharp.SKSizeI GetRawSurfaceSize ();
	protected override SkiaSharp.SKSizeI GetSurfaceSize ();
	protected override void OnDrawFrame ();
	protected virtual void OnDrawFrame (SKPaintSurfaceEventArgs e);
	protected override bool UpdateSurfaceSize (ElmSharp.Rect geometry);
}
```

#### New Type: SkiaSharp.Views.Tizen.SKGLSurfaceView

```csharp
public class SKGLSurfaceView : SkiaSharp.Views.Tizen.CustomRenderingView, ElmSharp.Accessible.IAccessibleObject {
	// constructors
	public SKGLSurfaceView (ElmSharp.EvasObject parent);
	// properties
	public SkiaSharp.GRContext GRContext { get; }
	// events
	public event System.EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;
	// methods
	protected override void CreateDrawingSurface ();
	protected override void CreateNativeResources (ElmSharp.EvasObject parent);
	protected override void DestroyDrawingSurface ();
	protected override void DestroyNativeResources ();
	protected override SkiaSharp.SKSizeI GetSurfaceSize ();
	protected override void OnDrawFrame ();
	protected virtual void OnDrawFrame (SKPaintGLSurfaceEventArgs e);
	protected override bool UpdateSurfaceSize (ElmSharp.Rect geometry);
}
```

#### New Type: SkiaSharp.Views.Tizen.SKPaintGLSurfaceEventArgs

```csharp
public class SKPaintGLSurfaceEventArgs : System.EventArgs {
	// constructors
	public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTarget renderTarget);
	public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTarget renderTarget, SkiaSharp.GRSurfaceOrigin origin, SkiaSharp.SKColorType colorType);
	public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTarget renderTarget, SkiaSharp.GRSurfaceOrigin origin, SkiaSharp.SKImageInfo info);
	public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTarget renderTarget, SkiaSharp.GRSurfaceOrigin origin, SkiaSharp.SKImageInfo info, SkiaSharp.SKImageInfo rawInfo);
	// properties
	public SkiaSharp.GRBackendRenderTarget BackendRenderTarget { get; }
	public SkiaSharp.SKColorType ColorType { get; }
	public SkiaSharp.SKImageInfo Info { get; }
	public SkiaSharp.GRSurfaceOrigin Origin { get; }
	public SkiaSharp.SKImageInfo RawInfo { get; }
	public SkiaSharp.SKSurface Surface { get; }
}
```

#### New Type: SkiaSharp.Views.Tizen.SKPaintSurfaceEventArgs

```csharp
public class SKPaintSurfaceEventArgs : System.EventArgs {
	// constructors
	public SKPaintSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.SKImageInfo info);
	public SKPaintSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.SKImageInfo info, SkiaSharp.SKImageInfo rawInfo);
	// properties
	public SkiaSharp.SKImageInfo Info { get; }
	public SkiaSharp.SKImageInfo RawInfo { get; }
	public SkiaSharp.SKSurface Surface { get; }
}
```

#### New Type: SkiaSharp.Views.Tizen.ScalingInfo

```csharp
public static class ScalingInfo {
	// properties
	public static int Dpi { get; }
	public static string Profile { get; }
	public static double ScalingFactor { get; }
	// methods
	public static double FromPixel (double v);
	public static void SetScalingFactor (double? scalingFactor);
	public static double ToPixel (double v);
}
```

#### New Type: SkiaSharp.Views.Tizen.TizenExtensions

```csharp
public static class TizenExtensions {
	// methods
	public static ElmSharp.Color ToColor (this SkiaSharp.SKColor color);
	public static ElmSharp.Point ToPoint (this SkiaSharp.SKPoint point);
	public static ElmSharp.Point ToPoint (this SkiaSharp.SKPointI point);
	public static ElmSharp.Rect ToRect (this SkiaSharp.SKRect rect);
	public static ElmSharp.Rect ToRect (this SkiaSharp.SKRectI rect);
	public static SkiaSharp.SKColor ToSKColor (this ElmSharp.Color color);
	public static SkiaSharp.SKColorF ToSKColorF (this Tizen.NUI.Color color);
	public static SkiaSharp.SKPoint ToSKPoint (this ElmSharp.Point point);
	public static SkiaSharp.SKPoint ToSKPoint (this Tizen.NUI.Position point);
	public static SkiaSharp.SKPointI ToSKPointI (this ElmSharp.Point point);
	public static SkiaSharp.SKPointI ToSKPointI (this Tizen.NUI.Position2D point);
	public static SkiaSharp.SKRect ToSKRect (this ElmSharp.Rect rect);
	public static SkiaSharp.SKRect ToSKRect (this Tizen.NUI.Rectangle rect);
	public static SkiaSharp.SKRectI ToSKRectI (this ElmSharp.Rect rect);
	public static SkiaSharp.SKRectI ToSKRectI (this Tizen.NUI.Rectangle rect);
	public static SkiaSharp.SKSize ToSKSize (this ElmSharp.Size size);
	public static SkiaSharp.SKSize ToSKSize (this Tizen.NUI.Size size);
	public static SkiaSharp.SKSizeI ToSKSizeI (this ElmSharp.Size size);
	public static SkiaSharp.SKSizeI ToSKSizeI (this Tizen.NUI.Size2D size);
	public static ElmSharp.Size ToSize (this SkiaSharp.SKSize size);
	public static ElmSharp.Size ToSize (this SkiaSharp.SKSizeI size);
}
```

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

