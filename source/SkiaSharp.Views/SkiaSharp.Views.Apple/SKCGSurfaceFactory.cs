using System;
using System.Runtime.InteropServices;
using CoreGraphics;

#if __TVOS__
namespace SkiaSharp.Views.tvOS
#elif __WATCHOS__
namespace SkiaSharp.Views.watchOS
#elif __IOS__
namespace SkiaSharp.Views.iOS
#elif __MACOS__
namespace SkiaSharp.Views.Mac
#endif
{
	internal class SKCGSurfaceFactory : IDisposable
	{
		private const int BitsPerByte = 8; // 1 byte = 8 bits
		private const CGBitmapFlags BitmapFlags = CGBitmapFlags.ByteOrder32Big | CGBitmapFlags.PremultipliedLast;

		private IntPtr bitmapData;
		private int lastLength;

		public SKImageInfo Info { get; private set; }

		public SKSurface CreateSurface(CGRect contentsBounds, nfloat scale, out SKImageInfo info)
		{
			// apply a scale
			contentsBounds.Width *= scale;
			contentsBounds.Height *= scale;

			// get context details
			Info = info = CreateInfo((int)contentsBounds.Width, (int)contentsBounds.Height);

			// if there are no pixels, clean up and return
			if (info.Width == 0 || info.Height == 0 || lastLength == 0)
			{
				Dispose();
				return null;
			}

			// allocate a memory block for the drawing process
			var newLength = info.BytesSize;
			if (lastLength != newLength)
			{
				lastLength = newLength;
				if (bitmapData != IntPtr.Zero)
					bitmapData = Marshal.ReAllocCoTaskMem(bitmapData, newLength);
				else
					bitmapData = Marshal.AllocCoTaskMem(newLength);
			}

			return SKSurface.Create(info, bitmapData, info.RowBytes);
		}

		public void DrawSurface(CGContext ctx, CGRect viewBounds, SKImageInfo info, SKSurface surface)
		{
			if (info.Width == 0 || info.Height == 0 || lastLength == 0)
				return;

			surface.Canvas.Flush();

			// draw the image onto the context
			using (var dataProvider = new CGDataProvider(bitmapData, lastLength))
			using (var colorSpace = CGColorSpace.CreateDeviceRGB())
			using (var image = new CGImage(info.Width, info.Height, BitsPerByte, info.BytesPerPixel * BitsPerByte, info.RowBytes, colorSpace, BitmapFlags, dataProvider, null, false, CGColorRenderingIntent.Default))
			{
#if __IOS__ || __TVOS__ || __WATCHOS__
				// we need to flip the image as we are mixing CoreGraphics and UIKit functions:
				// https://developer.apple.com/library/ios/documentation/2DDrawing/Conceptual/DrawingPrintingiOS/GraphicsDrawingOverview/GraphicsDrawingOverview.html#//apple_ref/doc/uid/TP40010156-CH14-SW26
				ctx.SaveState();
				ctx.TranslateCTM(0, viewBounds.Height);
				ctx.ScaleCTM(1, -1);
				// draw the image
				ctx.DrawImage(viewBounds, image);
				ctx.RestoreState();
#elif __MACOS__
				// draw the image
				ctx.DrawImage(viewBounds, image);
#else
#error Plaform-specific code missing
#endif
			}
		}

		public void Dispose()
		{
			// make sure we free the image data
			if (bitmapData != IntPtr.Zero)
			{
				Marshal.FreeCoTaskMem(bitmapData);
				bitmapData = IntPtr.Zero;
			}
			Info = CreateInfo(0, 0);
		}

		private SKImageInfo CreateInfo(int width, int height) =>
			new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
	}
}
