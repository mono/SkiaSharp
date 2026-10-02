using System.Threading.Tasks;

using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace SkiaSharp.Views.Maui.Controls
{
	/// <summary>An image source that wraps an <see cref="T:SkiaSharp.SKImage" /> for use with .NET MAUI image controls.</summary>
	/// <remarks>Use this class to display SkiaSharp images in MAUI <c>Image</c> controls. SKImage is immutable and can be safely shared across threads.</remarks>
	public sealed partial class SKImageImageSource : ImageSource, ISKImageImageSource
	{
		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Maui.Controls.SKImageImageSource" /> class.</summary>
		/// <remarks />
		public SKImageImageSource()
		{
		}

		/// <summary>Identifies the <see cref="P:SkiaSharp.Views.Maui.Controls.SKImageImageSource.Image" /> bindable property.</summary>
		/// <remarks />
		public static readonly BindableProperty ImageProperty = BindableProperty.Create(nameof(Image), typeof(SKImage), typeof(SKImageImageSource), default(SKImage));

		/// <summary>Gets or sets the underlying <see cref="T:SkiaSharp.SKImage" />.</summary>
		/// <value>The SkiaSharp image to display.</value>
		/// <remarks />
		public SKImage Image
		{
			get { return (SKImage)GetValue(ImageProperty); }
			set { SetValue(ImageProperty, value); }
		}

		/// <summary>Cancels any pending image loading operation.</summary>
		/// <returns>A task that returns <see langword="true" /> if the operation was cancelled; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public override Task<bool> Cancel()
		{
			return Task.FromResult(false);
		}

		/// <summary>Implicitly converts an <see cref="T:SkiaSharp.SKImage" /> to an <see cref="T:SkiaSharp.Views.Maui.Controls.SKImageImageSource" />.</summary>
		/// <param name="image">The image to convert.</param>
		/// <returns>A new image source wrapping the image.</returns>
		/// <remarks />
		public static implicit operator SKImageImageSource(SKImage image)
		{
			return new SKImageImageSource
			{
				Image = image
			};
		}

		/// <summary>Implicitly converts an <see cref="T:SkiaSharp.Views.Maui.Controls.SKImageImageSource" /> to an <see cref="T:SkiaSharp.SKImage" />.</summary>
		/// <param name="source">The image source to convert.</param>
		/// <returns>The underlying image.</returns>
		/// <remarks />
		public static implicit operator SKImage(SKImageImageSource source)
		{
			return source?.Image;
		}

		/// <summary>Called when a property value changes.</summary>
		/// <param name="propertyName">The name of the property that changed.</param>
		/// <remarks />
		protected override void OnPropertyChanged(string propertyName = null)
		{
			if (propertyName == ImageProperty.PropertyName)
				OnSourceChanged();
			base.OnPropertyChanged(propertyName);
		}
	}

	/// <summary>An image source that wraps an <see cref="T:SkiaSharp.SKBitmap" /> for use with .NET MAUI image controls.</summary>
	/// <remarks>Use this class to display SkiaSharp bitmaps in MAUI <c>Image</c> controls. Implicit conversions are provided for convenience.</remarks>
	public sealed partial class SKBitmapImageSource : ImageSource, ISKBitmapImageSource
	{
		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Maui.Controls.SKBitmapImageSource" /> class.</summary>
		/// <remarks />
		public SKBitmapImageSource()
		{
		}

		/// <summary>Identifies the <see cref="P:SkiaSharp.Views.Maui.Controls.SKBitmapImageSource.Bitmap" /> bindable property.</summary>
		/// <remarks />
		public static readonly BindableProperty BitmapProperty = BindableProperty.Create(nameof(Bitmap), typeof(SKBitmap), typeof(SKBitmapImageSource), default(SKBitmap));

		/// <summary>Gets or sets the underlying <see cref="T:SkiaSharp.SKBitmap" />.</summary>
		/// <value>The SkiaSharp bitmap to display.</value>
		/// <remarks />
		public SKBitmap Bitmap
		{
			get { return (SKBitmap)GetValue(BitmapProperty); }
			set { SetValue(BitmapProperty, value); }
		}

		/// <summary>Cancels any pending image loading operation.</summary>
		/// <returns>A task that returns <see langword="true" /> if the operation was cancelled; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public override Task<bool> Cancel()
		{
			return Task.FromResult(false);
		}

		/// <summary>Implicitly converts an <see cref="T:SkiaSharp.SKBitmap" /> to an <see cref="T:SkiaSharp.Views.Maui.Controls.SKBitmapImageSource" />.</summary>
		/// <param name="bitmap">The bitmap to convert.</param>
		/// <returns>A new image source wrapping the bitmap.</returns>
		/// <remarks />
		public static implicit operator SKBitmapImageSource(SKBitmap bitmap)
		{
			return new SKBitmapImageSource
			{
				Bitmap = bitmap
			};
		}

		/// <summary>Implicitly converts an <see cref="T:SkiaSharp.Views.Maui.Controls.SKBitmapImageSource" /> to an <see cref="T:SkiaSharp.SKBitmap" />.</summary>
		/// <param name="source">The image source to convert.</param>
		/// <returns>The underlying bitmap.</returns>
		/// <remarks />
		public static implicit operator SKBitmap(SKBitmapImageSource source)
		{
			return source?.Bitmap;
		}

		/// <summary>Called when a property value changes.</summary>
		/// <param name="propertyName">The name of the property that changed.</param>
		/// <remarks />
		protected override void OnPropertyChanged(string propertyName = null)
		{
			if (propertyName == BitmapProperty.PropertyName)
				OnSourceChanged();
			base.OnPropertyChanged(propertyName);
		}
	}

	/// <summary>An image source that wraps an <see cref="T:SkiaSharp.SKPixmap" /> for use with .NET MAUI image controls.</summary>
	/// <remarks>Use this class to display SkiaSharp pixmaps in MAUI <c>Image</c> controls. A pixmap provides direct access to pixel data and can be used for low-level image manipulation.</remarks>
	public sealed partial class SKPixmapImageSource : ImageSource, ISKPixmapImageSource
	{
		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Maui.Controls.SKPixmapImageSource" /> class.</summary>
		/// <remarks />
		public SKPixmapImageSource()
		{
		}

		/// <summary>Identifies the <see cref="P:SkiaSharp.Views.Maui.Controls.SKPixmapImageSource.Pixmap" /> bindable property.</summary>
		/// <remarks />
		public static readonly BindableProperty PixmapProperty = BindableProperty.Create(nameof(Pixmap), typeof(SKPixmap), typeof(SKPixmapImageSource), default(SKPixmap));

		/// <summary>Gets or sets the underlying <see cref="T:SkiaSharp.SKPixmap" />.</summary>
		/// <value>The SkiaSharp pixmap to display.</value>
		/// <remarks />
		public SKPixmap Pixmap
		{
			get { return (SKPixmap)GetValue(PixmapProperty); }
			set { SetValue(PixmapProperty, value); }
		}

		/// <summary>Cancels any pending image loading operation.</summary>
		/// <returns>A task that returns <see langword="true" /> if the operation was cancelled; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public override Task<bool> Cancel()
		{
			return Task.FromResult(false);
		}

		/// <summary>Implicitly converts an <see cref="T:SkiaSharp.SKPixmap" /> to an <see cref="T:SkiaSharp.Views.Maui.Controls.SKPixmapImageSource" />.</summary>
		/// <param name="pixmap">The pixmap to convert.</param>
		/// <returns>A new image source wrapping the pixmap.</returns>
		/// <remarks />
		public static implicit operator SKPixmapImageSource(SKPixmap pixmap)
		{
			return new SKPixmapImageSource
			{
				Pixmap = pixmap
			};
		}

		/// <summary>Implicitly converts an <see cref="T:SkiaSharp.Views.Maui.Controls.SKPixmapImageSource" /> to an <see cref="T:SkiaSharp.SKPixmap" />.</summary>
		/// <param name="source">The image source to convert.</param>
		/// <returns>The underlying pixmap.</returns>
		/// <remarks />
		public static implicit operator SKPixmap(SKPixmapImageSource source)
		{
			return source?.Pixmap;
		}

		/// <summary>Called when a property value changes.</summary>
		/// <param name="propertyName">The name of the property that changed.</param>
		/// <remarks />
		protected override void OnPropertyChanged(string propertyName = null)
		{
			if (propertyName == PixmapProperty.PropertyName)
				OnSourceChanged();
			base.OnPropertyChanged(propertyName);
		}
	}

	/// <summary>An image source that wraps an <see cref="T:SkiaSharp.SKPicture" /> for use with .NET MAUI image controls.</summary>
	/// <remarks>Use this class to display SkiaSharp pictures (recorded drawing commands) in MAUI <c>Image</c> controls. Pictures are resolution-independent and can be rendered at any size specified by <see cref="P:SkiaSharp.Views.Maui.Controls.SKPictureImageSource.Dimensions" />.</remarks>
	public sealed partial class SKPictureImageSource : ImageSource, ISKPictureImageSource
	{
		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Maui.Controls.SKPictureImageSource" /> class.</summary>
		/// <remarks />
		public SKPictureImageSource()
		{
		}

		/// <summary>Identifies the <see cref="P:SkiaSharp.Views.Maui.Controls.SKPictureImageSource.Picture" /> bindable property.</summary>
		/// <remarks />
		public static readonly BindableProperty PictureProperty = BindableProperty.Create(nameof(Picture), typeof(SKPicture), typeof(SKPictureImageSource), default(SKPicture));

		/// <summary>Identifies the <see cref="P:SkiaSharp.Views.Maui.Controls.SKPictureImageSource.Dimensions" /> bindable property.</summary>
		/// <remarks />
		public static readonly BindableProperty DimensionsProperty = BindableProperty.Create(nameof(Dimensions), typeof(SKSizeI), typeof(SKPictureImageSource), default(SKSizeI));

		/// <summary>Gets or sets the underlying <see cref="T:SkiaSharp.SKPicture" />.</summary>
		/// <value>The SkiaSharp picture to display.</value>
		/// <remarks />
		public SKPicture Picture
		{
			get { return (SKPicture)GetValue(PictureProperty); }
			set { SetValue(PictureProperty, value); }
		}

		/// <summary>Gets or sets the dimensions at which the picture should be rendered.</summary>
		/// <value>The size in pixels at which to render the picture.</value>
		/// <remarks>Since pictures are resolution-independent, this property specifies the output size when rasterizing the picture for display.</remarks>
		public SKSizeI Dimensions
		{
			get { return (SKSizeI)GetValue(DimensionsProperty); }
			set { SetValue(DimensionsProperty, value); }
		}

		/// <summary>Cancels any pending image loading operation.</summary>
		/// <returns>A task that returns <see langword="true" /> if the operation was cancelled; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public override Task<bool> Cancel()
		{
			return Task.FromResult(false);
		}

		/// <summary>Explicitly converts an <see cref="T:SkiaSharp.Views.Maui.Controls.SKPictureImageSource" /> to an <see cref="T:SkiaSharp.SKPicture" />.</summary>
		/// <param name="source">The image source to convert.</param>
		/// <returns>The underlying picture.</returns>
		/// <remarks />
		public static explicit operator SKPicture(SKPictureImageSource source)
		{
			return source?.Picture;
		}

		/// <summary>Called when a property value changes.</summary>
		/// <param name="propertyName">The name of the property that changed.</param>
		/// <remarks />
		protected override void OnPropertyChanged(string propertyName = null)
		{
			if (propertyName == PictureProperty.PropertyName)
				OnSourceChanged();
			else if (propertyName == DimensionsProperty.PropertyName)
				OnSourceChanged();
			base.OnPropertyChanged(propertyName);
		}
	}
}
