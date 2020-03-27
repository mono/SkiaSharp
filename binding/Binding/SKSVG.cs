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

			// TODO: there seems to be a memory issue with things getting destroyed in the incorrect order
			//return SKObject.Referenced (SKObject.GetObject<SKCanvas> (SkiaApi.sk_svgcanvas_create_with_stream (&bounds, stream.Handle)), stream);

			var writer = new SKXmlStreamWriter (stream);
			return SKObject.Owned (SKObject.GetObject<SKCanvas> (SkiaApi.sk_svgcanvas_create_with_writer (&bounds, writer.Handle)), writer);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use Create(SKRect, Stream) instead.")]
		public static SKCanvas Create (SKRect bounds, SKXmlWriter writer)
		{
			if (writer == null)
				throw new ArgumentNullException (nameof (writer));

			return SKObject.Referenced (SKCanvas.GetObject (SkiaApi.sk_svgcanvas_create_with_writer (&bounds, writer.Handle)), writer);
		}
	}
}
