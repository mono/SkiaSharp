using System;
using System.Threading;
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
		private GlesContext glesContext;

		private IAsyncAction renderLoopWorker;
		private IAsyncAction renderOnceWorker;

		public AngleSwapChainPanel()
		{
			glesContext = new GlesContext();

			renderLoopWorker = null;
			renderOnceWorker = null;

			DrawInBackground = false;
			EnableRenderLoop = false;

			ContentsScale = CompositionScaleX;

			Loaded += OnLoaded;
			Unloaded += OnUnloaded;

			CompositionScaleChanged += OnCompositionChanged;
		}

		public bool DrawInBackground { get; set; }

		public double ContentsScale { get; private set; }

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

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			ContentsScale = CompositionScaleX;

			CreateRenderSurface();
			EnableRenderLoop = true;
		}

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			EnableRenderLoop = false;
			DestroyRenderSurface();
		}

		private void OnCompositionChanged(SwapChainPanel sender, object args)
		{
			if (ContentsScale != CompositionScaleX)
			{
				ContentsScale = CompositionScaleX;

				DestroyRenderSurface();
				CreateRenderSurface();
			}
		}

		private void CreateRenderSurface()
		{
			if (glesContext != null && !glesContext.HasSurface)
			{
				glesContext.CreateSurface(this, null, (float)ContentsScale);
			}
		}

		private void DestroyRenderSurface()
		{
			glesContext?.DestroySurface();
		}

		private void RenderFrame()
		{
			if (!glesContext.HasSurface)
				return;

			glesContext.MakeCurrent();
			glesContext.GetSurfaceDimensions(out var panelWidth, out var panelHeight);
			glesContext.SetViewportSize(panelWidth, panelHeight);

			OnRenderFrame(new Rect(0, 0, panelWidth, panelHeight));

			if (!glesContext.SwapBuffers())
			{
				// The call to eglSwapBuffers might not be successful (i.e. due to Device Lost)
				// If the call fails, then we must reinitialize EGL and the GL resources.
			}
		}

		private void RenderOnce(IAsyncAction action)
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

			// we are finished, so null out
			var run = Interlocked.Exchange(ref renderOnceWorker, null);
		}

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
