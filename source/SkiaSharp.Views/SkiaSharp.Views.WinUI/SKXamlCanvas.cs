using System;
using Windows.ApplicationModel;

#if WINDOWS
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
#else
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
#endif

#if WINDOWS
namespace SkiaSharp.Views.Windows
#else
namespace SkiaSharp.Views.UWP
#endif
{
	/// <summary>
	/// A XAML canvas that can be drawn on using SkiaSharp drawing commands.
	/// </summary>
	public partial class SKXamlCanvas : Canvas
	{
		private const float DpiBase = 96.0f;

		private static readonly DependencyProperty ProxyVisibilityProperty =
			DependencyProperty.Register(
				"ProxyVisibility",
				typeof(Visibility),
				typeof(SKXamlCanvas),
				new PropertyMetadata(Visibility.Visible, OnVisibilityChanged));

		private static bool designMode = DesignMode.DesignModeEnabled;

		private IntPtr pixels;
		private WriteableBitmap bitmap;
		private ImageBrush brush;
		private bool ignorePixelScaling;
		private bool isVisible = true;

		// workaround for https://github.com/mono/SkiaSharp/issues/1118
		private int loadUnloadCounter = 0;

		/// <summary>
		/// Creates a new instance of <see cref="SKXamlCanvas" />.
		/// </summary>
		public SKXamlCanvas()
		{
			if (designMode)
				return;

#if !WINDOWS
			var display = DisplayInformation.GetForCurrentView();
			OnDpiChanged(display);
#endif

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

		/// <summary>
		/// Gets the current canvas size.
		/// </summary>
		/// <remarks>
		/// The canvas size may be different to the view size as a result of the current device's pixel density.
		/// </remarks>
		public SKSize CanvasSize { get; private set; }

		/// <summary>
		/// Gets or sets a value indicating whether the drawing canvas should be resized on high resolution displays.
		/// </summary>
		/// <remarks>
		/// By default, when <see langword="false" />, the canvas is resized to 1 canvas pixel per display pixel. When <see langword="true" />, the canvas is resized to device independent pixels, and then stretched to fill the view. Although performance is improved and all objects are the same size on different display densities, blurring and pixelation may occur.
		/// </remarks>
		public bool IgnorePixelScaling
		{
			get => ignorePixelScaling;
			set
			{
				ignorePixelScaling = value;
				Invalidate();
			}
		}

		/// <summary>
		/// Gets the current DPI for the canvas.
		/// </summary>
		public double Dpi { get; private set; } = 1;

		/// <summary>
		/// Occurs when the surface needs to be redrawn.
		/// </summary>
		/// <remarks>
		/// <para>
		/// There are two ways to draw on this surface: by overriding the
		/// <see cref="SKXamlCanvas.OnPaintSurface(SKPaintSurfaceEventArgs)" />
		/// method, or by attaching a handler to the
		/// <see cref="SKXamlCanvas.PaintSurface" />
		/// event.
		/// </para>
		/// </remarks>
		/// <example>
		/// <code language="csharp">
		/// myView.PaintSurface += (sender, e) => 
		/// {
		///     var surface = e.Surface;
		///     var surfaceWidth = e.Info.Width;
		///     var surfaceHeight = e.Info.Height;
		///     var canvas = surface.Canvas;
		/// 
		///     // draw on the canvas
		/// };
		/// </code>
		/// </example>
		public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;

		protected virtual void OnPaintSurface(SKPaintSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}

		private static void OnVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is SKXamlCanvas canvas && e.NewValue is Visibility visibility)
			{
				canvas.isVisible = visibility == Visibility.Visible;
				canvas.Invalidate();
			}
		}

#if WINDOWS
		private void OnXamlRootChanged(XamlRoot xamlRoot = null, XamlRootChangedEventArgs e = null)
		{
			var root = xamlRoot ?? XamlRoot;
			var newDpi = root?.RasterizationScale ?? 1.0;
			if (newDpi != Dpi)
			{
				Dpi = newDpi;
				UpdateBrushScale();
				Invalidate();
			}
		}
#else
		private void OnDpiChanged(DisplayInformation sender, object args = null)
		{
			Dpi = sender.LogicalDpi / DpiBase;
			Invalidate();
		}
#endif

		private void OnSizeChanged(object sender, SizeChangedEventArgs e)
		{
			Invalidate();
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			loadUnloadCounter++;
			if (loadUnloadCounter != 1)
				return;

#if WINDOWS
			XamlRoot.Changed += OnXamlRootChanged;
			OnXamlRootChanged();
#else
			var display = DisplayInformation.GetForCurrentView();
			display.DpiChanged += OnDpiChanged;

			OnDpiChanged(display);
#endif
		}

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			loadUnloadCounter--;
			if (loadUnloadCounter != 0)
				return;

#if WINDOWS
			if (XamlRoot != null)
			{
				XamlRoot.Changed -= OnXamlRootChanged;
			}
#else
			var display = DisplayInformation.GetForCurrentView();
			display.DpiChanged -= OnDpiChanged;
#endif

			FreeBitmap();
		}

		/// <summary>
		/// Invalidates the entire surface of the control and causes the control to be redrawn.
		/// </summary>
		public void Invalidate()
		{
#if WINDOWS
			DispatcherQueue?.TryEnqueue(DispatcherQueuePriority.Normal, DoInvalidate);
#else
			Dispatcher?.RunAsync(CoreDispatcherPriority.Normal, DoInvalidate);
#endif
		}

		private void DoInvalidate()
		{
			if (designMode)
				return;

			if (!isVisible)
				return;

			var (info, viewSize, dpi) = CreateBitmap();

			if (info.Width <= 0 || info.Height <= 0)
			{
				CanvasSize = SKSize.Empty;
				return;
			}

			// This is here because the property name is confusing and backwards.
			// True actually means to ignore the pixel scaling of the raw pixel
			// size and instead use the view size such that sizes match the XAML
			// elements.
			var matchUI = IgnorePixelScaling;

			var userVisibleSize = matchUI ? viewSize : info.Size;
			CanvasSize = userVisibleSize;

			using (var surface = SKSurface.Create(info, pixels, info.RowBytes))
			{
				if (matchUI)
				{
					var canvas = surface.Canvas;
					canvas.Scale(dpi);
					canvas.Save();
				}

				OnPaintSurface(new SKPaintSurfaceEventArgs(surface, info.WithSize(userVisibleSize), info));
			}
			bitmap.Invalidate();
		}

		private (SKSizeI ViewSize, SKSizeI PixelSize, float Dpi) CreateSize()
		{
			var w = ActualWidth;
			var h = ActualHeight;

			if (!IsPositive(w) || !IsPositive(h))
				return (SKSizeI.Empty, SKSizeI.Empty, 1);

			var dpi = (float)Dpi;
			var viewSize = new SKSizeI((int)w, (int)h);
			var pixelSize = new SKSizeI((int)(w * dpi), (int)(h * dpi));

			return (viewSize, pixelSize, dpi);

			static bool IsPositive(double value)
			{
				return !double.IsNaN(value) && !double.IsInfinity(value) && value > 0;
			}
		}

		private (SKImageInfo Info, SKSizeI PixelSize, float Dpi) CreateBitmap()
		{
			var (viewSize, pixelSize, dpi) = CreateSize();
			var info = new SKImageInfo(pixelSize.Width, pixelSize.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);

			if (bitmap?.PixelWidth != info.Width || bitmap?.PixelHeight != info.Height)
				FreeBitmap();

			if (bitmap == null && info.Width > 0 && info.Height > 0)
			{
				bitmap = new WriteableBitmap(info.Width, info.Height);
				pixels = bitmap.GetPixels();

				brush = new ImageBrush
				{
					ImageSource = bitmap,
					AlignmentX = AlignmentX.Left,
					AlignmentY = AlignmentY.Top,
					Stretch = Stretch.None
				};
				UpdateBrushScale();

				Background = brush;
			}

			return (info, viewSize, dpi);
		}

		private void UpdateBrushScale()
		{
			if (brush == null)
				return;

			var scale = 1.0 / Dpi;

			brush.Transform = new ScaleTransform
			{
				ScaleX = scale,
				ScaleY = scale
			};
		}

		private void FreeBitmap()
		{
			Background = null;
			brush = null;
			bitmap = null;
			pixels = IntPtr.Zero;
		}
	}
}
