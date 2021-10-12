using Microsoft.Extensions.Logging;
using Microsoft.Maui;

namespace SkiaSharp.Views.Maui.Handlers
{
	public partial class SKImageSourceService : ImageSourceService,
		IImageSourceService<ISKImageImageSource>,
		IImageSourceService<ISKBitmapImageSource>,
		IImageSourceService<ISKPixmapImageSource>,
		IImageSourceService<ISKPictureImageSource>
	{
		public SKImageSourceService()
			: this(null)
		{
		}
		public SKImageSourceService(ILogger? logger)
			: base(logger)
		{
		}
	}
}
