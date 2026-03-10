# API diff: SkiaSharp.Views.Mac.dll

## SkiaSharp.Views.Mac.dll

### Namespace SkiaSharp.Views.Mac

#### New Type: SkiaSharp.Views.Mac.SKMetalView

```csharp
public class SKMetalView : MetalKit.MTKView, AppKit.INSAccessibility, AppKit.INSAccessibilityElementProtocol, AppKit.INSAppearanceCustomization, AppKit.INSDraggingDestination, AppKit.INSTouchBarProvider, AppKit.INSUserActivityRestoring, AppKit.INSUserInterfaceItemIdentification, CoreAnimation.ICALayerDelegate, Foundation.INSCoding, Foundation.INSObjectProtocol, MetalKit.IMTKViewDelegate, ObjCRuntime.INativeObject, System.ComponentModel.IComponent, System.IDisposable, System.IEquatable<Foundation.NSObject> {
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

#### New Type: SkiaSharp.Views.Mac.SKPaintMetalSurfaceEventArgs

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


