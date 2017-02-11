using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
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

		private byte[] pixels;
		private GCHandle buff;
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
			FreeBitmap(true);
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
			using (var surface = SKSurface.Create(info, buff.AddrOfPinnedObject(), info.RowBytes))
			{
				OnPaintSurface(new SKPaintSurfaceEventArgs(surface, info));
			}

			var stream = bitmap.PixelBuffer.AsStream();
			stream.Seek(0, SeekOrigin.Begin);
			stream.Write(pixels, 0, pixels.Length);

			bitmap.Invalidate();
		}

		private void CreateBitmap(SKImageInfo info)
		{
			if (bitmap == null || bitmap.PixelWidth != info.Width || bitmap.PixelHeight != info.Height)
			{
				var recreateArray = pixels == null || pixels.Length != info.BytesSize;

				FreeBitmap(recreateArray);

				if (recreateArray)
				{
					pixels = new byte[info.BytesSize];
					buff = GCHandle.Alloc(pixels, GCHandleType.Pinned);
				}
				bitmap = new WriteableBitmap(info.Width, info.Height);

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

		private void FreeBitmap(bool freeArray)
		{
			if (bitmap != null)
			{
				bitmap = null;
			}
			if (freeArray)
			{
				if (buff.IsAllocated)
				{
					buff.Free();
					buff = default(GCHandle);
				}
				pixels = null;
			}
		}
	}
}
