using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;

namespace SkiaSharp.Views.Android
{
	/// <summary>An implementation of <see cref="T:Android.Views.SurfaceView" /> that uses the dedicated surface for displaying a hardware-accelerated <see cref="T:SkiaSharp.SKSurface" />.</summary>
	/// <remarks />
	public class SKSurfaceView : SurfaceView, ISurfaceHolderCallback
	{
		private SurfaceFactory surfaceFactory;

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Android.SKSurfaceView" /> class.</summary>
		/// <param name="context">The <see cref="T:Android.Content.Context" /> the view is running in, through which it can access the current theme, resources, etc.</param>
		/// <remarks>Use this constructor when creating the view programmatically from code.</remarks>
		public SKSurfaceView(Context context)
			: base(context)
		{
			Initialize();
		}

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Android.SKSurfaceView" /> class with the specified XML attributes.</summary>
		/// <param name="context">The <see cref="T:Android.Content.Context" /> the view is running in, through which it can access the current theme, resources, etc.</param>
		/// <param name="attrs">The attributes of the XML tag that is inflating the view.</param>
		/// <remarks>This constructor is called when inflating the view from an Android XML layout file.</remarks>
		public SKSurfaceView(Context context, IAttributeSet attrs)
			: base(context, attrs)
		{
			Initialize();
		}

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Android.SKSurfaceView" /> class with the specified XML attributes and style.</summary>
		/// <param name="context">The <see cref="T:Android.Content.Context" /> the view is running in, through which it can access the current theme, resources, etc.</param>
		/// <param name="attrs">The attributes of the XML tag that is inflating the view.</param>
		/// <param name="defStyle">An attribute in the current theme that contains a reference to a style resource that supplies default values for the view. Can be 0 to not look for defaults.</param>
		/// <remarks>This constructor is called when inflating the view from an Android XML layout file and applying a class-specific base style from a theme attribute.</remarks>
		public SKSurfaceView(Context context, IAttributeSet attrs, int defStyle)
			: base(context, attrs, defStyle)
		{
			Initialize();
		}

		private void Initialize()
		{
			surfaceFactory = new SurfaceFactory();
			Holder.AddCallback(this);
		}

		/// <summary>Gets the current canvas size.</summary>
		/// <value>The current size of the canvas.</value>
		/// <remarks>The canvas size may be different to the view size as a result of the current device's pixel density.</remarks>
		public SKSize CanvasSize => surfaceFactory.Info.Size;

		// ISurfaceHolderCallback

		/// <summary>This is called immediately after any structural changes (format or size) have been made to the surface.</summary>
		/// <param name="holder">The <see cref="T:Android.Views.ISurfaceHolder" /> whose surface has changed.</param>
		/// <param name="format">The new <see cref="T:Android.Graphics.Format" /> of the surface.</param>
		/// <param name="width">The new width of the surface.</param>
		/// <param name="height">The new height of the surface.</param>
		/// <remarks />
		public virtual void SurfaceChanged(ISurfaceHolder holder, Format format, int width, int height)
		{
			surfaceFactory.UpdateCanvasSize(width, height);
		}

		/// <summary>Called immediately after the surface is first created.</summary>
		/// <param name="holder">The <see cref="T:Android.Views.ISurfaceHolder" /> whose surface is being created.</param>
		/// <remarks />
		public virtual void SurfaceCreated(ISurfaceHolder holder)
		{
			var surfaceFrame = Holder.SurfaceFrame.ToSKRect();
			surfaceFactory.UpdateCanvasSize(surfaceFrame.Width, surfaceFrame.Height);
		}

		/// <summary>Called immediately before a surface is being destroyed.</summary>
		/// <param name="holder">The <see cref="T:Android.Views.ISurfaceHolder" /> whose surface is being destroyed.</param>
		/// <remarks />
		public virtual void SurfaceDestroyed(ISurfaceHolder holder)
		{
			surfaceFactory.Dispose();
		}

		// lock / unlock the SKSurface

		/// <summary>Start editing the pixels in the surface. The returned <see cref="T:SkiaSharp.Views.Android.SKLockedSurface" /> can be used to get the surface for drawing into the surface's bitmap.</summary>
		/// <returns>The <see cref="T:SkiaSharp.Views.Android.SKLockedSurface" /> with the locked surface.</returns>
		/// <remarks />
		public SKLockedSurface LockSurface()
		{
			var canvas = Holder.LockCanvas();
			if (canvas == null)
				return null;

			surfaceFactory.UpdateCanvasSize(canvas.Width, canvas.Height);
			return new SKLockedSurface(canvas, surfaceFactory);
		}

		/// <summary>Finish editing pixels in the surface. After this call, the surface's current pixels will be shown on the screen, but its content is lost.</summary>
		/// <param name="surface">The <see cref="T:SkiaSharp.Views.Android.SKLockedSurface" /> with the locked surface.</param>
		/// <remarks />
		public void UnlockSurfaceAndPost(SKLockedSurface surface)
		{
			var canvas = surface.Post();
			Holder.UnlockCanvasAndPost(canvas);
		}

		// bitmap creation / disposal

		/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.Views.Android.SKSurfaceView" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:SkiaSharp.Views.Android.SKSurfaceView" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
		protected override void Dispose(bool disposing)
		{
			surfaceFactory.Dispose();

			base.Dispose(disposing);
		}
	}
}
