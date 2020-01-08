/*
using System;
using OpenTK.GLWidget;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Xamarin.Forms.Platform.GTK;

namespace SkiaSharp.Views.Forms
{
	public class SKGLWidget : GLWidget
	{
		private const SKColorType colorType = SKColorType.Rgba8888;
		private const GRSurfaceOrigin surfaceOrigin = GRSurfaceOrigin.BottomLeft;

		private GRContext grContext;
		private GRBackendRenderTarget renderTarget;
		private SKSurface surface;

		public SKGLWidget()
			: base(new GraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 8))
		{
			Initialize();
		}

		public SKGLWidget(GraphicsMode mode)
			: base(mode)
		{
			Initialize();
		}

		public SKGLWidget(GraphicsMode mode, int major, int minor, GraphicsContextFlags flags)
			: base(mode, major, minor, flags)
		{
			Initialize();
		}

		private void Initialize()
		{
			if (!GtkOpenGL.IsInitialized)
				throw new InvalidOperationException("Call GtkOpenGL.Init() before using SKGLWidget.");
		}

		public SKSize CanvasSize => new SKSize(renderTarget.Width, renderTarget.Height);

		public GRContext GRContext => grContext;

		public event EventHandler<Desktop.SKPaintGLSurfaceEventArgs> PaintSurface;

		protected override void OnRenderFrame()
		{
			base.OnRenderFrame();

			// create the contexts if not done already
			if (grContext == null)
			{
				var glInterface = GRGlInterface.CreateNativeGlInterface();
				grContext = GRContext.CreateGl(glInterface);
			}

			// manage the drawing surface
			var alloc = Allocation;
			var res = (int)Math.Max(1.0, Screen.Resolution / 96.0);
			var w = Math.Max(0, alloc.Width * res);
			var h = Math.Max(0, alloc.Height * res);
			if (renderTarget == null || surface == null || renderTarget.Width != w || renderTarget.Height != h)
			{
				// create or update the dimensions
				renderTarget?.Dispose();
				GL.GetInteger(GetPName.FramebufferBinding, out var framebuffer);
				GL.GetInteger(GetPName.StencilBits, out var stencil);
				GL.GetInteger(GetPName.Samples, out var samples);
				var maxSamples = grContext.GetMaxSurfaceSampleCount(colorType);
				if (samples > maxSamples)
					samples = maxSamples;
				var glInfo = new GRGlFramebufferInfo((uint)framebuffer, colorType.ToGlSizedFormat());
				renderTarget = new GRBackendRenderTarget(w, h, samples, stencil, glInfo);

				// create the surface
				surface?.Dispose();
				surface = SKSurface.Create(grContext, renderTarget, surfaceOrigin, colorType);
			}

			using (new SKAutoCanvasRestore(surface.Canvas, true))
			{
				// start drawing
				OnPaintSurface(new Desktop.SKPaintGLSurfaceEventArgs(surface, renderTarget, surfaceOrigin, colorType));
			}

			// update the control
			surface.Canvas.Flush();
		}

		protected virtual void OnPaintSurface(Desktop.SKPaintGLSurfaceEventArgs e)
		{
			// invoke the event
			PaintSurface?.Invoke(this, e);
		}

		public override void Destroy()
		{
			base.Destroy();

			// clean up
			surface?.Dispose();
			surface = null;
			renderTarget?.Dispose();
			renderTarget = null;
			grContext?.Dispose();
			grContext = null;
		}
	}
}
*/
