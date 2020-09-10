# API diff: SkiaSharp.Views.UWP.dll

## SkiaSharp.Views.UWP.dll

### Namespace SkiaSharp.Views.UWP

#### New Type: SkiaSharp.Views.UWP.SKSwapChainPanel

```csharp
public class SKSwapChainPanel : Windows.UI.Xaml.FrameworkElement, System.Collections.IEnumerable, System.IDisposable, Uno.UI.DataBinding.IWeakReferenceProvider, Windows.UI.Composition.IAnimationObject, Windows.UI.Xaml.DependencyObject, Windows.UI.Xaml.IDataContextProvider, Windows.UI.Xaml.IDependencyObjectStoreProvider, Windows.UI.Xaml.IFrameworkElement, Windows.UI.Xaml.ILayoutConstraints, Windows.UI.Xaml.IUIElement {
	// constructors
	public SKSwapChainPanel ();
	// properties
	public SkiaSharp.SKSize CanvasSize { get; }
	public double ContentsScale { get; }
	public bool DrawInBackground { get; set; }
	public bool EnableRenderLoop { get; set; }
	public SkiaSharp.GRContext GRContext { get; }
	// events
	public event System.EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;
	// methods
	public void Invalidate ();
	protected virtual void OnPaintSurface (SKPaintGLSurfaceEventArgs e);
}
```



