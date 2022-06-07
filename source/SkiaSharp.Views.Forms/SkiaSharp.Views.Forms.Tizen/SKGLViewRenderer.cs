using SkiaSharp.Views.Tizen;

#if __MAUI__
using Microsoft.Maui.Controls;
using TForms = Microsoft.Maui.Controls.Compatibility.Forms;
using SKFormsView = SkiaSharp.Views.Maui.Controls.SKGLView;
using SKNativeView = SkiaSharp.Views.Tizen.SKGLSurfaceView;
#else
using Xamarin.Forms;

using TForms = Xamarin.Forms.Forms;

using SKFormsView = SkiaSharp.Views.Forms.SKGLView;
using SKNativeView = SkiaSharp.Views.Tizen.SKGLSurfaceView;

[assembly: ExportRenderer(typeof(SKFormsView), typeof(SkiaSharp.Views.Forms.SKGLViewRenderer))]
#endif

#if __MAUI__
namespace SkiaSharp.Views.Maui.Controls.Compatibility
#else
namespace SkiaSharp.Views.Forms
#endif
{
#if __MAUI__ && __TIZEN__
	[System.Obsolete]
#endif
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
