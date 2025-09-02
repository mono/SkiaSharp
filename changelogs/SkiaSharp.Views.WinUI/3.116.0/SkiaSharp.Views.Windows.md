# API diff: SkiaSharp.Views.Windows.dll

## SkiaSharp.Views.Windows.dll

> Assembly Version Changed: 3.116.0.0 vs 2.88.0.0

### Namespace SkiaSharp.Views.Windows

#### Type Changed: SkiaSharp.Views.Windows.SKPaintGLSurfaceEventArgs

Removed constructors:

```csharp
[Obsolete]
public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTargetDesc renderTarget);

[Obsolete]
public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTarget renderTarget, SkiaSharp.GRSurfaceOrigin origin, SkiaSharp.SKColorType colorType, SkiaSharp.GRGlFramebufferInfo glInfo);
```

Removed property:

```csharp
[Obsolete]
public SkiaSharp.GRBackendRenderTargetDesc RenderTarget { get; }
```


#### Removed Type SkiaSharp.Views.Windows.Extensions
#### New Type: SkiaSharp.Views.Windows.AngleSwapChainPanel

```csharp
public class AngleSwapChainPanel : Microsoft.UI.Xaml.Controls.SwapChainPanel {
	// constructors
	public AngleSwapChainPanel ();
	// properties
	public double ContentsScale { get; }
	public bool DrawInBackground { get; set; }
	public bool EnableRenderLoop { get; set; }
	// methods
	public void Invalidate ();
	protected virtual void OnDestroyingContext ();
	protected virtual void OnRenderFrame (Windows.Foundation.Rect rect);
}
```

#### New Type: SkiaSharp.Views.Windows.SKSwapChainPanel

```csharp
public class SKSwapChainPanel : SkiaSharp.Views.Windows.AngleSwapChainPanel {
	// constructors
	public SKSwapChainPanel ();
	// properties
	public SkiaSharp.SKSize CanvasSize { get; }
	public SkiaSharp.GRContext GRContext { get; }
	// events
	public event System.EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;
	// methods
	protected override void OnDestroyingContext ();
	protected virtual void OnPaintSurface (SKPaintGLSurfaceEventArgs e);
	protected override void OnRenderFrame (Windows.Foundation.Rect rect);
}
```


