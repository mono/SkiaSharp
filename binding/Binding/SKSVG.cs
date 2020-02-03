using System;
using System.IO;

namespace SkiaSharp
{
	public unsafe class SKSvgCanvas
	{
		private SKSvgCanvas ()
		{
		}

		// Create

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

		public static SKCanvas Create (SKRect bounds, SKXmlWriter writer)
		{
			if (writer == null)
				throw new ArgumentNullException (nameof (writer));

			return Referenced (SKObject.GetObject<SKCanvas> (SkiaApi.sk_svgcanvas_create_with_writer (&bounds, writer.Handle)), writer);
		}

		//

		private static SKCanvas Owned (SKCanvas canvas, SKObject stream)
		{
			if (stream != null) {
				if (canvas != null)
					canvas.SetDisposeChild (stream);
				else
					stream.Dispose ();
			}

			return canvas;
		}

		private static SKCanvas Referenced (SKCanvas canvas, SKObject stream)
		{
			if (stream != null && canvas != null)
				canvas.KeepAlive (stream);

			return canvas;
		}
	}
}
