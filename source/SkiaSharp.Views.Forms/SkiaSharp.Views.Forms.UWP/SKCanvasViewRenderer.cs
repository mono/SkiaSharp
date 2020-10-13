using Xamarin.Forms.Platform.UWP;

using SKFormsView = SkiaSharp.Views.Forms.SKCanvasView;
using SKNativeView = SkiaSharp.Views.UWP.SKXamlCanvas;

[assembly: ExportRenderer(typeof(SKFormsView), typeof(SkiaSharp.Views.Forms.SKCanvasViewRenderer))]

namespace SkiaSharp.Views.Forms
{
	public class SKCanvasViewRenderer : SKCanvasViewRendererBase<SKFormsView, SKNativeView>
	{
		protected override SKNativeView CreateNativeControl() =>
			GetType() == typeof(SKCanvasViewRenderer)
				? new SKNativeView()
				: base.CreateNativeControl();
	}
}
