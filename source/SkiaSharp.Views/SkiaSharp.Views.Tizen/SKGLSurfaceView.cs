using ElmSharp;
using System;
using System.Runtime.InteropServices;

namespace SkiaSharp.Views.Tizen
{
	/// <summary>
	/// OpenGL surface for Skia.
	/// </summary>
	public class SKGLSurfaceView : CustomRenderingView
	{
		// EFL-related members
		private readonly Interop.Evas.GL.Config glConfig = new Interop.Evas.GL.Config()
		{
			color_format = Interop.Evas.GL.ColorFormat.RGBA_8888,
			depth_bits = Interop.Evas.GL.DepthBits.BIT_24,
			stencil_bits = Interop.Evas.GL.StencilBits.BIT_8,
			options_bits = Interop.Evas.GL.OptionsBits.NONE,
			multisample_bits = Interop.Evas.GL.MultisampleBits.HIGH,
		};

		// pointer to glConfig passed to the native side
		private IntPtr unmanagedGlConfig;

		// connects the EFL with OpenGL
		private IntPtr glEvas;

		// drawing context
		private IntPtr glContext;

		// access to OpenGL API
		private Interop.Evas.GL.ApiWrapper glApi;

		// EFL wrapper for a OpenGL surface
		private IntPtr glSurface;

		// Skia-related members
		private GRContext context;

		private GRBackendRenderTargetDesc renderTarget = new GRBackendRenderTargetDesc
		{
			Config = GRPixelConfig.Unknown,
			Origin = GRSurfaceOrigin.TopLeft,
		};

		/// <summary>
		/// Creates new instance with the given object as its parent.
		/// </summary>
		/// <param name="parent">The parent object.</param>
		public SKGLSurfaceView(EvasObject parent) : base(parent)
		{
		}

		public event EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;

		public GRContext GRContext => context;

		protected sealed override int SurfaceWidth => renderTarget.Width;

		protected sealed override int SurfaceHeight => renderTarget.Height;

		/// <summary>
		/// Performs the drawing to the specified surface.
		/// </summary>
		/// <param name="surface">Surface to draw to.</param>
		/// <param name="renderTarget">Description of the rendering context.</param>
		protected virtual void OnDrawFrame(SKSurface surface, GRBackendRenderTargetDesc renderTarget)
		{
			PaintSurface?.Invoke(this, new SKPaintGLSurfaceEventArgs(surface, renderTarget));
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
				glApi.GlClear(GlesInterop.Gles.GL_STENCIL_BUFFER_BIT);

				// create the surface
				using (var surface = SKSurface.Create(context, renderTarget))
				{
					// draw using SkiaSharp
					OnDrawFrame(surface, renderTarget);

					surface.Canvas.Flush();
				}

				// flush the SkiaSharp contents to GL
				context.Flush();
			}
		}

		protected sealed override bool UpdateSurfaceSize(Rect geometry)
		{
			if (geometry.Width != renderTarget.Width || geometry.Height != renderTarget.Height)
			{
				// size has changed, update geometry
				renderTarget.Width = geometry.Width;
				renderTarget.Height = geometry.Height;

				return true;
			}
			else
			{
				return false;
			}
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
				glEvas = Interop.Evas.GL.evas_gl_new(Interop.Evas.evas_object_evas_get(parent));

				// copy the configuration to the native side
				unmanagedGlConfig = Marshal.AllocHGlobal(Marshal.SizeOf(glConfig));
				Marshal.StructureToPtr(glConfig, unmanagedGlConfig, false);

				// initialize the context
				glContext = Interop.Evas.GL.evas_gl_context_create(glEvas, IntPtr.Zero);

				// obtain the OpenGL function pointers
				glApi = new Interop.Evas.GL.ApiWrapper(glEvas);
			}
		}

		private void DestroyGL()
		{
			if (glEvas != IntPtr.Zero)
			{
				// zero the instance
				glApi = null;

				// destroy the context
				Interop.Evas.GL.evas_gl_context_destroy(glEvas, glContext);
				glContext = IntPtr.Zero;

				// release the unmanaged memory
				Marshal.FreeHGlobal(unmanagedGlConfig);
				unmanagedGlConfig = IntPtr.Zero;

				// destroy the EFL wrapper
				Interop.Evas.GL.evas_gl_free(glEvas);
				glEvas = IntPtr.Zero;
			}
		}

		private void CreateSurface()
		{
			if (glSurface == IntPtr.Zero)
			{
				// create the surface
				glSurface = Interop.Evas.GL.evas_gl_surface_create(glEvas, unmanagedGlConfig, renderTarget.Width, renderTarget.Height);

				// copy the native surface to the image
				Interop.Evas.GL.NativeSurfaceOpenGL nativeSurface;
				Interop.Evas.GL.evas_gl_native_surface_get(glEvas, glSurface, out nativeSurface);
				Interop.Evas.Image.evas_object_image_native_surface_set(EvasImage, ref nativeSurface);

				// switch to the current OpenGL context
				Interop.Evas.GL.evas_gl_make_current(glEvas, glSurface, glContext);

				// resize the viewport
				glApi.GlViewport(0, 0, renderTarget.Width, renderTarget.Height);

				// initialize the Skia's context
				CreateContext();
				FillRenderTarget();
			}
		}

		private void DestroySurface()
		{
			if (glSurface != IntPtr.Zero)
			{
				// finalize the Skia's context
				DestroyContext();

				// disconnect the surface from the image
				Interop.Evas.Image.evas_object_image_native_surface_set(EvasImage, IntPtr.Zero);

				// destroy the surface
				Interop.Evas.GL.evas_gl_surface_destroy(glEvas, glSurface);
				glSurface = IntPtr.Zero;
			}
		}

		private void CreateContext()
		{
			// create the interface using the function pointers provided by the EFL
			var glInterface = GRGlInterface.AssembleInterface((c, n) => glApi.GetFunctionPointer(n));
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

		private void FillRenderTarget()
		{
			// copy the properties of the current surface
			var currentRenderTarget = SKGLDrawable.CreateRenderTarget();
			renderTarget.SampleCount = currentRenderTarget.SampleCount;
			renderTarget.StencilBits = currentRenderTarget.StencilBits;
			renderTarget.RenderTargetHandle = currentRenderTarget.RenderTargetHandle;

			GuessPixelFormat();
		}

		private void GuessPixelFormat()
		{
			if (renderTarget.Config != GRPixelConfig.Unknown)
			{
				// already set, nothing to do
				return;
			}

			// emulator and target use different versions of pixel format
			// try to guess which one is available by creating a surface

			foreach (var config in new GRPixelConfig[] { GRPixelConfig.Rgba8888, GRPixelConfig.Bgra8888 })
			{
				if (renderTarget.Config == GRPixelConfig.Unknown)
				{
					renderTarget.Config = config;
					using (var surface = SKSurface.Create(context, renderTarget))
					{
						if (surface == null)
						{
							renderTarget.Config = GRPixelConfig.Unknown;
						}
					}
				}
			}

			if (renderTarget.Config == GRPixelConfig.Unknown)
			{
				throw new InvalidOperationException("Context does not support neither RGBA8888 nor BGRA8888 pixel format");
			}
		}
	}
}
