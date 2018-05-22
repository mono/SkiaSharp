using System;
using System.Runtime.InteropServices;
using ElmSharp;
using SkiaSharp.Views.GlesInterop;
using SkiaSharp.Views.Tizen.Interop;

namespace SkiaSharp.Views.Tizen
{
	public class SKGLSurfaceView : CustomRenderingView
	{
		private readonly Evas.Config glConfig;

		private IntPtr glConfigPtr;
		private IntPtr glEvas;
		private IntPtr glContext;
		private IntPtr glSurface;

		private GRContext context;
		private GRBackendRenderTargetDesc renderTarget;

		public SKGLSurfaceView(EvasObject parent)
			: base(parent)
		{
			glConfig = new Evas.Config()
			{
				color_format = Evas.ColorFormat.RGBA_8888,
				depth_bits = Evas.DepthBits.BIT_24,
				stencil_bits = Evas.StencilBits.BIT_8,
				options_bits = Evas.OptionsBits.NONE,
				multisample_bits = Evas.MultisampleBits.HIGH,
			};

			var isBgra = SKImageInfo.PlatformColorType == SKColorType.Bgra8888;
			renderTarget = new GRBackendRenderTargetDesc
			{
				Config = isBgra ? GRPixelConfig.Bgra8888 : GRPixelConfig.Rgba8888,
				Origin = GRSurfaceOrigin.BottomLeft,
			};
		}

		public event EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;

		public GRContext GRContext => context;

		protected override SKSizeI GetSurfaceSize() => renderTarget.Size;

		protected virtual void OnDrawFrame(SKPaintGLSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}

		protected sealed override void CreateNativeResources(EvasObject parent)
		{
			CreateGL(parent);
		}

		protected sealed override void DestroyNativeResources()
		{
			DestroyGL();
		}

		protected sealed override void OnDrawFrame()
		{
			if (glSurface != IntPtr.Zero)
			{
				Gles.glClear(Gles.GL_STENCIL_BUFFER_BIT);

				// create the surface
				using (var surface = SKSurface.Create(context, renderTarget))
				{
					// draw using SkiaSharp
					OnDrawFrame(new SKPaintGLSurfaceEventArgs(surface, renderTarget));

					surface.Canvas.Flush();
				}

				// flush the SkiaSharp contents to GL
				context.Flush();
			}
		}

		protected sealed override bool UpdateSurfaceSize(Rect geometry)
		{
			var changed =
				geometry.Width != renderTarget.Width ||
				geometry.Height != renderTarget.Height;

			if (changed)
			{
				// size has changed, update geometry
				renderTarget.Width = geometry.Width;
				renderTarget.Height = geometry.Height;
			}

			return changed;
		}

		protected sealed override void CreateDrawingSurface()
		{
			CreateSurface();
		}

		protected sealed override void DestroyDrawingSurface()
		{
			DestroySurface();
		}

		private void CreateGL(EvasObject parent)
		{
			if (glEvas == IntPtr.Zero)
			{
				// initialize the OpenGL (the EFL way)
				glEvas = Evas.evas_gl_new(Interop.Evas.evas_object_evas_get(parent));

				// copy the configuration to the native side
				glConfigPtr = Marshal.AllocHGlobal(Marshal.SizeOf(glConfig));
				Marshal.StructureToPtr(glConfig, glConfigPtr, false);

				// initialize the context
				glContext = Evas.evas_gl_context_create(glEvas, IntPtr.Zero);
			}
		}

		private void DestroyGL()
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

		private void CreateSurface()
		{
			if (glSurface == IntPtr.Zero)
			{
				// create the surface
				glSurface = Evas.evas_gl_surface_create(glEvas, glConfigPtr, renderTarget.Width, renderTarget.Height);

				// copy the native surface to the image
				Evas.NativeSurfaceOpenGL nativeSurface;
				Evas.evas_gl_native_surface_get(glEvas, glSurface, out nativeSurface);
				Evas.evas_object_image_native_surface_set(evasImage, ref nativeSurface);

				// switch to the current OpenGL context
				Evas.evas_gl_make_current(glEvas, glSurface, glContext);

				// resize the viewport
				Gles.glViewport(0, 0, renderTarget.Width, renderTarget.Height);

				// initialize the Skia's context
				CreateContext();

				// copy the properties of the current surface
				var currentRenderTarget = SKGLDrawable.CreateRenderTarget();
				renderTarget.SampleCount = currentRenderTarget.SampleCount;
				renderTarget.StencilBits = currentRenderTarget.StencilBits;
				renderTarget.RenderTargetHandle = currentRenderTarget.RenderTargetHandle;
			}
		}

		private void DestroySurface()
		{
			if (glSurface != IntPtr.Zero)
			{
				// finalize the Skia's context
				DestroyContext();

				// disconnect the surface from the image
				Evas.evas_object_image_native_surface_set(evasImage, IntPtr.Zero);

				// destroy the surface
				Evas.evas_gl_surface_destroy(glEvas, glSurface);
				glSurface = IntPtr.Zero;
			}
		}

		private void CreateContext()
		{
			// create the interface using the function pointers provided by the EFL
			var glInterface = GRGlInterface.CreateNativeEvasInterface(glEvas);
			context = GRContext.Create(GRBackend.OpenGL, glInterface);
		}

		private void DestroyContext()
		{
			if (context != null)
			{
				// dispose the unmanaged memory
				context.Dispose();
				context = null;
			}
		}
	}
}
