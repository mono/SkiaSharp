using System;
using Uno;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace SkiaSharp.Views.Windows
{
	/// <summary>A XAML control that uses hardware-accelerated rendering via ANGLE to draw using SkiaSharp.</summary>
	/// <remarks>This control uses an OpenGL ES context via ANGLE to provide GPU-accelerated SkiaSharp drawing. It inherits from <see cref="T:SkiaSharp.Views.Windows.AngleSwapChainPanel" /> and provides SkiaSharp-specific rendering functionality.</remarks>
	public class SKSwapChainPanel : Grid
	{
		// Uno Platform exposes no swap chain or GPU context to draw into, so the panel renders on the CPU through an
		// SKXamlCanvas and hands its raster surface to PaintSurface.
		private readonly SKXamlCanvas canvas;
		private GRBackendRenderTarget renderTarget;
		private bool enableRenderLoop;
		private bool renderLoopAttached;

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Windows.SKSwapChainPanel" /> class.</summary>
		/// <remarks />
		public SKSwapChainPanel()
		{
			canvas = new SKXamlCanvas();
			canvas.PaintSurface += (_, e) =>
				OnPaintSurface(new SKPaintGLSurfaceEventArgs(e.Surface, GetRenderTarget(e.RawInfo), GRSurfaceOrigin.TopLeft, e.Info, e.RawInfo));
			Children.Add(canvas);

			Loaded += (_, _) => UpdateRenderLoop();
			Unloaded += (_, _) =>
			{
				UpdateRenderLoop();
				FreeRenderTarget();
			};
			RegisterPropertyChangedCallback(VisibilityProperty, (_, _) => UpdateRenderLoop());
		}

		/// <summary>Gets the current canvas size in pixels.</summary>
		/// <value>The size of the drawing canvas in pixels.</value>
		/// <remarks />
		public SKSize CanvasSize => canvas.CanvasSize;

		/// <summary>Gets the GPU context used for rendering.</summary>
		/// <value>The <see cref="T:SkiaSharp.GRContext" /> used for GPU-accelerated rendering, or <see langword="null" /> when the panel renders on the CPU, as it does on Uno Platform targets.</value>
		/// <remarks />
		public GRContext GRContext => null;

		/// <summary>Gets the scale factor between logical and physical pixels.</summary>
		/// <value>The current display scale factor.</value>
		/// <remarks />
		public double ContentsScale => canvas.Dpi;

		/// <summary>Gets or sets a value indicating whether rendering occurs in the background.</summary>
		/// <value><see langword="true" /> to render in the background; otherwise, <see langword="false" />.</value>
		/// <remarks />
		[NotImplemented]
		public bool DrawInBackground
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		/// <summary>Gets or sets a value indicating whether continuous rendering is enabled.</summary>
		/// <value><see langword="true" /> to render continuously; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool EnableRenderLoop
		{
			get => enableRenderLoop;
			set
			{
				enableRenderLoop = value;
				UpdateRenderLoop();
			}
		}

		/// <summary>Requests that the control be redrawn.</summary>
		/// <remarks />
		public void Invalidate() =>
			canvas.Invalidate();

		/// <summary>Occurs when the surface needs to be repainted.</summary>
		/// <remarks>Handle this event to perform drawing operations on the GPU-accelerated surface.</remarks>
		public event EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;

		/// <summary>Raises the <see cref="E:SkiaSharp.Views.Windows.SKSwapChainPanel.PaintSurface" /> event.</summary>
		/// <param name="e">The event arguments containing the surface and render target information.</param>
		/// <remarks>Override this method to perform custom drawing on the surface without subscribing to the <see cref="E:SkiaSharp.Views.Windows.SKSwapChainPanel.PaintSurface" /> event.</remarks>
		protected virtual void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
		{
			// invoke the event
			PaintSurface?.Invoke(this, e);
		}

		// Handlers commonly size their drawing from BackendRenderTarget, so it describes the raster surface; there is
		// no framebuffer behind it.
		private GRBackendRenderTarget GetRenderTarget(SKImageInfo info)
		{
			if (renderTarget == null || renderTarget.Width != info.Width || renderTarget.Height != info.Height)
			{
				FreeRenderTarget();
				renderTarget = new GRBackendRenderTarget(info.Width, info.Height, 0, 0, new GRGlFramebufferInfo(0, info.ColorType.ToGlSizedFormat()));
			}
			return renderTarget;
		}

		private void FreeRenderTarget()
		{
			renderTarget?.Dispose();
			renderTarget = null;
		}

		// The loop only runs while there is something to show, so a hidden or unloaded panel costs nothing.
		private void UpdateRenderLoop()
		{
			var run = enableRenderLoop && IsLoaded && Visibility == Visibility.Visible;
			if (run == renderLoopAttached)
				return;

			if (run)
				CompositionTarget.Rendering += OnRendering;
			else
				CompositionTarget.Rendering -= OnRendering;
			renderLoopAttached = run;
		}

		private void OnRendering(object sender, object e) =>
			canvas.Invalidate();
	}
}
