using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OpenTK.Graphics;
using System.Windows.Media.Media3D;
using SkiaSharp.Views.Desktop;
using OpenTK.Wpf;
using SkiaSharp;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform.Windows;
using OpenTK;
using System.Windows.Interop;
using OpenTK.Platform;
#if NETCOREAPP || NET
using OpenTK.Mathematics;
#endif

namespace SkiaSharp.Views.WPF
{
	/// <summary>A WPF element that uses OpenGL for hardware-accelerated SkiaSharp rendering.</summary>
	/// <remarks />
	[DefaultEvent("PaintSurface")]
	[DefaultProperty("Name")]
	public class SKGLElement : GLWpfControl, IDisposable
	{
		private const SKColorType colorType = SKColorType.Rgba8888;
		private const GRSurfaceOrigin surfaceOrigin = GRSurfaceOrigin.BottomLeft;

		private bool designMode;

		private GRContext grContext;
		private GRGlFramebufferInfo glInfo;
		private GRBackendRenderTarget renderTarget;
		private SKSurface surface;
		private SKCanvas canvas;

		private SKSizeI lastSize;

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.WPF.SKGLElement" /> class.</summary>
		/// <remarks />
		public SKGLElement()
			: base()
		{
			Initialize();
		}

		private void Initialize()
		{
			designMode = DesignerProperties.GetIsInDesignMode(this);
			var settings = new GLWpfControlSettings() { MajorVersion = 2, MinorVersion = 1, RenderContinuously = false };

			this.Render += OnPaint;

			this.Loaded += SKGLElement_Loaded;
			this.Unloaded += SKGLElement_Unloaded;

#if NETCOREAPP
			this.RegisterToEventsDirectly = false;
#endif

			Start(settings);
		}

		private void SKGLElement_Unloaded(object sender, RoutedEventArgs e)
		{
			Release();
		}
		private void SKGLElement_Loaded(object sender, RoutedEventArgs e)
		{
			InvalidateVisual();
		}

		/// <summary>Gets the current canvas size in pixels.</summary>
		/// <value>The current canvas size in pixels.</value>
		/// <remarks />
		public SKSize CanvasSize => lastSize;

		/// <summary>Gets the GPU context used for rendering.</summary>
		/// <value>The GPU context.</value>
		/// <remarks />
		public GRContext GRContext => grContext;

		/// <summary>Occurs when the surface needs to be redrawn.</summary>
		/// <remarks />
		[Category("Appearance")]
		public event EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;

		private SKSizeI GetSize()
		{
			var currentWidth = ActualWidth;
			var currentHeight = ActualHeight;

			if (currentWidth < 0 ||
				currentHeight < 0)
			{
				currentWidth = 0;
				currentHeight = 0;
			}

			PresentationSource source = PresentationSource.FromVisual(this);

			double dpiX = 1.0;
			double dpiY = 1.0;
			if (source != null)
			{
				dpiX = source.CompositionTarget.TransformToDevice.M11;
				dpiY = source.CompositionTarget.TransformToDevice.M22;
			}

			return new SKSizeI((int)(currentWidth * dpiX), (int)(currentHeight * dpiY));
		}

		/// <param name="drawingContext">The WPF drawing context for this render pass.</param>
		/// <summary>Participates in rendering operations when overridden in a derived class.</summary>
		/// <remarks />
		protected override void OnRender(DrawingContext drawingContext)
		{
			if (grContext != null)
			{
				grContext.ResetContext();
			}
			base.OnRender(drawingContext);
		}

		/// <param name="e">The elapsed time since the last paint event.</param>
		/// <summary>Called when the control needs to be painted.</summary>
		/// <remarks />
		protected virtual void OnPaint(TimeSpan e)
		{
			if (disposed)
			{
				return;
			}
			if (designMode)
			{
				return;
			}

			// create the contexts if not done already
			if (grContext == null)
			{
				var glInterface = GRGlInterface.Create();
				grContext = GRContext.CreateGl(glInterface);
			}

			// get the new surface size
			var newSize = GetSize();

			GL.ClearColor(Color4.Transparent);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

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
		}

		/// <param name="e">The event arguments containing the surface and drawing information.</param>
		/// <summary>Raises the <see cref="E:SkiaSharp.Views.WPF.SKGLElement.PaintSurface" /> event.</summary>
		/// <remarks />
		protected virtual void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
		{
			// invoke the event
			PaintSurface?.Invoke(this, e);
		}

		private bool disposed = false;


		/// <param name="disposing">
		///           <see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <summary>Releases the unmanaged resources used by the element and optionally releases the managed resources.</summary>
		/// <remarks />
		protected virtual void Dispose(bool disposing)
		{
			if (disposed)
			{
				return;
			}

			Release();

			disposed = true;
		}

		private void Release()
		{
			canvas = null;
			surface?.Dispose();
			surface = null;
			renderTarget?.Dispose();
			renderTarget = null;
			grContext?.Dispose();
			grContext = null;
		}

		/// <summary>Releases all resources used by the element.</summary>
		/// <remarks />
		public void Dispose()
        {
            Dispose(true);
		}
	}

}
