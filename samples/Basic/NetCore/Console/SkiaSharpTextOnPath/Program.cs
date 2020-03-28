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
				const float textSize = 80;

				// the the canvas and properties
				var canvas = surface.Canvas;

				var paint = new SKPaint
				{
					IsAntialias = true,
					TextAlign = SKTextAlign.Center,
					TextSize = textSize,
					StrokeWidth = 2
				};

				// create a circular path
				using var path = SKPath.ParseSvgPathData("M 64 256 A 128 128 0 0 0 448 256 A 128 128 0 0 0 64 256");

				// draw text on the path, with various tweaks, saving each file.
				var alignments = new[] { SKTextAlign.Left, SKTextAlign.Center, SKTextAlign.Right };
				var warpings = new[] { SKGlyphWarping.SpacingOnly, SKGlyphWarping.SpacingAndGlyphs };
				var hOffsets = new[] { 0f, -textSize, textSize };
				var vOffsets = new[] { 0f, textSize / 2, textSize };

				var text = @"The quick brown fox jumps over the lazy dog!";

				paint.Color = SKColors.White;

				foreach (var alignment in alignments)
				{
					foreach (var warping in warpings)
					{
						foreach (var hOffset in hOffsets)
						{
							foreach (var vOffset in vOffsets)
							{
								// make sure the canvas is blank
								canvas.Clear(SKColors.SkyBlue);

								// draw the path
								paint.Color = SKColors.Black;
								paint.Style = SKPaintStyle.Stroke;
								canvas.DrawPath(path, paint);

								// draw the text on the path
								paint.TextAlign = alignment;
								paint.GlyphWarping = warping;
								paint.Color = SKColors.White;
								paint.Style = SKPaintStyle.Fill;
								canvas.DrawTextOnPath(text, path, hOffset, vOffset, paint);

								// save the file
								var filename = $"output_{alignment}_{warping}_H{hOffset}_V{vOffset}.png";
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
