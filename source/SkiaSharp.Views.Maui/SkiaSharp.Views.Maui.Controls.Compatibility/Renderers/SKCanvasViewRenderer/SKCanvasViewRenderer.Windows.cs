using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;

using SKFormsView = SkiaSharp.Views.Maui.Controls.SKCanvasView;
using SKNativeView = SkiaSharp.Views.Windows.SKXamlCanvas;

namespace SkiaSharp.Views.Maui.Controls.Compatibility
{
	public class SKCanvasViewRenderer : SKCanvasViewRendererBase<SKFormsView, SKNativeView>
	{
		protected override SKNativeView CreateNativeControl() =>
			GetType() == typeof(SKCanvasViewRenderer)
				? new SKNativeView()
				: base.CreateNativeControl();
	}
}
