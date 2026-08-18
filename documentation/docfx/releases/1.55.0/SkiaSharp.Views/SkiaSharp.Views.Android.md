# API diff: SkiaSharp.Views.Android.dll

## SkiaSharp.Views.Android.dll

> Assembly Version Changed: 1.55.0.0 vs 0.0.0.0

### New Namespace SkiaSharp.Views.Android

#### New Type: SkiaSharp.Views.Android.AndroidExtensions

```csharp
public static class AndroidExtensions {
	// methods
	public static Android.Graphics.Color ToColor (this SkiaSharp.SKColor color);
	public static Android.Graphics.Matrix ToMatrix (this SkiaSharp.SKMatrix matrix);
	public static Android.Graphics.PointF ToPoint (this SkiaSharp.SKPoint point);
	public static Android.Graphics.Point ToPoint (this SkiaSharp.SKPointI point);
	public static Android.Graphics.RectF ToRect (this SkiaSharp.SKRect rect);
	public static Android.Graphics.Rect ToRect (this SkiaSharp.SKRectI rect);
	public static SkiaSharp.SKColor ToSKColor (this Android.Graphics.Color color);
	public static SkiaSharp.SKMatrix ToSKMatrix (this Android.Graphics.Matrix matrix);
	public static SkiaSharp.SKPointI ToSKPoint (this Android.Graphics.Point point);
	public static SkiaSharp.SKPoint ToSKPoint (this Android.Graphics.PointF point);
	public static SkiaSharp.SKRectI ToSKRect (this Android.Graphics.Rect rect);
	public static SkiaSharp.SKRect ToSKRect (this Android.Graphics.RectF rect);
}
```

#### New Type: SkiaSharp.Views.Android.Extensions

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

#### New Type: SkiaSharp.Views.Android.SKCanvasView

```csharp
public class SKCanvasView : Android.Views.View, Android.Graphics.Drawables.Drawable.ICallback, Android.Runtime.IJavaObject, Android.Views.Accessibility.IAccessibilityEventSource, Android.Views.KeyEvent.ICallback, Java.Interop.IJavaPeerable, System.IDisposable {
	// constructors
	public SKCanvasView (Android.Content.Context context);
	public SKCanvasView (Android.Content.Context context, Android.Util.IAttributeSet attrs);
	protected SKCanvasView (IntPtr javaReference, Android.Runtime.JniHandleOwnership transfer);
	public SKCanvasView (Android.Content.Context context, Android.Util.IAttributeSet attrs, int defStyleAttr);
	// properties
	public SkiaSharp.SKSize CanvasSize { get; }
	// events
	public event System.EventHandler<SKPaintSurfaceEventArgs> PaintSurface;
	// methods
	protected override void Dispose (bool disposing);
	protected override void OnDraw (Android.Graphics.Canvas canvas);
	protected virtual void OnDraw (SkiaSharp.SKSurface surface, SkiaSharp.SKImageInfo info);
	protected override void OnSizeChanged (int w, int h, int oldw, int oldh);
}
```

#### New Type: SkiaSharp.Views.Android.SKGLSurfaceView

```csharp
public class SKGLSurfaceView : Android.Opengl.GLSurfaceView, Android.Graphics.Drawables.Drawable.ICallback, Android.Runtime.IJavaObject, Android.Views.Accessibility.IAccessibilityEventSource, Android.Views.ISurfaceHolderCallback, Android.Views.ISurfaceHolderCallback2, Android.Views.KeyEvent.ICallback, Java.Interop.IJavaPeerable, System.IDisposable {
	// constructors
	public SKGLSurfaceView (Android.Content.Context context);
	public SKGLSurfaceView (Android.Content.Context context, Android.Util.IAttributeSet attrs);
	// properties
	public SkiaSharp.SKSize CanvasSize { get; }
	// methods
	public virtual void SetRenderer (SKGLSurfaceView.ISKRenderer renderer);

	// inner types
	public interface ISKRenderer {
		// methods
		public virtual void OnDrawFrame (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTargetDesc renderTarget);
	}
}
```

#### New Type: SkiaSharp.Views.Android.SKGLSurfaceViewRenderer

```csharp
public abstract class SKGLSurfaceViewRenderer : Java.Lang.Object, Android.Opengl.GLSurfaceView.IRenderer, Android.Runtime.IJavaObject, Java.Interop.IJavaPeerable, System.IDisposable {
	// constructors
	protected SKGLSurfaceViewRenderer ();
	// properties
	public SkiaSharp.SKSize CanvasSize { get; }
	// methods
	protected override void Dispose (bool disposing);
	public virtual void OnDrawFrame (Javax.Microedition.Khronos.Opengles.IGL10 gl);
	protected virtual void OnDrawFrame (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTargetDesc renderTarget);
	public virtual void OnSurfaceChanged (Javax.Microedition.Khronos.Opengles.IGL10 gl, int width, int height);
	public virtual void OnSurfaceCreated (Javax.Microedition.Khronos.Opengles.IGL10 gl, Javax.Microedition.Khronos.Egl.EGLConfig config);
}
```

#### New Type: SkiaSharp.Views.Android.SKLockedSurface

```csharp
public class SKLockedSurface {
	// properties
	public SkiaSharp.SKCanvas Canvas { get; }
	public SkiaSharp.SKImageInfo ImageInfo { get; }
	public SkiaSharp.SKSurface Surface { get; }
}
```

#### New Type: SkiaSharp.Views.Android.SKPaintGLSurfaceEventArgs

```csharp
public class SKPaintGLSurfaceEventArgs : System.EventArgs {
	// constructors
	public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTargetDesc renderTarget);
	// properties
	public SkiaSharp.GRBackendRenderTargetDesc RenderTarget { get; }
	public SkiaSharp.SKSurface Surface { get; }
}
```

#### New Type: SkiaSharp.Views.Android.SKPaintSurfaceEventArgs

```csharp
public class SKPaintSurfaceEventArgs : System.EventArgs {
	// constructors
	public SKPaintSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.SKImageInfo info);
	// properties
	public SkiaSharp.SKImageInfo Info { get; }
	public SkiaSharp.SKSurface Surface { get; }
}
```

#### New Type: SkiaSharp.Views.Android.SKSurfaceView

```csharp
public class SKSurfaceView : Android.Views.SurfaceView, Android.Graphics.Drawables.Drawable.ICallback, Android.Runtime.IJavaObject, Android.Views.Accessibility.IAccessibilityEventSource, Android.Views.ISurfaceHolderCallback, Android.Views.KeyEvent.ICallback, Java.Interop.IJavaPeerable, System.IDisposable {
	// constructors
	public SKSurfaceView (Android.Content.Context context);
	public SKSurfaceView (Android.Content.Context context, Android.Util.IAttributeSet attrs);
	public SKSurfaceView (Android.Content.Context context, Android.Util.IAttributeSet attrs, int defStyle);
	// properties
	public SkiaSharp.SKSize CanvasSize { get; }
	// methods
	protected override void Dispose (bool disposing);
	public SKLockedSurface LockSurface ();
	public virtual void SurfaceChanged (Android.Views.ISurfaceHolder holder, Android.Graphics.Format format, int width, int height);
	public virtual void SurfaceCreated (Android.Views.ISurfaceHolder holder);
	public virtual void SurfaceDestroyed (Android.Views.ISurfaceHolder holder);
	public void UnlockSurfaceAndPost (SKLockedSurface surface);
}
```

