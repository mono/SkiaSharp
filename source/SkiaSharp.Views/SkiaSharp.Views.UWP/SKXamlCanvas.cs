using System;
using Windows.ApplicationModel;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace SkiaSharp.Views.UWP
{
	public partial class SKXamlCanvas : Canvas
	{
		private static bool designMode = DesignMode.DesignModeEnabled;

		private IntPtr pixels;
		private WriteableBitmap bitmap;
		private double dpi;
		private bool ignorePixelScaling;

		public SKXamlCanvas()
		{
			Initialize();
		}

		private void Initialize()
		{
			if (designMode)
				return;

			SizeChanged += OnSizeChanged;
			Unloaded += OnUnloaded;

			// get the scale from the current display
			var display = DisplayInformation.GetForCurrentView();
			OnDpiChanged(display);
			display.DpiChanged += OnDpiChanged;
		}

		public SKSize CanvasSize => bitmap == null ? SKSize.Empty : new SKSize(bitmap.PixelWidth, bitmap.PixelHeight);

		public bool IgnorePixelScaling
		{
			get { return ignorePixelScaling; }
			set
			{
				ignorePixelScaling = value;
				Invalidate();
			}
		}

		public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;

		protected virtual void OnPaintSurface(SKPaintSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}

		private void OnDpiChanged(DisplayInformation sender, object args = null)
		{
			dpi = sender.LogicalDpi / 96.0f;
			Invalidate();
		}

		private void OnSizeChanged(object sender, SizeChangedEventArgs e)
		{
			Invalidate();
		}

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			FreeBitmap();
		}

		public void Invalidate()
		{
			var action = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, DoInvalidate);
		}

		private void DoInvalidate()
		{
			if (designMode)
				return;

			if (ActualWidth == 0 || ActualHeight == 0 || Visibility != Visibility.Visible)
				return;

			int width, height;
			if (IgnorePixelScaling)
			{
				width = (int)ActualWidth;
				height = (int)ActualHeight;
			}
			else
			{
				width = (int)(ActualWidth * dpi);
				height = (int)(ActualHeight * dpi);
			}

			var info = new SKImageInfo(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
			CreateBitmap(info);
			using (var surface = SKSurface.Create(info, pixels, info.RowBytes))
			{
				OnPaintSurface(new SKPaintSurfaceEventArgs(surface, info));
			}
			bitmap.Invalidate();
		}

		private void CreateBitmap(SKImageInfo info)
		{
			if (bitmap == null || bitmap.PixelWidth != info.Width || bitmap.PixelHeight != info.Height)
			{
				FreeBitmap();

				bitmap = new WriteableBitmap(info.Width, info.Height);
				pixels = bitmap.GetPixels();

				var brush = new ImageBrush
				{
					ImageSource = bitmap,
					AlignmentX = AlignmentX.Left,
					AlignmentY = AlignmentY.Top,
					Stretch = Stretch.None
				};
				if (!IgnorePixelScaling)
				{
					var scale = 1.0 / dpi;
					brush.Transform = new ScaleTransform
					{
						ScaleX = scale,
						ScaleY = scale
					};
				}
				Background = brush;
			}
		}

		private void FreeBitmap()
		{
			Background = null;
			bitmap = null;
			pixels = IntPtr.Zero;
		}
	}
}
