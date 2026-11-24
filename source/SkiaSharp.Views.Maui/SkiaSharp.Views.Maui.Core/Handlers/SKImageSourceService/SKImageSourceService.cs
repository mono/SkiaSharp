using Microsoft.Extensions.Logging;
using Microsoft.Maui;

namespace SkiaSharp.Views.Maui.Handlers
{
	/// <summary>Provides an image source service that converts SkiaSharp image sources to platform-native images for use with MAUI image controls.</summary>
	/// <remarks>This service handles conversion of <see cref="T:SkiaSharp.Views.Maui.ISKBitmapImageSource" />, <see cref="T:SkiaSharp.Views.Maui.ISKImageImageSource" />, <see cref="T:SkiaSharp.Views.Maui.ISKPixmapImageSource" />, and <see cref="T:SkiaSharp.Views.Maui.ISKPictureImageSource" /> to platform-native image formats.</remarks>
	public partial class SKImageSourceService : ImageSourceService,
		IImageSourceService<ISKImageImageSource>,
		IImageSourceService<ISKBitmapImageSource>,
		IImageSourceService<ISKPixmapImageSource>,
		IImageSourceService<ISKPictureImageSource>
	{
		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Maui.Handlers.SKImageSourceService" /> class.</summary>
		/// <remarks />
		public SKImageSourceService()
			: this(null)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Maui.Handlers.SKImageSourceService" /> class with the specified logger.</summary>
		/// <param name="logger">The logger to use for diagnostic output, or <see langword="null" /> for no logging.</param>
		/// <remarks />
		public SKImageSourceService(ILogger? logger)
			: base(logger)
		{
		}
	}
}
