using System;
using Microsoft.AspNetCore.Html;

using static Microsoft.DotNet.Interactive.Formatting.PocketViewTags;

namespace SkiaSharp.DotNet.Interactive
{
	public static class RasterRenderer
	{
		public static IHtmlContent Render(this SKBitmap bitmap)
		{
			using var image = SKImage.FromBitmap(bitmap);
			return Render(image);
		}

		public static IHtmlContent Render(this SKPixmap pixmap)
		{
			using var image = SKImage.FromPixels(pixmap);
			return Render(image);
		}

		public static IHtmlContent Render(this SKPicture picture)
		{
			using var image = SKImage.FromPicture(picture, picture.CullRect.Size.ToSizeI());
			return Render(image);
		}

		public static IHtmlContent Render(this SKSurface surface)
		{
			using var image = surface.Snapshot();
			return Render(image);
		}

		public static IHtmlContent Render(this SKImage image)
		{
			using var data = image.Encode(SKEncodedImageFormat.Png, 100);
			return GetHtml(data);
		}

		private static IHtmlContent GetHtml(SKData data)
		{
			var base64 = Convert.ToBase64String(data.AsSpan());

			return div(
				img[src: "data:image/png;base64," + base64]);
		}
	}
}
