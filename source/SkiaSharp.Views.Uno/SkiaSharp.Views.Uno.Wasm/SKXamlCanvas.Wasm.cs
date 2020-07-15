using System;
using System.Runtime.InteropServices;
using Uno.Foundation;
using Windows.UI.Xaml;

namespace SkiaSharp.Views.UWP
{
	public partial class SKXamlCanvas : FrameworkElement
	{
		private IntPtr pixels;
		private int pixelWidth;
		private int pixelHeight;

		static SKXamlCanvas()
		{
			const string js = @"
(window || global).SkiaSharp_Views_UWP_SKXamlCanvas = class SKXamlCanvas {
  static invalidateCanvas(pData, canvasId, width, height) {
    var htmlCanvas = document.getElementById(canvasId);
    htmlCanvas.width = width;
    htmlCanvas.height = height;

    var ctx = htmlCanvas.getContext('2d');
    if (!ctx)
      return false;

    var buffer = new Uint8ClampedArray(Module.HEAPU8.buffer, pData, width * height * 4);
    var imageData = new ImageData(buffer, width, height);
    ctx.putImageData(imageData, 0, 0);

    return true;
  }
};";
			WebAssemblyRuntime.InvokeJS(js);
		}

		public SKXamlCanvas()
			: base("canvas")
		{
			Initialize();
		}

		partial void DoUnloaded() =>
			FreeBitmap();

		private SKSize GetCanvasSize() =>
			new SKSize(pixelWidth, pixelHeight);

		private void DoInvalidate()
		{
			if (designMode)
				return;

			if (!isVisible)
				return;

			if (ActualWidth <= 0 || ActualHeight <= 0)
				return;

			int width, height;
			if (IgnorePixelScaling)
			{
				width = (int)ActualWidth;
				height = (int)ActualHeight;
			}
			else
			{
				width = (int)(ActualWidth * Dpi);
				height = (int)(ActualHeight * Dpi);
			}

			var info = new SKImageInfo(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Opaque);
			CreateBitmap(info);

			using (var surface = SKSurface.Create(info, pixels, info.RowBytes))
			{
				OnPaintSurface(new SKPaintSurfaceEventArgs(surface, info));
			}

			WebAssemblyRuntime.InvokeJS($"SkiaSharp_Views_UWP_SKXamlCanvas.invalidateCanvas({pixels}, \"{HtmlId}\", {info.Width}, {pixelHeight});");
		}

		private unsafe void CreateBitmap(SKImageInfo info)
		{
			if (pixels == IntPtr.Zero || pixelWidth != info.Width || pixelHeight != info.Height)
			{
				FreeBitmap();

				var ptr = Marshal.AllocHGlobal(info.BytesSize);

				pixels = ptr;
				pixelWidth = info.Width;
				pixelHeight = info.Height;
			}
		}

		private void FreeBitmap()
		{
			if (pixels != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(pixels);
				pixels = IntPtr.Zero;
			}
		}
	}
}
