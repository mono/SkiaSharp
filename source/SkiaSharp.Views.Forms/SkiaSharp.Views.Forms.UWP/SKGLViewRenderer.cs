using Xamarin.Forms.Platform.UWP;

using SKFormsView = SkiaSharp.Views.Forms.SKGLView;
using SKNativeView = SkiaSharp.Views.UWP.SKSwapChainPanel;

[assembly: ExportRenderer(typeof(SKFormsView), typeof(SkiaSharp.Views.Forms.SKGLViewRenderer))]

namespace SkiaSharp.Views.Forms
{
	public class SKGLViewRenderer : SKGLViewRendererBase<SKFormsView, SKNativeView>
	{
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
