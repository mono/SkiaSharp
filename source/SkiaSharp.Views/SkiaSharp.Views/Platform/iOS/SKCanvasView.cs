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
	/// <summary>A UIKit view that can be drawn on using SkiaSharp drawing commands.</summary>
	/// <remarks />
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
		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.iOS.SKCanvasView" /> class.</summary>
		/// <remarks />
		public SKCanvasView()
		{
			Initialize();
		}

		// created in code
		/// <param name="frame">The frame used by the view, expressed in iOS points.</param>
		/// <summary>Initializes the <see cref="T:SkiaSharp.Views.iOS.SKCanvasView" /> with the specified frame.</summary>
		/// <remarks />
		public SKCanvasView(CGRect frame)
			: base(frame)
		{
			Initialize();
		}

		// created via designer
		/// <param name="p">The pointer (handle) to the unmanaged object.</param>
		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.iOS.SKCanvasView" /> class from a native handle.</summary>
		/// <remarks>This constructor is used by the Xamarin.iOS runtime when creating managed representations of unmanaged objects. It is not intended to be called directly from user code.</remarks>
		public SKCanvasView(IntPtr p)
			: base(p)
		{
		}

		// created via designer
		/// <summary>Called after the object has been loaded from the nib file. Overriders must call the base method.</summary>
		/// <remarks />
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

		/// <summary>Gets the current canvas size.</summary>
		/// <value>The current canvas size in pixels.</value>
		/// <remarks>The canvas size may be different to the view size as a result of the current device's pixel density.</remarks>
		public SKSize CanvasSize { get; private set; }

		/// <summary>Gets or sets a value indicating whether the drawing canvas should be resized on high resolution displays.</summary>
		/// <value>
		///           <see langword="true" /> to ignore pixel scaling; otherwise, <see langword="false" />.</value>
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

		/// <param name="rect">The rectangle to draw.</param>
		/// <summary>Draws the view within the passed-in rectangle.</summary>
		/// <remarks />
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

		/// <param name="window">The window to which the view is being added, or <see langword="null" /> if the view is being removed from a window.</param>
		/// <summary>Called when the view is about to move to a window.</summary>
		/// <remarks />
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

		/// <summary>Occurs when the canvas needs to be redrawn.</summary>
		/// <remarks>
		///           <format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// There are two ways to draw on this surface: by overriding the
		/// <xref:SkiaSharp.Views.iOS.SKCanvasView.OnPaintSurface(SkiaSharp.Views.iOS.SKPaintSurfaceEventArgs)>
		/// method, or by attaching a handler to the
		/// <xref:SkiaSharp.Views.iOS.SKCanvasView.PaintSurface>
		/// event.
		///
		/// ## Examples
		///
		/// ```csharp
		/// myView.PaintSurface += (sender, e) => {
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
		/// <xref:SkiaSharp.Views.iOS.SKCanvasView.OnPaintSurface(SkiaSharp.Views.iOS.SKPaintSurfaceEventArgs)>
		/// method, or by attaching a handler to the
		/// <xref:SkiaSharp.Views.iOS.SKCanvasView.PaintSurface>
		/// event.
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

		/// <summary>Lays out subviews.</summary>
		/// <remarks />
		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			Layer.SetNeedsDisplay();
		}

		/// <param name="disposing">
		///           <see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.Views.iOS.SKCanvasView" /> and optionally releases the managed resources.</summary>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:SkiaSharp.Views.iOS.SKCanvasView" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			drawable?.Dispose();
			drawable = null;
		}
	}
}
