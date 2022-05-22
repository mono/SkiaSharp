# API diff: SkiaSharp.Views.iOS.dll

## SkiaSharp.Views.iOS.dll

> Assembly Version Changed: 1.55.0.0 vs 0.0.0.0

### New Namespace SkiaSharp.Views.iOS

#### New Type: SkiaSharp.Views.iOS.AppleExtensions

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

#### New Type: SkiaSharp.Views.iOS.Extensions

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

#### New Type: SkiaSharp.Views.iOS.ISKCanvasLayerDelegate

```csharp
public interface ISKCanvasLayerDelegate {
	// methods
	public virtual void DrawInSurface (SkiaSharp.SKSurface surface, SkiaSharp.SKImageInfo info);
}
```

#### New Type: SkiaSharp.Views.iOS.ISKGLLayerDelegate

```csharp
public interface ISKGLLayerDelegate {
	// methods
	public virtual void DrawInSurface (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTargetDesc renderTarget);
}
```

#### New Type: SkiaSharp.Views.iOS.SKCanvasLayer

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

#### New Type: SkiaSharp.Views.iOS.SKCanvasView

```csharp
public class SKCanvasView : UIKit.UIView, CoreAnimation.ICALayerDelegate, Foundation.INSCoding, Foundation.INSObjectProtocol, ObjCRuntime.INativeObject, System.Collections.IEnumerable, System.ComponentModel.IComponent, System.IDisposable, System.IEquatable<Foundation.NSObject>, UIKit.IUIAccessibilityIdentification, UIKit.IUIAppearance, UIKit.IUIAppearanceContainer, UIKit.IUICoordinateSpace, UIKit.IUIDynamicItem, UIKit.IUIFocusEnvironment, UIKit.IUIFocusItem, UIKit.IUIFocusItemContainer, UIKit.IUILargeContentViewerItem, UIKit.IUIPasteConfigurationSupporting, UIKit.IUIResponderStandardEditActions, UIKit.IUITraitEnvironment, UIKit.IUIUserActivityRestoring {
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
	public override void Draw (CoreGraphics.CGRect rect);
	public virtual void DrawInSurface (SkiaSharp.SKSurface surface, SkiaSharp.SKImageInfo info);
	public override void LayoutSubviews ();
}
```

#### New Type: SkiaSharp.Views.iOS.SKGLLayer

```csharp
public class SKGLLayer : CoreAnimation.CAEAGLLayer, CoreAnimation.ICAMediaTiming, Foundation.INSCoding, Foundation.INSObjectProtocol, Foundation.INSSecureCoding, ObjCRuntime.INativeObject, OpenGLES.IEAGLDrawable, System.IDisposable, System.IEquatable<Foundation.NSObject> {
	// constructors
	public SKGLLayer ();
	// properties
	public SkiaSharp.SKSize CanvasSize { get; }
	public override CoreGraphics.CGRect Frame { get; set; }
	public ISKGLLayerDelegate SKDelegate { get; set; }
	// events
	public event System.EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;
	// methods
	protected override void Dispose (bool disposing);
	public virtual void DrawInSurface (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTargetDesc renderTarget);
	public virtual void Render ();
}
```

#### New Type: SkiaSharp.Views.iOS.SKGLView

```csharp
public class SKGLView : GLKit.GLKView, CoreAnimation.ICALayerDelegate, Foundation.INSCoding, Foundation.INSObjectProtocol, GLKit.IGLKViewDelegate, ObjCRuntime.INativeObject, System.Collections.IEnumerable, System.ComponentModel.IComponent, System.IDisposable, System.IEquatable<Foundation.NSObject>, UIKit.IUIAccessibilityIdentification, UIKit.IUIAppearance, UIKit.IUIAppearanceContainer, UIKit.IUICoordinateSpace, UIKit.IUIDynamicItem, UIKit.IUIFocusEnvironment, UIKit.IUIFocusItem, UIKit.IUIFocusItemContainer, UIKit.IUILargeContentViewerItem, UIKit.IUIPasteConfigurationSupporting, UIKit.IUIResponderStandardEditActions, UIKit.IUITraitEnvironment, UIKit.IUIUserActivityRestoring {
	// constructors
	public SKGLView ();
	public SKGLView (CoreGraphics.CGRect frame);
	public SKGLView (IntPtr p);
	// properties
	public SkiaSharp.SKSize CanvasSize { get; }
	public override CoreGraphics.CGRect Frame { get; set; }
	// events
	public event System.EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;
	// methods
	public override void AwakeFromNib ();
	public virtual void DrawInRect (GLKit.GLKView view, CoreGraphics.CGRect rect);
	public virtual void DrawInSurface (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTargetDesc renderTarget);
}
```

#### New Type: SkiaSharp.Views.iOS.SKPaintGLSurfaceEventArgs

```csharp
public class SKPaintGLSurfaceEventArgs : System.EventArgs {
	// constructors
	public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTargetDesc renderTarget);
	// properties
	public SkiaSharp.GRBackendRenderTargetDesc RenderTarget { get; }
	public SkiaSharp.SKSurface Surface { get; }
}
```

#### New Type: SkiaSharp.Views.iOS.SKPaintSurfaceEventArgs

```csharp
public class SKPaintSurfaceEventArgs : System.EventArgs {
	// constructors
	public SKPaintSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.SKImageInfo info);
	// properties
	public SkiaSharp.SKImageInfo Info { get; }
	public SkiaSharp.SKSurface Surface { get; }
}
```

#### New Type: SkiaSharp.Views.iOS.iOSExtensions

```csharp
public static class iOSExtensions {
	// methods
	public static SkiaSharp.SKColor ToSKColor (this UIKit.UIColor color);
	public static UIKit.UIColor ToUIColor (this SkiaSharp.SKColor color);
}
```

