using System;
using System.Diagnostics;
using System.Threading;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SkiaSharp.Views.GlesInterop;

namespace SkiaSharp.Views.UWP
{
	public class AngleSwapChainPanel : SwapChainPanel
	{
		private const int Yes = 1;
		private const int No = 0;

		private static bool designMode = DesignMode.DesignModeEnabled;

		private IAsyncAction renderLoopWorker;
		private IAsyncAction renderOnceWorker;

		private GlesBackingOption backingOption;
		private GlesMultisampling multisampling;
		private GlesDepthFormat depthFormat;
		private GlesStencilFormat stencilFormat;
		private double contentsScale;

		private int propertiesChanged;
		private int renderbufferWidth;
		private int renderbufferHeight;

		private uint framebuffer;
		private uint renderbuffer;
		private uint depthbuffer;

		public AngleSwapChainPanel()
		{
			Initialize();
		}

		private void Initialize()
		{
			if (designMode)
				return;

			renderLoopWorker = null;
			renderOnceWorker = null;

			backingOption = GlesBackingOption.Detroyed;
			multisampling = GlesMultisampling.None;
			depthFormat = GlesDepthFormat.None;
			stencilFormat = GlesStencilFormat.None;
			contentsScale = 1;

			propertiesChanged = No;
			renderbufferWidth = 0;
			renderbufferHeight = 0;

			Context = null;
			DrawInBackground = false;

			SizeChanged += OnSizeChanged;
			CompositionScaleChanged += OnCompositionChanged;
		}

		public GlesContext Context { get; set; }

		public GlesBackingOption BackingOption
		{
			get { return backingOption; }
			set
			{
				backingOption = value;
				Interlocked.Exchange(ref propertiesChanged, Yes);
				Invalidate();
			}
		}

		public GlesMultisampling Multisampling
		{
			get { return multisampling; }
			set
			{
				multisampling = value;
				Interlocked.Exchange(ref propertiesChanged, Yes);
				Invalidate();
			}
		}

		public GlesDepthFormat DepthFormat
		{
			get { return depthFormat; }
			set
			{
				depthFormat = value;
				Interlocked.Exchange(ref propertiesChanged, Yes);
				Invalidate();
			}
		}

		public GlesStencilFormat StencilFormat
		{
			get { return stencilFormat; }
			set
			{
				stencilFormat = value;
				Interlocked.Exchange(ref propertiesChanged, Yes);
				Invalidate();
			}
		}

		public double ContentsScale
		{
			get { return contentsScale; }
			set
			{
				contentsScale = value;

				CalculateBufferSize();
			}
		}

		public bool DrawInBackground { get; set; }

		public bool EnableRenderLoop
		{
			get { return renderLoopWorker != null; }
			set
			{
				if (value)
				{
					// if the render loop is already running then do not start another thread. 
					if (renderLoopWorker == null || renderLoopWorker.Status != AsyncStatus.Started)
					{
						// run task on a dedicated high priority background thread. 
						renderLoopWorker = ThreadPool.RunAsync(RenderLoop);
					}
				}
				else
				{
					// if the loop is running
					if (renderLoopWorker != null)
					{
						// stop it
						renderLoopWorker.Cancel();
						renderLoopWorker = null;
					}
				}
			}
		}

		public void Invalidate()
		{
			// we don't need to update if we have a loop
			if (!EnableRenderLoop)
			{
				// we are finished drawing the previous background frame
				if (Interlocked.CompareExchange(ref renderOnceWorker, null, null) == null)
				{
					if (DrawInBackground)
					{
						// draw from another thread with normal priority (if nothing is running already)
						var run = Interlocked.CompareExchange(ref renderOnceWorker, ThreadPool.RunAsync(RenderOnce), null);
					}
					else
					{
						// draw on this thread, blocking
						RenderFrame();
					}
				}
			}
		}

		protected virtual void OnRenderFrame(Rect rect)
		{
		}

		private void OnCompositionChanged(SwapChainPanel sender, object args)
		{
			CalculateBufferSize();
		}

		private void OnSizeChanged(object sender, SizeChangedEventArgs e)
		{
			CalculateBufferSize();
		}

		private void CalculateBufferSize()
		{
			renderbufferWidth = (int)(ActualWidth * ContentsScale);
			renderbufferHeight = (int)(ActualHeight * ContentsScale);

			Interlocked.Exchange(ref propertiesChanged, Yes);
			Invalidate();
		}

		private void UpdateBuffers()
		{
			var context = GlesContext.CurrentContext;

			// make a copy as they may change
			var rbWidth = renderbufferWidth;
			var rbHeight = renderbufferHeight;
			var backing = BackingOption;
			var multisampling = Multisampling;
			var depth = DepthFormat;
			var stencil = StencilFormat;

			// make sure we really do need to update
			var needsUpdate = Interlocked.CompareExchange(ref propertiesChanged, No, Yes) == Yes;

			if (!needsUpdate)
				return;

			// destroy the old buffers
			if (renderbuffer != 0)
			{
				Gles.glDeleteRenderbuffers(1, ref renderbuffer);
				renderbuffer = 0;
			}
			if (depthbuffer != 0)
			{
				Gles.glDeleteRenderbuffers(1, ref depthbuffer);
				depthbuffer = 0;
			}
			if (framebuffer != 0)
			{
				Gles.glDeleteFramebuffers(1, ref framebuffer);
				framebuffer = 0;
			}

			// re-create buffers
			Gles.glGenRenderbuffers(1, ref renderbuffer);
			Gles.glBindRenderbuffer(Gles.GL_RENDERBUFFER, renderbuffer);
			Gles.glGenFramebuffers(1, ref framebuffer);
			Gles.glBindFramebuffer(Gles.GL_FRAMEBUFFER, framebuffer);

			// create the surface
			context.SetSurface(this, rbWidth, rbHeight, backing, multisampling, GlesRenderTarget.Renderbuffer);
			Gles.glFramebufferRenderbuffer(Gles.GL_FRAMEBUFFER, Gles.GL_COLOR_ATTACHMENT0, Gles.GL_RENDERBUFFER, renderbuffer);

			// set up the depth and stencil buffers
			if (depth != GlesDepthFormat.None)
			{
				uint fmt = 0;
				if (depth == GlesDepthFormat.Format16)
				{
					if (stencil == GlesStencilFormat.None)
					{
						fmt = Gles.GL_DEPTH_COMPONENT16;
					}
					else
					{
						fmt = Gles.GL_DEPTH24_STENCIL8_OES;
					}
				}
				else if (depth == GlesDepthFormat.Format24)
				{
					fmt = Gles.GL_DEPTH24_STENCIL8_OES;
				}

				Gles.glGenRenderbuffers(1, ref depthbuffer);
				Gles.glBindRenderbuffer(Gles.GL_RENDERBUFFER, depthbuffer);

				if (multisampling == GlesMultisampling.FourTimes)
				{
					Gles.glRenderbufferStorageMultisampleANGLE(Gles.GL_RENDERBUFFER, 4, fmt, rbWidth, rbHeight);
				}
				else
				{
					Gles.glRenderbufferStorage(Gles.GL_RENDERBUFFER, fmt, rbWidth, rbHeight);
				}
				Gles.glFramebufferRenderbuffer(Gles.GL_FRAMEBUFFER, Gles.GL_DEPTH_ATTACHMENT, Gles.GL_RENDERBUFFER, depthbuffer);
			}

			// format for stencils will always be D24_S8 here.
			if (stencil != GlesStencilFormat.None)
			{
				Gles.glFramebufferRenderbuffer(Gles.GL_FRAMEBUFFER, Gles.GL_STENCIL_ATTACHMENT, Gles.GL_RENDERBUFFER, depthbuffer);
			}

			Gles.glBindRenderbuffer(Gles.GL_RENDERBUFFER, renderbuffer);
		}

		private void RenderFrame()
		{
			var ctx = Context;
			if (ctx == null)
				return;

			if (renderbufferWidth == 0 || renderbufferHeight == 0)
				return;

			GlesContext.CurrentContext = ctx;

			UpdateBuffers();

			ctx.SetViewportSize(renderbufferWidth, renderbufferHeight);

			OnRenderFrame(new Rect(0, 0, renderbufferWidth, renderbufferHeight));

			ctx.SwapBuffers(GlesRenderTarget.Renderbuffer, renderbufferWidth, renderbufferHeight);

			GlesContext.CurrentContext = null;
		}

		private void RenderOnce(IAsyncAction action)
		{
			// we loop as a refresh may be needed again
			do
			{
				if (DrawInBackground)
				{
					// run on this background thread
					RenderFrame();
				}
				else
				{
					// run in the main thread, block this one
					Dispatcher.RunAsync(CoreDispatcherPriority.Normal, RenderFrame).AsTask().Wait();
				}
			}
			while (Interlocked.CompareExchange(ref propertiesChanged, Yes, Yes) == Yes);

			// we are finished, so null out
			var run = Interlocked.Exchange(ref renderOnceWorker, null);
		}

		// create a task for rendering that will be run on a background thread. 
		private void RenderLoop(IAsyncAction action)
		{
			while (action.Status == AsyncStatus.Started)
			{
				// we are still drawing the previous frame that wace a once-off
				if (Interlocked.CompareExchange(ref renderOnceWorker, null, null) != null)
				{
					continue;
				}

				if (DrawInBackground)
				{
					// run on this background thread
					RenderFrame();
				}
				else
				{
					// run in the main thread, block this one
					Dispatcher.RunAsync(CoreDispatcherPriority.Normal, RenderFrame).AsTask().Wait();
				}
			}
		}
	}
}
