using Microsoft.Maui;

namespace SkiaSharp.Views.Maui
{
	/// <summary>Defines the interface for an image source backed by an <see cref="T:SkiaSharp.SKImage" />.</summary>
	/// <remarks>This interface is implemented by <see cref="T:SkiaSharp.Views.Maui.Controls.SKImageImageSource" /> and used by image source handlers to convert SkiaSharp images to platform-native images.</remarks>
	public interface ISKImageImageSource : IImageSource
	{
		/// <summary>Gets the underlying SkiaSharp image.</summary>
		/// <value>The <see cref="T:SkiaSharp.SKImage" /> that provides the image data.</value>
		/// <remarks>The image is converted to a platform-native image format when the image source is used with MAUI image controls. Unlike <see cref="T:SkiaSharp.SKBitmap" />, <see cref="T:SkiaSharp.SKImage" /> is immutable and may be GPU-backed.</remarks>
		SKImage Image { get; }
	}

	/// <summary>Defines the interface for an image source backed by an <see cref="T:SkiaSharp.SKBitmap" />.</summary>
	/// <remarks>This interface is implemented by <see cref="T:SkiaSharp.Views.Maui.Controls.SKBitmapImageSource" /> and used by image source handlers to convert SkiaSharp bitmaps to platform-native images.</remarks>
	public interface ISKBitmapImageSource : IImageSource
	{
		/// <summary>Gets the underlying SkiaSharp bitmap.</summary>
		/// <value>The <see cref="T:SkiaSharp.SKBitmap" /> that provides the image data.</value>
		/// <remarks>The bitmap is converted to a platform-native image format when the image source is used with MAUI image controls.</remarks>
		SKBitmap Bitmap { get; }
	}

	/// <summary>Defines the interface for an image source backed by an <see cref="T:SkiaSharp.SKPixmap" />.</summary>
	/// <remarks>This interface is implemented by <see cref="T:SkiaSharp.Views.Maui.Controls.SKPixmapImageSource" /> and used by image source handlers to convert SkiaSharp pixmaps to platform-native images.</remarks>
	public interface ISKPixmapImageSource : IImageSource
	{
		/// <summary>Gets the underlying SkiaSharp pixmap.</summary>
		/// <value>The <see cref="T:SkiaSharp.SKPixmap" /> that provides the image data.</value>
		/// <remarks>The pixmap is converted to a platform-native image format when the image source is used with MAUI image controls. An <see cref="T:SkiaSharp.SKPixmap" /> provides direct access to pixel data in memory.</remarks>
		SKPixmap Pixmap { get; }
	}

	/// <summary>Defines the interface for an image source backed by an <see cref="T:SkiaSharp.SKPicture" />.</summary>
	/// <remarks>This interface is implemented by <see cref="T:SkiaSharp.Views.Maui.Controls.SKPictureImageSource" /> and used by image source handlers to convert SkiaSharp pictures to platform-native images. Since pictures are resolution-independent, the <see cref="P:SkiaSharp.Views.Maui.ISKPictureImageSource.Dimensions" /> property specifies the target rasterization size.</remarks>
	public interface ISKPictureImageSource : IImageSource
	{
		/// <summary>Gets the underlying SkiaSharp picture.</summary>
		/// <value>The <see cref="T:SkiaSharp.SKPicture" /> that contains the recorded drawing commands.</value>
		/// <remarks>The picture is rasterized to the size specified by <see cref="P:SkiaSharp.Views.Maui.ISKPictureImageSource.Dimensions" /> when the image source is used with MAUI image controls.</remarks>
		SKPicture Picture { get; }

		/// <summary>Gets the dimensions to use when rasterizing the picture.</summary>
		/// <value>The target size in pixels for the rasterized image.</value>
		/// <remarks>Since <see cref="T:SkiaSharp.SKPicture" /> is resolution-independent, this property specifies the pixel dimensions to use when converting the picture to a bitmap for display.</remarks>
		SKSizeI Dimensions { get; }
	}
}
