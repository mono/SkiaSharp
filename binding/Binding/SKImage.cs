using System;

namespace SkiaSharp
{
	public class SKImage : IDisposable
	{
		internal IntPtr handle;

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (handle != IntPtr.Zero) {
				SkiaApi.sk_image_unref (handle);
				handle = IntPtr.Zero;
			}
		}

		~SKImage()
		{
			Dispose (false);
		}
	}
}

