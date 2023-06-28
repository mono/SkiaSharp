using SkiaSharp.Views.Tizen;

using Microsoft.Maui.Controls;
using TForms = Microsoft.Maui.Controls.Compatibility.Forms;
using SKFormsView = SkiaSharp.Views.Maui.Controls.SKGLView;
using SKNativeView = SkiaSharp.Views.Tizen.SKGLSurfaceView;

namespace SkiaSharp.Views.Maui.Controls.Compatibility
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
