using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace SkiaSharpSample.Controllers;

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
	public IActionResult Get()
	{
		using var image = CreateImage("SkiaSharp");
		return File(image.ToArray(), "image/png");
	}

	// GET api/images/text
	[HttpGet("{text}")]
	public IActionResult Get(string text)
	{
		using var image = CreateImage(text ?? "SkiaSharp");
		return File(image.ToArray(), "image/png");
	}

	private SKData CreateImage(string text)
	{
		logger.LogInformation($"Creating image with text: {text}");

		// create a surface
		var info = new SKImageInfo(256, 256);
		using var surface = SKSurface.Create(info);

		// get the canvas and properties
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
		canvas.DrawText(text, coord, SKTextAlign.Center, font, paint);

		// retrieve the encoded image
		using var image = surface.Snapshot();
		return image.Encode(SKEncodedImageFormat.Png, 100);
	}
}
