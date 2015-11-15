//
// Shows the basics of using Skia from C# 	
//
// Author:
//   Miguel de Icaza
//
// Copyright 2015 Xamarin Inc
//

using System;
using SkiaSharp;
using System.Runtime.InteropServices;
using System.IO;

namespace Driver
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			if (Marshal.SizeOf<IntPtr> () == 4) {
				Console.Error.WriteLine ("Need 64 bit runtime");
				return;
			}
			var info = new SKImageInfo (1024, 768, SKColorType.Rgba_8888, SKAlphaType.Opaque);

			using (var surface = SKSurface.Create (info)) {
				var canvas = surface.Canvas;
				var paint = new SKPaint ();
				var r = new SKRect (0, 0, 64, 768);
					
				for (int x = 0; x <= 1024; x += 16) {
					
					paint.Color = new SKColor ((byte)(x/16), 0, 0);
					canvas.DrawRect (r, paint);
					r.Left += 16;
					r.Right += 16;
				}

				var tf = SKTypeface.FromFamilyName ("Consolas");
				ushort[] glyphs;
				var n = tf.CharsToGlyphs ("Hello world", out glyphs);
			
				canvas.DrawPoints (SKPointMode.Lines, new SKPoint[] { new SKPoint (0, 0), new SKPoint (1024, 768) }, paint);
				canvas.DrawLine (0, 768, 1024, 0, paint);

				paint.TextSize = 80;
				canvas.DrawText ("Hello", 100, 100, paint);
				paint.Typeface = tf;
				canvas.DrawText ("Hello", 100, 150, paint);
				using (var output = File.Create ("/tmp/file.png"))
					surface.Snapshot ().Encode ().SaveTo (output);

			}
		}
	}
}