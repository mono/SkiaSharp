#if !__WATCHOS__

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
		public SKCanvasView()
		{
			Initialize();
		}

		// created in code
		public SKCanvasView(CGRect frame)
			: base(frame)
		{
			Initialize();
		}

		// created via designer
		public SKCanvasView(IntPtr p)
			: base(p)
		{
		}

		// created via designer
		public override void AwakeFromNib()
		{
			Initialize();
		}

		private void Initialize()
		{
			designMode = ((IComponent)this).Site?.DesignMode == true || !Extensions.IsValidEnvironment;

			if (designMode)
				return;

			drawable = new SKCGSurfaceFactory();
		}

		public SKSize CanvasSize { get; private set; }

		public bool IgnorePixelScaling
		{
			get => ignorePixelScaling;
			set
			{
				ignorePixelScaling = value;
				SetNeedsDisplay();
			}
		}

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
#pragma warning disable CS0618 // Type or member is obsolete
			DrawInSurface(surface, info);
#pragma warning restore CS0618 // Type or member is obsolete

			// draw the surface to the context
			drawable.DrawSurface(ctx, Bounds, info, surface);
		}

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

		public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;

		protected virtual void OnPaintSurface(SKPaintSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete("Use OnPaintSurface(SKPaintSurfaceEventArgs) instead.")]
		public virtual void DrawInSurface(SKSurface surface, SKImageInfo info)
		{
		}

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

#endif
