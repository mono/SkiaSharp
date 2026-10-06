# API diff: SkiaSharp.Views.Blazor.dll

## SkiaSharp.Views.Blazor.dll

> Assembly Version Changed: 2.88.0.0 vs 0.0.0.0

### New Namespace SkiaSharp.Views.Blazor

#### New Type: SkiaSharp.Views.Blazor.SKCanvasView

```csharp
public class SKCanvasView : Microsoft.AspNetCore.Components.ComponentBase, Microsoft.AspNetCore.Components.IComponent, Microsoft.AspNetCore.Components.IHandleAfterRender, Microsoft.AspNetCore.Components.IHandleEvent, System.IDisposable {
	// constructors
	public SKCanvasView ();
	// properties
	public System.Collections.Generic.IReadOnlyDictionary<System.String,System.Object> AdditionalAttributes { get; set; }
	public bool EnableRenderLoop { get; set; }
	public bool IgnorePixelScaling { get; set; }
	public System.Action<SKPaintSurfaceEventArgs> OnPaintSurface { get; set; }
	// methods
	protected override void BuildRenderTree (Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder);
	public virtual void Dispose ();
	public void Invalidate ();
	protected override System.Threading.Tasks.Task OnAfterRenderAsync (bool firstRender);
}
```

#### New Type: SkiaSharp.Views.Blazor.SKGLView

```csharp
public class SKGLView : Microsoft.AspNetCore.Components.ComponentBase, Microsoft.AspNetCore.Components.IComponent, Microsoft.AspNetCore.Components.IHandleAfterRender, Microsoft.AspNetCore.Components.IHandleEvent, System.IDisposable {
	// constructors
	public SKGLView ();
	// properties
	public System.Collections.Generic.IReadOnlyDictionary<System.String,System.Object> AdditionalAttributes { get; set; }
	public bool EnableRenderLoop { get; set; }
	public bool IgnorePixelScaling { get; set; }
	public System.Action<SKPaintGLSurfaceEventArgs> OnPaintSurface { get; set; }
	// methods
	protected override void BuildRenderTree (Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder);
	public virtual void Dispose ();
	public void Invalidate ();
	protected override System.Threading.Tasks.Task OnAfterRenderAsync (bool firstRender);
}
```

#### New Type: SkiaSharp.Views.Blazor.SKPaintGLSurfaceEventArgs

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

#### New Type: SkiaSharp.Views.Blazor.SKPaintSurfaceEventArgs

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

### New Namespace SkiaSharp.Views.Blazor.Internal

#### New Type: SkiaSharp.Views.Blazor.Internal.ActionHelper

```csharp
public class ActionHelper {
	// constructors
	public ActionHelper (System.Action action);
	// methods
	public void Invoke ();
}
```

#### New Type: SkiaSharp.Views.Blazor.Internal.FloatFloatActionHelper

```csharp
public class FloatFloatActionHelper {
	// constructors
	public FloatFloatActionHelper (System.Action<System.Single,System.Single> action);
	// methods
	public void Invoke (float width, float height);
}
```

