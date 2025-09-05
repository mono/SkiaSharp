using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;

namespace SkiaSharp.Views.Android
{
	/// <summary>
	/// An implementation of <see cref="global::Android.Views.SurfaceView" /> that uses the dedicated surface for displaying a hardware-accelerated <see cref="SKSurface" />.
	/// </summary>
	public class SKSurfaceView : SurfaceView, ISurfaceHolderCallback
	{
		private SurfaceFactory surfaceFactory;

		/// <summary>
		/// Simple constructor to use when creating a <see cref="SKSurfaceView" /> from code.
		/// </summary>
		/// <param name="context">The <see cref="global::Android.Content.Context" /> the view is running in, through which it can access the current theme, resources, etc.</param>
		public SKSurfaceView(Context context)
			: base(context)
		{
			Initialize();
		}

		/// <summary>
		/// Constructor that is called when inflating a <see cref="SKSurfaceView" /> from XML.
		/// </summary>
		/// <param name="context">The <see cref="global::Android.Content.Context" /> the view is running in, through which it can access the current theme, resources, etc.</param>
		/// <param name="attrs">The attributes of the XML tag that is inflating the view.</param>
		public SKSurfaceView(Context context, IAttributeSet attrs)
			: base(context, attrs)
		{
			Initialize();
		}

		/// <summary>
		/// Perform inflation from XML and apply a class-specific base style from a theme attribute.
		/// </summary>
		/// <param name="context">The <see cref="global::Android.Content.Context" /> the view is running in, through which it can access the current theme, resources, etc.</param>
		/// <param name="attrs">The attributes of the XML tag that is inflating the view.</param>
		/// <param name="defStyle">An attribute in the current theme that contains a reference to a style resource that supplies default values for the view. Can be 0 to not look for defaults.</param>
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

		/// <summary>
		/// Gets the current canvas size.
		/// </summary>
		/// <remarks>The canvas size may be different to the view size as a result of the current device's pixel density.</remarks>
		public SKSize CanvasSize => surfaceFactory.Info.Size;

		// ISurfaceHolderCallback

		/// <summary>
		/// This is called immediately after any structural changes (format or size) have been made to the surface.
		/// </summary>
		/// <param name="holder">The <see cref="global::Android.Views.ISurfaceHolder" /> whose surface has changed.</param>
		/// <param name="format">The new <see cref="global::Android.Graphics.Format" /> of the surface.</param>
		/// <param name="width">The new width of the surface.</param>
		/// <param name="height">The new height of the surface.</param>
		public virtual void SurfaceChanged(ISurfaceHolder holder, Format format, int width, int height)
		{
			surfaceFactory.UpdateCanvasSize(width, height);
		}

		/// <summary>
		/// This is called immediately after the surface is first created
		/// </summary>
		/// <param name="holder">The <see cref="global::Android.Views.ISurfaceHolder" /> whose surface is being created.</param>
		public virtual void SurfaceCreated(ISurfaceHolder holder)
		{
			var surfaceFrame = Holder.SurfaceFrame.ToSKRect();
			surfaceFactory.UpdateCanvasSize(surfaceFrame.Width, surfaceFrame.Height);
		}

		/// <summary>
		/// This is called immediately before a surface is being destroyed
		/// </summary>
		/// <param name="holder">The <see cref="global::Android.Views.ISurfaceHolder" /> whose surface is being destroyed.</param>
		public virtual void SurfaceDestroyed(ISurfaceHolder holder)
		{
			surfaceFactory.Dispose();
		}

		// lock / unlock the SKSurface

		/// <summary>
		/// Start editing the pixels in the surface. The returned <see cref="SKLockedSurface" /> can be used to get the surface for drawing into the surface's bitmap.
		/// </summary>
		/// <returns>The <see cref="SKLockedSurface" /> with the locked surface.</returns>
		public SKLockedSurface LockSurface()
		{
			var canvas = Holder.LockCanvas();
			if (canvas == null)
				return null;

			surfaceFactory.UpdateCanvasSize(canvas.Width, canvas.Height);
			return new SKLockedSurface(canvas, surfaceFactory);
		}

		/// <summary>
		/// Finish editing pixels in the surface. After this call, the surface's current pixels will be shown on the screen, but its content is lost.
		/// </summary>
		/// <param name="surface">The <see cref="SKLockedSurface" /> with the locked surface.</param>
		public void UnlockSurfaceAndPost(SKLockedSurface surface)
		{
			var canvas = surface.Post();
			Holder.UnlockCanvasAndPost(canvas);
		}

		// bitmap creation / disposal

		protected override void Dispose(bool disposing)
		{
			surfaceFactory.Dispose();

			base.Dispose(disposing);
		}
	}
}
