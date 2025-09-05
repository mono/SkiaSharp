using System;
using System.ComponentModel;
using Android.Content;
using Android.Opengl;
using Android.Util;

namespace SkiaSharp.Views.Android
{
	/// <summary>
	/// An implementation of <see cref="global::Android.Opengl.GLSurfaceView" /> that uses the dedicated surface for displaying a hardware-accelerated <see cref="SKSurface" />.
	/// </summary>
	public class SKGLSurfaceView : GLSurfaceView
	{
		private SKGLSurfaceViewRenderer renderer;

		/// <summary>
		/// Simple constructor to use when creating a <see cref="SKGLSurfaceView" /> from code.
		/// </summary>
		/// <param name="context">The <see cref="global::Android.Content.Context" /> the view is running in, through which it can access the current theme, resources, etc.</param>
		public SKGLSurfaceView(Context context)
			: base(context)
		{
			Initialize();
		}

		/// <summary>
		/// Constructor that is called when inflating a <see cref="SKGLSurfaceView" /> from XML.
		/// </summary>
		/// <param name="context">The <see cref="global::Android.Content.Context" /> the view is running in, through which it can access the current theme, resources, etc.</param>
		/// <param name="attrs">The attributes of the XML tag that is inflating the view.</param>
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

		/// <summary>
		/// Gets the current canvas size.
		/// </summary>
		/// <remarks>
		/// The canvas size may be different to the view size as a result of the current device's pixel density.
		/// </remarks>
		public SKSize CanvasSize => renderer.CanvasSize;

		/// <summary>
		/// Gets the current GPU context.
		/// </summary>
		public GRContext GRContext => renderer.GRContext;

		/// <summary>
		/// Occurs when the surface needs to be redrawn.
		/// </summary>
		/// <remarks>
		/// There are two ways to draw on this surface: by overriding the
		/// <see cref="SkiaSharp.Views.Android.SKGLSurfaceView.OnPaintSurface(SkiaSharp.Views.Android.SKPaintGLSurfaceEventArgs)" />
		/// method, or by attaching a handler to the
		/// <see cref="SkiaSharp.Views.Android.SKGLSurfaceView.PaintSurface" />
		/// event.
		/// ## Examples
		/// ```csharp
		/// myView.PaintSurface += (sender, e) => {
		/// var surface = e.Surface;
		/// var surfaceWidth = e.BackendRenderTarget.Width;
		/// var surfaceHeight = e.BackendRenderTarget.Height;
		/// var canvas = surface.Canvas;
		/// // draw on the canvas
		/// canvas.Flush ();
		/// };
		/// ```
		/// </remarks>
		public event EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;

		protected virtual void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
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
		}
	}
}
