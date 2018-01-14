using Android.Content;
using Android.Opengl;
using Android.Util;

namespace SkiaSharp.Views.Android
{
	public class SKGLTextureView : GLTextureView
	{
		private SKGLTextureViewRenderer renderer;
		private ISKRenderer skRenderer;

		public SKGLTextureView(Context context)
			: base(context)
		{
			Initialize();
		}

		public SKGLTextureView(Context context, IAttributeSet attrs)
			: base(context, attrs)
		{
			Initialize();
		}

		private void Initialize()
		{
			SetEGLContextClientVersion(2);
			SetEGLConfigChooser(8, 8, 8, 8, 0, 8);

			renderer = new InternalRenderer(this);
			SetRenderer(renderer);
		}

		public SKSize CanvasSize => renderer.CanvasSize;

		public GRContext GRContext => renderer.GRContext;

		public virtual void SetRenderer(ISKRenderer renderer)
		{
			skRenderer = renderer;
		}

		public interface ISKRenderer
		{
			void OnDrawFrame(SKSurface surface, GRBackendRenderTargetDesc renderTarget);
		}

		private class InternalRenderer : SKGLTextureViewRenderer
		{
			private readonly SKGLTextureView textureView;

			public InternalRenderer(SKGLTextureView textureView)
			{
				this.textureView = textureView;
			}

			protected override void OnDrawFrame(SKSurface surface, GRBackendRenderTargetDesc renderTarget)
			{
				textureView.skRenderer?.OnDrawFrame(surface, renderTarget);
			}
		}
	}
}
