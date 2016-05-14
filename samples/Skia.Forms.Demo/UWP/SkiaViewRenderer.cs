using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer (typeof(Skia.Forms.Demo.SkiaView), typeof(Skia.Forms.Demo.UWP.SkiaViewRenderer))]

namespace Skia.Forms.Demo.UWP
{
	public class SkiaViewRenderer : ViewRenderer<SkiaView, NativeSkiaView>
	{
		public SkiaViewRenderer ()
		{
		}

		protected override void OnElementChanged (ElementChangedEventArgs<SkiaView> e)
		{
			base.OnElementChanged(e);

			if (Control == null && Element != null) {
				SetNativeControl (new NativeSkiaView (Element));
			}
		}
	}
}
