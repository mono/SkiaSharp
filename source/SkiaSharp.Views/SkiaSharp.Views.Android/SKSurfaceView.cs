using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;

namespace SkiaSharp.Views.Android
{
	public class SKSurfaceView : SurfaceView, ISurfaceHolderCallback
	{
		private Bitmap bitmap;

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
			Holder.AddCallback(this);
		}

		public SKSize CanvasSize => bitmap == null ? SKSize.Empty : new SKSize(bitmap.Width, bitmap.Height);

		// ISurfaceHolderCallback

		public virtual void SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height)
		{
			CreateBitmap(width, height);
		}

		public virtual void SurfaceCreated(ISurfaceHolder holder)
		{
			CreateBitmap(holder.SurfaceFrame.Width(), holder.SurfaceFrame.Height());
		}

		public virtual void SurfaceDestroyed(ISurfaceHolder holder)
		{
			FreeBitmap();
		}

		// lock / unlock the SKSurface

		public SKLockedSurface LockSurface()
		{
			var canvas = Holder.LockCanvas();
			return new SKLockedSurface(canvas, bitmap);
		}

		public void UnlockSurfaceAndPost(SKLockedSurface surface)
		{
			var canvas = surface.Post();
			Holder.UnlockCanvasAndPost(canvas);
		}

		// bitmap creation / disposal

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			FreeBitmap();
		}

		private void CreateBitmap(int width, int height)
		{
			// create the bitmap data
			if (bitmap == null || bitmap.Width != width || bitmap.Height != height)
			{
				FreeBitmap();
				bitmap = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);
			}
		}

		private void FreeBitmap()
		{
			if (bitmap != null)
			{
				// free and recycle the bitmap data
				bitmap.Recycle();
				bitmap.Dispose();
				bitmap = null;
			}
		}
	}
}
