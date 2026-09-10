using System;
using System.ComponentModel;
using Android.Content;
using Android.Opengl;
using Android.Util;

namespace SkiaSharp.Views.Android
{
	/// <summary>An implementation of <see cref="T:Android.Opengl.GLSurfaceView" /> that uses the dedicated surface for displaying a hardware-accelerated <see cref="T:SkiaSharp.SKSurface" />.</summary>
	/// <remarks />
	public class SKGLSurfaceView : GLSurfaceView
	{
		private SKGLSurfaceViewRenderer renderer;

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Android.SKGLSurfaceView" /> class.</summary>
		/// <param name="context">The <see cref="T:Android.Content.Context" /> the view is running in, through which it can access the current theme, resources, etc.</param>
		/// <remarks>Use this constructor when creating the view programmatically from code.</remarks>
		public SKGLSurfaceView(Context context)
			: base(context)
		{
			Initialize();
		}

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Android.SKGLSurfaceView" /> class with the specified XML attributes.</summary>
		/// <param name="context">The <see cref="T:Android.Content.Context" /> the view is running in, through which it can access the current theme, resources, etc.</param>
		/// <param name="attrs">The attributes of the XML tag that is inflating the view.</param>
		/// <remarks>This constructor is called when inflating the view from an Android XML layout file.</remarks>
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

		/// <summary>Gets the current canvas size.</summary>
		/// <value>The current size of the canvas.</value>
		/// <remarks>The canvas size may be different to the view size as a result of the current device's pixel density.</remarks>
		public SKSize CanvasSize => renderer.CanvasSize;

		/// <summary>Gets the current GPU context.</summary>
		/// <value>The current GPU context.</value>
		/// <remarks />
		public GRContext GRContext => renderer.GRContext;

		/// <summary>Occurs when the surface needs to be redrawn.</summary>
		/// <remarks><format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// There are two ways to draw on this surface: by overriding the
		/// <xref:SkiaSharp.Views.Android.SKGLSurfaceView.OnPaintSurface(SkiaSharp.Views.Android.SKPaintGLSurfaceEventArgs)>
		/// method, or by attaching a handler to the
		/// <xref:SkiaSharp.Views.Android.SKGLSurfaceView.PaintSurface>
		/// event.
		///
		/// ## Examples
		///
		/// ```csharp
		/// myView.PaintSurface += (sender, e) => {
		///     var surface = e.Surface;
		///     var surfaceWidth = e.BackendRenderTarget.Width;
		///     var surfaceHeight = e.BackendRenderTarget.Height;
		///
		///     var canvas = surface.Canvas;
		///
		///     // draw on the canvas
		///
		///     canvas.Flush ();
		/// };
		/// ```
		/// ]]></format></remarks>
		public event EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;

		/// <summary>Implement this to draw on the canvas.</summary>
		/// <param name="e">The event arguments that contain the drawing surface and information.</param>
		/// <remarks><format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// There are two ways to draw on this surface: by overriding the
		/// <xref:SkiaSharp.Views.Android.SKGLSurfaceView.OnPaintSurface(SkiaSharp.Views.Android.SKPaintGLSurfaceEventArgs)>
		/// method, or by attaching a handler to the
		/// <xref:SkiaSharp.Views.Android.SKGLSurfaceView.PaintSurface>
		/// event.
		///
		/// > [!IMPORTANT]
		/// > If this method is overridden, then the base must be called, otherwise the
		/// > event will not be fired.
		///
		/// ## Examples
		///
		/// ```csharp
		/// protected override void OnPaintSurface (SKPaintGLSurfaceEventArgs e)
		/// {
		///     // call the base method
		///     base.OnPaintSurface (e);
		///
		///     var surface = e.Surface;
		///     var surfaceWidth = e.BackendRenderTarget.Width;
		///     var surfaceHeight = e.BackendRenderTarget.Height;
		///
		///     var canvas = surface.Canvas;
		///
		///     // draw on the canvas
		///
		///     canvas.Flush ();
		/// }
		/// ```
		/// ]]></format></remarks>
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
