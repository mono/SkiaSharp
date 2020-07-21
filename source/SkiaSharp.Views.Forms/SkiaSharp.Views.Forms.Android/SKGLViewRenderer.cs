using System;
using System.ComponentModel;
using Android.Content;
using Android.Opengl;
using Xamarin.Forms;

using SKFormsView = SkiaSharp.Views.Forms.SKGLView;
using SKNativeView = SkiaSharp.Views.Android.SKGLTextureView;

[assembly: ExportRenderer(typeof(SKFormsView), typeof(SkiaSharp.Views.Forms.SKGLViewRenderer))]

namespace SkiaSharp.Views.Forms
{
	public class SKGLViewRenderer : SKGLViewRendererBase<SKFormsView, SKNativeView>
	{
		public SKGLViewRenderer(Context context)
			: base(context)
		{
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete("This constructor is obsolete as of version 2.5. Please use SKGLViewRenderer(Context) instead.")]
		public SKGLViewRenderer()
			: base()
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
