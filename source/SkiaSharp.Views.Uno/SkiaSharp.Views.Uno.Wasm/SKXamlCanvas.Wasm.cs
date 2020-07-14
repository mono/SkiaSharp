using System;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Uno.Extensions;
using Uno.Foundation;
using Uno.Logging;
using Windows.UI.Xaml;

namespace SkiaSharp.Views.UWP
{
	public partial class SKXamlCanvas : FrameworkElement
	{
		private IntPtr pixels;
		private int pixelWidth;
		private int pixelHeight;

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

			if (ActualWidth <= 0 || ActualHeight <= 0 || !isVisible)
			{
				if (this.Log().IsEnabled(LogLevel.Trace))
					this.Log().Trace($"Ignore Invalidate. ActualSize: {ActualWidth}x{ActualHeight}, isVisible:{isVisible}");

				return;
			}

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
				if (this.Log().IsEnabled(LogLevel.Trace))
					this.Log().Trace($"Invalidated. BytesSize={info.BytesSize} ColorType={info.ColorType} IsOpaque:{info.IsOpaque} {info.Width}x{info.Height}");

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

				if (this.Log().IsEnabled(LogLevel.Trace))
					this.Log().Trace($"Allocated new buffer. ptr: {ptr}, newSize: {info.Width}x{info.Height}, oldSize: {pixelWidth}x{pixelHeight}");

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
