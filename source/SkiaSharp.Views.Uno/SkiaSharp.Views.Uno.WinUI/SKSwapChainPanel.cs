using System;
using Uno;
using Microsoft.UI.Xaml;

namespace SkiaSharp.Views.Windows
{
	/// <summary>A XAML control that uses hardware-accelerated rendering via ANGLE to draw using SkiaSharp.</summary>
	/// <remarks>This control uses an OpenGL ES context via ANGLE to provide GPU-accelerated SkiaSharp drawing. It inherits from <see cref="T:SkiaSharp.Views.Windows.AngleSwapChainPanel" /> and provides SkiaSharp-specific rendering functionality.</remarks>
	public class SKSwapChainPanel : FrameworkElement
	{
		private bool enableRenderLoop = false;

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Windows.SKSwapChainPanel" /> class.</summary>
		/// <remarks />
		public SKSwapChainPanel()
		{
			ThrowIfUnsupported();
		}

		/// <summary>Gets or sets a value indicating whether unsupported rendering operations raise an exception.</summary>
		/// <value><see langword="true" /> to raise an exception for unsupported operations; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public static bool RaiseOnUnsupported { get; set; } = true;

		/// <summary>Gets the current canvas size in pixels.</summary>
		/// <value>The size of the drawing canvas in pixels.</value>
		/// <remarks />
		public SKSize CanvasSize
		{
			get
			{
				ThrowIfUnsupported();
				return new SKSize();
			}
		}

		/// <summary>Gets the GPU context used for rendering.</summary>
		/// <value>The <see cref="T:SkiaSharp.GRContext" /> used for GPU-accelerated rendering.</value>
		/// <remarks />
		public GRContext GRContext
		{
			get
			{
				ThrowIfUnsupported();
				return null;
			}
		}

		/// <summary>Gets the scale factor between logical and physical pixels.</summary>
		/// <value>The current display scale factor.</value>
		/// <remarks />
		public double ContentsScale { get; private set; }

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
			set => enableRenderLoop = value;
		}

		/// <summary>Requests that the control be redrawn.</summary>
		/// <remarks />
		public void Invalidate()
		{
		}

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

		// Only Windows App SDK has a swap chain to draw into; on Uno Platform targets, SKCanvasElement is the
		// GPU-backed SkiaSharp control.
		private static void ThrowIfUnsupported()
		{
			if (RaiseOnUnsupported)
				throw new NotSupportedException($"SKSwapChainPanel is not supported for Skia based platforms");
		}
	}
}
