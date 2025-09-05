using System;
using System.ComponentModel;
using AppKit;
using CoreGraphics;
using Foundation;

namespace SkiaSharp.Views.Mac
{
	/// <summary>
	/// A view that can be drawn on using SkiaSharp drawing commands.
	/// </summary>
	[Register(nameof(SKCanvasView))]
	[DesignTimeVisible(true)]
	public class SKCanvasView : NSView
	{
		private SKCGSurfaceFactory drawable;
		private bool ignorePixelScaling;

		// created in code
		/// <summary>
		/// Default constructor that initializes a new instance of <see cref="SKCanvasView" />.
		/// </summary>
		public SKCanvasView()
		{
			Initialize();
		}

		// created in code
		/// <summary>
		/// Initializes the <see cref="SKCanvasView" /> with the specified frame.
		/// </summary>
		/// <param name="frame">The frame used by the view.</param>
		public SKCanvasView(CGRect frame)
			: base(frame)
		{
			Initialize();
		}

		// created via designer
		/// <summary>
		/// A constructor used when creating managed representations of unmanaged objects; Called by the runtime.
		/// </summary>
		/// <param name="p">The pointer (handle) to the unmanaged object.</param>
		public SKCanvasView(IntPtr p)
			: base(p)
		{
		}

		// created via designer
		/// <summary>
		/// Called after the object has been loaded from the nib file. Overriders must call the base method.
		/// </summary>
		public override void AwakeFromNib()
		{
			Initialize();
		}

		private void Initialize()
		{
			drawable = new SKCGSurfaceFactory();
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
				NeedsDisplay = true;
			}
		}

		/// <summary>
		/// Occurs when the canvas needs to be redrawn.
		/// </summary>
		/// <remarks>
		/// <para>
		/// There are two ways to draw on this surface: by overriding the
		/// <see cref="SKCanvasView.OnPaintSurface(SKPaintSurfaceEventArgs)" />
		/// method, or by attaching a handler to the
		/// <see cref="SKCanvasView.PaintSurface" />
		/// event.
		/// </para>
		/// </remarks>
		/// <example>
		/// <code language="csharp">
		/// myView.PaintSurface += (sender, e) => 
		/// {
		///     var surface = e.Surface;
		///     var surfaceWidth = e.Info.Width;
		///     var surfaceHeight = e.Info.Height;
		///     var canvas = surface.Canvas;
		/// 
		///     // draw on the canvas
		/// };
		/// </code>
		/// </example>
		public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;

		protected virtual void OnPaintSurface(SKPaintSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}

		/// <summary>
		/// Draws the view within the passed-in rectangle.
		/// </summary>
		/// <param name="dirtyRect">The rectangle to draw.</param>
		public override void DrawRect(CGRect dirtyRect)
		{
			base.DrawRect(dirtyRect);

			// create the skia context
			using (var surface = drawable.CreateSurface(Bounds, Window.BackingScaleFactor, out var info))
			{
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
					skiaCanvas.Scale((float)Window.BackingScaleFactor);
					skiaCanvas.Save();
				}

				using (var ctx = NSGraphicsContext.CurrentContext.CGContext)
				{
					// draw on the image using SKiaSharp
					OnPaintSurface(new SKPaintSurfaceEventArgs(surface, info.WithSize(userVisibleSize), info));

					// draw the surface to the context
					drawable.DrawSurface(ctx, Bounds, info, surface);
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			drawable.Dispose();
		}
	}
}
