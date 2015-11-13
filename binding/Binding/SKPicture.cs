using System;

namespace SkiaSharp
{
	public class SKPicture : IDisposable
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
				SkiaApi.sk_picture_unref (handle);
				handle = IntPtr.Zero;
			}
		}

		~SKPicture()
		{
			Dispose (false);
		}
	}
}

