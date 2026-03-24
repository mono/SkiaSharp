using System;
using System.IO;

using SkiaSharp;

namespace SkiaSharpSample
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");
			Console.WriteLine("Your platform color type is " + SKImageInfo.PlatformColorType);

			// crate a surface
			var info = new SKImageInfo(256, 256);
			using var surface = SKSurface.Create(info);

			// the the canvas and properties
			var canvas = surface.Canvas;

			// make sure the canvas is blank
			canvas.Clear(SKColors.White);

			// draw some text
			using var paint = new SKPaint
			{
				Color = SKColors.Black,
				IsAntialias = true,
				Style = SKPaintStyle.Fill
			};
			using var font = new SKFont
			{
				Size = 24
			};
			var coord = new SKPoint(info.Width / 2, (info.Height + font.Size) / 2);
			canvas.DrawText("SkiaSharp", coord, SKTextAlign.Center, font, paint);

			// save the file
			using var image = surface.Snapshot();
			using var data = image.Encode(SKEncodedImageFormat.Png, 100);
			using var stream = File.OpenWrite("output.png");
			data.SaveTo(stream);
		}
	}
}
