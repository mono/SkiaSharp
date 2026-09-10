using System;
using System.ComponentModel;
using CoreAnimation;
using CoreGraphics;

#if __TVOS__
namespace SkiaSharp.Views.tvOS
#elif __IOS__
namespace SkiaSharp.Views.iOS
#elif __MACOS__
namespace SkiaSharp.Views.Mac
#endif
{
	/// <summary>A CoreAnimation layer that can be drawn on using SkiaSharp drawing commands.</summary>
	/// <remarks />
	public class SKCanvasLayer : CALayer
	{
		private readonly SKCGSurfaceFactory drawable;
		private bool ignorePixelScaling;

		/// <summary>Initializes a new instance of the <see cref="SKCanvasLayer" /> class.</summary>
		/// <remarks />
		public SKCanvasLayer()
		{
			drawable = new SKCGSurfaceFactory();

			SetNeedsDisplay();
			NeedsDisplayOnBoundsChange = true;
		}

		/// <summary>Gets the current canvas size.</summary>
		/// <value>The current canvas size in pixels.</value>
		/// <remarks>The canvas size may be different to the view size as a result of the current device's pixel density.</remarks>
		public SKSize CanvasSize { get; private set; }

		/// <summary>Gets or sets a value indicating whether the drawing canvas should be resized on high resolution displays.</summary>
		/// <value>
		///           <see langword="true" /> if the canvas should ignore pixel scaling; otherwise, <see langword="false" />.</value>
		/// <remarks>By default, when false, the canvas is resized to 1 canvas pixel per display pixel. When true, the canvas is resized to device independent pixels, and then stretched to fill the view. Although performance is improved and all objects are the same size on different display densities, blurring and pixelation may occur.</remarks>
		public bool IgnorePixelScaling
		{
			get => ignorePixelScaling;
			set
			{
				ignorePixelScaling = value;
				SetNeedsDisplay();
			}
		}

		/// <param name="ctx">The prepared context to draw into.</param>
		/// <summary>Draws the layer on the specified context.</summary>
		/// <remarks />
		public override void DrawInContext(CGContext ctx)
		{
			base.DrawInContext(ctx);

			// create the skia context
			using var surface = drawable.CreateSurface(Bounds, ContentsScale, out var info);

			if (info.Width == 0 || info.Height == 0)
			{
				CanvasSize = SKSize.Empty;
				return;
			}

			var userVisibleSize = IgnorePixelScaling
				? new SKSizeI((int)Bounds.Width, (int)Bounds.Height)
				: info.Size;

			CanvasSize = userVisibleSize;

			if (IgnorePixelScaling)
			{
				var skiaCanvas = surface.Canvas;
				skiaCanvas.Scale((float)ContentsScale);
				skiaCanvas.Save();
			}

			// draw on the image using SKiaSharp
			OnPaintSurface(new SKPaintSurfaceEventArgs(surface, info.WithSize(userVisibleSize), info));

			// draw the surface to the context
			drawable.DrawSurface(ctx, Bounds, info, surface);
		}

		/// <summary>Occurs when the canvas needs to be redrawn.</summary>
		/// <remarks>
		///           <format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// There are two ways to draw on this surface: by overriding the
		/// <c>OnPaintSurface</c> method, or by attaching a handler to the
		/// <c>PaintSurface</c> event.
		///
		/// ## Examples
		///
		/// ```csharp
		/// myLayer.PaintSurface += (sender, e) => {
		///     var surface = e.Surface;
		///     var surfaceWidth = e.Info.Width;
		///     var surfaceHeight = e.Info.Height;
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
		public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;

		/// <param name="e">The event arguments that contain the drawing surface and information.</param>
		/// <summary>Implement this to draw on the canvas.</summary>
		/// <remarks>
		///           <format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// There are two ways to draw on this surface: by overriding the
		/// <c>OnPaintSurface</c> method, or by attaching a handler to the
		/// <c>PaintSurface</c> event.
		///
		/// > [!IMPORTANT]
		/// > If this method is overridden, then the base must be called, otherwise the
		/// > event will not be fired.
		///
		/// ## Examples
		///
		/// ```csharp
		/// protected override void OnPaintSurface (SKPaintSurfaceEventArgs e)
		/// {
		///     // call the base method
		///     base.OnPaintSurface (e);
		///
		///     var surface = e.Surface;
		///     var surfaceWidth = e.Info.Width;
		///     var surfaceHeight = e.Info.Height;
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
		protected virtual void OnPaintSurface(SKPaintSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}

		/// <param name="disposing">
		///           <see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <summary>Releases the unmanaged resources used by the <see cref="SKCanvasLayer" /> and optionally releases the managed resources.</summary>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="SKCanvasLayer" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			drawable.Dispose();
		}
	}
}
