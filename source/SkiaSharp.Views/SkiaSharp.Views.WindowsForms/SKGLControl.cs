using System;
using System.ComponentModel;
using System.Windows.Forms;
using OpenTK;
#if WINDOWS
using OpenTK.GLControl;
#endif
using OpenTK.Graphics;
using OpenTK.Graphics.ES20;

namespace SkiaSharp.Views.Desktop
{
	/// <summary>A hardware-accelerated control that can be drawn on using SkiaSharp drawing commands.</summary>
	/// <remarks />
	[DefaultEvent("PaintSurface")]
	[DefaultProperty("Name")]
	public class SKGLControl : GLControl
	{
		private const SKColorType colorType = SKColorType.Rgba8888;
		private const GRSurfaceOrigin surfaceOrigin = GRSurfaceOrigin.BottomLeft;

		private GRContext grContext;
		private GRGlFramebufferInfo glInfo;
		private GRBackendRenderTarget renderTarget;
		private SKSurface surface;
		private SKCanvas canvas;

		private SKSizeI lastSize;

#if WINDOWS
		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Desktop.SKGLControl" /> class.</summary>
		/// <remarks />
		public SKGLControl()
			: base(new GLControlSettings { AlphaBits = 8, RedBits = 8, GreenBits = 8, BlueBits = 8, DepthBits = 24, StencilBits = 8 })
		{
			Initialize();
		}

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Desktop.SKGLControl" /> class with the specified OpenTK control settings.</summary>
		/// <param name="settings">The OpenTK control settings.</param>
		/// <remarks />
		public SKGLControl(GLControlSettings settings)
			: base(settings)
		{
			Initialize();
		}
#else
		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Desktop.SKGLControl" /> class.</summary>
		/// <remarks />
		public SKGLControl()
			: base(new GraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 8))
		{
			Initialize();
		}

		/// <param name="mode">The OpenTK graphics mode that defines the color, depth, stencil, and accumulation buffer configuration for the control.</param>
		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Desktop.SKGLControl" /> class with the specified graphics mode.</summary>
		/// <remarks></remarks>
		public SKGLControl(GraphicsMode mode)
			: base(mode)
		{
			Initialize();
		}

		/// <param name="mode">The OpenTK graphics mode that defines the color, depth, stencil, and accumulation buffer configuration.</param>
		/// <param name="major">The major version number of the OpenGL context to create.</param>
		/// <param name="minor">The minor version number of the OpenGL context to create.</param>
		/// <param name="flags">A bitwise combination of <see cref="T:OpenTK.Graphics.GraphicsContextFlags" /> values that control context creation.</param>
		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Desktop.SKGLControl" /> class with the specified graphics mode, OpenGL version, and context flags.</summary>
		/// <remarks></remarks>
		public SKGLControl(GraphicsMode mode, int major, int minor, GraphicsContextFlags flags)
			: base(mode, major, minor, flags)
		{
			Initialize();
		}
#endif

		private void Initialize()
		{
			ResizeRedraw = true;
		}

		/// <summary>Gets the current canvas size.</summary>
		/// <value>The current size of the canvas.</value>
		/// <remarks>The canvas size may be different to the view size as a result of the current device's pixel density.</remarks>
		public SKSize CanvasSize => lastSize;

		/// <summary>Gets the current GPU context.</summary>
		/// <value>The current GPU context.</value>
		/// <remarks />
		public GRContext GRContext => grContext;

		/// <summary>Occurs when the surface needs to be redrawn.</summary>
		/// <remarks>
		///           <format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// There are two ways to draw on this surface: by overriding the
		/// <xref:SkiaSharp.Views.Desktop.SKGLControl.OnPaintSurface(SkiaSharp.Views.Desktop.SKPaintGLSurfaceEventArgs)>
		/// method, or by attaching a handler to the
		/// <xref:SkiaSharp.Views.Desktop.SKGLControl.PaintSurface>
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
		/// ]]></format>
		///         </remarks>
		[Category("Appearance")]
		public event EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;

		/// <param name="e">A PaintEventArgs that contains the event data.</param>
		/// <summary>Raises the Paint event.</summary>
		/// <remarks />
		protected override void OnPaint(PaintEventArgs e)
		{
			if (DesignMode)
			{
				e.Graphics.Clear(BackColor);
				return;
			}

			base.OnPaint(e);

			MakeCurrent();

			// create the contexts if not done already
			if (grContext == null)
			{
				var glInterface = GRGlInterface.Create();
				grContext = GRContext.CreateGl(glInterface);
			}

			// get the new surface size
			var newSize = new SKSizeI(Width, Height);

			// manage the drawing surface
			if (renderTarget == null || lastSize != newSize || !renderTarget.IsValid)
			{
				// create or update the dimensions
				lastSize = newSize;

				GL.GetInteger(GetPName.FramebufferBinding, out var framebuffer);
				GL.GetInteger(GetPName.StencilBits, out var stencil);
				GL.GetInteger(GetPName.Samples, out var samples);
				var maxSamples = grContext.GetMaxSurfaceSampleCount(colorType);
				if (samples > maxSamples)
					samples = maxSamples;
				glInfo = new GRGlFramebufferInfo((uint)framebuffer, colorType.ToGlSizedFormat());

				// destroy the old surface
				surface?.Dispose();
				surface = null;
				canvas = null;

				// re-create the render target
				renderTarget?.Dispose();
				renderTarget = new GRBackendRenderTarget(newSize.Width, newSize.Height, samples, stencil, glInfo);
			}

			// create the surface
			if (surface == null)
			{
				surface = SKSurface.Create(grContext, renderTarget, surfaceOrigin, colorType);
				canvas = surface.Canvas;
			}

			using (new SKAutoCanvasRestore(canvas, true))
			{
				// start drawing
				OnPaintSurface(new SKPaintGLSurfaceEventArgs(surface, renderTarget, surfaceOrigin, colorType));
			}

			// update the control
			canvas.Flush();
			SwapBuffers();
		}

		/// <param name="e">The event arguments that contain the drawing surface and information.</param>
		/// <summary>Implement this to draw on the canvas.</summary>
		/// <remarks>
		///           <format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// There are two ways to draw on this surface: by overriding the
		/// <xref:SkiaSharp.Views.Desktop.SKGLControl.OnPaintSurface(SkiaSharp.Views.Desktop.SKPaintGLSurfaceEventArgs)>
		/// method, or by attaching a handler to the
		/// <xref:SkiaSharp.Views.Desktop.SKGLControl.PaintSurface>
		/// event. If the method is overridden, then the base must be called.
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
		/// ]]></format>
		///         </remarks>
		protected virtual void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
		{
			// invoke the event
			PaintSurface?.Invoke(this, e);
		}

		/// <param name="disposing">
		///           <see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.Views.Desktop.SKGLControl" /> and optionally releases the managed resources.</summary>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:SkiaSharp.Views.Desktop.SKGLControl" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			// clean up
			canvas = null;
			surface?.Dispose();
			surface = null;
			renderTarget?.Dispose();
			renderTarget = null;
			grContext?.Dispose();
			grContext = null;
		}
	}
}
