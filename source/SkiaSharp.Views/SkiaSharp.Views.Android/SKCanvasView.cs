using System;
using System.ComponentModel;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;

namespace SkiaSharp.Views.Android
{
	public class SKCanvasView : View
	{
		private bool ignorePixelScaling;
		private bool designMode;
		private SurfaceFactory surfaceFactory;

		public SKCanvasView(Context context)
			: base(context)
		{
			Initialize();
		}

		public SKCanvasView(Context context, IAttributeSet attrs)
			: base(context, attrs)
		{
			Initialize();
		}

		public SKCanvasView(Context context, IAttributeSet attrs, int defStyleAttr)
			: base(context, attrs, defStyleAttr)
		{
			Initialize();
		}

		protected SKCanvasView(IntPtr javaReference, JniHandleOwnership transfer)
			: base(javaReference, transfer)
		{
			Initialize();
		}

		private void Initialize()
		{
			designMode = !Extensions.IsValidEnvironment;
			surfaceFactory = new SurfaceFactory();
		}

		public SKSize CanvasSize => surfaceFactory.Info.Size;

		public bool IgnorePixelScaling
		{
			get { return ignorePixelScaling; }
			set
			{
				ignorePixelScaling = value;
				UpdateCanvasSize(Width, Height);
				Invalidate();
			}
		}

		protected override void OnDraw(Canvas canvas)
		{
			base.OnDraw(canvas);

			if (designMode)
				return;

			// bail out if the view is not actually visible
			if (Visibility != ViewStates.Visible)
			{
				surfaceFactory.FreeBitmap();
				return;
			}

			// create a skia surface
			var surface = surfaceFactory.CreateSurface(out var info);
			if (surface == null)
				return;

			// draw using SkiaSharp
			OnPaintSurface(new SKPaintSurfaceEventArgs(surface, info));
#pragma warning disable CS0618 // Type or member is obsolete
			OnDraw(surface, info);
#pragma warning restore CS0618 // Type or member is obsolete

			// draw the surface to the view
			surfaceFactory.DrawSurface(surface, canvas);
		}

		protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
		{
			base.OnSizeChanged(w, h, oldw, oldh);

			// update the info with the new sizes
			UpdateCanvasSize(w, h);
		}

		public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;

		protected virtual void OnPaintSurface(SKPaintSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Use OnPaintSurface(SKPaintSurfaceEventArgs) instead.")]
		protected virtual void OnDraw(SKSurface surface, SKImageInfo info)
		{
		}

		protected override void OnDetachedFromWindow()
		{
			surfaceFactory.Dispose();

			base.OnDetachedFromWindow();
		}

		protected override void OnAttachedToWindow()
		{
			base.OnAttachedToWindow();

			UpdateCanvasSize(Width, Height);
			Invalidate();
		}

		protected override void Dispose(bool disposing)
		{
			surfaceFactory.Dispose();

			base.Dispose(disposing);
		}

		private void UpdateCanvasSize(int w, int h) =>
			surfaceFactory.UpdateCanvasSize(w, h, IgnorePixelScaling ? Resources.DisplayMetrics.Density : 1);
	}
}
