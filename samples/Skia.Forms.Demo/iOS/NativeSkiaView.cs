using System;
using UIKit;
using SkiaSharp;

namespace Skia.Forms.Demo.iOS
{
	public class NativeSkiaView: UIView
	{
		ISkiaViewController skiaView;

		public NativeSkiaView (SkiaView skiaView)
		{
			this.skiaView = skiaView;
		}

		public override void Draw (CoreGraphics.CGRect rect)
		{
			base.Draw (rect);

			// Just not that sharp using the scale pixel scale only
			// Going going 2x Pixel Scale
			var screenScale = (int)UIScreen.MainScreen.Scale * 2;
			var width = (int)Bounds.Width * screenScale;
			var height = (int)Bounds.Height * screenScale;

			IntPtr buff = System.Runtime.InteropServices.Marshal.AllocCoTaskMem (width * height * 4);
			try {

				using (var surface = SKSurface.Create (width, height, SKColorType.Rgba_8888, SKAlphaType.Premul, buff, width * 4)) {
					surface.Canvas.Scale (screenScale, screenScale);
					skiaView.SendDraw (surface.Canvas);
				}

				using (var colorSpace = CoreGraphics.CGColorSpace.CreateDeviceRGB ()) {
					int hack = ((int)CoreGraphics.CGBitmapFlags.ByteOrderDefault) | ((int)CoreGraphics.CGImageAlphaInfo.PremultipliedLast);
					using (var bContext = new CoreGraphics.CGBitmapContext (buff, width, height, 8, width * 4, colorSpace, (CoreGraphics.CGImageAlphaInfo) hack)) {
						using (var image = bContext.ToImage ()) {
							using (var context = UIGraphics.GetCurrentContext ()) {
								context.DrawImage (Bounds, image);
							}
						}
					}
				}
			} finally {
				if (buff != IntPtr.Zero)
					System.Runtime.InteropServices.Marshal.FreeCoTaskMem (buff);
			}
		}

		public override void LayoutSubviews ()
		{
			base.LayoutSubviews ();
			SetNeedsDisplay ();
		}
	}
}

