using SKFormsView = SkiaSharp.Views.Maui.Controls.SKCanvasView;
using SKNativeView = SkiaSharp.Views.iOS.SKCanvasView;

namespace SkiaSharp.Views.Maui.Controls.Compatibility
{
	public class SKCanvasViewRenderer : SKCanvasViewRendererBase<SKFormsView, SKNativeView>
	{
		public SKCanvasViewRenderer()
		{
			SetDisablesUserInteraction(true);
		}

		protected override SKNativeView CreateNativeControl()
		{
			var view = GetType() == typeof(SKCanvasViewRenderer)
				? new SKNativeView()
				: base.CreateNativeControl();

			// Force the opacity to false for consistency with the other platforms
			view.Opaque = false;

			return view;
		}
	}
}
