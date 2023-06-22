using System.Threading.Tasks;

#if __MAUI__
using Microsoft.Maui;
using Microsoft.Maui.Controls;
#else
using Xamarin.Forms;
#endif

#if __MAUI__
namespace SkiaSharp.Views.Maui.Controls
#else
namespace SkiaSharp.Views.Forms
#endif
{
	public sealed partial class SKImageImageSource : ImageSource
	{
		public static readonly BindableProperty ImageProperty = BindableProperty.Create(nameof(Image), typeof(SKImage), typeof(SKImageImageSource), default(SKImage));

		public SKImage Image
		{
			get { return (SKImage)GetValue(ImageProperty); }
			set { SetValue(ImageProperty, value); }
		}

		public override Task<bool> Cancel()
		{
			return Task.FromResult(false);
		}

		public static implicit operator SKImageImageSource(SKImage image)
		{
			return new SKImageImageSource
			{
				Image = image
			};
		}

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

	public sealed partial class SKBitmapImageSource : ImageSource
	{
		public static readonly BindableProperty BitmapProperty = BindableProperty.Create(nameof(Bitmap), typeof(SKBitmap), typeof(SKBitmapImageSource), default(SKBitmap));

		public SKBitmap Bitmap
		{
			get { return (SKBitmap)GetValue(BitmapProperty); }
			set { SetValue(BitmapProperty, value); }
		}

		public override Task<bool> Cancel()
		{
			return Task.FromResult(false);
		}

		public static implicit operator SKBitmapImageSource(SKBitmap bitmap)
		{
			return new SKBitmapImageSource
			{
				Bitmap = bitmap
			};
		}

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

	public sealed partial class SKPixmapImageSource : ImageSource
	{
		public static readonly BindableProperty PixmapProperty = BindableProperty.Create(nameof(Pixmap), typeof(SKPixmap), typeof(SKPixmapImageSource), default(SKPixmap));

		public SKPixmap Pixmap
		{
			get { return (SKPixmap)GetValue(PixmapProperty); }
			set { SetValue(PixmapProperty, value); }
		}

		public override Task<bool> Cancel()
		{
			return Task.FromResult(false);
		}

		public static implicit operator SKPixmapImageSource(SKPixmap pixmap)
		{
			return new SKPixmapImageSource
			{
				Pixmap = pixmap
			};
		}

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

	public sealed partial class SKPictureImageSource : ImageSource
	{
		public static readonly BindableProperty PictureProperty = BindableProperty.Create(nameof(Picture), typeof(SKPicture), typeof(SKPictureImageSource), default(SKPicture));

		public static readonly BindableProperty DimensionsProperty = BindableProperty.Create(nameof(Dimensions), typeof(SKSizeI), typeof(SKPictureImageSource), default(SKSizeI));

		public SKPicture Picture
		{
			get { return (SKPicture)GetValue(PictureProperty); }
			set { SetValue(PictureProperty, value); }
		}

		public SKSizeI Dimensions
		{
			get { return (SKSizeI)GetValue(DimensionsProperty); }
			set { SetValue(DimensionsProperty, value); }
		}

		public override Task<bool> Cancel()
		{
			return Task.FromResult(false);
		}

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
