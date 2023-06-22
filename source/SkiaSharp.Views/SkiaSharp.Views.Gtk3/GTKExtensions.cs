using Gdk;

using GC = System.GC;

namespace SkiaSharp.Views.Gtk
{
	public static class GTKExtensions
	{
		// Point

		public static SKPointI ToSKPointI(this Point point)
		{
			return new SKPointI(point.X, point.Y);
		}

		public static Point ToPoint(this SKPointI point)
		{
			return new Point(point.X, point.Y);
		}

		// Rectangle

		public static SKRectI ToSKRectI(this Rectangle rect)
		{
			return new SKRectI(rect.Left, rect.Top, rect.Right, rect.Bottom);
		}

		public static Rectangle ToRect(this SKRectI rect)
		{
			return new Rectangle(rect.Left, rect.Top, rect.Right, rect.Bottom);
		}

		// Size

		public static SKSizeI ToSKSizeI(this Size size)
		{
			return new SKSizeI(size.Width, size.Height);
		}

		public static Size ToSize(this SKSizeI size)
		{
			return new Size(size.Width, size.Height);
		}

		// Color

		private const float ColorMultiplier = 65535f / 255f;

		public static SKColor ToSKColor(this Color color)
		{
			var r = color.Red / ColorMultiplier;
			var g = color.Green / ColorMultiplier;
			var b = color.Blue / ColorMultiplier;
			return new SKColor((byte)r, (byte)g, (byte)b);
		}

		public static Color ToColor(this SKColor color)
		{
			return new Color(color.Red, color.Green, color.Blue);
		}

		// Pixbuf

		public static Pixbuf ToPixbuf(this SKPicture picture, SKSizeI dimensions)
		{
			using (var image = SKImage.FromPicture(picture, dimensions))
			{
				return image.ToPixbuf();
			}
		}

		public static Pixbuf ToPixbuf(this SKImage skiaImage)
		{
			// TODO: maybe keep the same color types where we can, instead of just going to the platform default

			var info = new SKImageInfo(skiaImage.Width, skiaImage.Height);
			var pix = new Pixbuf(Colorspace.Rgb, true, 8, info.Width, info.Height);

			// copy
			using (var pixmap = new SKPixmap(info, pix.Pixels, pix.Rowstride))
			{
				skiaImage.ReadPixels(pixmap, 0, 0);
			}

			// swap R and B
			if (info.ColorType == SKColorType.Bgra8888)
			{
				SKSwizzle.SwapRedBlue(pix.Pixels, info.Width * info.Height);
			}

			return pix;
		}

		public static Pixbuf ToPixbuf(this SKBitmap skiaBitmap)
		{
			using (var pixmap = skiaBitmap.PeekPixels())
			using (var image = SKImage.FromPixels(pixmap))
			{
				var pixbuf = image.ToPixbuf();
				GC.KeepAlive(skiaBitmap);
				return pixbuf;
			}
		}

		public static Pixbuf ToPixbuf(this SKPixmap pixmap)
		{
			using (var image = SKImage.FromPixels(pixmap))
			{
				return image.ToPixbuf();
			}
		}

		public static SKBitmap ToSKBitmap(this Pixbuf pixbuf)
		{
			// TODO: maybe keep the same color types where we can, instead of just going to the platform default

			var info = new SKImageInfo(pixbuf.Width, pixbuf.Height);
			var skiaBitmap = new SKBitmap(info);
			using (var pixmap = skiaBitmap.PeekPixels())
			{
				pixbuf.ToSKPixmap(pixmap);
			}
			return skiaBitmap;
		}

		public static SKImage ToSKImage(this Pixbuf pixbuf)
		{
			// TODO: maybe keep the same color types where we can, instead of just going to the platform default

			var info = new SKImageInfo(pixbuf.Width, pixbuf.Height);
			var image = SKImage.Create(info);
			using (var pixmap = image.PeekPixels())
			{
				pixbuf.ToSKPixmap(pixmap);
			}
			return image;
		}

		public static void ToSKPixmap(this Pixbuf pixbuf, SKPixmap pixmap)
		{
			// TODO: maybe keep the same color types where we can, instead of just going to the platform default

			var info = new SKImageInfo(pixbuf.Width, pixbuf.Height);
			using (var temp = new SKPixmap(info, pixbuf.Pixels))
			{
				temp.ReadPixels(pixmap);

				if (info.ColorType == SKColorType.Bgra8888)
				{
					SKSwizzle.SwapRedBlue(pixmap.GetPixels(), info.Width * info.Height);
				}
			}
		}
	}
}
