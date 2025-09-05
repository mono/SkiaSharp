#pragma warning disable CS0618

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Opengl;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Javax.Microedition.Khronos.Egl;
using Javax.Microedition.Khronos.Opengles;

using EGLConfig = Javax.Microedition.Khronos.Egl.EGLConfig;
using EGLContext = Javax.Microedition.Khronos.Egl.EGLContext;
using EGLDisplay = Javax.Microedition.Khronos.Egl.EGLDisplay;
using EGLSurface = Javax.Microedition.Khronos.Egl.EGLSurface;

#if HAS_UNO_WINUI
namespace SkiaSharp.Views.Windows
#elif HAS_UNO
namespace SkiaSharp.Views.UWP
#else
namespace SkiaSharp.Views.Android
#endif
{
	/// <summary>
	/// An implementation of <see cref="global::Android.Views.TextureView" /> that uses the dedicated surface for displaying OpenGL rendering.
	/// </summary>
#if HAS_UNO
	internal
#else
	public
#endif
	partial class GLTextureView : TextureView, TextureView.ISurfaceTextureListener, View.IOnLayoutChangeListener
	{
		internal static bool EnableLogging = false;

		private WeakReference<GLTextureView> thisWeakRef;
		private GLThread glThread;
		private IRenderer renderer;
		private bool detachedFromWindow;
		private IEGLConfigChooser eglConfigChooser;
		private IEGLContextFactory eglContextFactory;
		private IEGLWindowSurfaceFactory eglWindowSurfaceFactory;
		private IGLWrapper glWrapper;
		private int eglContextClientVersion;

		/// <summary>
		/// Simple constructor to use when creating a <see cref="GLTextureView" /> from code.
		/// </summary>
		/// <param name="context">The <see cref="global::Android.Content.Context" /> the view is running in, through which it can access the current theme, resources, etc.</param>
		public GLTextureView(Context context)
			: base(context)
		{
			Initialize();
		}

		/// <summary>
		/// Constructor that is called when inflating a <see cref="GLTextureView" /> from XML.
		/// </summary>
		/// <param name="context">The <see cref="global::Android.Content.Context" /> the view is running in, through which it can access the current theme, resources, etc.</param>
		/// <param name="attrs">The attributes of the XML tag that is inflating the view.</param>
		public GLTextureView(Context context, IAttributeSet attrs)
			: base(context, attrs)
		{
			Initialize();
		}

		private void Initialize()
		{
			thisWeakRef = new WeakReference<GLTextureView>(this);

			SurfaceTextureListener = this;
			AddOnLayoutChangeListener(this);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (glThread != null)
				{
					// GLThread may still be running if this view was never attached to a window.
					glThread.RequestExitAndWait();
				}
			}

			base.Dispose(disposing);
		}

		/// <summary>
		/// Gets or sets a value indicating whether the EGL context is preserved when the <see cref="global::Android.Views.TextureView" /> is paused and resumed.
		/// </summary>
		public bool PreserveEGLContextOnPause { get; set; }

		/// <summary>
		/// Gets or sets the current debug flags.
		/// </summary>
		public DebugFlags DebugFlags { get; set; }

		/// <summary>
		/// Set the GL wrapper.
		/// </summary>
		/// <param name="glWrapper">The GL wrapper.</param>
		/// <remarks>Wrapping is typically used for debugging purposes.</remarks>
		public void SetGLWrapper(IGLWrapper glWrapper)
		{
			this.glWrapper = glWrapper;
		}

		/// <summary>
		/// Set the renderer associated with this view. Also starts the thread that will call the renderer, which in turn causes the rendering to start.
		/// </summary>
		/// <param name="renderer">The renderer to use to perform OpenGL drawing.</param>
		/// <remarks>This method should be called once and only once in the life-cycle of a GLSurfaceView.</remarks>
		public void SetRenderer(IRenderer renderer)
		{
			CheckRenderThreadState();
			if (eglConfigChooser == null)
			{
				eglConfigChooser = new SimpleEGLConfigChooser(this, true);
			}
			if (eglContextFactory == null)
			{
				eglContextFactory = new DefaultContextFactory(this);
			}
			if (eglWindowSurfaceFactory == null)
			{
				eglWindowSurfaceFactory = new DefaultWindowSurfaceFactory();
			}
			this.renderer = renderer;
			glThread = new GLThread(thisWeakRef);
			glThread.Start();
		}

		/// <summary>
		/// Install a custom context factory.
		/// </summary>
		/// <param name="factory">The custom context factory.</param>
		/// <remarks>If this method is called, it must be called before <see cref="M:SkiaSharp.Views.Android.GLTextureView.SetRenderer(SkiaSharp.Views.Android.GLTextureView.IRenderer)" /> is called.</remarks>
		public void SetEGLContextFactory(IEGLContextFactory factory)
		{
			CheckRenderThreadState();
			eglContextFactory = factory;
		}

		/// <summary>
		/// Install a custom window surface factory.
		/// </summary>
		/// <param name="factory">The custom window surface factory.</param>
		/// <remarks>If this method is called, it must be called before <see cref="M:SkiaSharp.Views.Android.GLTextureView.SetRenderer(SkiaSharp.Views.Android.GLTextureView.IRenderer)" /> is called.</remarks>
		public void SetEGLWindowSurfaceFactory(IEGLWindowSurfaceFactory factory)
		{
			CheckRenderThreadState();
			eglWindowSurfaceFactory = factory;
		}

		/// <summary>
		/// Install a custom config chooser.
		/// </summary>
		/// <param name="configChooser">The custom config chooser.</param>
		/// <remarks>If no EGL config chooser is set, then by default the view will choose an RGB
		/// 888 surface with a depth buffer depth of at least 16 bits.
		/// If this method is called, it must be called before
		/// <see cref="SkiaSharp.Views.Android.GLTextureView.SetRenderer%2A" /> is called.</remarks>
		public void SetEGLConfigChooser(IEGLConfigChooser configChooser)
		{
			CheckRenderThreadState();
			eglConfigChooser = configChooser;
		}

		/// <summary>
		/// Install a config chooser which will choose a config as close to 16-bit RGB as possible, with or without an optional depth buffer as close to 16-bits as possible.
		/// </summary>
		/// <param name="needDepth">Whether or not to include a depth buffer.</param>
		/// <remarks>If no EGL config chooser is set, then by default the view will choose an RGB
		/// 888 surface with a depth buffer depth of at least 16 bits.
		/// If this method is called, it must be called before
		/// <see cref="SkiaSharp.Views.Android.GLTextureView.SetRenderer%2A" /> is called.</remarks>
		public void SetEGLConfigChooser(bool needDepth)
		{
			SetEGLConfigChooser(new SimpleEGLConfigChooser(this, needDepth));
		}

		/// <summary>
		/// Install a config chooser which will choose a config with at least the specified depth size and stencil size, and exactly the specified red size, green size, blue size and alpha size.
		/// </summary>
		/// <param name="redSize">The size of the red.</param>
		/// <param name="greenSize">The size of the green.</param>
		/// <param name="blueSize">The size of the blue.</param>
		/// <param name="alphaSize">The size of the alpha.</param>
		/// <param name="depthSize">The size of the depth buffer.</param>
		/// <param name="stencilSize">The size of the stencil buffer.</param>
		/// <remarks>If no EGL config chooser is set, then by default the view will choose an RGB
		/// 888 surface with a depth buffer depth of at least 16 bits.
		/// If this method is called, it must be called before
		/// <see cref="SkiaSharp.Views.Android.GLTextureView.SetRenderer%2A" /> is called.</remarks>
		public void SetEGLConfigChooser(int redSize, int greenSize, int blueSize, int alphaSize, int depthSize, int stencilSize)
		{
			SetEGLConfigChooser(new ComponentSizeChooser(this, redSize, greenSize, blueSize, alphaSize, depthSize, stencilSize));
		}

		/// <summary>
		/// Inform the default context factory and default config chooser which context client version to pick.
		/// </summary>
		/// <param name="version">The context client version to choose. Use 2 for OpenGL ES 2.0</param>
		/// <remarks>If this method is called, it must be called before <see cref="M:SkiaSharp.Views.Android.GLTextureView.SetRenderer(SkiaSharp.Views.Android.GLTextureView.IRenderer)" /> is called.</remarks>
		public void SetEGLContextClientVersion(int version)
		{
			CheckRenderThreadState();
			eglContextClientVersion = version;
		}

		/// <summary>
		/// Gets or sets the render mode.
		/// </summary>
		public Rendermode RenderMode
		{
			get { return glThread.GetRenderMode(); }
			set { glThread.SetRenderMode(value); }
		}

		/// <summary>
		/// Request that the renderer render a frame.
		/// </summary>
		public void RequestRender()
		{
			glThread.RequestRender();
		}

		/// <summary>
		/// Invoked when the specified texture is updated through Android.Graphics.SurfaceTexture.UpdateTexImage.
		/// </summary>
		/// <param name="surface">The surface texture.</param>
		public void OnSurfaceTextureUpdated(SurfaceTexture surface)
		{
			//glThread.RequestRender();
		}

		/// <summary>
		/// Invoked when a texture is ready for use.
		/// </summary>
		/// <param name="surface">The surface texture.</param>
		/// <param name="width">The width of the surface.</param>
		/// <param name="height">The height of the surface.</param>
		public void OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
		{
			glThread.OnSurfaceCreated();
			glThread.RequestRender();
		}

		/// <summary>
		/// Invoked when the specified texture is about to be destroyed.
		/// </summary>
		/// <param name="surface">The surface texture.</param>
		/// <returns>Returns <see langword="true" /> if no rendering should happen inside the surface texture after this method is invoked, otherwise <see langword="false" /> if the client needs to call Android.Graphics.SurfaceTexture.Release. Most applications should return <see langword="true" />.</returns>
		public bool OnSurfaceTextureDestroyed(SurfaceTexture surface)
		{
			// Surface will be destroyed when we return
			glThread.OnSurfaceDestroyed();
			return true;
		}

		/// <summary>
		/// Invoked when the texture's buffers size changed.
		/// </summary>
		/// <param name="surface">The surface texture.</param>
		/// <param name="w">The new width of the surface.</param>
		/// <param name="h">The new height of the surface.</param>
		public void OnSurfaceTextureSizeChanged(SurfaceTexture surface, int w, int h)
		{
			glThread.OnWindowResize(w, h);
		}

		/// <summary>
		/// Inform the view that the activity is paused.
		/// </summary>
		/// <remarks>The owner of this view must call this method when the activity is paused. Calling this method will pause the rendering thread. Must not be called before a renderer has been set.</remarks>
		public void OnPause()
		{
			glThread.OnPause();
		}

		/// <summary>
		/// Inform the view that the activity is resumed.
		/// </summary>
		/// <remarks>The owner of this view must call this method when the activity is resumed. Calling this method will recreate the OpenGL display and resume the rendering thread. Must not be called before a renderer has been set.</remarks>
		public void OnResume()
		{
			glThread.OnResume();
		}

		/// <summary>
		/// Queue an action to be run on the GL rendering thread.
		/// </summary>
		/// <param name="r">The action to be run on the GL rendering thread.</param>
		public void QueueEvent(Action r)
		{
			QueueEvent(new Java.Lang.Runnable(r));
		}

		/// <summary>
		/// Queue a runnable to be run on the GL rendering thread.
		/// </summary>
		/// <param name="r">The runnable to be run on the GL rendering thread.</param>
		public void QueueEvent(Java.Lang.IRunnable r)
		{
			glThread.QueueEvent(r);
		}

		protected override void OnAttachedToWindow()
		{
			base.OnAttachedToWindow();

			LogDebug($" OnAttachedToWindow");

			if (detachedFromWindow && (renderer != null))
			{
				var renderMode = Rendermode.Continuously;
				if (glThread != null)
				{
					renderMode = glThread.GetRenderMode();
				}
				glThread = new GLThread(thisWeakRef);
				if (renderMode != Rendermode.Continuously)
				{
					glThread.SetRenderMode(renderMode);
				}
				glThread.Start();
			}
			detachedFromWindow = false;
		}

		protected override void OnDetachedFromWindow()
		{
			LogDebug($" OnDetachedFromWindow reattach={detachedFromWindow}");

			if (glThread != null)
			{
				glThread.RequestExitAndWait();
			}
			detachedFromWindow = true;
			base.OnDetachedFromWindow();
		}

		private void CheckRenderThreadState()
		{
			if (glThread != null)
			{
				throw new Exception("setRenderer has already been called for this instance.");
			}
		}

		/// <summary>
		/// Called when the layout bounds of a view changes due to layout processing.
		/// </summary>
		/// <param name="v">The view whose bounds have changed.</param>
		/// <param name="left">The new value of the view's left property.</param>
		/// <param name="top">The new value of the view's top property.</param>
		/// <param name="right">The new value of the view's right property.</param>
		/// <param name="bottom">The new value of the view's bottom property.</param>
		/// <param name="oldLeft">The previous value of the view's left property.</param>
		/// <param name="oldTop">The previous value of the view's top property.</param>
		/// <param name="oldRight">The previous value of the view's right property.</param>
		/// <param name="oldBottom">The previous value of the view's bottom property.</param>
		public void OnLayoutChange(View v, int left, int top, int right, int bottom, int oldLeft, int oldTop, int oldRight, int oldBottom)
		{
			OnSurfaceTextureSizeChanged(SurfaceTexture, right - left, bottom - top);
		}

		[Conditional("DEBUG")]
		private static void LogDebug(string message)
		{
			if (EnableLogging)
			{
				Log.Debug("GLTextureView", message);
			}
		}

		[Conditional("DEBUG")]
		private static void LogError(string message)
		{
			Log.Error("GLTextureView", message);
		}

		/// <summary>
		/// An interface used to wrap a GL interface.
		/// </summary>
		/// <remarks>Typically used for implementing debugging and tracing on top of the default GL interface. You would typically use this by creating your own class that implemented all the GL methods by delegating to another GL instance. Then you could add your own behavior before or after calling the delegate. All the wrapper would do was instantiate and return the wrapped GL instance.</remarks>
		public interface IGLWrapper
		{
			IGL Wrap(IGL gl);
		}

		/// <summary>
		/// An interface for choosing an EGLConfig configuration from a list of potential configurations.
		/// </summary>
		public interface IEGLConfigChooser
		{
			EGLConfig ChooseConfig(IEGL10 egl, EGLDisplay display);
		}

		/// <summary>
		/// An interface for customizing the eglCreateContext and eglDestroyContext calls.
		/// </summary>
		public interface IEGLContextFactory
		{
			EGLContext CreateContext(IEGL10 egl, EGLDisplay display, EGLConfig eglConfig);

			void DestroyContext(IEGL10 egl, EGLDisplay display, EGLContext context);
		}

		/// <summary>
		/// An interface for customizing the eglCreateWindowSurface and eglDestroySurface calls.
		/// </summary>
		public interface IEGLWindowSurfaceFactory
		{
			EGLSurface CreateWindowSurface(IEGL10 egl, EGLDisplay display, EGLConfig config, Java.Lang.Object nativeWindow);

			void DestroySurface(IEGL10 egl, EGLDisplay display, EGLSurface surface);
		}

		/// <summary>
		/// A generic renderer interface.
		/// </summary>
		/// <remarks>The renderer is responsible for making OpenGL calls to render a frame.
		/// Typically, <see cref="SkiaSharp.Views.Android.GLTextureView" /> clients create their
		/// own classes that implement this interface, and then call
		/// <xref:SkiaSharp.Views.Android.GLTextureView.SetRenderer%2A?displayProperty=nameWithType>
		/// to register the renderer with the <see cref="SkiaSharp.Views.Android.GLTextureView" />.</remarks>
		public interface IRenderer
		{
			void OnDrawFrame(IGL10 gl);

			void OnSurfaceChanged(IGL10 gl, int width, int height);

			void OnSurfaceCreated(IGL10 gl, EGLConfig config);

			void OnSurfaceDestroyed();
		}

		private class DefaultContextFactory : IEGLContextFactory
		{
			private GLTextureView textureView;

			public DefaultContextFactory(GLTextureView textureView)
			{
				this.textureView = textureView;
			}

			public EGLContext CreateContext(IEGL10 egl, EGLDisplay display, EGLConfig config)
			{
				var attribList = new int[] {
					EglHelper.EGL_CONTEXT_CLIENT_VERSION, textureView.eglContextClientVersion,
					EGL10.EglNone
				};

				return egl.EglCreateContext(display, config, EGL10.EglNoContext, textureView.eglContextClientVersion != 0 ? attribList : null);
			}

			public void DestroyContext(IEGL10 egl, EGLDisplay display, EGLContext context)
			{
				LogDebug($"[DefaultContextFactory] DestroyContext tid={Thread.CurrentThread.ManagedThreadId} display={display} context={context}");

				if (!egl.EglDestroyContext(display, context))
				{
					var error = egl.EglGetError();
					LogError($"[DefaultContextFactory] eglDestroyContext failed: {error}");
					throw new Exception($"eglDestroyContext failed: {error}");
				}
			}
		}

		private class DefaultWindowSurfaceFactory : IEGLWindowSurfaceFactory
		{
			public EGLSurface CreateWindowSurface(IEGL10 egl, EGLDisplay display, EGLConfig config, Java.Lang.Object nativeWindow)
			{
				EGLSurface result = null;
				try
				{
					result = egl.EglCreateWindowSurface(display, config, nativeWindow, null);
				}
				catch (Exception ex)
				{
					// This exception indicates that the surface flinger surface
					// is not valid. This can happen if the surface flinger surface has
					// been torn down, but the application has not yet been
					// notified via SurfaceHolder.Callback.surfaceDestroyed.
					// In theory the application should be notified first,
					// but in practice sometimes it is not. See b/4588890
					LogError($"[DefaultWindowSurfaceFactory] eglCreateWindowSurface failed: {ex}");
				}
				return result;
			}

			public void DestroySurface(IEGL10 egl, EGLDisplay display, EGLSurface surface)
			{
				egl.EglDestroySurface(display, surface);
			}
		}

		private abstract class BaseConfigChooser : IEGLConfigChooser
		{
			private GLTextureView textureView;
			private int[] configSpec;

			public BaseConfigChooser(GLTextureView textureView, int[] configSpec)
			{
				this.textureView = textureView;
				this.configSpec = FilterConfigSpec(configSpec);
			}

			public EGLConfig ChooseConfig(IEGL10 egl, EGLDisplay display)
			{
				var numConfig = new int[1];
				if (!egl.EglChooseConfig(display, configSpec, null, 0, numConfig))
				{
					throw new Exception("eglChooseConfig failed");
				}

				var numConfigs = numConfig[0];
				if (numConfigs <= 0)
				{
					throw new Exception("No configs match configSpec");
				}

				var configs = new EGLConfig[numConfigs];
				if (!egl.EglChooseConfig(display, configSpec, configs, numConfigs, numConfig))
				{
					throw new Exception("eglChooseConfig#2 failed");
				}
				var config = ChooseConfig(egl, display, configs);
				if (config == null)
				{
					throw new Exception("No config chosen");
				}
				return config;
			}

			public abstract EGLConfig ChooseConfig(IEGL10 egl, EGLDisplay display, EGLConfig[] configs);

			private int[] FilterConfigSpec(int[] spec)
			{
				if (textureView.eglContextClientVersion != 2)
				{
					return spec;
				}

				// We know none of the subclasses define EGL_RENDERABLE_TYPE.
				// And we know the configSpec is well formed.
				var len = spec.Length;
				var newConfigSpec = new int[len + 2];
				Array.Copy(spec, 0, newConfigSpec, 0, len - 1);
				newConfigSpec[len - 1] = EGL10.EglRenderableType;
				newConfigSpec[len] = EglHelper.EGL_OPENGL_ES2_BIT;
				newConfigSpec[len + 1] = EGL10.EglNone;
				return newConfigSpec;
			}
		}

		private class ComponentSizeChooser : BaseConfigChooser
		{
			private int[] value;
			private int redSize;
			private int greenSize;
			private int blueSize;
			private int alphaSize;
			private int depthSize;
			private int stencilSize;

			public ComponentSizeChooser(GLTextureView textureView, int redSize, int greenSize, int blueSize, int alphaSize, int depthSize, int stencilSize)
				: base(textureView, new int[] {
					EGL10.EglRedSize, redSize,
					EGL10.EglGreenSize, greenSize,
					EGL10.EglBlueSize, blueSize,
					EGL10.EglAlphaSize, alphaSize,
					EGL10.EglDepthSize, depthSize,
					EGL10.EglStencilSize, stencilSize,
					EGL10.EglNone
				})
			{
				value = new int[1];
				this.redSize = redSize;
				this.greenSize = greenSize;
				this.blueSize = blueSize;
				this.alphaSize = alphaSize;
				this.depthSize = depthSize;
				this.stencilSize = stencilSize;
			}

			public override EGLConfig ChooseConfig(IEGL10 egl, EGLDisplay display, EGLConfig[] configs)
			{
				foreach (var config in configs)
				{
					var d = FindConfigAttrib(egl, display, config, EGL10.EglDepthSize, 0);
					var s = FindConfigAttrib(egl, display, config, EGL10.EglStencilSize, 0);
					if ((d >= depthSize) && (s >= stencilSize))
					{
						var r = FindConfigAttrib(egl, display, config, EGL10.EglRedSize, 0);
						var g = FindConfigAttrib(egl, display, config, EGL10.EglGreenSize, 0);
						var b = FindConfigAttrib(egl, display, config, EGL10.EglBlueSize, 0);
						var a = FindConfigAttrib(egl, display, config, EGL10.EglAlphaSize, 0);
						if ((r == redSize) && (g == greenSize) && (b == blueSize) && (a == alphaSize))
						{
							return config;
						}
					}
				}
				return null;
			}

			private int FindConfigAttrib(IEGL10 egl, EGLDisplay display, EGLConfig config, int attribute, int defaultValue)
			{
				if (egl.EglGetConfigAttrib(display, config, attribute, value))
				{
					return value[0];
				}
				return defaultValue;
			}
		}

		private class SimpleEGLConfigChooser : ComponentSizeChooser
		{
			public SimpleEGLConfigChooser(GLTextureView textureView, bool withDepthBuffer)
				: base(textureView, 8, 8, 8, 0, withDepthBuffer ? 16 : 0, 0)
			{
			}
		}

		private class EglHelper
		{
			public const int EGL_CONTEXT_CLIENT_VERSION = 0x3098;
			public const int EGL_OPENGL_ES2_BIT = 4;
			public const string MSM7K_RENDERER_PREFIX = "Q3Dimension MSM7500 ";
			public const int GLES_20 = 0x20000;

			private WeakReference<GLTextureView> textureViewWeakRef;
			private IEGL10 egl;
			private EGLDisplay eglDisplay;
			private EGLSurface eglSurface;
			private EGLContext eglContext;
			private EGLConfig eglConfig;

			public EglHelper(WeakReference<GLTextureView> glTextureViewWeakRef)
			{
				textureViewWeakRef = glTextureViewWeakRef;
			}

			public EGLConfig EglConfig => eglConfig;

			private int CurrentThreadId => Thread.CurrentThread.ManagedThreadId;

			public void Start()
			{
				LogDebug($"[GLThread {CurrentThreadId}][EglHelper] Start");

				// Get an EGL instance
				egl = EGLContext.EGL.JavaCast<IEGL10>();

				// Get to the default display.
				eglDisplay = egl.EglGetDisplay(EGL10.EglDefaultDisplay);

				if (eglDisplay == EGL10.EglNoDisplay)
				{
					throw new Exception("eglGetDisplay failed");
				}

				// We can now initialize EGL for that display
				var version = new int[2];
				if (!egl.EglInitialize(eglDisplay, version))
				{
					throw new Exception("eglInitialize failed");
				}
				if (!textureViewWeakRef.TryGetTarget(out GLTextureView view))
				{
					eglConfig = null;
					eglContext = null;
				}
				else
				{
					eglConfig = view.eglConfigChooser.ChooseConfig(egl, eglDisplay);
					// Create an EGL context. We want to do this as rarely as we can, because an
					// EGL context is a somewhat heavy object.
					eglContext = view.eglContextFactory.CreateContext(egl, eglDisplay, eglConfig);
				}
				if (eglContext == null || eglContext == EGL10.EglNoContext)
				{
					eglContext = null;

					var error = egl.EglGetError();
					LogError($"[GLThread {CurrentThreadId}][EglHelper] createContext failed: {error}");
					throw new Exception($"createContext failed: {error}");
				}

				LogDebug($"[GLThread {CurrentThreadId}][EglHelper] createContext {eglContext}");

				eglSurface = null;
			}

			public bool CreateSurface()
			{
				LogDebug($"[GLThread {CurrentThreadId}][EglHelper] CreateSurface");

				if (egl == null)
				{
					throw new Exception("egl not initialized");
				}
				if (eglDisplay == null)
				{
					throw new Exception("eglDisplay not initialized");
				}
				if (eglConfig == null)
				{
					throw new Exception("mEglConfig not initialized");
				}
				// The window size has changed, so we need to create a new surface.
				DestroySurfaceImpl();

				// Create an EGL surface we can render into.
				if (textureViewWeakRef.TryGetTarget(out GLTextureView view))
				{
					eglSurface = view.eglWindowSurfaceFactory.CreateWindowSurface(egl, eglDisplay, eglConfig, view.SurfaceTexture);
				}
				else
				{
					eglSurface = null;
				}
				if (eglSurface == null || eglSurface == EGL10.EglNoSurface)
				{
					var error = egl.EglGetError();
					if (error == EGL10.EglBadNativeWindow)
					{
						LogError($"[GLThread {CurrentThreadId}][EglHelper] createWindowSurface returned EGL_BAD_NATIVE_WINDOW");
					}
					return false;
				}
				// Before we can issue IGL commands, we need to make sure the context is 
				// current and bound to a surface.
				if (!egl.EglMakeCurrent(eglDisplay, eglSurface, eglSurface, eglContext))
				{
					// Could not make the context current, probably because the underlying
					// TextureView surface has been destroyed.
					LogError($"[GLThread {CurrentThreadId}][EglHelper] eglMakeCurrent failed: {egl.EglGetError()}");
					return false;
				}
				return true;
			}

			public IGL CreateGL()
			{
				var gl = eglContext.GL;
				if (textureViewWeakRef.TryGetTarget(out GLTextureView view))
				{
					if (view.glWrapper != null)
					{
						gl = view.glWrapper.Wrap(gl);
					}

					if (view.DebugFlags.HasFlag(DebugFlags.CheckGlError | DebugFlags.LogGlCalls))
					{
						var configFlags = 0;
						Java.IO.Writer log = null;
						if (view.DebugFlags.HasFlag(DebugFlags.CheckGlError))
						{
							configFlags |= (int)GLDebugConfig.CheckGlError;
						}
						if (view.DebugFlags.HasFlag(DebugFlags.LogGlCalls))
						{
							log = new LogWriter();
						}
						gl = GLDebugHelper.Wrap(gl, configFlags, log);
					}
				}
				return gl;
			}

			public int Swap()
			{
				if (!egl.EglSwapBuffers(eglDisplay, eglSurface))
				{
					return egl.EglGetError();
				}
				return EGL10.EglSuccess;
			}

			public void DestroySurface()
			{
				LogDebug($"[GLThread {CurrentThreadId}][EglHelper] DestroySurface");

				if (textureViewWeakRef.TryGetTarget(out GLTextureView view))
				{
					view.renderer.OnSurfaceDestroyed();
				}
				DestroySurfaceImpl();
			}

			private void DestroySurfaceImpl()
			{
				if (eglSurface != null && eglSurface != EGL10.EglNoSurface)
				{
					egl.EglMakeCurrent(eglDisplay, EGL10.EglNoSurface, EGL10.EglNoSurface, EGL10.EglNoContext);
					if (textureViewWeakRef.TryGetTarget(out GLTextureView view))
					{
						view.eglWindowSurfaceFactory.DestroySurface(egl, eglDisplay, eglSurface);
					}
					eglSurface = null;
				}
			}

			public void Finish()
			{
				LogDebug($"[GLThread {CurrentThreadId}][EglHelper] Finish");

				if (eglContext != null)
				{
					if (textureViewWeakRef.TryGetTarget(out GLTextureView view))
					{
						view.eglContextFactory.DestroyContext(egl, eglDisplay, eglContext);
					}
					eglContext = null;
				}
				if (eglDisplay != null)
				{
					egl.EglTerminate(eglDisplay);
					eglDisplay = null;
				}
			}
		}

		private class GLThread
		{
			private Thread thread;
			private GLThreadManager threadManager;
			private EglHelper eglHelper;
			private WeakReference<GLTextureView> textureViewWeakRef;

			// Once the thread is started, all accesses to the following member
			// variables are protected by the sGLThreadManager monitor
			private bool shouldExit;
			public bool exited;
			private bool requestPaused;
			private bool paused;
			private bool hasSurface;
			private bool surfaceIsBad;
			private bool waitingForSurface;
			private bool haveEglContext;
			private bool haveEglSurface;
			private bool finishedCreatingEglSurface;
			private bool shouldReleaseEglContext;
			private int width;
			private int height;
			private Rendermode renderMode;
			private bool requestRender;
			private bool renderComplete;
			private Queue<Java.Lang.IRunnable> eventQueue = new Queue<Java.Lang.IRunnable>();
			private bool surfaceSizeChanged = true;
			// End of member variables protected by the sGLThreadManager monitor.

			public GLThread(WeakReference<GLTextureView> glTextureViewWeakRef)
			{
				threadManager = new GLThreadManager();

				width = 0;
				height = 0;
				requestRender = true;
				renderMode = Rendermode.Continuously;
				textureViewWeakRef = glTextureViewWeakRef;
				thread = new Thread(new ThreadStart(Run));
			}

			public int Id => thread.ManagedThreadId;

			public void Start()
			{
				thread.Start();
			}

			public void Run()
			{
				thread.Name = "GLThread " + thread.ManagedThreadId;

				LogDebug($"[GLThread {Id}] Starting '{thread.Name}'");

				try
				{
					GuardedRun();
				}
				finally
				{
					threadManager.ThreadExiting(this);
				}
			}

			private void StopEglSurfaceLocked()
			{
				if (haveEglSurface)
				{
					haveEglSurface = false;
					eglHelper.DestroySurface();
				}
			}

			private void StopEglContextLocked()
			{
				if (haveEglContext)
				{
					eglHelper.Finish();
					haveEglContext = false;
					threadManager.ReleaseEglContextLocked(this);
				}
			}

			private void GuardedRun()
			{
				eglHelper = new EglHelper(textureViewWeakRef);
				haveEglContext = false;
				haveEglSurface = false;
				try
				{
					IGL10 gl = null;
					var createEglContext = false;
					var createEglSurface = false;
					var createGlInterface = false;
					var lostEglContext = false;
					var sizeChanged = false;
					var wantRenderNotification = false;
					var doRenderNotification = false;
					var askedToReleaseEglContext = false;
					var w = 0;
					var h = 0;
					Java.Lang.IRunnable ev = null;

					while (true)
					{
						lock (threadManager)
						{
							while (true)
							{
								if (shouldExit)
								{
									return;
								}

								if (eventQueue.Count > 0)
								{
									ev = eventQueue.Dequeue();
									break;
								}

								// Update the pause state.
								var pausing = false;
								if (paused != requestPaused)
								{
									pausing = requestPaused;
									paused = requestPaused;
									Monitor.PulseAll(threadManager);

									LogDebug($"[GLThread {Id}] paused is now {paused}");
								}

								// Do we need to give up the EGL context?
								if (shouldReleaseEglContext)
								{
									LogDebug($"[GLThread {Id}] Releasing EGL context because asked to");

									StopEglSurfaceLocked();
									StopEglContextLocked();
									shouldReleaseEglContext = false;
									askedToReleaseEglContext = true;
								}

								// Have we lost the EGL context?
								if (lostEglContext)
								{
									StopEglSurfaceLocked();
									StopEglContextLocked();
									lostEglContext = false;
								}

								// When pausing, release the EGL surface:
								if (pausing && haveEglSurface)
								{
									LogDebug($"[GLThread {Id}] Releasing EGL surface because paused");

									StopEglSurfaceLocked();
								}

								// When pausing, optionally release the EGL Context:
								if (pausing && haveEglContext)
								{
									textureViewWeakRef.TryGetTarget(out GLTextureView view);
									var preserveEglContextOnPause = view == null ? false : view.PreserveEGLContextOnPause;
									if (!preserveEglContextOnPause || threadManager.ShouldReleaseEGLContextWhenPausing())
									{
										StopEglContextLocked();

										LogDebug($"[GLThread {Id}] Releasing EGL context because paused");
									}
								}

								// When pausing, optionally terminate EGL:
								if (pausing)
								{
									if (threadManager.ShouldTerminateEGLWhenPausing())
									{
										eglHelper.Finish();

										LogDebug($"[GLThread {Id}] Terminating EGL because paused");
									}
								}

								// Have we lost the TextureView surface?
								if ((!hasSurface) && (!waitingForSurface))
								{
									LogDebug($"[GLThread {Id}] Noticed TextureView surface lost");

									if (haveEglSurface)
									{
										StopEglSurfaceLocked();
									}
									waitingForSurface = true;
									surfaceIsBad = false;
									Monitor.PulseAll(threadManager);
								}

								// Have we acquired the surface view surface?
								if (hasSurface && waitingForSurface)
								{
									LogDebug($"[GLThread {Id}] Noticed TextureView surface acquired");

									waitingForSurface = false;
									Monitor.PulseAll(threadManager);
								}

								if (doRenderNotification)
								{
									LogDebug($"[GLThread {Id}] Sending render notification");

									wantRenderNotification = false;
									doRenderNotification = false;
									renderComplete = true;
									Monitor.PulseAll(threadManager);
								}

								// Ready to draw?
								if (IsReadyToDraw())
								{
									// If we don't have an EGL context, try to acquire one.
									if (!haveEglContext)
									{
										if (askedToReleaseEglContext)
										{
											askedToReleaseEglContext = false;
										}
										else if (threadManager.TryAcquireEglContextLocked(this))
										{
											try
											{
												eglHelper.Start();
											}
											catch (Exception)
											{
												threadManager.ReleaseEglContextLocked(this);
												throw;
											}
											haveEglContext = true;
											createEglContext = true;

											Monitor.PulseAll(threadManager);
										}
									}

									if (haveEglContext && !haveEglSurface)
									{
										haveEglSurface = true;
										createEglSurface = true;
										createGlInterface = true;
										sizeChanged = true;
									}

									if (haveEglSurface)
									{
										if (surfaceSizeChanged)
										{
											sizeChanged = true;
											w = width;
											h = height;
											wantRenderNotification = true;

											LogDebug($"[GLThread {Id}] Noticing that we want render notification");

											// Destroy and recreate the EGL surface.
											createEglSurface = true;
											surfaceSizeChanged = false;
										}
										requestRender = false;
										Monitor.PulseAll(threadManager);
										break;
									}
								}

								LogDebug($"[GLThread {Id}] Waiting mHaveEglContext={haveEglContext} mHaveEglSurface={haveEglSurface} mFinishedCreatingEglSurface={finishedCreatingEglSurface} paused={paused} hasSurface={hasSurface} surfaceIsBad={surfaceIsBad} mWaitingForSurface={waitingForSurface} mWidth={width} mHeight={height} mRequestRender={requestRender} mRenderMode={renderMode}");

								// By design, this is the only place in a GLThread thread where we Wait().
								Monitor.Wait(threadManager);
							}
						} // end of lock(sGLThreadManager)

						if (ev != null)
						{
							ev.Run();
							ev = null;
							continue;
						}

						if (createEglSurface)
						{
							LogDebug($"[GLThread {Id}] EGL create surface");

							if (eglHelper.CreateSurface())
							{
								lock (threadManager)
								{
									finishedCreatingEglSurface = true;
									Monitor.PulseAll(threadManager);
								}
							}
							else
							{
								lock (threadManager)
								{
									finishedCreatingEglSurface = true;
									surfaceIsBad = true;
									Monitor.PulseAll(threadManager);
								}
								continue;
							}
							createEglSurface = false;
						}

						if (createGlInterface)
						{
							gl = eglHelper.CreateGL().JavaCast<IGL10>();

							threadManager.CheckGLDriver(gl);
							createGlInterface = false;
						}

						if (createEglContext)
						{
							LogDebug($"[GLThread {Id}] OnSurfaceCreated");

							if (textureViewWeakRef.TryGetTarget(out GLTextureView view))
							{
								view.renderer.OnSurfaceCreated(gl, eglHelper.EglConfig);
							}
							createEglContext = false;
						}

						if (sizeChanged)
						{
							LogDebug($"[GLThread {Id}] OnSurfaceChanged({w}, {h})");

							if (textureViewWeakRef.TryGetTarget(out GLTextureView view))
							{
								view.renderer.OnSurfaceChanged(gl, w, h);
							}
							sizeChanged = false;
						}

						{
							LogDebug($"[GLThread {Id}] OnDrawFrame");

							if (textureViewWeakRef.TryGetTarget(out GLTextureView view))
							{
								view.renderer.OnDrawFrame(gl);
							}
						}

						var swapError = eglHelper.Swap();
						switch (swapError)
						{
							case EGL10.EglSuccess:
								break;
							case EGL11.EglContextLost:
								LogDebug($"[GLThread {Id}] EGL context lost");
								lostEglContext = true;
								break;
							default:
								// Other errors typically mean that the current surface is bad,
								// probably because the TextureView surface has been destroyed,
								// but we haven't been notified yet.
								LogError($"[GLThread {Id}] eglSwapBuffers failed: {swapError}");

								lock (threadManager)
								{
									surfaceIsBad = true;
									Monitor.PulseAll(threadManager);
								}
								break;
						}

						if (wantRenderNotification)
						{
							doRenderNotification = true;
						}
					}
				}
				finally
				{
					lock (threadManager)
					{
						StopEglSurfaceLocked();
						StopEglContextLocked();
					}
				}
			}

			public bool IsAbleToDraw()
			{
				return haveEglContext && haveEglSurface && IsReadyToDraw();
			}

			private bool IsReadyToDraw()
			{
				return (!paused) && hasSurface && (!surfaceIsBad) && (width > 0) && (height > 0) && (requestRender || (renderMode == Rendermode.Continuously));
			}

			public void SetRenderMode(Rendermode mode)
			{
				lock (threadManager)
				{
					renderMode = mode;
					Monitor.PulseAll(threadManager);
				}
			}

			public Rendermode GetRenderMode()
			{
				lock (threadManager)
				{
					return renderMode;
				}
			}

			public void RequestRender()
			{
				lock (threadManager)
				{
					requestRender = true;
					Monitor.PulseAll(threadManager);
				}
			}

			public void OnSurfaceCreated()
			{
				lock (threadManager)
				{
					LogDebug($"[GLThread {Id}] OnSurfaceCreated");

					hasSurface = true;
					finishedCreatingEglSurface = false;
					Monitor.PulseAll(threadManager);
					while (waitingForSurface && !finishedCreatingEglSurface && !exited)
					{
						try
						{
							Monitor.Wait(threadManager);
						}
						catch (Exception)
						{
							Thread.CurrentThread.Interrupt();
						}
					}
				}
			}

			public void OnSurfaceDestroyed()
			{
				lock (threadManager)
				{
					LogDebug($"[GLThread {Id}] OnSurfaceDestroyed");

					hasSurface = false;
					Monitor.PulseAll(threadManager);
					while ((!waitingForSurface) && (!exited))
					{
						try
						{
							Monitor.Wait(threadManager);
						}
						catch (Exception)
						{
							Thread.CurrentThread.Interrupt();
						}
					}
				}
			}

			public void OnPause()
			{
				lock (threadManager)
				{
					LogDebug($"[GLThread {Id}] OnPause");

					requestPaused = true;
					Monitor.PulseAll(threadManager);

					while ((!exited) && (!paused))
					{
						LogDebug($"[GLThread {Id}] OnPause: Waiting for paused==True");

						try
						{
							Monitor.Wait(threadManager);
						}
						catch (Exception)
						{
							Thread.CurrentThread.Interrupt();
						}
					}
				}
			}

			public void OnResume()
			{
				lock (threadManager)
				{
					LogDebug($"[GLThread {Id}] OnResume");

					requestPaused = false;
					requestRender = true;
					renderComplete = false;
					Monitor.PulseAll(threadManager);
					while ((!exited) && paused && (!renderComplete))
					{
						LogDebug($"[GLThread {Id}] OnResume: Waiting for paused==False");

						try
						{
							Monitor.Wait(threadManager);
						}
						catch (Exception)
						{
							Thread.CurrentThread.Interrupt();
						}
					}
				}
			}

			public void OnWindowResize(int w, int h)
			{
				lock (threadManager)
				{
					width = w;
					height = h;
					surfaceSizeChanged = true;
					requestRender = true;
					renderComplete = false;
					Monitor.PulseAll(threadManager);

					// Wait for thread to react to resize and render a frame
					while (!exited && !paused && !renderComplete && IsAbleToDraw())
					{
						LogDebug($"[GLThread {Id}] OnWindowResize: Waiting for render complete");

						try
						{
							Monitor.Wait(threadManager);
						}
						catch (Exception)
						{
							Thread.CurrentThread.Interrupt();
						}
					}
				}
			}

			public void RequestExitAndWait()
			{
				// don't call this from GLThread thread or it is a guaranteed deadlock!
				lock (threadManager)
				{
					shouldExit = true;
					Monitor.PulseAll(threadManager);
					while (!exited)
					{
						try
						{
							Monitor.Wait(threadManager);
						}
						catch (Exception)
						{
							Thread.CurrentThread.Interrupt();
						}
					}
				}
			}

			public void RequestReleaseEglContextLocked()
			{
				shouldReleaseEglContext = true;
				Monitor.PulseAll(threadManager);
			}

			public void QueueEvent(Java.Lang.IRunnable r)
			{
				if (r == null)
				{
					throw new ArgumentNullException(nameof(r));
				}

				lock (threadManager)
				{
					eventQueue.Enqueue(r);
					Monitor.PulseAll(threadManager);
				}
			}
		}

		private class LogWriter : Java.IO.Writer
		{
			private Java.Lang.StringBuilder builder = new Java.Lang.StringBuilder();

			public override void Close()
			{
				FlushBuilder();
			}

			public override void Flush()
			{
				FlushBuilder();
			}

			public override void Write(char[] buf, int offset, int count)
			{
				for (var i = 0; i < count; i++)
				{
					var c = buf[offset + i];
					if (c == '\n')
					{
						FlushBuilder();
					}
					else
					{
						builder.Append(c);
					}
				}
			}

			private void FlushBuilder()
			{
				if (builder.Length() > 0)
				{
					LogDebug($"[LogWriter] {builder.ToString()}");
					builder.Delete(0, builder.Length());
				}
			}
		}

		private class GLThreadManager
		{
			private bool glesVersionCheckComplete;
			private int glesVersion;
			private bool glesDriverCheckComplete;
			private bool multipleGLESContextsAllowed;
			private bool limitedGLESContexts;
			private GLThread eglOwner;

			public void ThreadExiting(GLThread thread)
			{
				lock (this)
				{
					LogDebug($"[GLThreadManager] ThreadExiting: tid = '{eglOwner?.Id}'");

					thread.exited = true;
					if (eglOwner == thread)
					{
						eglOwner = null;
					}
					Monitor.PulseAll(this);
				}
			}

			public bool TryAcquireEglContextLocked(GLThread thread)
			{
				if (eglOwner == thread || eglOwner == null)
				{
					eglOwner = thread;
					Monitor.PulseAll(this);
					return true;
				}
				CheckGLESVersion();
				if (multipleGLESContextsAllowed)
				{
					return true;
				}
				// Notify the owning thread that it should release the context.
				if (eglOwner != null)
				{
					eglOwner.RequestReleaseEglContextLocked();
				}
				return false;
			}

			public void ReleaseEglContextLocked(GLThread thread)
			{
				if (eglOwner == thread)
				{
					eglOwner = null;
				}
				Monitor.PulseAll(this);
			}

			public bool ShouldReleaseEGLContextWhenPausing()
			{
				lock (this)
				{
					// Release the EGL context when pausing even if
					// the hardware supports multiple EGL contexts.
					// Otherwise the device could run out of EGL contexts.
					return limitedGLESContexts;
				}
			}

			public bool ShouldTerminateEGLWhenPausing()
			{
				lock (this)
				{
					CheckGLESVersion();
					return !multipleGLESContextsAllowed;
				}
			}

			public void CheckGLDriver(IGL10 gl)
			{
				lock (this)
				{
					if (!glesDriverCheckComplete)
					{
						CheckGLESVersion();
						var renderer = gl.GlGetString(GL10.GlRenderer);
						if (glesVersion < EglHelper.GLES_20)
						{
							multipleGLESContextsAllowed = !renderer.StartsWith(EglHelper.MSM7K_RENDERER_PREFIX);
							Monitor.PulseAll(this);
						}
						limitedGLESContexts = !multipleGLESContextsAllowed;

						LogDebug($"[GLThreadManager] CheckGLDriver: renderer = '{renderer}' multipleContextsAllowed = '{multipleGLESContextsAllowed}' mLimitedGLESContexts = '{limitedGLESContexts}'");

						glesDriverCheckComplete = true;
					}
				}
			}

			private void CheckGLESVersion()
			{
				// This check was required for some pre-Android-3.0 hardware. Android 3.0 provides
				// support for hardware-accelerated views, therefore multiple EGL contexts are
				// supported on all Android 3.0+ EGL drivers.
				if (!glesVersionCheckComplete)
				{
					// SystemProperties.getInt("ro.opengles.version", ConfigurationInfo.GL_ES_VERSION_UNDEFINED)
					var activityManager = ActivityManager.FromContext(Application.Context);
					var configInfo = activityManager.DeviceConfigurationInfo;
					if (configInfo.ReqGlEsVersion != ConfigurationInfo.GlEsVersionUndefined)
					{
						glesVersion = configInfo.ReqGlEsVersion;
					}
					else
					{
						glesVersion = 1 << 16; // Lack of property means OpenGL ES version 1
					}
					if (glesVersion >= EglHelper.GLES_20)
					{
						multipleGLESContextsAllowed = true;
					}
					glesVersionCheckComplete = true;
				}
			}
		}
	}
}
