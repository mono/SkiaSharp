# API diff: SkiaSharp.Views.tvOS.dll

## SkiaSharp.Views.tvOS.dll

> Assembly Version Changed: 3.119.0.0 vs 3.116.0.0

### Namespace SkiaSharp.Views.tvOS

#### New Type: SkiaSharp.Views.tvOS.SKMetalView

```csharp
public class SKMetalView : MetalKit.MTKView, CoreAnimation.ICALayerDelegate, Foundation.INSCoding, Foundation.INSObjectProtocol, MetalKit.IMTKViewDelegate, ObjCRuntime.INativeObject, System.Collections.IEnumerable, System.ComponentModel.IComponent, System.IDisposable, System.IEquatable<Foundation.NSObject>, UIKit.IUIAccessibilityIdentification, UIKit.IUIAppearance, UIKit.IUIAppearanceContainer, UIKit.IUICoordinateSpace, UIKit.IUIDynamicItem, UIKit.IUIFocusEnvironment, UIKit.IUIFocusItem, UIKit.IUIFocusItemContainer, UIKit.IUIResponderStandardEditActions, UIKit.IUITraitEnvironment, UIKit.IUIUserActivityRestoring {
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

#### New Type: SkiaSharp.Views.tvOS.SKPaintMetalSurfaceEventArgs

```csharp
public class SKPaintMetalSurfaceEventArgs : System.EventArgs {
	// constructors
	public SKPaintMetalSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTarget renderTarget);
	public SKPaintMetalSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTarget renderTarget, SkiaSharp.GRSurfaceOrigin origin, SkiaSharp.SKColorType colorType);
	public SKPaintMetalSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTarget renderTarget, SkiaSharp.GRSurfaceOrigin origin, SkiaSharp.SKImageInfo info);
	public SKPaintMetalSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTarget renderTarget, SkiaSharp.GRSurfaceOrigin origin, SkiaSharp.SKImageInfo info, SkiaSharp.SKImageInfo rawInfo);
	// properties
	public SkiaSharp.GRBackendRenderTarget BackendRenderTarget { get; }
	public SkiaSharp.SKColorType ColorType { get; }
	public SkiaSharp.SKImageInfo Info { get; }
	public SkiaSharp.GRSurfaceOrigin Origin { get; }
	public SkiaSharp.SKImageInfo RawInfo { get; }
	public SkiaSharp.SKSurface Surface { get; }
}
```


