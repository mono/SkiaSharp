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

		public SKSize CanvasSize => drawable?.Info.Size ?? SKSize.Empty;

		public bool IgnorePixelScaling
		{
			get { return ignorePixelScaling; }
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
			using (var surface = drawable.CreateSurface(Bounds, IgnorePixelScaling ? 1 : ContentScaleFactor, out var info))
			{
				if (info.Width == 0 || info.Height == 0)
					return;

				using (var ctx = UIGraphics.GetCurrentContext())
				{
					// draw on the image using SKiaSharp
					OnPaintSurface(new SKPaintSurfaceEventArgs(surface, info));
#pragma warning disable CS0618 // Type or member is obsolete
					DrawInSurface(surface, info);
#pragma warning restore CS0618 // Type or member is obsolete

					// draw the surface to the context
					drawable.DrawSurface(ctx, Bounds, info, surface);
				}
			}
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
