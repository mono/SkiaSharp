using System;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SkiaSharp;
using SkiaSharp.Views;

namespace SkiaSharp.Views
{
	public class SKSwapChainPanel : SwapChainPanel
	{
		private static bool designMode = DesignMode.DesignModeEnabled;

		private GRContext context;

		private GlesContext glesContext;
		private object mRenderSurfaceCriticalSection = new object();
		private IAsyncAction mRenderLoopWorker;
		private GRBackendRenderTargetDesc renderTarget;

		public SKSwapChainPanel()
		{
			Initialize();
		}

		private void Initialize()
		{
			if (designMode)
				return;

			glesContext = new GlesContext();
			Window.Current.VisibilityChanged += OnVisibilityChanged;

			Loaded += OnLoaded;
			Unloaded += OnUnloaded;
		}

		public event EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;

		protected virtual void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
		{
			// invoke the event
			PaintSurface?.Invoke(this, e);
		}

		protected virtual void InvalidateSurface()
		{
			if (designMode)
				return;

			// we can't draw if we haven't got a surface and context
			if (glesContext?.HasValidSurface != true)
				return;

			// make sure we are drawing on our context
			GlesContext.CurrentContext = glesContext;

			// create the SkiaSharp context
			if (context == null)
			{
				var glInterface = GRGlInterface.CreateNativeAngleInterface();
				context = GRContext.Create(GRBackend.OpenGL, glInterface);

				renderTarget = SKGLDrawable.CreateRenderTarget();
			}

			// set the size, or resize
			int width, height;
			glesContext.GetSurfaceDimensions(out width, out height);
			if (renderTarget.Width != width || renderTarget.Height != height)
			{
				renderTarget.Width = width;
				renderTarget.Height = height;
				// set the viewport, if the surface has changed
				glesContext.SetViewportSize(width, height);
			}

			// create the surface
			using (var surface = SKSurface.Create(context, renderTarget))
			{
				// draw to the SkiaSharp surface
				OnPaintSurface(new SKPaintGLSurfaceEventArgs(surface, renderTarget));

				// flush the canvas
				surface.Canvas.Flush();
			}

			// flush the SkiaSharp context to the GL context
			context.Flush();
		}

		private void OnVisibilityChanged(object sender, VisibilityChangedEventArgs e)
		{
			if (e.Visible && glesContext.HasValidSurface)
			{
				StartRenderLoop();
			}
			else
			{
				StopRenderLoop();
			}
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			// The SwapChainPanel has been created and arranged in the page layout, so EGL can be initialized. 
			CreateRenderSurface();
			StartRenderLoop();
		}

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			StopRenderLoop();
			DestroyRenderSurface();
		}

		private void CreateRenderSurface()
		{
			if (glesContext != null && !glesContext.HasValidSurface)
			{
				// The app can configure the the SwapChainPanel which may boost performance. 
				glesContext.SetSurface(this);
			}
		}

		private void DestroyRenderSurface()
		{
			if (glesContext != null)
			{
				glesContext.SetSurface(null);
			}
		}

		private void RecoverFromLostDevice()
		{
			var recoverOnUiThread = Dispatcher.RunAsync(CoreDispatcherPriority.High, new DispatchedHandler(() =>
			{
				// Stop the render loop, reset OpenGLES, recreate the render surface 
				// and start the render loop again to recover from a lost device. 
				StopRenderLoop();
				lock (mRenderSurfaceCriticalSection)
				{
					DestroyRenderSurface();
					glesContext.Reset();
					CreateRenderSurface();
				}
				StartRenderLoop();
			}));
		}

		private void StartRenderLoop()
		{
			// If the render loop is already running then do not start another thread. 
			if (mRenderLoopWorker != null && mRenderLoopWorker.Status == AsyncStatus.Started)
			{
				return;
			}

			// Run task on a dedicated high priority background thread. 
			mRenderLoopWorker = ThreadPool.RunAsync(RenderLoop, WorkItemPriority.High, WorkItemOptions.TimeSliced);
		}

		// Create a task for rendering that will be run on a background thread. 
		private void RenderLoop(IAsyncAction action)
		{
			lock (mRenderSurfaceCriticalSection)
			{
				while (action.Status == AsyncStatus.Started)
				{
					InvalidateSurface();

					// The call to eglSwapBuffers might not be successful (i.e. due to Device Lost) 
					// If the call fails, then we must reinitialize EGL and the GL resources. 
					if (!glesContext.SwapBuffers())
					{
						// XAML objects like the SwapChainPanel must only be manipulated on the UI thread. 
						RecoverFromLostDevice();
					}
				}
			}
		}

		private void StopRenderLoop()
		{
			if (mRenderLoopWorker != null)
			{
				mRenderLoopWorker.Cancel();
				mRenderLoopWorker = null;
			}
		}
	}
}
