#nullable disable

using System;
using System.ComponentModel;
using System.IO;

namespace SkiaSharp
{
	/// <summary>A specialized <see cref="T:SkiaSharp.SKCanvas" /> which generates SVG commands from its draw calls.</summary>
	/// <remarks>The canvas may buffer some drawing calls, so the output is not guaranteed to be valid or complete until the canvas instance is deleted.</remarks>
	public unsafe class SKSvgCanvas
	{
		private SKSvgCanvas ()
		{
		}

		// Create

		/// <summary>Creates a new <see cref="T:SkiaSharp.SKCanvas" /> which generates SVG commands and writes them to the specified .NET stream.</summary>
		/// <param name="bounds">The initial SVG viewport (viewBox attribute on the root SVG element).</param>
		/// <param name="stream">The .NET stream to write the SVG data to.</param>
		/// <returns>Returns the new SVG canvas.</returns>
		/// <remarks>The canvas may buffer some drawing calls, so the output is not guaranteed to be valid or complete until the canvas instance is disposed. The stream must remain valid during the lifetime of the returned canvas.</remarks>
		public static SKCanvas Create (SKRect bounds, Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			var managed = new SKManagedWStream (stream);
			return SKObject.Owned (Create (bounds, managed), managed);
		}

		/// <summary>Creates a new <see cref="T:SkiaSharp.SKCanvas" /> which generates SVG commands and writes them to the specified stream.</summary>
		/// <param name="bounds">The initial SVG viewport (viewBox attribute on the root SVG element).</param>
		/// <param name="stream">The SkiaSharp stream to write the SVG data to.</param>
		/// <returns>Returns the new SVG canvas.</returns>
		/// <remarks>The canvas may buffer some drawing calls, so the output is not guaranteed to be valid or complete until the canvas instance is disposed. The stream must remain valid during the lifetime of the returned canvas.</remarks>
		public static SKCanvas Create (SKRect bounds, SKWStream stream)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			return SKObject.Referenced (SKCanvas.GetObject (SkiaApi.sk_svgcanvas_create_with_stream (&bounds, stream.Handle)), stream);
		}
	}
}
