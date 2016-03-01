using System;
using CoreGraphics;
using UIKit;

using SkiaSharp;

namespace Skia.tvOS.Demo
{
	public partial class SkiaView : UIView
	{
		private const int bitmapInfo = ((int)CGBitmapFlags.ByteOrder32Big) | ((int)CGImageAlphaInfo.PremultipliedLast);

		private Action<SKCanvas, int, int> onDrawCallback;

		public SkiaView (IntPtr handle)
			: base (handle)
		{
		}

		public Action<SKCanvas, int, int> OnDrawCallback {
			get {
				return onDrawCallback;
			}
			set {
				onDrawCallback = value;
				SetNeedsDisplayInRect (Bounds);
			}
		}

		public override void Draw (CGRect rect)
		{
			base.Draw (rect);

			var screenScale = UIScreen.MainScreen.Scale;
			var width = (int)(Bounds.Width * screenScale);
			var height = (int)(Bounds.Height * screenScale);

			IntPtr buff = System.Runtime.InteropServices.Marshal.AllocCoTaskMem (width * height * 4);
			try {
				using (var surface = SKSurface.Create (width, height, SKColorType.N_32, SKAlphaType.Premul, buff, width * 4)) {
					var skcanvas = surface.Canvas;
					skcanvas.Scale ((float)screenScale, (float)screenScale);
					using (new SKAutoCanvasRestore (skcanvas, true)) {

						onDrawCallback (skcanvas, (int)Bounds.Width, (int)Bounds.Height);
					}
				}

				using (var colorSpace = CGColorSpace.CreateDeviceRGB ())
				using (var bContext = new CGBitmapContext (buff, width, height, 8, width * 4, colorSpace, (CGImageAlphaInfo)bitmapInfo))
				using (var image = bContext.ToImage ())
				using (var context = UIGraphics.GetCurrentContext ()) {
					// flip the image for CGContext.DrawImage
					context.TranslateCTM (0, Frame.Height);
					context.ScaleCTM (1, -1);
					context.DrawImage (Bounds, image);
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
