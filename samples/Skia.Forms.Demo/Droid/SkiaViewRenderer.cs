using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer (typeof(Skia.Forms.Demo.SkiaView), typeof(Skia.Forms.Demo.Droid.SkiaViewRenderer))]

namespace Skia.Forms.Demo.Droid
{
	public class SkiaViewRenderer : ViewRenderer<SkiaView, NativeSkiaView>
	{
		NativeSkiaView view;

		public SkiaViewRenderer ()
		{
		}

		protected override void OnElementChanged (ElementChangedEventArgs<SkiaView> e)
		{
			base.OnElementChanged (e);

			if (Control == null) {
				view = new NativeSkiaView (Context, Element);
				SetNativeControl (view);
			}
		}
	}
}

