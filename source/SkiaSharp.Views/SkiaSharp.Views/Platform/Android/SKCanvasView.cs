using System;
using System.ComponentModel;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;

namespace SkiaSharp.Views.Android
{
	/// <summary>A view that can be drawn on using SkiaSharp drawing commands.</summary>
	/// <remarks />
	public class SKCanvasView : View
	{
		private bool ignorePixelScaling;
		private bool designMode;
		private SurfaceFactory surfaceFactory;
		private float density;

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Android.SKCanvasView" /> class.</summary>
		/// <param name="context">The <see cref="T:Android.Content.Context" /> the view is running in, through which it can access the current theme, resources, etc.</param>
		/// <remarks>Use this constructor when creating the view programmatically from code.</remarks>
		public SKCanvasView(Context context)
			: base(context)
		{
			Initialize();
		}

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Android.SKCanvasView" /> class with the specified XML attributes.</summary>
		/// <param name="context">The <see cref="T:Android.Content.Context" /> the view is running in, through which it can access the current theme, resources, etc.</param>
		/// <param name="attrs">The attributes of the XML tag that is inflating the view.</param>
		/// <remarks>This constructor is called when inflating the view from an Android XML layout file.</remarks>
		public SKCanvasView(Context context, IAttributeSet attrs)
			: base(context, attrs)
		{
			Initialize(attrs);
		}

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Android.SKCanvasView" /> class with the specified XML attributes and style.</summary>
		/// <param name="context">The <see cref="T:Android.Content.Context" /> the view is running in, through which it can access the current theme, resources, etc.</param>
		/// <param name="attrs">The attributes of the XML tag that is inflating the view.</param>
		/// <param name="defStyleAttr">An attribute in the current theme that contains a reference to a style resource that supplies default values for the view. Can be 0 to not look for defaults.</param>
		/// <remarks>This constructor is called when inflating the view from an Android XML layout file and applying a class-specific base style from a theme attribute.</remarks>
		public SKCanvasView(Context context, IAttributeSet attrs, int defStyleAttr)
			: base(context, attrs, defStyleAttr)
		{
			Initialize(attrs);
		}

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Android.SKCanvasView" /> class from a JNI object reference.</summary>
		/// <param name="javaReference">A <see cref="T:System.IntPtr" /> a Java Native Interface (JNI) object reference.</param>
		/// <param name="transfer">A <see cref="T:Android.Runtime.JniHandleOwnership" /> indicating how to handle Java reference.</param>
		/// <remarks>This constructor is used by the Xamarin.Android runtime when creating managed representations of JNI objects. It is not intended to be called directly from user code.</remarks>
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

		/// <summary>Gets the current canvas size.</summary>
		/// <value>The current size of the canvas.</value>
		/// <remarks>The canvas size may be different to the view size as a result of the current device's pixel density.</remarks>
		public SKSize CanvasSize { get; private set; }

		/// <summary>Gets or sets a value indicating whether the drawing canvas should be resized on high resolution displays.</summary>
		/// <value><see langword="true" /> if the canvas should ignore pixel scaling; otherwise, <see langword="false" />.</value>
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

		/// <summary>Implement this to do your drawing.</summary>
		/// <param name="canvas">The canvas on which the background will be drawn.</param>
		/// <remarks />
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

		/// <summary>This is called during layout when the size of this view has changed. If you were just added to the view hierarchy, you're called with the old values of 0.</summary>
		/// <param name="w">Current width of this view.</param>
		/// <param name="h">Current height of this view.</param>
		/// <param name="oldw">Old width of this view.</param>
		/// <param name="oldh">Old height of this view.</param>
		/// <remarks />
		protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
		{
			base.OnSizeChanged(w, h, oldw, oldh);

			// update the info with the new sizes
			surfaceFactory.UpdateCanvasSize(w, h);
		}

		/// <summary>Occurs when the canvas needs to be redrawn.</summary>
		/// <remarks><format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// There are two ways to draw on this surface: by overriding the
		/// <xref:SkiaSharp.Views.Android.SKCanvasView.OnPaintSurface(SkiaSharp.Views.Android.SKPaintSurfaceEventArgs)>
		/// method, or by attaching a handler to the
		/// <xref:SkiaSharp.Views.Android.SKCanvasView.PaintSurface>
		/// event.
		///
		/// ## Examples
		///
		/// ```csharp
		/// SKCanvasView myView = ...;
		///
		/// myView.PaintSurface += (sender, e) => {
		///     var surface = e.Surface;
		///     var surfaceWidth = e.Info.Width;
		///     var surfaceHeight = e.Info.Height;
		///
		///     var canvas = surface.Canvas;
		///
		///     // draw on the canvas
		///     canvas.Flush();
		/// };
		/// ```
		/// ]]></format></remarks>
		public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;

		/// <summary>Implement this to draw on the canvas.</summary>
		/// <param name="e">The event arguments that contain the drawing surface and information.</param>
		/// <remarks><format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// There are two ways to draw on this surface: by overriding the
		/// <xref:SkiaSharp.Views.Android.SKCanvasView.OnPaintSurface(SkiaSharp.Views.Android.SKPaintSurfaceEventArgs)>
		/// method, or by attaching a handler to the
		/// <xref:SkiaSharp.Views.Android.SKCanvasView.PaintSurface>
		/// event. If the method is overridden, then the base must be called.
		///
		/// ## Examples
		///
		/// ```csharp
		/// protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
		/// {
		///     // call the base method
		///     base.OnPaintSurface(e);
		///
		///     var surface = e.Surface;
		///     var surfaceWidth = e.Info.Width;
		///     var surfaceHeight = e.Info.Height;
		///
		///     var canvas = surface.Canvas;
		///
		///     // draw on the canvas
		///     canvas.Flush();
		/// }
		/// ```
		/// ]]></format></remarks>
		protected virtual void OnPaintSurface(SKPaintSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}

		/// <summary>This is called when the view is detached from a window.</summary>
		/// <remarks />
		protected override void OnDetachedFromWindow()
		{
			surfaceFactory.Dispose();

			base.OnDetachedFromWindow();
		}

		/// <summary>Called when the view is attached to a window.</summary>
		/// <remarks />
		protected override void OnAttachedToWindow()
		{
			base.OnAttachedToWindow();

			surfaceFactory.UpdateCanvasSize(Width, Height);
			Invalidate();
		}

		/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.Views.Android.SKCanvasView" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:SkiaSharp.Views.Android.SKCanvasView" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
		protected override void Dispose(bool disposing)
		{
			surfaceFactory.Dispose();

			base.Dispose(disposing);
		}
	}
}
