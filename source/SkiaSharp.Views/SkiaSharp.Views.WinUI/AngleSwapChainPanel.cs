using System;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using SkiaSharp.Views.GlesInterop;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.System.Threading;
using Windows.UI.Core;

#if WINDOWS
namespace SkiaSharp.Views.Windows
#else
namespace SkiaSharp.Views.UWP
#endif
{
	/// <summary>A XAML SwapChainPanel that uses ANGLE to provide an OpenGL ES rendering context.</summary>
	/// <remarks>This is the base class for <see cref="T:SkiaSharp.Views.Windows.SKSwapChainPanel" /> and provides the OpenGL ES context management via ANGLE (Almost Native Graphics Layer Engine).</remarks>
	public class AngleSwapChainPanel : SwapChainPanel
	{
		private static readonly DependencyProperty ProxyVisibilityProperty =
			DependencyProperty.Register(
				"ProxyVisibility",
				typeof(Visibility),
				typeof(AngleSwapChainPanel),
				new PropertyMetadata(Visibility.Visible, OnVisibilityChanged));

		private static readonly bool designMode = DesignMode.DesignModeEnabled;

		private readonly object locker = new object();

		private bool isVisible = true;
		private bool isLoaded = false;

		private GlesContext glesContext;

		private IAsyncAction renderLoopWorker;
		private IAsyncAction renderOnceWorker;

		private bool enableRenderLoop;

		private double lastCompositionScaleX = 0.0;
		private double lastCompositionScaleY = 0.0;

		private bool pendingSizeChange = false;

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Windows.AngleSwapChainPanel" /> class.</summary>
		/// <remarks />
		public AngleSwapChainPanel()
		{
			lastCompositionScaleX = CompositionScaleX;
			lastCompositionScaleY = CompositionScaleY;

			glesContext = null;

			renderLoopWorker = null;
			renderOnceWorker = null;

			DrawInBackground = false;
			EnableRenderLoop = false;

			ContentsScale = CompositionScaleX;

			Loaded += OnLoaded;
			Unloaded += OnUnloaded;

			CompositionScaleChanged += OnCompositionChanged;
			SizeChanged += OnSizeChanged;

			var binding = new Binding
			{
				Path = new PropertyPath(nameof(Visibility)),
				Source = this
			};
			SetBinding(ProxyVisibilityProperty, binding);
		}

		/// <summary>Gets or sets a value indicating whether rendering should occur on a background thread.</summary>
		/// <value><see langword="true" /> if rendering should occur on a background thread; otherwise, <see langword="false" />. The default is <see langword="false" />.</value>
		/// <remarks />
		public bool DrawInBackground { get; set; }

		/// <summary>Gets the scale factor applied to the contents of the panel.</summary>
		/// <value>The current composition scale factor, typically matching the display's DPI scaling.</value>
		/// <remarks />
		public double ContentsScale { get; private set; }

		/// <summary>Gets or sets a value indicating whether a continuous render loop is enabled.</summary>
		/// <value><see langword="true" /> if continuous rendering is enabled; otherwise, <see langword="false" />. The default is <see langword="false" />.</value>
		/// <remarks>When enabled, the panel will continuously render frames. When disabled, call <see cref="M:SkiaSharp.Views.Windows.AngleSwapChainPanel.Invalidate" /> to request a single frame render.</remarks>
		public bool EnableRenderLoop
		{
			get => enableRenderLoop;
			set
			{
				if (enableRenderLoop != value)
				{
					enableRenderLoop = value;
					UpdateRenderLoop(value);
				}
			}
		}

		/// <summary>Requests that the panel render a new frame.</summary>
		/// <remarks>This method has no effect when <see cref="P:SkiaSharp.Views.Windows.AngleSwapChainPanel.EnableRenderLoop" /> is <see langword="true" /> or when the panel is not loaded.</remarks>
		public void Invalidate()
		{
			if (!isLoaded || EnableRenderLoop)
				return;

			if (DrawInBackground)
			{
				lock (locker)
				{
					// if we haven't fired a render thread, start one
					if (renderOnceWorker == null)
					{
						renderOnceWorker = ThreadPool.RunAsync(RenderOnce);
					}
				}
			}
			else
			{
				// draw on this thread, blocking
				RenderFrame();
			}
		}

		/// <summary>Called when a frame should be rendered.</summary>
		/// <param name="rect">The rectangle defining the render area dimensions in pixels.</param>
		/// <remarks>Override this method to perform OpenGL rendering operations.</remarks>
		protected virtual void OnRenderFrame(Rect rect)
		{
		}

		/// <summary>Called when the OpenGL ES context is being destroyed.</summary>
		/// <remarks>Override this method to perform cleanup of any OpenGL resources before the context is destroyed.</remarks>
		protected virtual void OnDestroyingContext()
		{
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			glesContext = new GlesContext();

			isLoaded = true;

			ContentsScale = CompositionScaleX;

			EnsureRenderSurface();
			UpdateRenderLoop(EnableRenderLoop);
			Invalidate();
		}

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			OnDestroyingContext();

			CompositionScaleChanged -= OnCompositionChanged;
			SizeChanged -= OnSizeChanged;

			UpdateRenderLoop(false);
			DestroyRenderSurface();

			isLoaded = false;

			glesContext?.Dispose();
			glesContext = null;
		}

		private static void OnVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is AngleSwapChainPanel panel && e.NewValue is Visibility visibility)
			{
				panel.isVisible = visibility == Visibility.Visible;
				panel.UpdateRenderLoop(panel.isVisible && panel.EnableRenderLoop);
				panel.Invalidate();
			}
		}

		private void OnCompositionChanged(SwapChainPanel sender, object args)
		{
			if (lastCompositionScaleX == CompositionScaleX &&
				lastCompositionScaleY == CompositionScaleY)
			{
				return;
			}

			lastCompositionScaleX = CompositionScaleX;
			lastCompositionScaleY = CompositionScaleY;

			pendingSizeChange = true;

			ContentsScale = CompositionScaleX;

			DestroyRenderSurface();
			EnsureRenderSurface();
			Invalidate();
		}

		private void OnSizeChanged(object sender, SizeChangedEventArgs e)
		{
			pendingSizeChange = true;

			EnsureRenderSurface();
			Invalidate();
		}

		private void EnsureRenderSurface()
		{
			if (isLoaded && glesContext?.HasSurface != true && ActualWidth > 0 && ActualHeight > 0)
			{
				// detach and re-attach the size events as we need to go after the event added by ANGLE
				// otherwise our size will still be the old size

				SizeChanged -= OnSizeChanged;
				CompositionScaleChanged -= OnCompositionChanged;

				glesContext.CreateSurface(this, null, CompositionScaleX);

				SizeChanged += OnSizeChanged;
				CompositionScaleChanged += OnCompositionChanged;
			}
		}

		private void DestroyRenderSurface()
		{
			glesContext?.DestroySurface();
		}

		private void RenderFrame()
		{
			if (designMode || !isLoaded || !isVisible || glesContext?.HasSurface != true)
				return;

			glesContext.MakeCurrent();

			if (pendingSizeChange)
			{
				pendingSizeChange = false;

				if (!EnableRenderLoop)
					glesContext.SwapBuffers();
			}

			glesContext.GetSurfaceDimensions(out var panelWidth, out var panelHeight);
			glesContext.SetViewportSize(panelWidth, panelHeight);

			OnRenderFrame(new Rect(0, 0, panelWidth, panelHeight));

			if (!glesContext.SwapBuffers())
			{
				// The call to eglSwapBuffers might not be successful (i.e. due to Device Lost)
				// If the call fails, then we must reinitialize EGL and the GL resources.
			}
		}

		private void UpdateRenderLoop(bool start)
		{
			if (!isLoaded)
				return;

			lock (locker)
			{
				if (start)
				{
					// if the render loop is not running, start it
					if (renderLoopWorker?.Status != AsyncStatus.Started)
					{
						renderLoopWorker = ThreadPool.RunAsync(RenderLoop);
					}
				}
				else
				{
					// stop the current render loop
					renderLoopWorker?.Cancel();
					renderLoopWorker = null;
				}
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

			lock (locker)
			{
				// we are finished, so null out
				renderOnceWorker = null;
			}
		}

		private void RenderLoop(IAsyncAction action)
		{
			while (action.Status == AsyncStatus.Started)
			{
				if (DrawInBackground)
				{
					// run on this background thread
					RenderFrame();
				}
				else
				{
					// run in the main thread, block this one
					var tcs = new TaskCompletionSource();
					DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
					{
						RenderFrame();
						tcs.SetResult();
					});
					tcs.Task.Wait();
				}
			}
		}
	}
}
