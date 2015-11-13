using System;

namespace SkiaSharp
{
	public class SKMaskFilter : IDisposable
	{
		internal IntPtr handle;
		public SKMaskFilter (SKBlurStyle blurStyle, float sigma)
		{
			handle = SkiaApi.sk_maskfilter_new_blur (blurStyle, sigma);
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (handle != IntPtr.Zero) {
				SkiaApi.sk_maskfilter_unref (handle);
				handle = IntPtr.Zero;
			}
		}

		~SKMaskFilter()
		{
			Dispose (false);
		}
	}
}

