using System;
using System.ComponentModel;
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
			return SKObject.Owned (Create (bounds, managed), managed);
		}

		public static SKCanvas Create (SKRect bounds, SKWStream stream)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			return SKObject.Referenced (SKCanvas.GetObject (SkiaApi.sk_svgcanvas_create_with_stream (&bounds, stream.Handle)), stream);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use Create(SKRect, Stream) instead.")]
		public static SKCanvas Create (SKRect bounds, SKXmlWriter writer)
		{
			if (writer == null)
				throw new ArgumentNullException (nameof (writer));

			var canvas = SKCanvas.GetObject (SkiaApi.sk_svgcanvas_create_with_writer (&bounds, writer.Handle));
			writer.RevokeOwnership (canvas);
			return SKObject.Referenced (canvas, writer);
		}
	}
}
