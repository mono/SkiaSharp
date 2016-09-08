#if __MAC__
using System;
using System.ComponentModel;
using AppKit;
using CoreGraphics;

namespace SkiaSharp.Views
{
	[DesignTimeVisible(true)]
	public class SKView : NSView
	{
		private SKDrawable drawable;

		// created in code
		public SKView()
		{
			Initialize();
		}

		// created in code
		public SKView(CGRect frame)
			: base(frame)
		{

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

		public virtual void Draw(SKSurface surface, SKImageInfo info)
		{
			// empty
		}

		public override void DrawRect(CGRect dirtyRect)
		{
			base.DrawRect(dirtyRect);

			var ctx = NSGraphicsContext.CurrentContext.CGContext;

			// create the skia context
			SKImageInfo info;
			var surface = drawable.CreateSurface(Bounds, Window.BackingScaleFactor, out info);

			// draw on the image using SKiaSharp
			Draw(surface, info);

			// draw the surface to the context
			drawable.DrawSurface(ctx, Bounds, info, surface);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			drawable.Dispose();
		}
	}
}
#endif
