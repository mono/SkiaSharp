using Xamarin.Forms;

using TForms = Xamarin.Forms.Forms;

using SKFormsView = SkiaSharp.Views.Forms.SKCanvasView;
using SKNativeView = SkiaSharp.Views.Tizen.SKCanvasView;

[assembly: ExportRenderer(typeof(SKFormsView), typeof(SkiaSharp.Views.Forms.SKCanvasViewRenderer))]

namespace SkiaSharp.Views.Forms
{
	public class SKCanvasViewRenderer : SKCanvasViewRendererBase<SKFormsView, SKNativeView>, IRegisterable
	{
		protected override SKNativeView CreateNativeControl() =>
			GetType() == typeof(SKCanvasViewRenderer)
				? new SKNativeView(TForms.NativeParent)
				: base.CreateNativeControl();
	}
}
