using System;
using System.ComponentModel;
using Android.Content;
using Android.Opengl;
using Android.Util;

#if HAS_UNO_WINUI
namespace SkiaSharp.Views.Windows
#elif HAS_UNO
namespace SkiaSharp.Views.UWP
#else
namespace SkiaSharp.Views.Android
#endif
{
#if HAS_UNO
	internal
#else
	public
#endif
	partial class SKGLTextureView : GLTextureView
	{
		private SKGLTextureViewRenderer renderer;

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete]
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

		public event EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;

		protected virtual void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete("Use PaintSurface instead.")]
		public virtual void SetRenderer(ISKRenderer renderer)
		{
			skRenderer = renderer;
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete("Use SKGLTextureView.PaintSurface instead.")]
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

			protected override void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
			{
				textureView.OnPaintSurface(e);
			}

			[EditorBrowsable (EditorBrowsableState.Never)]
			[Obsolete("Use OnPaintSurface(SKPaintGLSurfaceEventArgs) instead.")]
			protected override void OnDrawFrame(SKSurface surface, GRBackendRenderTargetDesc renderTarget)
			{
				textureView.skRenderer?.OnDrawFrame(surface, renderTarget);
			}
		}
	}
}
