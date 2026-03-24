using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace SkiaSharpSample.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ImagesController : ControllerBase
	{
		private readonly ILogger<ImagesController> logger;

		public ImagesController(ILogger<ImagesController> logger)
		{
			this.logger = logger;
		}

		// GET api/images
		[HttpGet]
		public ActionResult<IEnumerable<string>> Get()
		{
			using (var image = CreateImage("SkiaSharp"))
			{
				return File(image.ToArray(), "image/png");
			}
		}

		// GET api/image/text
		[HttpGet("{text}")]
		public ActionResult<string> Get(string text)
		{
			using (var image = CreateImage(text ?? "SkiaSharp"))
			{
				return File(image.ToArray(), "image/png");
			}
		}

		private SKData CreateImage(string text)
		{
			logger.LogInformation($"Creating image with text: {text}");

			// create a surface
			var info = new SKImageInfo(256, 256);
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
					TextSize = 24
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
