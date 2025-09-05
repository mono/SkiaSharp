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
	/// <summary>
	/// A CoreAnimation layer that can be drawn on using SkiaSharp drawing commands.
	/// </summary>
	public class SKCanvasLayer : CALayer
	{
		private readonly SKCGSurfaceFactory drawable;
		private bool ignorePixelScaling;

		/// <summary>
		/// Default constructor that initializes a new instance of <see cref="SKCanvasLayer" />.
		/// </summary>
		public SKCanvasLayer()
		{
			drawable = new SKCGSurfaceFactory();

			SetNeedsDisplay();
			NeedsDisplayOnBoundsChange = true;
		}

		/// <summary>
		/// Gets the current canvas size.
		/// </summary>
		/// <remarks>
		/// The canvas size may be different to the view size as a result of the current device's pixel density.
		/// </remarks>
		public SKSize CanvasSize { get; private set; }

		/// <summary>
		/// Gets or sets a value indicating whether the drawing canvas should be resized on high resolution displays.
		/// </summary>
		/// <remarks>
		/// By default, when false, the canvas is resized to 1 canvas pixel per display pixel. When true, the canvas is resized to device independent pixels, and then stretched to fill the view. Although performance is improved and all objects are the same size on different display densities, blurring and pixelation may occur.
		/// </remarks>
		public bool IgnorePixelScaling
		{
			get => ignorePixelScaling;
			set
			{
				ignorePixelScaling = value;
				SetNeedsDisplay();
			}
		}

		/// <summary>
		/// Draws the layer on the specified context.
		/// </summary>
		/// <param name="ctx">The prepared context to draw into.</param>
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

		/// <summary>
		/// Occurs when the the canvas needs to be redrawn.
		/// </summary>
		/// <remarks>
		/// There are two ways to draw on this surface: by overriding the
		/// <see cref="SKCanvasLayer.OnPaintSurface(SKPaintSurfaceEventArgs)" />
		/// method, or by attaching a handler to the
		/// <see cref="SKCanvasLayer.PaintSurface" />
		/// event.
		/// > [!NOTE]
		/// > If a version of SkiaSharp prior to version v1.68.x is being used, then the
		/// > <see cref="SKCanvasLayer.DrawInSurface(SkiaSharp.SKSurface,SkiaSharp.SKImageInfo)" />
		/// > method should be overridden instead of
		/// > <see cref="SKCanvasLayer.OnPaintSurface(SKPaintSurfaceEventArgs)" />.
		/// ## Examples
		/// ```csharp
		/// myLayer.PaintSurface += (sender, e) => {
		/// var surface = e.Surface;
		/// var surfaceWidth = e.Info.Width;
		/// var surfaceHeight = e.Info.Height;
		/// var canvas = surface.Canvas;
		/// // draw on the canvas
		/// canvas.Flush ();
		/// };
		/// ```
		/// </remarks>
		public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;

		protected virtual void OnPaintSurface(SKPaintSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			drawable.Dispose();
		}
	}
}
