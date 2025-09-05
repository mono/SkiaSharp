#nullable disable

using System;
using System.ComponentModel;
using System.IO;

namespace SkiaSharp
{
	/// <summary>
	/// A specialized <see cref="SKCanvas" /> which generates SVG commands from its draw calls.
	/// </summary>
	/// <remarks>The canvas may buffer some drawing calls, so the output is not guaranteed to be valid or complete until the canvas instance is deleted.</remarks>
	public unsafe class SKSvgCanvas
	{
		private SKSvgCanvas ()
		{
		}

		// Create

		/// <param name="bounds"></param>
		/// <param name="stream"></param>
		public static SKCanvas Create (SKRect bounds, Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			var managed = new SKManagedWStream (stream);
			return SKObject.Owned (Create (bounds, managed), managed);
		}

		/// <param name="bounds"></param>
		/// <param name="stream"></param>
		public static SKCanvas Create (SKRect bounds, SKWStream stream)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			return SKObject.Referenced (SKCanvas.GetObject (SkiaApi.sk_svgcanvas_create_with_stream (&bounds, stream.Handle)), stream);
		}
	}
}
