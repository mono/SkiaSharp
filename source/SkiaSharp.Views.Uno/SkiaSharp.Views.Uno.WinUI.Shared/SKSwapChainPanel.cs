using System;
using Uno;
using Windows.ApplicationModel;
using Windows.Graphics.Display;
using Windows.UI.Core;
#if WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#endif

#if WINDOWS || WINUI
namespace SkiaSharp.Views.Windows
#else
namespace SkiaSharp.Views.UWP
#endif
{
	/// <summary>A XAML control that uses hardware-accelerated rendering via ANGLE to draw using SkiaSharp.</summary>
	/// <remarks>This control uses an OpenGL ES context via ANGLE to provide GPU-accelerated SkiaSharp drawing. It inherits from <see cref="T:SkiaSharp.Views.Windows.AngleSwapChainPanel" /> and provides SkiaSharp-specific rendering functionality.</remarks>
	public partial class SKSwapChainPanel : FrameworkElement
	{
		/// <summary>Gets or sets a value indicating whether unsupported rendering operations raise an exception.</summary>
		/// <value><see langword="true" /> to raise an exception for unsupported operations; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public static bool RaiseOnUnsupported { get; set; } = true;

		private static readonly DependencyProperty ProxyVisibilityProperty =
			DependencyProperty.Register(
				"ProxyVisibility",
				typeof(Visibility),
				typeof(SKSwapChainPanel),
				new PropertyMetadata(Visibility.Visible, OnVisibilityChanged));

		private static bool designMode = DesignMode.DesignModeEnabled;

		private bool isVisible = true;
		private bool enableRenderLoop = false;

		// workaround for https://github.com/mono/SkiaSharp/issues/1118
		private int loadUnloadCounter = 0;

		private void Initialize()
		{
			if (designMode)
				return;

			var display = DisplayInformation.GetForCurrentView();
			OnDpiChanged(display);

			Loaded += OnLoaded;
			Unloaded += OnUnloaded;
			SizeChanged += OnSizeChanged;

			var binding = new Binding
			{
				Path = new PropertyPath(nameof(Visibility)),
				Source = this
			};
			SetBinding(ProxyVisibilityProperty, binding);
		}

		/// <summary>Gets the current canvas size in pixels.</summary>
		/// <value>The size of the drawing canvas in pixels.</value>
		/// <remarks />
		public SKSize CanvasSize => GetCanvasSize();

		/// <summary>Gets the GPU context used for rendering.</summary>
		/// <value>The <see cref="T:SkiaSharp.GRContext" /> used for GPU-accelerated rendering.</value>
		/// <remarks />
		public GRContext GRContext => GetGRContext();

		/// <summary>Gets the scale factor between logical and physical pixels.</summary>
		/// <value>The current display scale factor.</value>
		/// <remarks />
		public double ContentsScale { get; private set; }

		/// <summary>Gets or sets a value indicating whether rendering occurs in the background.</summary>
		/// <value><see langword="true" /> to render in the background; otherwise, <see langword="false" />.</value>
		/// <remarks />
		[NotImplemented]
		public bool DrawInBackground
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		/// <summary>Gets or sets a value indicating whether continuous rendering is enabled.</summary>
		/// <value><see langword="true" /> to render continuously; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool EnableRenderLoop
		{
			get => enableRenderLoop;
			set
			{
				if (enableRenderLoop != value)
				{
					enableRenderLoop = value;
					DoEnableRenderLoop(enableRenderLoop);
				}
			}
		}

		/// <summary>Requests that the control be redrawn.</summary>
		/// <remarks />
		public new void Invalidate()
		{
			_ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, DoInvalidate);
		}

		/// <summary>Occurs when the surface needs to be repainted.</summary>
		/// <remarks>Handle this event to perform drawing operations on the GPU-accelerated surface.</remarks>
		public event EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;

		/// <param name="e">The event arguments containing the surface and render target information.</param>
		/// <summary>Raises the <see cref="E:SkiaSharp.Views.Windows.SKSwapChainPanel.PaintSurface" /> event.</summary>
		/// <remarks>Override this method to perform custom drawing on the surface without subscribing to the <see cref="E:SkiaSharp.Views.Windows.SKSwapChainPanel.PaintSurface" /> event.</remarks>
		protected virtual void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
		{
			// invoke the event
			PaintSurface?.Invoke(this, e);
		}

		private static void OnVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is SKSwapChainPanel canvas && e.NewValue is Visibility visibility)
			{
				canvas.isVisible = visibility == Visibility.Visible;
				canvas.DoUpdateBounds();
				canvas.Invalidate();
			}
		}

		private void OnDpiChanged(DisplayInformation sender, object args = null)
		{
			ContentsScale = sender.LogicalDpi / 96.0f;
			DoUpdateBounds();
			Invalidate();
		}

		private void OnSizeChanged(object sender, SizeChangedEventArgs e)
		{
			DoUpdateBounds();
			Invalidate();
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			loadUnloadCounter++;
			if (loadUnloadCounter != 1)
				return;

			DoLoaded();

			var display = DisplayInformation.GetForCurrentView();
			display.DpiChanged += OnDpiChanged;

			OnDpiChanged(display);
		}

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			loadUnloadCounter--;
			if (loadUnloadCounter != 0)
				return;

			DoUnloaded();

			var display = DisplayInformation.GetForCurrentView();
			display.DpiChanged -= OnDpiChanged;
		}

		partial void DoLoaded();

		partial void DoUnloaded();

		partial void DoUpdateBounds();

		partial void DoEnableRenderLoop(bool enable);
	}
}
