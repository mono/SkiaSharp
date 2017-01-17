//
// Bindings for Skia's SVG features
//
// Author:
//   Matthew Leibowitz
//
// Copyright 2017 Xamarin Inc
//

using System;

namespace SkiaSharp
{
	public class SKSvgCanvas
	{
		private SKSvgCanvas ()
		{
		}
		
		public static SKCanvas Create (SKRect bounds, SKXmlWriter writer)
		{
			if (writer == null) {
				throw new ArgumentNullException (nameof (writer));
			}

			return SKObject.GetObject<SKCanvas> (SkiaApi.sk_svgcanvas_create (ref bounds, writer.Handle));
		}
	}
}
