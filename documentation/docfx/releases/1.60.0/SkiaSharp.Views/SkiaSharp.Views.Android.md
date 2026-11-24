# API diff: SkiaSharp.Views.Android.dll

## SkiaSharp.Views.Android.dll

> Assembly Version Changed: 1.60.0.0 vs 1.59.0.0

### Namespace SkiaSharp.Views.Android

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

#### New Type: SkiaSharp.Views.Android.SKGLTextureView

```csharp
public class SKGLTextureView : SkiaSharp.Views.Android.GLTextureView, Android.Graphics.Drawables.Drawable.ICallback, Android.Runtime.IJavaObject, Android.Views.Accessibility.IAccessibilityEventSource, Android.Views.KeyEvent.ICallback, Android.Views.TextureView.ISurfaceTextureListener, Android.Views.View.IOnLayoutChangeListener, Java.Interop.IJavaPeerable, System.IDisposable {
	// constructors
	public SKGLTextureView (Android.Content.Context context);
	public SKGLTextureView (Android.Content.Context context, Android.Util.IAttributeSet attrs);
	// properties
	public SkiaSharp.SKSize CanvasSize { get; }
	public SkiaSharp.GRContext GRContext { get; }
	// methods
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
	protected virtual void OnDrawFrame (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTargetDesc renderTarget);
	public virtual void OnSurfaceChanged (Javax.Microedition.Khronos.Opengles.IGL10 gl, int width, int height);
	public virtual void OnSurfaceCreated (Javax.Microedition.Khronos.Opengles.IGL10 gl, Javax.Microedition.Khronos.Egl.EGLConfig config);
	public virtual void OnSurfaceDestroyed ();
}
```


