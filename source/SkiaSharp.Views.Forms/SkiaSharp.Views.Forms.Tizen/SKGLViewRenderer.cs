using Xamarin.Forms;

using TForms = Xamarin.Forms.Forms;

using SKFormsView = SkiaSharp.Views.Forms.SKGLView;
using SKNativeView = SkiaSharp.Views.Tizen.SKGLSurfaceView;

[assembly: ExportRenderer(typeof(SKFormsView), typeof(SkiaSharp.Views.Forms.SKGLViewRenderer))]

namespace SkiaSharp.Views.Forms
{
	public class SKGLViewRenderer : SKGLViewRendererBase<SKFormsView, SKNativeView>
	{
		protected override SKNativeView CreateNativeControl() =>
			GetType() == typeof(SKGLViewRenderer)
				? new SKNativeView(TForms.NativeParent)
				: base.CreateNativeControl();

		protected override void SetupRenderLoop(bool oneShot)
		{
			if (oneShot)
			{
				Control.Invalidate();
			}

			Control.RenderingMode = Element.HasRenderLoop ? RenderingMode.Continuously : RenderingMode.WhenDirty;
		}
	}
}
