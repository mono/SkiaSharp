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
		private float density;

		public SKCanvasView(Context context)
			: base(context)
		{
			Initialize();
		}

		public SKCanvasView(Context context, IAttributeSet attrs)
			: base(context, attrs)
		{
			Initialize(attrs);
		}

		public SKCanvasView(Context context, IAttributeSet attrs, int defStyleAttr)
			: base(context, attrs, defStyleAttr)
		{
			Initialize(attrs);
		}

		protected SKCanvasView(IntPtr javaReference, JniHandleOwnership transfer)
			: base(javaReference, transfer)
		{
			Initialize();
		}

		private void Initialize(IAttributeSet attrs = null)
		{
			designMode = !Extensions.IsValidEnvironment;
			surfaceFactory = new SurfaceFactory();
			density = Resources.DisplayMetrics.Density;

			if (attrs != null)
			{
				using var a = Context.ObtainStyledAttributes(attrs, Resource.Styleable.SKCanvasView);

				var N = a?.IndexCount ?? 0;
				for (var i = 0; i < N; ++i)
				{
					var attr = a?.GetIndex(i) ?? 0;
					if (attr == Resource.Styleable.SKCanvasView_ignorePixelScaling)
						IgnorePixelScaling = a?.GetBoolean(attr, false);
				}

				a?.Recycle();
			}
		}

		public SKSize CanvasSize { get; private set; }

		public bool IgnorePixelScaling
		{
			get => ignorePixelScaling;
			set
			{
				ignorePixelScaling = value;
				surfaceFactory.UpdateCanvasSize(Width, Height);
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
			{
				CanvasSize = SKSize.Empty;
				return;
			}

			var userVisibleSize = IgnorePixelScaling
				? new SKSizeI((int)(info.Width / density), (int)(info.Height / density))
				: info.Size;

			CanvasSize = userVisibleSize;

			if (IgnorePixelScaling)
			{
				var skiaCanvas = surface.Canvas;
				skiaCanvas.Scale(density);
				skiaCanvas.Save();
			}

			// draw using SkiaSharp
			OnPaintSurface(new SKPaintSurfaceEventArgs(surface, info.WithSize(userVisibleSize), info));
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
			surfaceFactory.UpdateCanvasSize(w, h);
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

			surfaceFactory.UpdateCanvasSize(Width, Height);
			Invalidate();
		}

		protected override void Dispose(bool disposing)
		{
			surfaceFactory.Dispose();

			base.Dispose(disposing);
		}
	}
}
