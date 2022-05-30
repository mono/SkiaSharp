# API diff: SkiaSharp.Views.Maui.Core.dll

## SkiaSharp.Views.Maui.Core.dll

> Assembly Version Changed: 2.88.0.0 vs 0.0.0.0

### New Namespace SkiaSharp.Views.Maui

#### New Type: SkiaSharp.Views.Maui.Extensions

```csharp
public static class Extensions {
	// methods
	public static Microsoft.Maui.Graphics.Color ToMauiColor (this SkiaSharp.SKColor color);
	public static Microsoft.Maui.Graphics.Color ToMauiColor (this SkiaSharp.SKColorF color);
	public static Microsoft.Maui.Graphics.Point ToMauiPoint (this SkiaSharp.SKPoint point);
	public static Microsoft.Maui.Graphics.Point ToMauiPoint (this SkiaSharp.SKPointI point);
	public static Microsoft.Maui.Graphics.PointF ToMauiPointF (this SkiaSharp.SKPoint point);
	public static Microsoft.Maui.Graphics.PointF ToMauiPointF (this SkiaSharp.SKPointI point);
	public static Microsoft.Maui.Graphics.Rect ToMauiRectangle (this SkiaSharp.SKRect rect);
	public static Microsoft.Maui.Graphics.Rect ToMauiRectangle (this SkiaSharp.SKRectI rect);
	public static Microsoft.Maui.Graphics.RectF ToMauiRectangleF (this SkiaSharp.SKRect rect);
	public static Microsoft.Maui.Graphics.RectF ToMauiRectangleF (this SkiaSharp.SKRectI rect);
	public static Microsoft.Maui.Graphics.Size ToMauiSize (this SkiaSharp.SKSize size);
	public static Microsoft.Maui.Graphics.Size ToMauiSize (this SkiaSharp.SKSizeI size);
	public static Microsoft.Maui.Graphics.SizeF ToMauiSizeF (this SkiaSharp.SKSize size);
	public static Microsoft.Maui.Graphics.SizeF ToMauiSizeF (this SkiaSharp.SKSizeI size);
	public static SkiaSharp.SKColor ToSKColor (this Microsoft.Maui.Graphics.Color color);
	public static SkiaSharp.SKColorF ToSKColorF (this Microsoft.Maui.Graphics.Color color);
	public static SkiaSharp.SKPoint ToSKPoint (this Microsoft.Maui.Graphics.Point point);
	public static SkiaSharp.SKPoint ToSKPoint (this Microsoft.Maui.Graphics.PointF point);
	public static SkiaSharp.SKRect ToSKRect (this Microsoft.Maui.Graphics.Rect rect);
	public static SkiaSharp.SKRect ToSKRect (this Microsoft.Maui.Graphics.RectF rect);
	public static SkiaSharp.SKSize ToSKSize (this Microsoft.Maui.Graphics.Size size);
	public static SkiaSharp.SKSize ToSKSize (this Microsoft.Maui.Graphics.SizeF size);
}
```

#### New Type: SkiaSharp.Views.Maui.ISKBitmapImageSource

```csharp
public interface ISKBitmapImageSource : Microsoft.Maui.IImageSource {
	// properties
	public virtual SkiaSharp.SKBitmap Bitmap { get; }
}
```

#### New Type: SkiaSharp.Views.Maui.ISKCanvasView

```csharp
public interface ISKCanvasView : Microsoft.Maui.IElement, Microsoft.Maui.ITransform, Microsoft.Maui.IView {
	// properties
	public virtual SkiaSharp.SKSize CanvasSize { get; }
	public virtual bool EnableTouchEvents { get; }
	public virtual bool IgnorePixelScaling { get; }
	// methods
	public virtual void InvalidateSurface ();
	public virtual void OnCanvasSizeChanged (SkiaSharp.SKSizeI size);
	public virtual void OnPaintSurface (SKPaintSurfaceEventArgs e);
	public virtual void OnTouch (SKTouchEventArgs e);
}
```

#### New Type: SkiaSharp.Views.Maui.ISKImageImageSource

```csharp
public interface ISKImageImageSource : Microsoft.Maui.IImageSource {
	// properties
	public virtual SkiaSharp.SKImage Image { get; }
}
```

#### New Type: SkiaSharp.Views.Maui.ISKPictureImageSource

```csharp
public interface ISKPictureImageSource : Microsoft.Maui.IImageSource {
	// properties
	public virtual SkiaSharp.SKSizeI Dimensions { get; }
	public virtual SkiaSharp.SKPicture Picture { get; }
}
```

#### New Type: SkiaSharp.Views.Maui.ISKPixmapImageSource

```csharp
public interface ISKPixmapImageSource : Microsoft.Maui.IImageSource {
	// properties
	public virtual SkiaSharp.SKPixmap Pixmap { get; }
}
```

#### New Type: SkiaSharp.Views.Maui.SKMouseButton

```csharp
[Serializable]
public enum SKMouseButton {
	Left = 1,
	Middle = 2,
	Right = 3,
	Unknown = 0,
}
```

#### New Type: SkiaSharp.Views.Maui.SKPaintGLSurfaceEventArgs

```csharp
public class SKPaintGLSurfaceEventArgs : System.EventArgs {
	// constructors
	public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTarget renderTarget);
	public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTarget renderTarget, SkiaSharp.GRSurfaceOrigin origin, SkiaSharp.SKColorType colorType);
	// properties
	public SkiaSharp.GRBackendRenderTarget BackendRenderTarget { get; }
	public SkiaSharp.SKColorType ColorType { get; }
	public SkiaSharp.GRSurfaceOrigin Origin { get; }
	public SkiaSharp.SKSurface Surface { get; }
}
```

#### New Type: SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs

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

#### New Type: SkiaSharp.Views.Maui.SKTouchAction

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

#### New Type: SkiaSharp.Views.Maui.SKTouchDeviceType

```csharp
[Serializable]
public enum SKTouchDeviceType {
	Mouse = 1,
	Pen = 2,
	Touch = 0,
}
```

#### New Type: SkiaSharp.Views.Maui.SKTouchEventArgs

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

### New Namespace SkiaSharp.Views.Maui.Handlers

#### New Type: SkiaSharp.Views.Maui.Handlers.SKCanvasViewHandler

```csharp
public class SKCanvasViewHandler : Microsoft.Maui.Handlers.ViewHandler`2[SkiaSharp.Views.Maui.ISKCanvasView,System.Object], Microsoft.Maui.IElementHandler, Microsoft.Maui.IPlatformViewHandler, Microsoft.Maui.IViewHandler {
	// constructors
	public SKCanvasViewHandler ();
	public SKCanvasViewHandler (Microsoft.Maui.PropertyMapper mapper, Microsoft.Maui.CommandMapper commands);
	// fields
	public static Microsoft.Maui.CommandMapper<SkiaSharp.Views.Maui.ISKCanvasView,SkiaSharp.Views.Maui.Handlers.SKCanvasViewHandler> SKCanvasViewCommandMapper;
	public static Microsoft.Maui.PropertyMapper<SkiaSharp.Views.Maui.ISKCanvasView,SkiaSharp.Views.Maui.Handlers.SKCanvasViewHandler> SKCanvasViewMapper;
	// methods
	protected override object CreatePlatformView ();
	public static void MapEnableTouchEvents (SKCanvasViewHandler handler, SkiaSharp.Views.Maui.ISKCanvasView canvasView);
	public static void MapIgnorePixelScaling (SKCanvasViewHandler handler, SkiaSharp.Views.Maui.ISKCanvasView canvasView);
	public static void OnInvalidateSurface (SKCanvasViewHandler handler, SkiaSharp.Views.Maui.ISKCanvasView canvasView, object args);
}
```

#### New Type: SkiaSharp.Views.Maui.Handlers.SKImageSourceService

```csharp
public class SKImageSourceService : Microsoft.Maui.ImageSourceService, Microsoft.Maui.IImageSourceService, Microsoft.Maui.IImageSourceService<SkiaSharp.Views.Maui.ISKBitmapImageSource>, Microsoft.Maui.IImageSourceService<SkiaSharp.Views.Maui.ISKImageImageSource>, Microsoft.Maui.IImageSourceService<SkiaSharp.Views.Maui.ISKPictureImageSource>, Microsoft.Maui.IImageSourceService<SkiaSharp.Views.Maui.ISKPixmapImageSource> {
	// constructors
	public SKImageSourceService ();
	public SKImageSourceService (Microsoft.Extensions.Logging.ILogger logger);
}
```

