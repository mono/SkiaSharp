using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.Platform.Tizen;
using Tizen.NUI.BaseComponents;

namespace SkiaSharp.Views.Maui.Controls.Compatibility
{
	public sealed class SKImageSourceHandler : IImageSourceHandler
	{
		private StreamImageSourceHandler handler = new StreamImageSourceHandler();

		public Task<bool> LoadImageAsync(ImageView image, ImageSource imageSource, CancellationToken cancelationToken = default(CancellationToken))
		{
			ImageSource newSource = null;

			switch (imageSource)
			{
				case SKImageImageSource imageImageSource:
					newSource = ImageSource.FromStream(() => ToStream(imageImageSource.Image));
					break;
				case SKBitmapImageSource bitmapImageSource:
					newSource = ImageSource.FromStream(() => ToStream(SKImage.FromBitmap(bitmapImageSource.Bitmap)));
					break;
				case SKPixmapImageSource pixmapImageSource:
					newSource = ImageSource.FromStream(() => ToStream(SKImage.FromPixels(pixmapImageSource.Pixmap)));
					break;
				case SKPictureImageSource pictureImageSource:
					newSource = ImageSource.FromStream(() => ToStream(SKImage.FromPicture(pictureImageSource.Picture, pictureImageSource.Dimensions)));
					break;
			}

			return handler.LoadImageAsync(image, newSource, cancelationToken);
		}

		private static Stream ToStream(SKImage skiaImage)
		{
			return skiaImage.Encode().AsStream();
		}
	}
}
