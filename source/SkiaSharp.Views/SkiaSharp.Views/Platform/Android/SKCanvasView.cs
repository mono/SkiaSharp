using System;
using System.ComponentModel;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;

namespace SkiaSharp.Views.Android
{
	/// <summary>
	/// A view that can be drawn on using SkiaSharp drawing commands.
	/// </summary>
	public class SKCanvasView : View
	{
		private bool ignorePixelScaling;
		private bool designMode;
		private SurfaceFactory surfaceFactory;
		private float density;

		/// <summary>
		/// Simple constructor to use when creating a <see cref="SKCanvasView" /> from code.
		/// </summary>
		/// <param name="context">The <see cref="global::Android.Content.Context" /> the view is running in, through which it can access the current theme, resources, etc.</param>
		public SKCanvasView(Context context)
			: base(context)
		{
			Initialize();
		}

		/// <summary>
		/// Constructor that is called when inflating a <see cref="SKCanvasView" /> from XML.
		/// </summary>
		/// <param name="context">The <see cref="global::Android.Content.Context" /> the view is running in, through which it can access the current theme, resources, etc.</param>
		/// <param name="attrs">The attributes of the XML tag that is inflating the view.</param>
		public SKCanvasView(Context context, IAttributeSet attrs)
			: base(context, attrs)
		{
			Initialize(attrs);
		}

		/// <summary>
		/// Perform inflation from XML and apply a class-specific base style from a theme attribute.
		/// </summary>
		/// <param name="context">The <see cref="global::Android.Content.Context" /> the view is running in, through which it can access the current theme, resources, etc.</param>
		/// <param name="attrs">The attributes of the XML tag that is inflating the view.</param>
		/// <param name="defStyleAttr">An attribute in the current theme that contains a reference to a style resource that supplies default values for the view. Can be 0 to not look for defaults.</param>
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
			designMode = !EnvironmentExtensions.IsValidEnvironment;
			surfaceFactory = new SurfaceFactory();
			density = Resources.DisplayMetrics.Density;

			if (attrs != null)
			{
				using var a = Context.ObtainStyledAttributes(attrs, Resource.Styleable.SKCanvasView);

				var N = a.IndexCount;
				for (var i = 0; i < N; ++i)
				{
					var attr = a.GetIndex(i);
					if (attr == Resource.Styleable.SKCanvasView_ignorePixelScaling)
						IgnorePixelScaling = a.GetBoolean(attr, false);
				}

				a.Recycle();
			}
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

			// draw the surface to the view
			surfaceFactory.DrawSurface(surface, canvas);
		}

		protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
		{
			base.OnSizeChanged(w, h, oldw, oldh);

			// update the info with the new sizes
			surfaceFactory.UpdateCanvasSize(w, h);
		}

		/// <summary>
		/// Occurs when the the canvas needs to be redrawn.
		/// </summary>
		/// <remarks>There are two ways to draw on this surface: by overriding the
		/// <see cref="SkiaSharp.Views.Android.SKCanvasView.OnPaintSurface(SkiaSharp.Views.Android.SKPaintSurfaceEventArgs)" />
		/// method, or by attaching a handler to the
		/// <see cref="SkiaSharp.Views.Android.SKCanvasView.PaintSurface" />
		/// event.
		/// ## Examples
		/// ```csharp
		/// SKCanvasView myView = ...;
		/// myView.PaintSurface += (sender, e) => {
		/// var surface = e.Surface;
		/// var surfaceWidth = e.Info.Width;
		/// var surfaceHeight = e.Info.Height;
		/// var canvas = surface.Canvas;
		/// // draw on the canvas
		/// canvas.Flush();
		/// };
		/// ```</remarks>
		public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;

		protected virtual void OnPaintSurface(SKPaintSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
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
