using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using SkiaSharp;

namespace SkiaSharpSample.Controllers
{
	public class ImagesController : ApiController
	{
		// GET api/images
		[HttpGet]
		public HttpResponseMessage Get()
		{
			using (var image = CreateImage("SkiaSharp"))
			{
				var response = new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new ByteArrayContent(image.ToArray())
				};

				response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");

				return response;
			}
		}

		// GET api/images/text
		[HttpGet]
		public HttpResponseMessage Get(string id)
		{
			using (var image = CreateImage(id ?? "SkiaSharp"))
			{
				var response = new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new ByteArrayContent(image.ToArray())
				};

				response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");

				return response;
			}
		}

		private SKData CreateImage(string text)
		{
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
