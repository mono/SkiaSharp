using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer (typeof(Skia.Forms.Demo.SkiaView), typeof(Skia.Forms.Demo.iOS.SkiaViewRenderer))]

namespace Skia.Forms.Demo.iOS
{
	public class SkiaViewRenderer: ViewRenderer<SkiaView, NativeSkiaView>
	{
		public SkiaViewRenderer ()
		{
		}

		protected override void OnElementChanged (ElementChangedEventArgs<SkiaView> e)
		{
			base.OnElementChanged (e);

			if (Control == null)
				SetNativeControl (new NativeSkiaView (Element));
		}
	}
}

