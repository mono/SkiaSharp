#if __IOS__ || __TVOS__
using System;
using System.ComponentModel;
using CoreGraphics;
using UIKit;

namespace SkiaSharp.Views
{
	[DesignTimeVisible(true)]
	public class SKView : UIView
	{
		private SKDrawable drawable;

		// created in code
		public SKView()
		{
			Initialize();
		}

		// created in code
		public SKView(CGRect frame)
		{
			Frame = frame;

			Initialize();
		}

		// created via designer
		public SKView(IntPtr p)
			: base(p)
		{
		}

		// created via designer
		public override void AwakeFromNib()
		{
			Initialize();
		}

		private void Initialize()
		{
			drawable = new SKDrawable();
		}

		public override void Draw(CGRect rect)
		{
			base.Draw(rect);

			var ctx = UIGraphics.GetCurrentContext();

			// create the skia context
			SKImageInfo info;
			var surface = drawable.CreateSurface(Bounds, ContentScaleFactor, out info);

			// draw on the image using SKiaSharp
			Draw(surface, info);

			// draw the surface to the context
			drawable.DrawSurface(ctx, Bounds, info, surface);
		}

		public virtual void Draw(SKSurface surface, SKImageInfo info)
		{
			// empty
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			Layer.SetNeedsDisplay();
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			drawable.Dispose();
		}
	}
}
#endif
