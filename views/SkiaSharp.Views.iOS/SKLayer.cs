using System;
using System.Runtime.InteropServices;
using CoreAnimation;
using CoreGraphics;

namespace SkiaSharp.Views
{
	public class SKLayer : CALayer
	{
		private const int BitsPerByte = 8; // 1 byte = 8 bits
		private const CGBitmapFlags BitmapFlags = CGBitmapFlags.ByteOrder32Big | CGBitmapFlags.PremultipliedLast;

		private IntPtr bitmapData;
		private int lastLength;

		public SKLayer()
		{
			SetNeedsDisplay();
		}

		public ISKLayerDelegate SKDelegate { get; set; }

		public override void DrawInContext(CGContext ctx)
		{
			base.DrawInContext(ctx);

			// get context details
			var bmp = ctx.AsBitmapContext();
			var width = (int)bmp.Width;
			var height = (int)bmp.Height;
			var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);

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

			// draw on the image using SKiaSharp
			using (var surface = SKSurface.Create(info, bitmapData, info.RowBytes))
			{
				DrawInSurface(surface, info);
				SKDelegate?.DrawInSurface(surface, info);

				surface.Canvas.Flush();
			}

			// draw the image onto the context
			using (var dataProvider = new CGDataProvider(bitmapData, lastLength))
			using (var image = new CGImage(width, height, BitsPerByte, info.BytesPerPixel * BitsPerByte, info.RowBytes, bmp.ColorSpace, BitmapFlags, dataProvider, null, false, CGColorRenderingIntent.Default))
			{
				// we need to flip the image as we are mixing CoreGraphics and UIKit functions:
				// https://developer.apple.com/library/ios/documentation/2DDrawing/Conceptual/DrawingPrintingiOS/GraphicsDrawingOverview/GraphicsDrawingOverview.html#//apple_ref/doc/uid/TP40010156-CH14-SW26
				ctx.SaveState();
				ctx.TranslateCTM(0, Frame.Height);
				ctx.ScaleCTM(1, -1);
				// draw the image
				ctx.DrawImage(Bounds, image);
				ctx.RestoreState();
			}
		}

		public virtual void DrawInSurface(SKSurface surface, SKImageInfo info)
		{
		}

		public override void LayoutSublayers()
		{
			base.LayoutSublayers();

			SetNeedsDisplay();
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			// make sure we free the image data
			if (bitmapData != IntPtr.Zero)
			{
				Marshal.FreeCoTaskMem(bitmapData);
				bitmapData = IntPtr.Zero;
			}
		}
	}
}
