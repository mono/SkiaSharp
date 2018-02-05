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

namespace SkiaSharp.Views.Android
{
	public class GLTextureView : TextureView, TextureView.ISurfaceTextureListener, View.IOnLayoutChangeListener
	{
		private const bool EnableLogging = false;

		private WeakReference<GLTextureView> thisWeakRef;
		private GLThread mGLThread;
		private IRenderer mRenderer;
		private bool mDetached;
		private IEGLConfigChooser mEGLConfigChooser;
		private IEGLContextFactory mEGLContextFactory;
		private IEGLWindowSurfaceFactory mEGLWindowSurfaceFactory;
		private IGLWrapper mGLWrapper;
		private int mEGLContextClientVersion;

		public GLTextureView(Context context)
			: base(context)
		{
			Initialize();
		}

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

		//~GLTextureView()
		//{
		//	try
		//	{
		//		if (mGLThread != null)
		//		{
		//			// GLThread may still be running if this view was never attached to a window.
		//			mGLThread.requestExitAndWait();
		//		}
		//	}
		//	finally
		//	{
		//		//base.Finalize();
		//	}
		//}

		public bool PreserveEGLContextOnPause { get; set; }

		public DebugFlags DebugFlags { get; set; }

		public void SetGLWrapper(IGLWrapper glWrapper)
		{
			mGLWrapper = glWrapper;
		}

		public void SetRenderer(IRenderer renderer)
		{
			CheckRenderThreadState();
			if (mEGLConfigChooser == null)
			{
				mEGLConfigChooser = new SimpleEGLConfigChooser(this, true);
			}
			if (mEGLContextFactory == null)
			{
				mEGLContextFactory = new DefaultContextFactory(this);
			}
			if (mEGLWindowSurfaceFactory == null)
			{
				mEGLWindowSurfaceFactory = new DefaultWindowSurfaceFactory();
			}
			mRenderer = renderer;
			mGLThread = new GLThread(thisWeakRef);
			mGLThread.Start();
		}

		public void SetEGLContextFactory(IEGLContextFactory factory)
		{
			CheckRenderThreadState();
			mEGLContextFactory = factory;
		}

		public void SetEGLWindowSurfaceFactory(IEGLWindowSurfaceFactory factory)
		{
			CheckRenderThreadState();
			mEGLWindowSurfaceFactory = factory;
		}

		public void SetEGLConfigChooser(IEGLConfigChooser configChooser)
		{
			CheckRenderThreadState();
			mEGLConfigChooser = configChooser;
		}

		public void SetEGLConfigChooser(bool needDepth)
		{
			SetEGLConfigChooser(new SimpleEGLConfigChooser(this, needDepth));
		}

		public void SetEGLConfigChooser(int redSize, int greenSize, int blueSize, int alphaSize, int depthSize, int stencilSize)
		{
			SetEGLConfigChooser(new ComponentSizeChooser(this, redSize, greenSize, blueSize, alphaSize, depthSize, stencilSize));
		}

		public void SetEGLContextClientVersion(int version)
		{
			CheckRenderThreadState();
			mEGLContextClientVersion = version;
		}

		public Rendermode RenderMode
		{
			get { return mGLThread.GetRenderMode(); }
			set { mGLThread.SetRenderMode(value); }
		}

		public void RequestRender()
		{
			mGLThread.RequestRender();
		}

		public void OnSurfaceTextureUpdated(SurfaceTexture surface)
		{
			//mGLThread.RequestRender();
		}

		public void OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
		{
			mGLThread.OnSurfaceCreated();
		}

		public bool OnSurfaceTextureDestroyed(SurfaceTexture surface)
		{
			// Surface will be destroyed when we return
			mGLThread.OnSurfaceDestroyed();
			return true;
		}

		public void OnSurfaceTextureSizeChanged(SurfaceTexture surface, int w, int h)
		{
			mGLThread.OnWindowResize(w, h);
		}

		public void OnPause()
		{
			mGLThread.OnPause();
		}

		public void OnResume()
		{
			mGLThread.OnResume();
		}

		public void QueueEvent(Action r)
		{
			QueueEvent(new Java.Lang.Runnable(r));
		}

		public void QueueEvent(Java.Lang.IRunnable r)
		{
			mGLThread.QueueEvent(r);
		}

		protected override void OnAttachedToWindow()
		{
			base.OnAttachedToWindow();

			LogDebug($" OnAttachedToWindow");

			if (mDetached && (mRenderer != null))
			{
				Rendermode renderMode = Rendermode.Continuously;
				if (mGLThread != null)
				{
					renderMode = mGLThread.GetRenderMode();
				}
				mGLThread = new GLThread(thisWeakRef);
				if (renderMode != Rendermode.Continuously)
				{
					mGLThread.SetRenderMode(renderMode);
				}
				mGLThread.Start();
			}
			mDetached = false;
		}

		protected override void OnDetachedFromWindow()
		{
			LogDebug($" OnDetachedFromWindow reattach={mDetached}");

			if (mGLThread != null)
			{
				mGLThread.RequestExitAndWait();
			}
			mDetached = true;
			base.OnDetachedFromWindow();
		}

		private void CheckRenderThreadState()
		{
			if (mGLThread != null)
			{
				throw new Exception("setRenderer has already been called for this instance.");
			}
		}

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

		public interface IGLWrapper
		{
			IGL Wrap(IGL gl);
		}

		public interface IEGLConfigChooser
		{
			EGLConfig ChooseConfig(IEGL10 egl, EGLDisplay display);
		}

		public interface IEGLContextFactory
		{
			EGLContext CreateContext(IEGL10 egl, EGLDisplay display, EGLConfig eglConfig);

			void DestroyContext(IEGL10 egl, EGLDisplay display, EGLContext context);
		}

		public interface IEGLWindowSurfaceFactory
		{
			EGLSurface CreateWindowSurface(IEGL10 egl, EGLDisplay display, EGLConfig config, Java.Lang.Object nativeWindow);

			void DestroySurface(IEGL10 egl, EGLDisplay display, EGLSurface surface);
		}

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
				int[] attrib_list = {
					EglHelper.EGL_CONTEXT_CLIENT_VERSION, textureView.mEGLContextClientVersion,
					EGL10.EglNone
				};

				return egl.EglCreateContext(display, config, EGL10.EglNoContext, textureView.mEGLContextClientVersion != 0 ? attrib_list : null);
			}

			public void DestroyContext(IEGL10 egl, EGLDisplay display, EGLContext context)
			{
				LogDebug($"[DefaultContextFactory] DestroyContext tid={Thread.CurrentThread.ManagedThreadId} display={display} context={context}");

				if (!egl.EglDestroyContext(display, context))
				{
					int error = egl.EglGetError();
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
				int[] num_config = new int[1];
				if (!egl.EglChooseConfig(display, configSpec, null, 0, num_config))
				{
					throw new Exception("eglChooseConfig failed");
				}

				int numConfigs = num_config[0];

				if (numConfigs <= 0)
				{
					throw new Exception("No configs match configSpec");
				}

				EGLConfig[] configs = new EGLConfig[numConfigs];
				if (!egl.EglChooseConfig(display, configSpec, configs, numConfigs, num_config))
				{
					throw new Exception("eglChooseConfig#2 failed");
				}
				EGLConfig config = ChooseConfig(egl, display, configs);
				if (config == null)
				{
					throw new Exception("No config chosen");
				}
				return config;
			}

			public abstract EGLConfig ChooseConfig(IEGL10 egl, EGLDisplay display, EGLConfig[] configs);

			private int[] FilterConfigSpec(int[] spec)
			{
				if (textureView.mEGLContextClientVersion != 2)
				{
					return spec;
				}

				// We know none of the subclasses define EGL_RENDERABLE_TYPE.
				// And we know the configSpec is well formed.
				int len = spec.Length;
				int[] newConfigSpec = new int[len + 2];
				System.Array.Copy(spec, 0, newConfigSpec, 0, len - 1);
				newConfigSpec[len - 1] = EGL10.EglRenderableType;
				newConfigSpec[len] = EglHelper.EGL_OPENGL_ES2_BIT;
				newConfigSpec[len + 1] = EGL10.EglNone;
				return newConfigSpec;
			}
		}

		private class ComponentSizeChooser : BaseConfigChooser
		{
			private int[] mValue;
			private int mRedSize;
			private int mGreenSize;
			private int mBlueSize;
			private int mAlphaSize;
			private int mDepthSize;
			private int mStencilSize;

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
				mValue = new int[1];
				mRedSize = redSize;
				mGreenSize = greenSize;
				mBlueSize = blueSize;
				mAlphaSize = alphaSize;
				mDepthSize = depthSize;
				mStencilSize = stencilSize;
			}

			public override EGLConfig ChooseConfig(IEGL10 egl, EGLDisplay display, EGLConfig[] configs)
			{
				foreach (EGLConfig config in configs)
				{
					int d = FindConfigAttrib(egl, display, config, EGL10.EglDepthSize, 0);
					int s = FindConfigAttrib(egl, display, config, EGL10.EglStencilSize, 0);
					if ((d >= mDepthSize) && (s >= mStencilSize))
					{
						int r = FindConfigAttrib(egl, display, config, EGL10.EglRedSize, 0);
						int g = FindConfigAttrib(egl, display, config, EGL10.EglGreenSize, 0);
						int b = FindConfigAttrib(egl, display, config, EGL10.EglBlueSize, 0);
						int a = FindConfigAttrib(egl, display, config, EGL10.EglAlphaSize, 0);
						if ((r == mRedSize) && (g == mGreenSize) && (b == mBlueSize) && (a == mAlphaSize))
						{
							return config;
						}
					}
				}
				return null;
			}

			private int FindConfigAttrib(IEGL10 egl, EGLDisplay display, EGLConfig config, int attribute, int defaultValue)
			{
				if (egl.EglGetConfigAttrib(display, config, attribute, mValue))
				{
					return mValue[0];
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
			public const string kMSM7K_RENDERER_PREFIX = "Q3Dimension MSM7500 ";
			public const int kGLES_20 = 0x20000;

			private WeakReference<GLTextureView> mGLTextureViewWeakRef;
			private IEGL10 mEgl;
			private EGLDisplay mEglDisplay;
			private EGLSurface mEglSurface;
			private EGLContext mEglContext;
			private EGLConfig mEglConfig;

			public EglHelper(WeakReference<GLTextureView> glTextureViewWeakRef)
			{
				mGLTextureViewWeakRef = glTextureViewWeakRef;
			}

			public EGLConfig EglConfig => mEglConfig;

			private int CurrentThreadId => Thread.CurrentThread.ManagedThreadId;

			public void Start()
			{
				LogDebug($"[GLThread {CurrentThreadId}][EglHelper] Start");

				// Get an EGL instance
				mEgl = EGLContext.EGL.JavaCast<IEGL10>();

				// Get to the default display.
				mEglDisplay = mEgl.EglGetDisplay(EGL10.EglDefaultDisplay);

				if (mEglDisplay == EGL10.EglNoDisplay)
				{
					throw new Exception("eglGetDisplay failed");
				}

				// We can now initialize EGL for that display
				int[] version = new int[2];
				if (!mEgl.EglInitialize(mEglDisplay, version))
				{
					throw new Exception("eglInitialize failed");
				}
				if (!mGLTextureViewWeakRef.TryGetTarget(out GLTextureView view))
				{
					mEglConfig = null;
					mEglContext = null;
				}
				else
				{
					mEglConfig = view.mEGLConfigChooser.ChooseConfig(mEgl, mEglDisplay);
					// Create an EGL context. We want to do this as rarely as we can, because an
					// EGL context is a somewhat heavy object.
					mEglContext = view.mEGLContextFactory.CreateContext(mEgl, mEglDisplay, mEglConfig);
				}
				if (mEglContext == null || mEglContext == EGL10.EglNoContext)
				{
					mEglContext = null;

					int error = mEgl.EglGetError();
					LogError($"[GLThread {CurrentThreadId}][EglHelper] createContext failed: {error}");
					throw new Exception($"createContext failed: {error}");
				}

				LogDebug($"[GLThread {CurrentThreadId}][EglHelper] createContext {mEglContext}");

				mEglSurface = null;
			}

			public bool CreateSurface()
			{
				LogDebug($"[GLThread {CurrentThreadId}][EglHelper] CreateSurface");

				if (mEgl == null)
				{
					throw new Exception("egl not initialized");
				}
				if (mEglDisplay == null)
				{
					throw new Exception("eglDisplay not initialized");
				}
				if (mEglConfig == null)
				{
					throw new Exception("mEglConfig not initialized");
				}
				// The window size has changed, so we need to create a new surface.
				DestroySurfaceImp();

				// Create an EGL surface we can render into.
				if (mGLTextureViewWeakRef.TryGetTarget(out GLTextureView view))
				{
					mEglSurface = view.mEGLWindowSurfaceFactory.CreateWindowSurface(mEgl, mEglDisplay, mEglConfig, view.SurfaceTexture);
				}
				else
				{
					mEglSurface = null;
				}
				if (mEglSurface == null || mEglSurface == EGL10.EglNoSurface)
				{
					int error = mEgl.EglGetError();
					if (error == EGL10.EglBadNativeWindow)
					{
						LogError($"[GLThread {CurrentThreadId}][EglHelper] createWindowSurface returned EGL_BAD_NATIVE_WINDOW");
					}
					return false;
				}
				// Before we can issue IGL commands, we need to make sure the context is 
				// current and bound to a surface.
				if (!mEgl.EglMakeCurrent(mEglDisplay, mEglSurface, mEglSurface, mEglContext))
				{
					// Could not make the context current, probably because the underlying
					// TextureView surface has been destroyed.
					LogError($"[GLThread {CurrentThreadId}][EglHelper] eglMakeCurrent failed: {mEgl.EglGetError()}");
					return false;
				}
				return true;
			}

			public IGL CreateGL()
			{
				IGL gl = mEglContext.GL;
				if (mGLTextureViewWeakRef.TryGetTarget(out GLTextureView view))
				{
					if (view.mGLWrapper != null)
					{
						gl = view.mGLWrapper.Wrap(gl);
					}

					if (view.DebugFlags.HasFlag(DebugFlags.CheckGlError | DebugFlags.LogGlCalls))
					{
						int configFlags = 0;
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
				if (!mEgl.EglSwapBuffers(mEglDisplay, mEglSurface))
				{
					return mEgl.EglGetError();
				}
				return EGL10.EglSuccess;
			}

			public void DestroySurface()
			{
				LogDebug($"[GLThread {CurrentThreadId}][EglHelper] DestroySurface");

				if (mGLTextureViewWeakRef.TryGetTarget(out GLTextureView view))
				{
					view.mRenderer.OnSurfaceDestroyed();
				}
				DestroySurfaceImp();
			}

			private void DestroySurfaceImp()
			{
				if (mEglSurface != null && mEglSurface != EGL10.EglNoSurface)
				{
					mEgl.EglMakeCurrent(mEglDisplay, EGL10.EglNoSurface, EGL10.EglNoSurface, EGL10.EglNoContext);
					if (mGLTextureViewWeakRef.TryGetTarget(out GLTextureView view))
					{
						view.mEGLWindowSurfaceFactory.DestroySurface(mEgl, mEglDisplay, mEglSurface);
					}
					mEglSurface = null;
				}
			}

			public void Finish()
			{
				LogDebug($"[GLThread {CurrentThreadId}][EglHelper] Finish");

				if (mEglContext != null)
				{
					if (mGLTextureViewWeakRef.TryGetTarget(out GLTextureView view))
					{
						view.mEGLContextFactory.DestroyContext(mEgl, mEglDisplay, mEglContext);
					}
					mEglContext = null;
				}
				if (mEglDisplay != null)
				{
					mEgl.EglTerminate(mEglDisplay);
					mEglDisplay = null;
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
			private bool mWaitingForSurface;
			private bool mHaveEglContext;
			private bool mHaveEglSurface;
			private bool mFinishedCreatingEglSurface;
			private bool mShouldReleaseEglContext;
			private int mWidth;
			private int mHeight;
			private Rendermode mRenderMode;
			private bool mRequestRender;
			private bool mRenderComplete;
			private Queue<Java.Lang.IRunnable> mEventQueue = new Queue<Java.Lang.IRunnable>();
			private bool mSizeChanged = true;
			// End of member variables protected by the sGLThreadManager monitor.

			public GLThread(WeakReference<GLTextureView> glTextureViewWeakRef)
			{
				threadManager = new GLThreadManager();

				mWidth = 0;
				mHeight = 0;
				mRequestRender = true;
				mRenderMode = Rendermode.Continuously;
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
				catch (Exception e)
				{
					// fall thru and exit normally
					throw e;
				}
				finally
				{
					threadManager.ThreadExiting(this);
				}
			}

			private void StopEglSurfaceLocked()
			{
				if (mHaveEglSurface)
				{
					mHaveEglSurface = false;
					eglHelper.DestroySurface();
				}
			}

			private void StopEglContextLocked()
			{
				if (mHaveEglContext)
				{
					eglHelper.Finish();
					mHaveEglContext = false;
					threadManager.ReleaseEglContextLocked(this);
				}
			}

			private void GuardedRun()
			{
				eglHelper = new EglHelper(textureViewWeakRef);
				mHaveEglContext = false;
				mHaveEglSurface = false;
				try
				{
					IGL10 gl = null;
					bool createEglContext = false;
					bool createEglSurface = false;
					bool createGlInterface = false;
					bool lostEglContext = false;
					bool sizeChanged = false;
					bool wantRenderNotification = false;
					bool doRenderNotification = false;
					bool askedToReleaseEglContext = false;
					int w = 0;
					int h = 0;
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

								if (mEventQueue.Count > 0)
								{
									ev = mEventQueue.Dequeue();
									break;
								}

								// Update the pause state.
								bool pausing = false;
								if (paused != requestPaused)
								{
									pausing = requestPaused;
									paused = requestPaused;
									Monitor.PulseAll(threadManager);

									LogDebug($"[GLThread {Id}] paused is now {paused}");
								}

								// Do we need to give up the EGL context?
								if (mShouldReleaseEglContext)
								{
									LogDebug($"[GLThread {Id}] Releasing EGL context because asked to");

									StopEglSurfaceLocked();
									StopEglContextLocked();
									mShouldReleaseEglContext = false;
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
								if (pausing && mHaveEglSurface)
								{
									LogDebug($"[GLThread {Id}] Releasing EGL surface because paused");

									StopEglSurfaceLocked();
								}

								// When pausing, optionally release the EGL Context:
								if (pausing && mHaveEglContext)
								{
									textureViewWeakRef.TryGetTarget(out GLTextureView view);
									bool preserveEglContextOnPause = view == null ? false : view.PreserveEGLContextOnPause;
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
								if ((!hasSurface) && (!mWaitingForSurface))
								{
									LogDebug($"[GLThread {Id}] Noticed TextureView surface lost");

									if (mHaveEglSurface)
									{
										StopEglSurfaceLocked();
									}
									mWaitingForSurface = true;
									surfaceIsBad = false;
									Monitor.PulseAll(threadManager);
								}

								// Have we acquired the surface view surface?
								if (hasSurface && mWaitingForSurface)
								{
									LogDebug($"[GLThread {Id}] Noticed TextureView surface acquired");

									mWaitingForSurface = false;
									Monitor.PulseAll(threadManager);
								}

								if (doRenderNotification)
								{
									LogDebug($"[GLThread {Id}] Sending render notification");

									wantRenderNotification = false;
									doRenderNotification = false;
									mRenderComplete = true;
									Monitor.PulseAll(threadManager);
								}

								// Ready to draw?
								if (IsReadyToDraw())
								{
									// If we don't have an EGL context, try to acquire one.
									if (!mHaveEglContext)
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
											catch (Exception t)
											{
												threadManager.ReleaseEglContextLocked(this);
												throw t;
											}
											mHaveEglContext = true;
											createEglContext = true;

											Monitor.PulseAll(threadManager);
										}
									}

									if (mHaveEglContext && !mHaveEglSurface)
									{
										mHaveEglSurface = true;
										createEglSurface = true;
										createGlInterface = true;
										sizeChanged = true;
									}

									if (mHaveEglSurface)
									{
										if (mSizeChanged)
										{
											sizeChanged = true;
											w = mWidth;
											h = mHeight;
											wantRenderNotification = true;

											LogDebug($"[GLThread {Id}] Noticing that we want render notification");

											// Destroy and recreate the EGL surface.
											createEglSurface = true;
											mSizeChanged = false;
										}
										mRequestRender = false;
										Monitor.PulseAll(threadManager);
										break;
									}
								}

								LogDebug($"[GLThread {Id}] Waiting mHaveEglContext={mHaveEglContext} mHaveEglSurface={mHaveEglSurface} mFinishedCreatingEglSurface={mFinishedCreatingEglSurface} paused={paused} hasSurface={hasSurface} surfaceIsBad={surfaceIsBad} mWaitingForSurface={mWaitingForSurface} mWidth={mWidth} mHeight={mHeight} mRequestRender={mRequestRender} mRenderMode={mRenderMode}");

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
									mFinishedCreatingEglSurface = true;
									Monitor.PulseAll(threadManager);
								}
							}
							else
							{
								lock (threadManager)
								{
									mFinishedCreatingEglSurface = true;
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
								view.mRenderer.OnSurfaceCreated(gl, eglHelper.EglConfig);
							}
							createEglContext = false;
						}

						if (sizeChanged)
						{
							LogDebug($"[GLThread {Id}] OnSurfaceChanged({w}, {h})");

							if (textureViewWeakRef.TryGetTarget(out GLTextureView view))
							{
								view.mRenderer.OnSurfaceChanged(gl, w, h);
							}
							sizeChanged = false;
						}

						{
							LogDebug($"[GLThread {Id}] OnDrawFrame");

							if (textureViewWeakRef.TryGetTarget(out GLTextureView view))
							{
								view.mRenderer.OnDrawFrame(gl);
							}
						}

						int swapError = eglHelper.Swap();
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
				return mHaveEglContext && mHaveEglSurface && IsReadyToDraw();
			}

			private bool IsReadyToDraw()
			{
				return (!paused) && hasSurface && (!surfaceIsBad) && (mWidth > 0) && (mHeight > 0) && (mRequestRender || (mRenderMode == Rendermode.Continuously));
			}

			public void SetRenderMode(Rendermode renderMode)
			{
				lock (threadManager)
				{
					mRenderMode = renderMode;
					Monitor.PulseAll(threadManager);
				}
			}

			public Rendermode GetRenderMode()
			{
				lock (threadManager)
				{
					return mRenderMode;
				}
			}

			public void RequestRender()
			{
				lock (threadManager)
				{
					mRequestRender = true;
					Monitor.PulseAll(threadManager);
				}
			}

			public void OnSurfaceCreated()
			{
				lock (threadManager)
				{
					LogDebug($"[GLThread {Id}] OnSurfaceCreated");

					hasSurface = true;
					mFinishedCreatingEglSurface = false;
					Monitor.PulseAll(threadManager);
					while (mWaitingForSurface && !mFinishedCreatingEglSurface && !exited)
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
					while ((!mWaitingForSurface) && (!exited))
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
					mRequestRender = true;
					mRenderComplete = false;
					Monitor.PulseAll(threadManager);
					while ((!exited) && paused && (!mRenderComplete))
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
					mWidth = w;
					mHeight = h;
					mSizeChanged = true;
					mRequestRender = true;
					mRenderComplete = false;
					Monitor.PulseAll(threadManager);

					// Wait for thread to react to resize and render a frame
					while (!exited && !paused && !mRenderComplete && IsAbleToDraw())
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
				mShouldReleaseEglContext = true;
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
					mEventQueue.Enqueue(r);
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
				for (int i = 0; i < count; i++)
				{
					char c = buf[offset + i];
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
						string renderer = gl.GlGetString(GL10.GlRenderer);
						if (glesVersion < EglHelper.kGLES_20)
						{
							multipleGLESContextsAllowed = !renderer.StartsWith(EglHelper.kMSM7K_RENDERER_PREFIX);
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
					ActivityManager activityManager = ActivityManager.FromContext(Application.Context);
					ConfigurationInfo configInfo = activityManager.DeviceConfigurationInfo;
					if (configInfo.ReqGlEsVersion != ConfigurationInfo.GlEsVersionUndefined)
					{
						glesVersion = configInfo.ReqGlEsVersion;
					}
					else
					{
						glesVersion = 1 << 16; // Lack of property means OpenGL ES version 1
					}
					if (glesVersion >= EglHelper.kGLES_20)
					{
						multipleGLESContextsAllowed = true;
					}
					glesVersionCheckComplete = true;
				}
			}
		}
	}
}
