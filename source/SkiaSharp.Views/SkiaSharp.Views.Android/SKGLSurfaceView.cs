using System;
using Android.Content;
using Android.Opengl;
using Android.Util;

namespace SkiaSharp.Views.Android
{
	public class SKGLSurfaceView : GLSurfaceView
	{
		private SKGLSurfaceViewRenderer renderer;

		[Obsolete]
		private ISKRenderer skRenderer;

		public SKGLSurfaceView(Context context)
			: base(context)
		{
			Initialize();
		}

		public SKGLSurfaceView(Context context, IAttributeSet attrs)
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

		[Obsolete("Use PaintSurface instead.")]
		public virtual void SetRenderer(ISKRenderer renderer)
		{
			skRenderer = renderer;
		}

		[Obsolete("Use SKGLSurfaceView.PaintSurface instead.")]
		public interface ISKRenderer
		{
			void OnDrawFrame(SKSurface surface, GRBackendRenderTargetDesc renderTarget);
		}

		private class InternalRenderer : SKGLSurfaceViewRenderer
		{
			private readonly SKGLSurfaceView surfaceView;

			public InternalRenderer(SKGLSurfaceView surfaceView)
			{
				this.surfaceView = surfaceView;
			}

			protected override void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
			{
				surfaceView.OnPaintSurface(e);
			}

			[Obsolete("Use OnPaintSurface(SKPaintGLSurfaceEventArgs) instead.")]
			protected override void OnDrawFrame(SKSurface surface, GRBackendRenderTargetDesc renderTarget)
			{
				surfaceView.skRenderer?.OnDrawFrame(surface, renderTarget);
			}
		}
	}
}
