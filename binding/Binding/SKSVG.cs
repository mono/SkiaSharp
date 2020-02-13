using System;

namespace SkiaSharp
{
	public unsafe class SKSvgCanvas
	{
		private SKSvgCanvas ()
		{
		}
		
		public static SKCanvas Create (SKRect bounds, SKXmlWriter writer)
		{
			if (writer == null) {
				throw new ArgumentNullException (nameof (writer));
			}

			return SKObject.GetObject<SKCanvas> (SkiaApi.sk_svgcanvas_create (&bounds, writer.Handle));
		}
	}
}
