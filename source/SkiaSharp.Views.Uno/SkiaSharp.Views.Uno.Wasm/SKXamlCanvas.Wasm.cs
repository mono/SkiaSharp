using System;
using System.Runtime.InteropServices;
using Uno.Foundation;
using Uno.UI.Runtime.WebAssembly;
using Windows.UI.Xaml;

namespace SkiaSharp.Views.UWP
{
	[HtmlElement("canvas")]
	public partial class SKXamlCanvas
	{
		private byte[] pixels;
		private GCHandle pixelsHandle;
		private int pixelWidth;
		private int pixelHeight;

		public SKXamlCanvas()
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

			using (var surface = SKSurface.Create(info, pixelsHandle.AddrOfPinnedObject(), info.RowBytes))
			{
				OnPaintSurface(new SKPaintSurfaceEventArgs(surface, info));
			}

			WebAssemblyRuntime.InvokeJS($"SkiaSharp.Views.UWP.SKXamlCanvas.invalidateCanvas({pixelsHandle.AddrOfPinnedObject()}, \"{this.GetHtmlId()}\", {info.Width}, {pixelHeight});");
		}

		private unsafe void CreateBitmap(SKImageInfo info)
		{
			if (pixels == null || pixelWidth != info.Width || pixelHeight != info.Height)
			{
				FreeBitmap();

				pixels = new byte[info.BytesSize];
				pixelsHandle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
				pixelWidth = info.Width;
				pixelHeight = info.Height;
			}
		}

		private void FreeBitmap()
		{
			if (pixels != null)
			{
				pixelsHandle.Free();
				pixels = null;
			}
		}
	}
}
