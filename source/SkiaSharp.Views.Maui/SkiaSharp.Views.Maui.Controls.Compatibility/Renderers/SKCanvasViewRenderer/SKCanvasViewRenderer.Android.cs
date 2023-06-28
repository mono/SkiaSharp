using Android.Content;

using SKFormsView = SkiaSharp.Views.Maui.Controls.SKCanvasView;
using SKNativeView = SkiaSharp.Views.Android.SKCanvasView;

namespace SkiaSharp.Views.Maui.Controls.Compatibility
{
	public class SKCanvasViewRenderer : SKCanvasViewRendererBase<SKFormsView, SKNativeView>
	{
		public SKCanvasViewRenderer(Context context)
			: base(context)
		{
		}

		protected override SKNativeView CreateNativeControl() =>
			GetType() == typeof(SKCanvasViewRenderer)
				? new SKNativeView(Context)
				: base.CreateNativeControl();
	}
}
