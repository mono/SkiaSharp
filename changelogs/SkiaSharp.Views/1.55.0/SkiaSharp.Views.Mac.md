# API diff: SkiaSharp.Views.Mac.dll

## SkiaSharp.Views.Mac.dll

> Assembly Version Changed: 1.55.0.0 vs 0.0.0.0

### New Namespace SkiaSharp.Views.Mac

#### New Type: SkiaSharp.Views.Mac.AppleExtensions

```csharp
public static class AppleExtensions {
	// methods
	public static CoreGraphics.CGColor ToCGColor (this SkiaSharp.SKColor color);
	public static CoreImage.CIColor ToCIColor (this SkiaSharp.SKColor color);
	public static CoreGraphics.CGPoint ToPoint (this SkiaSharp.SKPoint point);
	public static CoreGraphics.CGRect ToRect (this SkiaSharp.SKRect rect);
	public static SkiaSharp.SKColor ToSKColor (this CoreGraphics.CGColor color);
	public static SkiaSharp.SKColor ToSKColor (this CoreImage.CIColor color);
	public static SkiaSharp.SKPoint ToSKPoint (this CoreGraphics.CGPoint point);
	public static SkiaSharp.SKRect ToSKRect (this CoreGraphics.CGRect rect);
	public static SkiaSharp.SKSize ToSKSize (this CoreGraphics.CGSize size);
	public static CoreGraphics.CGSize ToSize (this SkiaSharp.SKSize size);
}
```

#### New Type: SkiaSharp.Views.Mac.Extensions

```csharp
public static class Extensions {
	// methods
	public static System.Drawing.Color ToDrawingColor (this SkiaSharp.SKColor color);
	public static System.Drawing.PointF ToDrawingPoint (this SkiaSharp.SKPoint point);
	public static System.Drawing.Point ToDrawingPoint (this SkiaSharp.SKPointI point);
	public static System.Drawing.RectangleF ToDrawingRect (this SkiaSharp.SKRect rect);
	public static System.Drawing.Rectangle ToDrawingRect (this SkiaSharp.SKRectI rect);
	public static System.Drawing.SizeF ToDrawingSize (this SkiaSharp.SKSize size);
	public static System.Drawing.Size ToDrawingSize (this SkiaSharp.SKSizeI size);
	public static SkiaSharp.SKColor ToSKColor (this System.Drawing.Color color);
	public static SkiaSharp.SKPointI ToSKPoint (this System.Drawing.Point point);
	public static SkiaSharp.SKPoint ToSKPoint (this System.Drawing.PointF point);
	public static SkiaSharp.SKRectI ToSKRect (this System.Drawing.Rectangle rect);
	public static SkiaSharp.SKRect ToSKRect (this System.Drawing.RectangleF rect);
	public static SkiaSharp.SKSizeI ToSKSize (this System.Drawing.Size size);
	public static SkiaSharp.SKSize ToSKSize (this System.Drawing.SizeF size);
}
```

#### New Type: SkiaSharp.Views.Mac.ISKCanvasLayerDelegate

```csharp
public interface ISKCanvasLayerDelegate {
	// methods
	public virtual void DrawInSurface (SkiaSharp.SKSurface surface, SkiaSharp.SKImageInfo info);
}
```

#### New Type: SkiaSharp.Views.Mac.ISKGLLayerDelegate

```csharp
public interface ISKGLLayerDelegate {
	// methods
	public virtual void DrawInSurface (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTargetDesc renderTarget);
}
```

#### New Type: SkiaSharp.Views.Mac.MacExtensions

```csharp
public static class MacExtensions {
	// methods
	public static AppKit.NSColor ToNSColor (this SkiaSharp.SKColor color);
	public static SkiaSharp.SKColor ToSKColor (this AppKit.NSColor color);
}
```

#### New Type: SkiaSharp.Views.Mac.SKCanvasLayer

```csharp
public class SKCanvasLayer : CoreAnimation.CALayer, CoreAnimation.ICAMediaTiming, Foundation.INSCoding, Foundation.INSObjectProtocol, Foundation.INSSecureCoding, ObjCRuntime.INativeObject, System.IDisposable, System.IEquatable<Foundation.NSObject> {
	// constructors
	public SKCanvasLayer ();
	// properties
	public SkiaSharp.SKSize CanvasSize { get; }
	public ISKCanvasLayerDelegate SKDelegate { get; set; }
	// events
	public event System.EventHandler<SKPaintSurfaceEventArgs> PaintSurface;
	// methods
	protected override void Dispose (bool disposing);
	public override void DrawInContext (CoreGraphics.CGContext ctx);
	public virtual void DrawInSurface (SkiaSharp.SKSurface surface, SkiaSharp.SKImageInfo info);
}
```

#### New Type: SkiaSharp.Views.Mac.SKCanvasView

```csharp
public class SKCanvasView : AppKit.NSView, AppKit.INSAccessibility, AppKit.INSAccessibilityElementProtocol, AppKit.INSAppearanceCustomization, AppKit.INSDraggingDestination, AppKit.INSTouchBarProvider, AppKit.INSUserActivityRestoring, AppKit.INSUserInterfaceItemIdentification, Foundation.INSCoding, Foundation.INSObjectProtocol, ObjCRuntime.INativeObject, System.IDisposable, System.IEquatable<Foundation.NSObject> {
	// constructors
	public SKCanvasView ();
	public SKCanvasView (CoreGraphics.CGRect frame);
	public SKCanvasView (IntPtr p);
	// properties
	public SkiaSharp.SKSize CanvasSize { get; }
	// events
	public event System.EventHandler<SKPaintSurfaceEventArgs> PaintSurface;
	// methods
	public override void AwakeFromNib ();
	protected override void Dispose (bool disposing);
	public virtual void DrawInSurface (SkiaSharp.SKSurface surface, SkiaSharp.SKImageInfo info);
	public override void DrawRect (CoreGraphics.CGRect dirtyRect);
}
```

#### New Type: SkiaSharp.Views.Mac.SKGLLayer

```csharp
public class SKGLLayer : CoreAnimation.CAOpenGLLayer, CoreAnimation.ICAMediaTiming, Foundation.INSCoding, Foundation.INSObjectProtocol, Foundation.INSSecureCoding, ObjCRuntime.INativeObject, System.IDisposable, System.IEquatable<Foundation.NSObject> {
	// constructors
	public SKGLLayer ();
	// properties
	public SkiaSharp.SKSize CanvasSize { get; }
	public ISKGLLayerDelegate SKDelegate { get; set; }
	// events
	public event System.EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;
	// methods
	public override void DrawInCGLContext (OpenGL.CGLContext glContext, OpenGL.CGLPixelFormat pixelFormat, double timeInterval, ref CoreVideo.CVTimeStamp timeStamp);
	public virtual void DrawInSurface (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTargetDesc renderTarget);
	public override void Release (OpenGL.CGLContext glContext);
}
```

#### New Type: SkiaSharp.Views.Mac.SKGLView

```csharp
public class SKGLView : AppKit.NSOpenGLView, AppKit.INSAccessibility, AppKit.INSAccessibilityElementProtocol, AppKit.INSAppearanceCustomization, AppKit.INSDraggingDestination, AppKit.INSTouchBarProvider, AppKit.INSUserActivityRestoring, AppKit.INSUserInterfaceItemIdentification, Foundation.INSCoding, Foundation.INSObjectProtocol, ObjCRuntime.INativeObject, System.IDisposable, System.IEquatable<Foundation.NSObject> {
	// constructors
	public SKGLView ();
	public SKGLView (CoreGraphics.CGRect frame);
	public SKGLView (IntPtr p);
	// properties
	public SkiaSharp.SKSize CanvasSize { get; }
	// events
	public event System.EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;
	// methods
	public override void AwakeFromNib ();
	public virtual void DrawInSurface (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTargetDesc renderTarget);
	public override void DrawRect (CoreGraphics.CGRect dirtyRect);
	public override void PrepareOpenGL ();
}
```

#### New Type: SkiaSharp.Views.Mac.SKPaintGLSurfaceEventArgs

```csharp
public class SKPaintGLSurfaceEventArgs : System.EventArgs {
	// constructors
	public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTargetDesc renderTarget);
	// properties
	public SkiaSharp.GRBackendRenderTargetDesc RenderTarget { get; }
	public SkiaSharp.SKSurface Surface { get; }
}
```

#### New Type: SkiaSharp.Views.Mac.SKPaintSurfaceEventArgs

```csharp
public class SKPaintSurfaceEventArgs : System.EventArgs {
	// constructors
	public SKPaintSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.SKImageInfo info);
	// properties
	public SkiaSharp.SKImageInfo Info { get; }
	public SkiaSharp.SKSurface Surface { get; }
}
```

