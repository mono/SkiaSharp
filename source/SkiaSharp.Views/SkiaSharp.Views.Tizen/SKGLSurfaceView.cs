using System;
using System.Runtime.InteropServices;
using ElmSharp;
using SkiaSharp.Views.GlesInterop;
using SkiaSharp.Views.Tizen.Interop;

namespace SkiaSharp.Views.Tizen
{
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
		private GRBackendRenderTarget renderTarget;
		private SKSurface surface;
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

		public event EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;

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

				// initialize the context
				glContext = Evas.evas_gl_context_create(glEvas, IntPtr.Zero);
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
					using (new SKAutoCanvasRestore(surface.Canvas, true))
					{
						// draw using SkiaSharp
						OnDrawFrame(new SKPaintGLSurfaceEventArgs(surface, renderTarget, surfaceOrigin, colorType));
					}

					// flush the SkiaSharp contents to GL
					surface.Canvas.Flush();
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
				var glInterface = GRGlInterface.CreateNativeEvasInterface(glEvas);
				context?.Dispose();
				context = GRContext.Create(GRBackend.OpenGL, glInterface);

				// create the render target
				renderTarget?.Dispose();
				Gles.glGetIntegerv(Gles.GL_FRAMEBUFFER_BINDING, out var framebuffer);
				Gles.glGetIntegerv(Gles.GL_STENCIL_BITS, out var stencil);
				Gles.glGetIntegerv(Gles.GL_SAMPLES, out var samples);
				var maxSamples = context.GetMaxSurfaceSampleCount(colorType);
				if (samples > maxSamples)
					samples = maxSamples;
				var glInfo = new GRGlFramebufferInfo((uint)framebuffer, colorType.ToGlSizedFormat());
				renderTarget = new GRBackendRenderTarget(surfaceSize.Width, surfaceSize.Height, samples, stencil, glInfo);

				// create the surface
				surface?.Dispose();
				surface = SKSurface.Create(context, renderTarget, surfaceOrigin, colorType);
			}
		}

		protected sealed override void DestroyDrawingSurface()
		{
			if (glSurface != IntPtr.Zero)
			{
				// dispose the unmanaged memory
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
