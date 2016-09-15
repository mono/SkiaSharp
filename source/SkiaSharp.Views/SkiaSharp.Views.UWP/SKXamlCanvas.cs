using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace SkiaSharp.Views
{
	public partial class SKXamlCanvas : Canvas
	{
		private static bool designMode = DesignMode.DesignModeEnabled;

		private byte[] pixels;
		private GCHandle buff;
		private WriteableBitmap bitmap;

		public SKXamlCanvas()
		{
			Initialize();
		}

		private void Initialize()
		{
			if (designMode)
				return;

			SizeChanged += OnSizeChanged;
			Tapped += OnTapped;
			Unloaded += OnUnloaded;
		}

		public int PixelWidth => (int)ActualWidth;

		public int PixelHeight => (int)ActualHeight;

		public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;

		protected virtual void OnPaintSurface(SKPaintSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}

		protected virtual void OnSizeChanged(object sender, SizeChangedEventArgs e)
		{
			Invalidate();
		}

		protected virtual void OnTapped(object sender, TappedRoutedEventArgs e)
		{
		}

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			FreeBitmap();
		}

		public void Invalidate()
		{
			if (designMode)
				return;

			CreateBitmap();
			var info = new SKImageInfo(PixelWidth, PixelHeight, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
			using (var surface = SKSurface.Create(info, buff.AddrOfPinnedObject(), PixelWidth * 4))
			{
				OnPaintSurface(new SKPaintSurfaceEventArgs(surface, info));
			}

			var stream = bitmap.PixelBuffer.AsStream();
			stream.Seek(0, SeekOrigin.Begin);
			stream.Write(pixels, 0, pixels.Length);

			bitmap.Invalidate();
		}

		private void CreateBitmap()
		{
			if (bitmap == null || bitmap.PixelWidth != PixelWidth || bitmap.PixelHeight != PixelHeight)
			{
				FreeBitmap();

				bitmap = new WriteableBitmap(PixelWidth, PixelHeight);
				pixels = new byte[bitmap.PixelWidth * bitmap.PixelHeight * 4];
				buff = GCHandle.Alloc(pixels, GCHandleType.Pinned);

				Background = new ImageBrush
				{
					ImageSource = bitmap,
					AlignmentX = AlignmentX.Left,
					AlignmentY = AlignmentY.Top,
					Stretch = Stretch.None
				};
			}
		}

		private void FreeBitmap()
		{
			if (bitmap != null)
			{
				bitmap = null;
			}
			if (buff.IsAllocated)
			{
				buff.Free();
				buff = default(GCHandle);
			}
			pixels = null;
		}
	}
}
