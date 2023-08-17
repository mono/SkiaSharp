using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;

namespace SkiaSharp.Views.Android
{
	public class SKSurfaceView : SurfaceView, ISurfaceHolderCallback
	{
		private SurfaceFactory surfaceFactory;

		public SKSurfaceView(Context context)
			: base(context)
		{
			Initialize();
		}

		public SKSurfaceView(Context context, IAttributeSet attrs)
			: base(context, attrs)
		{
			Initialize();
		}

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

		public SKSize CanvasSize => surfaceFactory.Info.Size;

		// ISurfaceHolderCallback

		public virtual void SurfaceChanged(ISurfaceHolder holder, Format format, int width, int height)
		{
			surfaceFactory.UpdateCanvasSize(width, height);
		}

		public virtual void SurfaceCreated(ISurfaceHolder holder)
		{
			var surfaceFrame = Holder.SurfaceFrame.ToSKRect();
			surfaceFactory.UpdateCanvasSize(surfaceFrame.Width, surfaceFrame.Height);
		}

		public virtual void SurfaceDestroyed(ISurfaceHolder holder)
		{
			surfaceFactory.Dispose();
		}

		// lock / unlock the SKSurface

		public SKLockedSurface LockSurface()
		{
			var canvas = Holder.LockCanvas();
			if (canvas == null)
				return null;

			surfaceFactory.UpdateCanvasSize(canvas.Width, canvas.Height);
			return new SKLockedSurface(canvas, surfaceFactory);
		}

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
