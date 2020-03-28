using System.IO;
using System.Linq;
using SkiaSharp;

namespace SkiaSharpTextOnPath
{
	class Program
	{
		static void Main(string[] args)
		{
			// crate a surface
			var info = new SKImageInfo(512, 512);
			using (var surface = SKSurface.Create(info))
			{
				// the the canvas and properties
				var canvas = surface.Canvas;

				// make sure the canvas is blank
				canvas.Clear(SKColors.SkyBlue);

				var paint = new SKPaint
				{
					IsAntialias = true,
					Style = SKPaintStyle.Fill,
					TextAlign = SKTextAlign.Center,
					TextSize = 24
				};

				// create a circular path
				using var path = SKPath.ParseSvgPathData("M 64 256 A 128 128 0 0 0 448 256 A 128 128 0 0 0 64 256");

				// draw the path
				paint.Color = SKColors.Black;
				canvas.DrawPath(path, paint);

				// draw text on the path, with various tweaks, saving each file.
				var alignments = new[] { SKTextAlign.Left, SKTextAlign.Center, SKTextAlign.Right };
				var adjustments = new[] { SKTextLengthAdjustment.SpacingOnly, SKTextLengthAdjustment.SpacingAndGlyphs };
				var hOffsets = new[] { 0f, -50f, 50f };
				var vOffsets = new[] { 0f, -10f, 10f };

				var text = @"The quick brown fox jumps over the lazy dog!";

				paint.Color = SKColors.White;

				foreach (var alignment in alignments)
				{
					foreach (var adjustment in adjustments)
					{
						foreach (var hOffset in hOffsets)
						{
							foreach (var vOffset in vOffsets)
							{
								canvas.DrawTextOnPath(text, path, hOffset, vOffset, paint, adjustment);

								// save the file
								var filename = $"output_${alignment}_${adjustment}_${hOffset}_${vOffset}.png";
								using var image = surface.Snapshot();
								using var data = image.Encode(SKEncodedImageFormat.Png, 100);
								using var stream = File.OpenWrite(filename);
								data.SaveTo(stream);
							}
						}
					}
				}
			}
		}
	}
}
