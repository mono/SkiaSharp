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
			var info = new SKImageInfo (1024, 768, SKColorType.Rgba_8888, SKAlphaType.Opaque);
			var buffer = new byte [1024 * 768 * 4];

			unsafe {
				fixed (byte * f = &buffer[0]) {	
					using (var surface = new SKSurface (info, (IntPtr) f, 1024 * 4)) {
						var canvas = surface.Canvas;
						var paint = new SKPaint ();
						canvas.DrawRect (new SKRect (10, 10, 1024, 758), paint);
					}
				}
			}
			File.WriteAllBytes ("/tmp/path", buffer);

		}
	}
}