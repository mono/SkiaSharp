using System;
using System.IO;

namespace SkiaSharp
{
	public unsafe class SKSvgCanvas
	{
		private SKSvgCanvas ()
		{
		}

		public static SKCanvas Create (SKRect bounds, Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			var managed = new SKManagedWStream (stream);
			return Owned (Create (bounds, managed), managed);
		}

		public static SKCanvas Create (SKRect bounds, SKWStream stream)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			return Referenced (SKObject.GetObject<SKCanvas> (SkiaApi.sk_svgcanvas_create (&bounds, stream.Handle)), stream);
		}

		private static SKCanvas Owned (SKCanvas canvas, SKWStream stream)
		{
			if (stream != null) {
				if (canvas != null)
					canvas.SetDisposeChild (stream);
				else
					stream.Dispose ();
			}

			return canvas;
		}

		private static SKCanvas Referenced (SKCanvas canvas, SKWStream stream)
		{
			if (stream != null && canvas != null)
				canvas.KeepAlive (stream);

			return canvas;
		}
	}
}
