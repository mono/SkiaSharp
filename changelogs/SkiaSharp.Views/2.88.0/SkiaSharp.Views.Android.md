# API diff: SkiaSharp.Views.Android.dll

## SkiaSharp.Views.Android.dll

> Assembly Version Changed: 2.88.0.0 vs 0.0.0.0

### New Namespace SkiaSharp.Views.Android

#### New Type: SkiaSharp.Views.Android.AndroidExtensions

```csharp
public static class AndroidExtensions {
	// methods
	public static Android.Graphics.Bitmap ToBitmap (this SkiaSharp.SKBitmap skiaBitmap);
	public static Android.Graphics.Bitmap ToBitmap (this SkiaSharp.SKImage skiaImage);
	public static Android.Graphics.Bitmap ToBitmap (this SkiaSharp.SKPixmap skiaPixmap);
	public static Android.Graphics.Bitmap ToBitmap (this SkiaSharp.SKPicture skiaPicture, SkiaSharp.SKSizeI dimensions);
	public static Android.Graphics.Color ToColor (this SkiaSharp.SKColor color);
	public static Android.Graphics.Matrix ToMatrix (this SkiaSharp.SKMatrix matrix);
	public static Android.Graphics.PointF ToPoint (this SkiaSharp.SKPoint point);
	public static Android.Graphics.Point ToPoint (this SkiaSharp.SKPointI point);
	public static Android.Graphics.RectF ToRect (this SkiaSharp.SKRect rect);
	public static Android.Graphics.Rect ToRect (this SkiaSharp.SKRectI rect);
	public static SkiaSharp.SKBitmap ToSKBitmap (this Android.Graphics.Bitmap bitmap);
	public static SkiaSharp.SKColor ToSKColor (this Android.Graphics.Color color);
	public static SkiaSharp.SKImage ToSKImage (this Android.Graphics.Bitmap bitmap);
	public static SkiaSharp.SKMatrix ToSKMatrix (this Android.Graphics.Matrix matrix);
	public static void ToSKPixmap (this Android.Graphics.Bitmap bitmap, SkiaSharp.SKPixmap pixmap);
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

#### New Type: SkiaSharp.Views.Android.GLTextureView

```csharp
public class GLTextureView : Android.Views.TextureView, Android.Graphics.Drawables.Drawable.ICallback, Android.Runtime.IJavaObject, Android.Views.Accessibility.IAccessibilityEventSource, Android.Views.KeyEvent.ICallback, Android.Views.TextureView.ISurfaceTextureListener, Android.Views.View.IOnLayoutChangeListener, Java.Interop.IJavaPeerable, System.IDisposable {
	// constructors
	public GLTextureView (Android.Content.Context context);
	public GLTextureView (Android.Content.Context context, Android.Util.IAttributeSet attrs);
	// properties
	public Android.Opengl.DebugFlags DebugFlags { get; set; }
	public bool PreserveEGLContextOnPause { get; set; }
	public Android.Opengl.Rendermode RenderMode { get; set; }
	// methods
	protected override void Dispose (bool disposing);
	protected override void OnAttachedToWindow ();
	protected override void OnDetachedFromWindow ();
	public virtual void OnLayoutChange (Android.Views.View v, int left, int top, int right, int bottom, int oldLeft, int oldTop, int oldRight, int oldBottom);
	public void OnPause ();
	public void OnResume ();
	public virtual void OnSurfaceTextureAvailable (Android.Graphics.SurfaceTexture surface, int width, int height);
	public virtual bool OnSurfaceTextureDestroyed (Android.Graphics.SurfaceTexture surface);
	public virtual void OnSurfaceTextureSizeChanged (Android.Graphics.SurfaceTexture surface, int w, int h);
	public virtual void OnSurfaceTextureUpdated (Android.Graphics.SurfaceTexture surface);
	public void QueueEvent (Java.Lang.IRunnable r);
	public void QueueEvent (System.Action r);
	public void RequestRender ();
	public void SetEGLConfigChooser (GLTextureView.IEGLConfigChooser configChooser);
	public void SetEGLConfigChooser (bool needDepth);
	public void SetEGLConfigChooser (int redSize, int greenSize, int blueSize, int alphaSize, int depthSize, int stencilSize);
	public void SetEGLContextClientVersion (int version);
	public void SetEGLContextFactory (GLTextureView.IEGLContextFactory factory);
	public void SetEGLWindowSurfaceFactory (GLTextureView.IEGLWindowSurfaceFactory factory);
	public void SetGLWrapper (GLTextureView.IGLWrapper glWrapper);
	public void SetRenderer (GLTextureView.IRenderer renderer);

	// inner types
	public interface IEGLConfigChooser {
		// methods
		public virtual Javax.Microedition.Khronos.Egl.EGLConfig ChooseConfig (Javax.Microedition.Khronos.Egl.IEGL10 egl, Javax.Microedition.Khronos.Egl.EGLDisplay display);
	}
	public interface IEGLContextFactory {
		// methods
		public virtual Javax.Microedition.Khronos.Egl.EGLContext CreateContext (Javax.Microedition.Khronos.Egl.IEGL10 egl, Javax.Microedition.Khronos.Egl.EGLDisplay display, Javax.Microedition.Khronos.Egl.EGLConfig eglConfig);
		public virtual void DestroyContext (Javax.Microedition.Khronos.Egl.IEGL10 egl, Javax.Microedition.Khronos.Egl.EGLDisplay display, Javax.Microedition.Khronos.Egl.EGLContext context);
	}
	public interface IEGLWindowSurfaceFactory {
		// methods
		public virtual Javax.Microedition.Khronos.Egl.EGLSurface CreateWindowSurface (Javax.Microedition.Khronos.Egl.IEGL10 egl, Javax.Microedition.Khronos.Egl.EGLDisplay display, Javax.Microedition.Khronos.Egl.EGLConfig config, Java.Lang.Object nativeWindow);
		public virtual void DestroySurface (Javax.Microedition.Khronos.Egl.IEGL10 egl, Javax.Microedition.Khronos.Egl.EGLDisplay display, Javax.Microedition.Khronos.Egl.EGLSurface surface);
	}
	public interface IGLWrapper {
		// methods
		public virtual Javax.Microedition.Khronos.Opengles.IGL Wrap (Javax.Microedition.Khronos.Opengles.IGL gl);
	}
	public interface IRenderer {
		// methods
		public virtual void OnDrawFrame (Javax.Microedition.Khronos.Opengles.IGL10 gl);
		public virtual void OnSurfaceChanged (Javax.Microedition.Khronos.Opengles.IGL10 gl, int width, int height);
		public virtual void OnSurfaceCreated (Javax.Microedition.Khronos.Opengles.IGL10 gl, Javax.Microedition.Khronos.Egl.EGLConfig config);
		public virtual void OnSurfaceDestroyed ();
	}
}
```

#### New Type: SkiaSharp.Views.Android.Resource

```csharp
public class Resource {
	// constructors
	public Resource ();

	// inner types
	public class Attribute {
		// fields
		public static int ignorePixelScaling;
	}
	public class Styleable {
		// fields
		public static int[] SKCanvasView;
		public static int SKCanvasView_ignorePixelScaling;
	}
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
	public bool IgnorePixelScaling { get; set; }
	// events
	public event System.EventHandler<SKPaintSurfaceEventArgs> PaintSurface;
	// methods
	protected override void Dispose (bool disposing);
	protected override void OnAttachedToWindow ();
	protected override void OnDetachedFromWindow ();
	protected override void OnDraw (Android.Graphics.Canvas canvas);

	[Obsolete]
protected virtual void OnDraw (SkiaSharp.SKSurface surface, SkiaSharp.SKImageInfo info);
	protected virtual void OnPaintSurface (SKPaintSurfaceEventArgs e);
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
	public SkiaSharp.GRContext GRContext { get; }
	// events
	public event System.EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;
	// methods
	protected virtual void OnPaintSurface (SKPaintGLSurfaceEventArgs e);

	[Obsolete]
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
	public SkiaSharp.GRContext GRContext { get; }
	// methods
	protected override void Dispose (bool disposing);
	public virtual void OnDrawFrame (Javax.Microedition.Khronos.Opengles.IGL10 gl);

	[Obsolete]
protected virtual void OnDrawFrame (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTargetDesc renderTarget);
	protected virtual void OnPaintSurface (SKPaintGLSurfaceEventArgs e);
	public virtual void OnSurfaceChanged (Javax.Microedition.Khronos.Opengles.IGL10 gl, int width, int height);
	public virtual void OnSurfaceCreated (Javax.Microedition.Khronos.Opengles.IGL10 gl, Javax.Microedition.Khronos.Egl.EGLConfig config);
}
```

#### New Type: SkiaSharp.Views.Android.SKGLTextureView

```csharp
public class SKGLTextureView : SkiaSharp.Views.Android.GLTextureView, Android.Graphics.Drawables.Drawable.ICallback, Android.Runtime.IJavaObject, Android.Views.Accessibility.IAccessibilityEventSource, Android.Views.KeyEvent.ICallback, Android.Views.TextureView.ISurfaceTextureListener, Android.Views.View.IOnLayoutChangeListener, Java.Interop.IJavaPeerable, System.IDisposable {
	// constructors
	public SKGLTextureView (Android.Content.Context context);
	public SKGLTextureView (Android.Content.Context context, Android.Util.IAttributeSet attrs);
	// properties
	public SkiaSharp.SKSize CanvasSize { get; }
	public SkiaSharp.GRContext GRContext { get; }
	// events
	public event System.EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;
	// methods
	protected virtual void OnPaintSurface (SKPaintGLSurfaceEventArgs e);

	[Obsolete]
public virtual void SetRenderer (SKGLTextureView.ISKRenderer renderer);

	// inner types
	public interface ISKRenderer {
		// methods
		public virtual void OnDrawFrame (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTargetDesc renderTarget);
	}
}
```

#### New Type: SkiaSharp.Views.Android.SKGLTextureViewRenderer

```csharp
public abstract class SKGLTextureViewRenderer : Java.Lang.Object, Android.Runtime.IJavaObject, Java.Interop.IJavaPeerable, GLTextureView.IRenderer, System.IDisposable {
	// constructors
	protected SKGLTextureViewRenderer ();
	// properties
	public SkiaSharp.SKSize CanvasSize { get; }
	public SkiaSharp.GRContext GRContext { get; }
	// methods
	protected override void Dispose (bool disposing);
	public virtual void OnDrawFrame (Javax.Microedition.Khronos.Opengles.IGL10 gl);

	[Obsolete]
protected virtual void OnDrawFrame (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTargetDesc renderTarget);
	protected virtual void OnPaintSurface (SKPaintGLSurfaceEventArgs e);
	public virtual void OnSurfaceChanged (Javax.Microedition.Khronos.Opengles.IGL10 gl, int width, int height);
	public virtual void OnSurfaceCreated (Javax.Microedition.Khronos.Opengles.IGL10 gl, Javax.Microedition.Khronos.Egl.EGLConfig config);
	public virtual void OnSurfaceDestroyed ();
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
	public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTarget renderTarget);

	[Obsolete]
public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTargetDesc renderTarget);
	public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTarget renderTarget, SkiaSharp.GRSurfaceOrigin origin, SkiaSharp.SKColorType colorType);
	public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTarget renderTarget, SkiaSharp.GRSurfaceOrigin origin, SkiaSharp.SKImageInfo info);

	[Obsolete]
public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTarget renderTarget, SkiaSharp.GRSurfaceOrigin origin, SkiaSharp.SKColorType colorType, SkiaSharp.GRGlFramebufferInfo glInfo);
	public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTarget renderTarget, SkiaSharp.GRSurfaceOrigin origin, SkiaSharp.SKImageInfo info, SkiaSharp.SKImageInfo rawInfo);
	// properties
	public SkiaSharp.GRBackendRenderTarget BackendRenderTarget { get; }
	public SkiaSharp.SKColorType ColorType { get; }
	public SkiaSharp.SKImageInfo Info { get; }
	public SkiaSharp.GRSurfaceOrigin Origin { get; }
	public SkiaSharp.SKImageInfo RawInfo { get; }

	[Obsolete]
public SkiaSharp.GRBackendRenderTargetDesc RenderTarget { get; }
	public SkiaSharp.SKSurface Surface { get; }
}
```

#### New Type: SkiaSharp.Views.Android.SKPaintSurfaceEventArgs

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

