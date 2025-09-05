using System;
using System.ComponentModel;
using CoreGraphics;
using Foundation;
using UIKit;

#if __TVOS__
namespace SkiaSharp.Views.tvOS
#elif __IOS__
namespace SkiaSharp.Views.iOS
#endif
{
	/// <summary>
	/// A UIKit view that can be drawn on using SkiaSharp drawing commands.
	/// </summary>
	[Register(nameof(SKCanvasView))]
	[DesignTimeVisible(true)]
	public class SKCanvasView : UIView, IComponent
	{
		// for IComponent
#pragma warning disable 67
		private event EventHandler DisposedInternal;
#pragma warning restore 67
		ISite IComponent.Site { get; set; }
		event EventHandler IComponent.Disposed
		{
			add { DisposedInternal += value; }
			remove { DisposedInternal -= value; }
		}
		private bool designMode;

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
		/// <param name="frame">The frame used by the view, expressed in tvOS points.</param>
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
			designMode = ((IComponent)this).Site?.DesignMode == true || !EnvironmentExtensions.IsValidEnvironment;

			if (designMode)
				return;

			drawable = new SKCGSurfaceFactory();
		}

		/// <summary>
		/// Gets the current canvas size.
		/// </summary>
		/// <remarks>The canvas size may be different to the view size as a result of the current device's pixel density.</remarks>
		public SKSize CanvasSize { get; private set; }

		/// <summary>
		/// Gets or sets a value indicating whether the drawing canvas should be resized on high resolution displays.
		/// </summary>
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

		/// <summary>
		/// Draws the view within the passed-in rectangle.
		/// </summary>
		/// <param name="rect">The rectangle to draw.</param>
		public override void Draw(CGRect rect)
		{
			base.Draw(rect);

			if (designMode || drawable == null)
				return;

			// create the skia context
			using var surface = drawable.CreateSurface(Bounds, ContentScaleFactor, out var info);

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
				skiaCanvas.Scale((float)ContentScaleFactor);
				skiaCanvas.Save();
			}

			using var ctx = UIGraphics.GetCurrentContext();

			// draw on the image using SKiaSharp
			OnPaintSurface(new SKPaintSurfaceEventArgs(surface, info.WithSize(userVisibleSize), info));

			// draw the surface to the context
			drawable.DrawSurface(ctx, Bounds, info, surface);
		}

		/// <param name="window"></param>
		public override void WillMoveToWindow(UIWindow window)
		{
			if (drawable != null)
			{
				// release the memory if we are leaving the window
				if (window == null)
					drawable?.Dispose();
				else
					SetNeedsDisplay();
			}

			base.WillMoveToWindow(window);
		}

		/// <summary>
		/// Occurs when the the canvas needs to be redrawn.
		/// </summary>
		/// <remarks>There are two ways to draw on this surface: by overriding the
		/// <see cref="SkiaSharp.Views.tvOS.SKCanvasView.OnPaintSurface(SkiaSharp.Views.tvOS.SKPaintSurfaceEventArgs)" />
		/// method, or by attaching a handler to the
		/// <see cref="SkiaSharp.Views.tvOS.SKCanvasView.PaintSurface" />
		/// event.
		/// > [!NOTE]
		/// > If a version of SkiaSharp prior to version v1.68.x is being used, then the
		/// > <see cref="SkiaSharp.Views.tvOS.SKCanvasView.DrawInSurface(SkiaSharp.SKSurface,SkiaSharp.SKImageInfo)" />
		/// > method should be overridden instead of
		/// > <see cref="SkiaSharp.Views.tvOS.SKCanvasView.OnPaintSurface(SkiaSharp.Views.tvOS.SKPaintSurfaceEventArgs)" />.
		/// ## Examples
		/// ```csharp
		/// myView.PaintSurface += (sender, e) => {
		/// var surface = e.Surface;
		/// var surfaceWidth = e.Info.Width;
		/// var surfaceHeight = e.Info.Height;
		/// var canvas = surface.Canvas;
		/// // draw on the canvas
		/// canvas.Flush ();
		/// };
		/// ```</remarks>
		public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;

		protected virtual void OnPaintSurface(SKPaintSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}

		/// <summary>
		/// Lays out subviews.
		/// </summary>
		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			Layer.SetNeedsDisplay();
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			drawable?.Dispose();
			drawable = null;
		}
	}
}
