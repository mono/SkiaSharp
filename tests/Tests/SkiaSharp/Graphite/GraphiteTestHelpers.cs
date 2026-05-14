using System;

namespace SkiaSharp.Tests
{
	internal static class GraphiteTestHelpers
	{
		/// <summary>
		/// Test-only wrapper around SKGraphiteContext.RequestReadPixels that blocks the
		/// calling thread until the readback callback has fired, then copies the first
		/// plane into the caller-supplied buffer. Production callers should drive
		/// completion themselves via Submit / CheckAsyncWorkCompletion.
		/// </summary>
		public static bool ReadPixelsSync (
			this SKGraphiteContext ctx,
			SKSurface surface,
			SKImageInfo dstInfo,
			byte[] dstPixels,
			int srcX,
			int srcY)
		{
			if (ctx is null) throw new ArgumentNullException (nameof (ctx));
			if (surface is null) throw new ArgumentNullException (nameof (surface));
			if (dstPixels is null) throw new ArgumentNullException (nameof (dstPixels));

			bool done = false;
			bool success = false;
			var srcRect = new SKRectI (srcX, srcY, srcX + dstInfo.Width, srcY + dstInfo.Height);

			ctx.RequestReadPixels (surface, dstInfo, srcRect, result => {
				try {
					if (result is null || result.PlaneCount == 0)
						return;
					result.CopyPlaneTo (0, dstPixels, dstInfo.Height);
					success = true;
				} finally {
					done = true;
				}
			});

			// Drive completion. Submit (non-syncing) flushes pending recordings; spinning on
			// CheckAsyncWorkCompletion ticks the callback once the GPU is done.
			ctx.Submit ();
			while (!done)
				ctx.CheckAsyncWorkCompletion ();

			return success;
		}
	}
}
