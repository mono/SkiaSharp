using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using SkiaSharp;

namespace Skia.Forms.Demo.UWP
{
    public partial class NativeSkiaView : Canvas
	{
		ISkiaViewController skiaView;

		public NativeSkiaView (SkiaView skiaView)
		{
			this.skiaView = skiaView;
			this.SizeChanged += OnSizeChanged;
			this.Tapped += OnTapped;
		}

		private void UpdateBitmap()
		{
			var resolutionScale = DisplayInformation.GetForCurrentView ().ResolutionScale;
			var screenScale = (float)resolutionScale / 100.0f;
			var width = (int)(ActualWidth * screenScale);
			var height = (int)(ActualHeight * screenScale);

			if (width == 0 || height == 0)
				return;

			IntPtr buff = Marshal.AllocCoTaskMem (width * height * 4);

			try {
				using (var surface = SKSurface.Create (width, height, SKColorType.N_32, SKAlphaType.Premul, buff, width * 4)) {
					var skcanvas = surface.Canvas;
					skcanvas.Scale (screenScale, screenScale);
					using (new SKAutoCanvasRestore (skcanvas, true)) {
						skiaView.SendDraw (skcanvas);
					}
				}

				var pixels = new byte[width * height * 4];
				Marshal.Copy (buff, pixels, 0, pixels.Length);

				var bitmap = new WriteableBitmap (width, height);

				var stream = bitmap.PixelBuffer.AsStream ();
				stream.Seek (0, SeekOrigin.Begin);
				stream.Write (pixels, 0, pixels.Length);

				bitmap.Invalidate ();

				var b = bitmap;
				Background = new ImageBrush {
					ImageSource = b,
					AlignmentX = AlignmentX.Center,
					AlignmentY = AlignmentY.Center,
					Stretch = Stretch.Fill
				};
			} finally {
				if (buff != IntPtr.Zero) {
					Marshal.FreeCoTaskMem (buff);
				}
			}
		}

		private void OnSizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdateBitmap ();
		}

		private void OnTapped(object sender, TappedRoutedEventArgs e)
		{
			skiaView.SendTap();
		}
	}
}
