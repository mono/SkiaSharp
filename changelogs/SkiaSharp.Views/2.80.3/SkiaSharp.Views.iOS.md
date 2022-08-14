# API diff: SkiaSharp.Views.iOS.dll

## SkiaSharp.Views.iOS.dll

### Namespace SkiaSharp.Views.iOS

#### New Type: SkiaSharp.Views.iOS.SKMetalView

```csharp
public class SKMetalView : MetalKit.MTKView, CoreAnimation.ICALayerDelegate, Foundation.INSCoding, Foundation.INSObjectProtocol, MetalKit.IMTKViewDelegate, ObjCRuntime.INativeObject, System.Collections.IEnumerable, System.ComponentModel.IComponent, System.IDisposable, System.IEquatable<Foundation.NSObject>, UIKit.IUIAccessibilityIdentification, UIKit.IUIAppearance, UIKit.IUIAppearanceContainer, UIKit.IUICoordinateSpace, UIKit.IUIDynamicItem, UIKit.IUIFocusEnvironment, UIKit.IUIFocusItem, UIKit.IUIFocusItemContainer, UIKit.IUILargeContentViewerItem, UIKit.IUIPasteConfigurationSupporting, UIKit.IUIResponderStandardEditActions, UIKit.IUITraitEnvironment, UIKit.IUIUserActivityRestoring {
	// constructors
	public SKMetalView ();
	public SKMetalView (CoreGraphics.CGRect frame);
	public SKMetalView (IntPtr p);
	public SKMetalView (CoreGraphics.CGRect frame, Metal.IMTLDevice device);
	// properties
	public SkiaSharp.SKSize CanvasSize { get; }
	public SkiaSharp.GRContext GRContext { get; }
	// events
	public event System.EventHandler<SKPaintMetalSurfaceEventArgs> PaintSurface;
	// methods
	public override void AwakeFromNib ();
	protected virtual void OnPaintSurface (SKPaintMetalSurfaceEventArgs e);
}
```

#### New Type: SkiaSharp.Views.iOS.SKPaintMetalSurfaceEventArgs

```csharp
public class SKPaintMetalSurfaceEventArgs : System.EventArgs {
	// constructors
	public SKPaintMetalSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTarget renderTarget);
	public SKPaintMetalSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTarget renderTarget, SkiaSharp.GRSurfaceOrigin origin, SkiaSharp.SKColorType colorType);
	// properties
	public SkiaSharp.GRBackendRenderTarget BackendRenderTarget { get; }
	public SkiaSharp.SKColorType ColorType { get; }
	public SkiaSharp.GRSurfaceOrigin Origin { get; }
	public SkiaSharp.SKSurface Surface { get; }
}
```


