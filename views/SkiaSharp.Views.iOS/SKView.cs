using System;
using System.ComponentModel;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace SkiaSharp.Views
{
	[DesignTimeVisible(true)]
	public class SKView : UIView, ISKLayerDelegate
	{
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
			((SKLayer)Layer).SKDelegate = this;

			// set the scale
			Layer.ContentsScale = UIScreen.MainScreen.Scale;
		}

		public virtual void DrawInSurface(SKSurface surface, SKImageInfo info)
		{
			// empty
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			Layer.SetNeedsDisplay();
		}

		[Export("layerClass")]
		public static Class LayerClass()
		{
			return new Class(typeof(SKLayer));
		}
	}
}
