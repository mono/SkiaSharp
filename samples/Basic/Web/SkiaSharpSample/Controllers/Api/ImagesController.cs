using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using SkiaSharp;
using Microsoft.AspNetCore.Mvc;

namespace SkiaSharpSample.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class ImagesController : ControllerBase
	{
		// GET api/images
		[HttpGet]
		public IActionResult Get()
		{
			using var image = CreateImage("SkiaSharp");
			return File(image.ToArray(), "image/png");
		}

		// GET api/images/text
		[HttpGet("{id?}")]
		public IActionResult Get(string id)
		{
			using var image = CreateImage(id ?? "SkiaSharp");
			return File(image.ToArray(), "image/png");
		}

		private SKData CreateImage(string text)
		{
			// create a surface
			var info = new SKImageInfo(512, 512);
			using (var surface = SKSurface.Create(info))
			{
				// the the canvas and properties
				var canvas = surface.Canvas;

				// make sure the canvas is blank
				canvas.Clear(SKColors.White);

				// draw some text
				var paint = new SKPaint
				{
					Color = SKColors.Black,
					IsAntialias = true,
					Style = SKPaintStyle.Fill,
					TextAlign = SKTextAlign.Center,
					TextSize = 48
				};
				var coord = new SKPoint(info.Width / 2, (info.Height + paint.TextSize) / 2);
				canvas.DrawText(text, coord, paint);

				// retrieve the encoded image
				using (var image = surface.Snapshot())
				{
					return image.Encode(SKEncodedImageFormat.Png, 100);
				}
			}
		}
	}
}
