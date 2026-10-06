# API diff: SkiaSharp.Views.UWP.dll

## SkiaSharp.Views.UWP.dll

> Assembly Version Changed: 1.55.0.0 vs 0.0.0.0

### New Namespace SkiaSharp.Views.UWP

#### New Type: SkiaSharp.Views.UWP.AngleSwapChainPanel

```csharp
public class AngleSwapChainPanel : Windows.UI.Xaml.Controls.SwapChainPanel, Windows.UI.Composition.IAnimationObject, Windows.UI.Composition.IVisualElement {
	// constructors
	public AngleSwapChainPanel ();
	// properties
	public GlesBackingOption BackingOption { get; set; }
	public double ContentsScale { get; set; }
	public GlesContext Context { get; set; }
	public GlesDepthFormat DepthFormat { get; set; }
	public bool DrawInBackground { get; set; }
	public bool EnableRenderLoop { get; set; }
	public GlesMultisampling Multisampling { get; set; }
	public GlesStencilFormat StencilFormat { get; set; }
	// methods
	public void Invalidate ();
	protected virtual void OnRenderFrame (Windows.Foundation.Rect rect);
}
```

#### New Type: SkiaSharp.Views.UWP.Extensions

```csharp
public static class Extensions {
}
```

#### New Type: SkiaSharp.Views.UWP.GlesBackingOption

```csharp
[Serializable]
public enum GlesBackingOption {
	Detroyed = 0,
	Retained = 1,
}
```

#### New Type: SkiaSharp.Views.UWP.GlesContext

```csharp
public class GlesContext : System.IDisposable {
	// constructors
	public GlesContext ();
	// properties
	public static GlesContext CurrentContext { get; set; }
	public bool HasValidSurface { get; }
	// methods
	public virtual void Dispose ();
	public void Reset ();
	public void SetSurface (Windows.UI.Xaml.Controls.SwapChainPanel swapChainPanel, int rbWidth, int rbHeight, GlesBackingOption backingOption, GlesMultisampling multisampling, GlesRenderTarget renderTarget);
	public void SetViewportSize (int width, int height);
	public bool SwapBuffers (GlesRenderTarget renderTarget, int width, int height);
}
```

#### New Type: SkiaSharp.Views.UWP.GlesDepthFormat

```csharp
[Serializable]
public enum GlesDepthFormat {
	Format16 = 1,
	Format24 = 2,
	None = 0,
}
```

#### New Type: SkiaSharp.Views.UWP.GlesMultisampling

```csharp
[Serializable]
public enum GlesMultisampling {
	FourTimes = 4,
	None = 0,
}
```

#### New Type: SkiaSharp.Views.UWP.GlesRenderTarget

```csharp
[Serializable]
public enum GlesRenderTarget {
	Renderbuffer = 36161,
}
```

#### New Type: SkiaSharp.Views.UWP.GlesStencilFormat

```csharp
[Serializable]
public enum GlesStencilFormat {
	Format8 = 1,
	None = 0,
}
```

#### New Type: SkiaSharp.Views.UWP.SKPaintGLSurfaceEventArgs

```csharp
public class SKPaintGLSurfaceEventArgs : System.EventArgs {
	// constructors
	public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTargetDesc renderTarget);
	// properties
	public SkiaSharp.GRBackendRenderTargetDesc RenderTarget { get; }
	public SkiaSharp.SKSurface Surface { get; }
}
```

#### New Type: SkiaSharp.Views.UWP.SKPaintSurfaceEventArgs

```csharp
public class SKPaintSurfaceEventArgs : System.EventArgs {
	// constructors
	public SKPaintSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.SKImageInfo info);
	// properties
	public SkiaSharp.SKImageInfo Info { get; }
	public SkiaSharp.SKSurface Surface { get; }
}
```

#### New Type: SkiaSharp.Views.UWP.SKSwapChainPanel

```csharp
public class SKSwapChainPanel : SkiaSharp.Views.UWP.AngleSwapChainPanel, Windows.UI.Composition.IAnimationObject, Windows.UI.Composition.IVisualElement {
	// constructors
	public SKSwapChainPanel ();
	// properties
	public SkiaSharp.SKSize CanvasSize { get; }
	// events
	public event System.EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;
	// methods
	protected override Windows.Foundation.Size ArrangeOverride (Windows.Foundation.Size finalSize);
	protected virtual void OnPaintSurface (SKPaintGLSurfaceEventArgs e);
	protected override void OnRenderFrame (Windows.Foundation.Rect rect);
}
```

#### New Type: SkiaSharp.Views.UWP.SKXamlCanvas

```csharp
public class SKXamlCanvas : Windows.UI.Xaml.Controls.Canvas, Windows.UI.Composition.IAnimationObject, Windows.UI.Composition.IVisualElement {
	// constructors
	public SKXamlCanvas ();
	// properties
	public SkiaSharp.SKSize CanvasSize { get; }
	// events
	public event System.EventHandler<SKPaintSurfaceEventArgs> PaintSurface;
	// methods
	public void Invalidate ();
	protected virtual void OnPaintSurface (SKPaintSurfaceEventArgs e);
}
```

#### New Type: SkiaSharp.Views.UWP.UWPExtensions

```csharp
public static class UWPExtensions {
	// methods
	public static Windows.UI.Color ToColor (this SkiaSharp.SKColor color);
	public static Windows.Foundation.Point ToPoint (this SkiaSharp.SKPoint point);
	public static Windows.Foundation.Rect ToRect (this SkiaSharp.SKRect rect);
	public static SkiaSharp.SKColor ToSKColor (this Windows.UI.Color color);
	public static SkiaSharp.SKPoint ToSKPoint (this Windows.Foundation.Point point);
	public static SkiaSharp.SKRect ToSKRect (this Windows.Foundation.Rect rect);
	public static SkiaSharp.SKSize ToSKSize (this Windows.Foundation.Size size);
	public static Windows.Foundation.Size ToSize (this SkiaSharp.SKSize size);
}
```

