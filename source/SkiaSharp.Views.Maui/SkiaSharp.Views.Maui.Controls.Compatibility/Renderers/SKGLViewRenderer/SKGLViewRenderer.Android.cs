using Android.Content;
using Android.Opengl;

using SKFormsView = SkiaSharp.Views.Maui.Controls.SKGLView;
using SKNativeView = SkiaSharp.Views.Android.SKGLTextureView;

namespace SkiaSharp.Views.Maui.Controls.Compatibility
{
	public class SKGLViewRenderer : SKGLViewRendererBase<SKFormsView, SKNativeView>
	{
		public SKGLViewRenderer(Context context)
			: base(context)
		{
		}

		protected override void SetupRenderLoop(bool oneShot)
		{
			if (oneShot)
			{
				Control.RequestRender();
			}

			Control.RenderMode = Element.HasRenderLoop
				? Rendermode.Continuously
				: Rendermode.WhenDirty;
		}

		protected override SKNativeView CreateNativeControl()
		{
			var view = GetType() == typeof(SKGLViewRenderer)
				? new SKNativeView(Context)
				: base.CreateNativeControl();

			// Force the opacity to false for consistency with the other platforms
			view.SetOpaque(false);

			return view;
		}
	}
}
