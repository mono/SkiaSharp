using System;
using System.Runtime.InteropServices;
using ElmSharp;
using SkiaSharp.Views.GlesInterop;
using SkiaSharp.Views.Tizen.Interop;
using Tizen;

namespace SkiaSharp.Views.Tizen
{
	/// <summary>
	/// A hardware-accelerated view that can be drawn on using SkiaSharp drawing commands.
	/// </summary>
	public class SKGLSurfaceView : CustomRenderingView
	{
		private const GRSurfaceOrigin surfaceOrigin = GRSurfaceOrigin.BottomLeft;
		private static readonly SKColorType colorType = SKImageInfo.PlatformColorType;

		private readonly Evas.Config glConfig;

		private IntPtr glConfigPtr;
		private IntPtr glEvas;
		private IntPtr glContext;
		private IntPtr glSurface;

		private GRContext context;
		private GRGlFramebufferInfo glInfo;
		private GRBackendRenderTarget renderTarget;
		private SKSurface surface;
		private SKCanvas canvas;
		private SKSizeI surfaceSize;

		public SKGLSurfaceView(EvasObject parent)
			: base(parent)
		{
			glConfig = new Evas.Config
			{
				color_format = Evas.ColorFormat.RGBA_8888,
				depth_bits = Evas.DepthBits.BIT_24,
				stencil_bits = Evas.StencilBits.BIT_8,
				options_bits = Evas.OptionsBits.NONE,
				multisample_bits = Evas.MultisampleBits.HIGH,
				gles_version = default(int)
			};
		}

		/// <summary>
		/// Occurs when the surface needs to be redrawn.
		/// </summary>
		/// <remarks>
		/// There are two ways to draw on this surface: by overriding the
		/// <see cref="SkiaSharp.Views.Tizen.SKGLSurfaceView.OnDrawFrame(SkiaSharp.Views.Tizen.SKPaintGLSurfaceEventArgs)" />
		/// method, or by attaching a handler to the
		/// <see cref="SkiaSharp.Views.Tizen.SKGLSurfaceView.PaintSurface" />
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

		/// <summary>
		/// Gets the current GPU context.
		/// </summary>
		public GRContext GRContext => context;

		protected override SKSizeI GetSurfaceSize() => surfaceSize;

		protected virtual void OnDrawFrame(SKPaintGLSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}

		protected sealed override void CreateNativeResources(EvasObject parent)
		{
			if (glEvas == IntPtr.Zero)
			{
				// initialize the OpenGL (the EFL way)
				glEvas = Evas.evas_gl_new(Evas.evas_object_evas_get(parent));

				// copy the configuration to the native side
				glConfigPtr = Marshal.AllocHGlobal(Marshal.SizeOf(glConfig));
				Marshal.StructureToPtr(glConfig, glConfigPtr, false);

				// try initialize the context with Open GL ES 3.x first
				glContext = Evas.evas_gl_context_version_create(glEvas, IntPtr.Zero, Evas.GLContextVersion.EVAS_GL_GLES_3_X);

				// if we could not get 3.x, try 2.x
				if (glContext == IntPtr.Zero)
				{
					Log.Debug("SKGLSurfaceView", "OpenGL ES 3.x was not available, trying 2.x.");
					glContext = Evas.evas_gl_context_version_create(glEvas, IntPtr.Zero, Evas.GLContextVersion.EVAS_GL_GLES_2_X);
				}

				// if that is not available, the default
				if (glContext == IntPtr.Zero)
				{
					Log.Debug("SKGLSurfaceView", "OpenGL ES 2.x was not available, trying the default.");
					glContext = Evas.evas_gl_context_create(glEvas, IntPtr.Zero);
				}
			}
		}

		protected sealed override void DestroyNativeResources()
		{
			if (glEvas != IntPtr.Zero)
			{
				// destroy the context
				Evas.evas_gl_context_destroy(glEvas, glContext);
				glContext = IntPtr.Zero;

				// release the unmanaged memory
				Marshal.FreeHGlobal(glConfigPtr);
				glConfigPtr = IntPtr.Zero;

				// destroy the EFL wrapper
				Evas.evas_gl_free(glEvas);
				glEvas = IntPtr.Zero;
			}
		}

		protected sealed override void OnDrawFrame()
		{
			if (glSurface != IntPtr.Zero)
			{
				Gles.glClear(Gles.GL_COLOR_BUFFER_BIT | Gles.GL_DEPTH_BUFFER_BIT | Gles.GL_STENCIL_BUFFER_BIT);

				if (surface != null && renderTarget != null && context != null)
				{
					using (new SKAutoCanvasRestore(canvas, true))
					{
						// draw using SkiaSharp
						OnDrawFrame(new SKPaintGLSurfaceEventArgs(surface, renderTarget, surfaceOrigin, colorType));
					}

					// flush the SkiaSharp contents to GL
					canvas.Flush();
					context.Flush();
				}
			}
		}

		protected sealed override bool UpdateSurfaceSize(Rect geometry)
		{
			var changed =
				geometry.Width != surfaceSize.Width ||
				geometry.Height != surfaceSize.Height;

			if (changed)
			{
				// size has changed, update geometry
				surfaceSize.Width = geometry.Width;
				surfaceSize.Height = geometry.Height;
			}

			return changed;
		}

		protected sealed override void CreateDrawingSurface()
		{
			if (glSurface == IntPtr.Zero)
			{
				// create the surface
				glSurface = Evas.evas_gl_surface_create(glEvas, glConfigPtr, surfaceSize.Width, surfaceSize.Height);

				// copy the native surface to the image
				Evas.evas_gl_native_surface_get(glEvas, glSurface, out var nativeSurface);
				Evas.evas_object_image_native_surface_set(evasImage, ref nativeSurface);

				// switch to the current OpenGL context
				Evas.evas_gl_make_current(glEvas, glSurface, glContext);

				// resize the viewport
				Gles.glViewport(0, 0, surfaceSize.Width, surfaceSize.Height);

				// create the interface using the function pointers provided by the EFL
				var glInterface = GRGlInterface.CreateEvas(glEvas);
				if (glInterface == null)
					Log.Error("SKGLSurfaceView", "Unable to create GRGlInterface.");
				if (!glInterface.Validate())
					Log.Error("SKGLSurfaceView", "The created GRGlInterface was not valid.");
				context?.Dispose();
				context = GRContext.CreateGl(glInterface);
				if (context == null)
					Log.Error("SKGLSurfaceView", "Unable to create the GRContext.");

				// create the render target
				renderTarget?.Dispose();
				Gles.glGetIntegerv(Gles.GL_FRAMEBUFFER_BINDING, out var framebuffer);
				Gles.glGetIntegerv(Gles.GL_STENCIL_BITS, out var stencil);
				Gles.glGetIntegerv(Gles.GL_SAMPLES, out var samples);
				var maxSamples = context.GetMaxSurfaceSampleCount(colorType);
				if (samples > maxSamples)
					samples = maxSamples;
				glInfo = new GRGlFramebufferInfo((uint)framebuffer, colorType.ToGlSizedFormat());
				renderTarget = new GRBackendRenderTarget(surfaceSize.Width, surfaceSize.Height, samples, stencil, glInfo);

				// create the surface
				surface?.Dispose();
				surface = SKSurface.Create(context, renderTarget, surfaceOrigin, colorType);
				canvas = surface.Canvas;
			}
		}

		protected sealed override void DestroyDrawingSurface()
		{
			if (glSurface != IntPtr.Zero)
			{
				// dispose the unmanaged memory
				canvas = null;
				surface?.Dispose();
				surface = null;
				renderTarget?.Dispose();
				renderTarget = null;
				context?.Dispose();
				context = null;

				// disconnect the surface from the image
				Evas.evas_object_image_native_surface_set(evasImage, IntPtr.Zero);

				// destroy the surface
				Evas.evas_gl_surface_destroy(glEvas, glSurface);
				glSurface = IntPtr.Zero;
			}
		}
	}
}
