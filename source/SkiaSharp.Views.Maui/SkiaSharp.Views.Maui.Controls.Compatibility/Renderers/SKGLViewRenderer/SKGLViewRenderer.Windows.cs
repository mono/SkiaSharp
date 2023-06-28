#if !WINDOWS
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;

using SKFormsView = SkiaSharp.Views.Maui.Controls.SKGLView;
using SKNativeView = SkiaSharp.Views.Windows.SKSwapChainPanel;

namespace SkiaSharp.Views.Maui.Controls.Compatibility
{
	public class SKGLViewRenderer : SKGLViewRendererBase<SKFormsView, SKNativeView>
	{
		protected override SKNativeView CreateNativeControl() =>
			GetType() == typeof(SKGLViewRenderer)
				? new SKNativeView()
				: base.CreateNativeControl();

		protected override void SetupRenderLoop(bool oneShot)
		{
			if (oneShot)
			{
				Control.Invalidate();
			}

			Control.EnableRenderLoop = Element.HasRenderLoop;
		}
	}
}
#endif
