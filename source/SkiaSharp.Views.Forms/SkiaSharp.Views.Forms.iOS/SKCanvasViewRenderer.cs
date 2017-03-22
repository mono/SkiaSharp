using Xamarin.Forms;

using SKFormsView = SkiaSharp.Views.Forms.SKCanvasView;
using SKNativeView = SkiaSharp.Views.iOS.SKCanvasView;

[assembly: ExportRenderer(typeof(SKFormsView), typeof(SkiaSharp.Views.Forms.SKCanvasViewRenderer))]

namespace SkiaSharp.Views.Forms
{
	public class SKCanvasViewRenderer : SKCanvasViewRendererBase<SKFormsView, SKNativeView>
	{
		protected override SKNativeView CreateNativeControl()
		{
			var view = base.CreateNativeControl();

			view.UserInteractionEnabled = false;
			// Force the opacity to false for consistency with the other platforms
			view.Opaque = false;

			return view;
		}
	}
}
