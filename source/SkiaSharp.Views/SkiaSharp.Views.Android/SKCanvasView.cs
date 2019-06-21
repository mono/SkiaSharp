﻿using System;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;

namespace SkiaSharp.Views.Android
{
	public class SKCanvasView : View
	{
		private Bitmap bitmap;
		private SKImageInfo info;
		private bool ignorePixelScaling;

		private bool designMode;

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

			if (designMode)
				return;

			// create the initial info
			info = new SKImageInfo(0, 0, SKColorType.Rgba8888, SKAlphaType.Premul);
		}

		public SKSize CanvasSize => info.Size;

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

			if (info.Width == 0 || info.Height == 0 || Visibility != ViewStates.Visible)
			{
				FreeBitmap();
				return;
			}

			// create the bitmap data if we need it
			if (bitmap == null || bitmap.Handle == IntPtr.Zero || bitmap.Width != info.Width || bitmap.Height != info.Height)
			{
				FreeBitmap();
				bitmap = Bitmap.CreateBitmap(info.Width, info.Height, Bitmap.Config.Argb8888);
			}

			// create a surface
			using (var surface = SKSurface.Create(info, bitmap.LockPixels(), info.RowBytes))
			{
				// draw using SkiaSharp
				OnPaintSurface(new SKPaintSurfaceEventArgs(surface, info));
#pragma warning disable CS0618 // Type or member is obsolete
				OnDraw(surface, info);
#pragma warning restore CS0618 // Type or member is obsolete

				surface.Canvas.Flush();
			}
			bitmap.UnlockPixels();

			// draw bitmap to canvas
			if (IgnorePixelScaling)
				canvas.DrawBitmap(bitmap, info.Rect.ToRect(), new RectF(0, 0, Width, Height), null);
			else
				canvas.DrawBitmap(bitmap, 0, 0, null);
		}

		protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
		{
			base.OnSizeChanged(w, h, oldw, oldh);

			// update the info with the new sizes
			UpdateCanvasSize(w, h);
		}

		private void UpdateCanvasSize(int w, int h)
		{
			if (designMode)
				return;

			if (IgnorePixelScaling)
			{
				var scale = Resources.DisplayMetrics.Density;
				info.Width = (int)(w / scale);
				info.Height = (int)(h / scale);
			}
			else
			{
				info.Width = w;
				info.Height = h;
			}
		}

		public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;

		protected virtual void OnPaintSurface(SKPaintSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}

		[Obsolete("Use OnPaintSurface(SKPaintSurfaceEventArgs) instead.")]
		protected virtual void OnDraw(SKSurface surface, SKImageInfo info)
		{
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				FreeBitmap();
			}
			base.Dispose(disposing);
		}

		private void FreeBitmap()
		{
			if (bitmap != null)
			{
				// free and recycle the bitmap data
				if (bitmap.Handle != IntPtr.Zero && !bitmap.IsRecycled)
				{
					bitmap.Recycle();
				}
				bitmap.Dispose();
				bitmap = null;
			}
		}
	}
}
