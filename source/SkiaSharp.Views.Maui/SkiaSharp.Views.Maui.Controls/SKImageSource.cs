using System.Threading.Tasks;

using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace SkiaSharp.Views.Maui.Controls
{
	/// <summary>
	/// Represents a <see cref="T:SkiaSharp.SKImage" /> image source.
	/// </summary>
	public sealed partial class SKImageImageSource : ImageSource, ISKImageImageSource
	{
		/// <summary>
		/// Identifies the <see cref="SKImageImageSource.Image" /> dependency property.
		/// </summary>
		public static readonly BindableProperty ImageProperty = BindableProperty.Create(nameof(Image), typeof(SKImage), typeof(SKImageImageSource), default(SKImage));

		/// <summary>
		/// Gets or sets the underlying <see cref="T:SkiaSharp.SKImage" /> of the image source.
		/// </summary>
		public SKImage Image
		{
			get { return (SKImage)GetValue(ImageProperty); }
			set { SetValue(ImageProperty, value); }
		}

		/// <summary>
		/// Request a cancel of the ImageSource loading.
		/// </summary>
		/// <returns>An awaitable Task with a result indicating if the Task was successfully cancelled.</returns>
		public override Task<bool> Cancel()
		{
			return Task.FromResult(false);
		}

		/// <summary>
		/// Allows implicit casting from a <see cref="T:SkiaSharp.SKImage" />.
		/// </summary>
		/// <param name="image">The image to use when creating the ImageSource.</param>
		/// <returns>Returns a new instance of <see cref="SKImageImageSource" /> with the <see cref="SKImageImageSource.Image" /> property set to the image.</returns>
		public static implicit operator SKImageImageSource(SKImage image)
		{
			return new SKImageImageSource
			{
				Image = image
			};
		}

		/// <summary>
		/// Allows implicit casting to a <see cref="T:SkiaSharp.SKImage" />.
		/// </summary>
		/// <param name="source">The ImageSource to retrieve the image from.</param>
		/// <returns>Returns the underlying <see cref="T:SkiaSharp.SKImage" /> of the ImageSource.</returns>
		public static implicit operator SKImage(SKImageImageSource source)
		{
			return source?.Image;
		}

		protected override void OnPropertyChanged(string propertyName = null)
		{
			if (propertyName == ImageProperty.PropertyName)
				OnSourceChanged();
			base.OnPropertyChanged(propertyName);
		}
	}

	/// <summary>
	/// Represents a <see cref="T:SkiaSharp.SKBitmap" /> image source.
	/// </summary>
	public sealed partial class SKBitmapImageSource : ImageSource, ISKBitmapImageSource
	{
		/// <summary>
		/// Identifies the <see cref="SKBitmapImageSource.Bitmap" /> dependency property.
		/// </summary>
		public static readonly BindableProperty BitmapProperty = BindableProperty.Create(nameof(Bitmap), typeof(SKBitmap), typeof(SKBitmapImageSource), default(SKBitmap));

		/// <summary>
		/// Gets or sets the underlying <see cref="T:SkiaSharp.SKBitmap" /> of the image source.
		/// </summary>
		public SKBitmap Bitmap
		{
			get { return (SKBitmap)GetValue(BitmapProperty); }
			set { SetValue(BitmapProperty, value); }
		}

		/// <summary>
		/// Request a cancel of the ImageSource loading.
		/// </summary>
		/// <returns>An awaitable Task with a result indicating if the Task was successfully cancelled.</returns>
		public override Task<bool> Cancel()
		{
			return Task.FromResult(false);
		}

		/// <summary>
		/// Allows implicit casting from a <see cref="T:SkiaSharp.SKBitmap" />.
		/// </summary>
		/// <param name="bitmap">The bitmap to use when creating the ImageSource.</param>
		/// <returns>Returns a new instance of <see cref="SKBitmapImageSource" /> with the <see cref="SKBitmapImageSource.Bitmap" /> property set to the bitmap.</returns>
		public static implicit operator SKBitmapImageSource(SKBitmap bitmap)
		{
			return new SKBitmapImageSource
			{
				Bitmap = bitmap
			};
		}

		/// <summary>
		/// Allows implicit casting to a <see cref="T:SkiaSharp.SKBitmap" />.
		/// </summary>
		/// <param name="source">The ImageSource to retrieve the bitmap from.</param>
		/// <returns>Returns the underlying <see cref="T:SkiaSharp.SKBitmap" /> of the ImageSource.</returns>
		public static implicit operator SKBitmap(SKBitmapImageSource source)
		{
			return source?.Bitmap;
		}

		protected override void OnPropertyChanged(string propertyName = null)
		{
			if (propertyName == BitmapProperty.PropertyName)
				OnSourceChanged();
			base.OnPropertyChanged(propertyName);
		}
	}

	/// <summary>
	/// Represents a <see cref="T:SkiaSharp.SKPixmap" /> image source.
	/// </summary>
	public sealed partial class SKPixmapImageSource : ImageSource, ISKPixmapImageSource
	{
		/// <summary>
		/// Identifies the <see cref="SKPixmapImageSource.Pixmap" /> dependency property.
		/// </summary>
		public static readonly BindableProperty PixmapProperty = BindableProperty.Create(nameof(Pixmap), typeof(SKPixmap), typeof(SKPixmapImageSource), default(SKPixmap));

		/// <summary>
		/// Gets or sets the underlying <see cref="T:SkiaSharp.SKPixmap" /> of the image source.
		/// </summary>
		public SKPixmap Pixmap
		{
			get { return (SKPixmap)GetValue(PixmapProperty); }
			set { SetValue(PixmapProperty, value); }
		}

		/// <summary>
		/// Request a cancel of the ImageSource loading.
		/// </summary>
		/// <returns>An awaitable Task with a result indicating if the Task was successfully cancelled.</returns>
		public override Task<bool> Cancel()
		{
			return Task.FromResult(false);
		}

		/// <summary>
		/// Allows implicit casting from a <see cref="T:SkiaSharp.SKPixmap" />.
		/// </summary>
		/// <param name="pixmap">The pixmap to use when creating the ImageSource.</param>
		/// <returns>Returns a new instance of <see cref="SKPixmapImageSource" /> with the <see cref="SKPixmapImageSource.Pixmap" /> property set to the bitmap.</returns>
		public static implicit operator SKPixmapImageSource(SKPixmap pixmap)
		{
			return new SKPixmapImageSource
			{
				Pixmap = pixmap
			};
		}

		/// <summary>
		/// Allows implicit casting to a <see cref="T:SkiaSharp.SKPixmap" />.
		/// </summary>
		/// <param name="source">The ImageSource to retrieve the pixmap from.</param>
		/// <returns>Returns the underlying <see cref="T:SkiaSharp.SKPixmap" /> of the ImageSource.</returns>
		public static implicit operator SKPixmap(SKPixmapImageSource source)
		{
			return source?.Pixmap;
		}

		protected override void OnPropertyChanged(string propertyName = null)
		{
			if (propertyName == PixmapProperty.PropertyName)
				OnSourceChanged();
			base.OnPropertyChanged(propertyName);
		}
	}

	/// <summary>
	/// Represents a <see cref="T:SkiaSharp.SKPicture" /> image source.
	/// </summary>
	public sealed partial class SKPictureImageSource : ImageSource, ISKPictureImageSource
	{
		/// <summary>
		/// Identifies the <see cref="SKPictureImageSource.Picture" /> dependency property.
		/// </summary>
		public static readonly BindableProperty PictureProperty = BindableProperty.Create(nameof(Picture), typeof(SKPicture), typeof(SKPictureImageSource), default(SKPicture));

		/// <summary>
		/// Identifies the <see cref="SKPictureImageSource.Dimensions" /> dependency property.
		/// </summary>
		public static readonly BindableProperty DimensionsProperty = BindableProperty.Create(nameof(Dimensions), typeof(SKSizeI), typeof(SKPictureImageSource), default(SKSizeI));

		/// <summary>
		/// Gets or sets the underlying <see cref="T:SkiaSharp.SKImage" /> of the image source.
		/// </summary>
		public SKPicture Picture
		{
			get { return (SKPicture)GetValue(PictureProperty); }
			set { SetValue(PictureProperty, value); }
		}

		/// <summary>
		/// Gets or sets the dimensions of the underlying picture.
		/// </summary>
		public SKSizeI Dimensions
		{
			get { return (SKSizeI)GetValue(DimensionsProperty); }
			set { SetValue(DimensionsProperty, value); }
		}

		/// <summary>
		/// Request a cancel of the ImageSource loading.
		/// </summary>
		/// <returns>An awaitable Task with a result indicating if the Task was successfully cancelled.</returns>
		public override Task<bool> Cancel()
		{
			return Task.FromResult(false);
		}

		/// <summary>
		/// Allows explicit casting to a <see cref="T:SkiaSharp.SKPicture" />.
		/// </summary>
		/// <param name="source">The ImageSource to retrieve the picture from.</param>
		/// <returns>Returns the underlying <see cref="T:SkiaSharp.SKPicture" /> of the ImageSource.</returns>
		public static explicit operator SKPicture(SKPictureImageSource source)
		{
			return source?.Picture;
		}

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
