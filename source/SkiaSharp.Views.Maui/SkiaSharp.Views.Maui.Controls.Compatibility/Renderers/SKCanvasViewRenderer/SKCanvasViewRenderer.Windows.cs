using System;

using SKFormsView = SkiaSharp.Views.Maui.Controls.SKCanvasView;
using SKNativeView = SkiaSharp.Views.Windows.SKXamlCanvas;

namespace SkiaSharp.Views.Maui.Controls.Compatibility
{
	[Obsolete("View renderers are obsolete in .NET MAUI. Use the handlers instead.")]
	public class SKCanvasViewRenderer : SKCanvasViewRendererBase<SKFormsView, SKNativeView>
	{
		protected override SKNativeView CreateNativeControl() =>
			GetType() == typeof(SKCanvasViewRenderer)
				? new SKNativeView()
				: base.CreateNativeControl();
	}
}
