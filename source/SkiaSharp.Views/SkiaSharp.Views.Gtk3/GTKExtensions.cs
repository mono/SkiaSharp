using Gdk;

using GC = System.GC;

namespace SkiaSharp.Views.Gtk
{
	/// <summary>Provides extension methods for converting between SkiaSharp types and GTK types.</summary>
	/// <remarks />
	public static class GTKExtensions
	{
		// Point

		/// <summary>Converts a <see cref="T:Gdk.Point" /> to an <see cref="T:SkiaSharp.SKPointI" />.</summary>
		/// <param name="point">The GDK point to convert.</param>
		/// <returns>The converted SkiaSharp point.</returns>
		/// <remarks />
		public static SKPointI ToSKPointI(this Point point)
		{
			return new SKPointI(point.X, point.Y);
		}

		/// <summary>Converts an <see cref="T:SkiaSharp.SKPointI" /> to a <see cref="T:Gdk.Point" />.</summary>
		/// <param name="point">The SkiaSharp point to convert.</param>
		/// <returns>The converted GDK point.</returns>
		/// <remarks />
		public static Point ToPoint(this SKPointI point)
		{
			return new Point(point.X, point.Y);
		}

		// Rectangle

		/// <summary>Converts a <see cref="T:Gdk.Rectangle" /> to an <see cref="T:SkiaSharp.SKRectI" />.</summary>
		/// <param name="rect">The GDK rectangle to convert.</param>
		/// <returns>The converted SkiaSharp integer rectangle.</returns>
		/// <remarks />
		public static SKRectI ToSKRectI(this Rectangle rect)
		{
			return new SKRectI(rect.Left, rect.Top, rect.Right, rect.Bottom);
		}

		/// <summary>Converts an <see cref="T:SkiaSharp.SKRectI" /> to a <see cref="T:Gdk.Rectangle" />.</summary>
		/// <param name="rect">The SkiaSharp rectangle to convert.</param>
		/// <returns>The converted GDK rectangle.</returns>
		/// <remarks />
		public static Rectangle ToRect(this SKRectI rect)
		{
			return new Rectangle(rect.Left, rect.Top, rect.Right, rect.Bottom);
		}

		// Size

		/// <summary>Converts a <see cref="T:Gdk.Size" /> to an <see cref="T:SkiaSharp.SKSizeI" />.</summary>
		/// <param name="size">The GDK size to convert.</param>
		/// <returns>The converted SkiaSharp size.</returns>
		/// <remarks />
		public static SKSizeI ToSKSizeI(this Size size)
		{
			return new SKSizeI(size.Width, size.Height);
		}

		/// <summary>Converts an <see cref="T:SkiaSharp.SKSizeI" /> to a <see cref="T:Gdk.Size" />.</summary>
		/// <param name="size">The SkiaSharp size to convert.</param>
		/// <returns>The converted GDK size.</returns>
		/// <remarks />
		public static Size ToSize(this SKSizeI size)
		{
			return new Size(size.Width, size.Height);
		}

		// Color

		private const float ColorMultiplier = 65535f / 255f;

		/// <summary>Converts a <see cref="T:Gdk.Color" /> to an <see cref="T:SkiaSharp.SKColor" />.</summary>
		/// <param name="color">The GDK color to convert.</param>
		/// <returns>The converted SkiaSharp color.</returns>
		/// <remarks />
		public static SKColor ToSKColor(this Color color)
		{
			var r = color.Red / ColorMultiplier;
			var g = color.Green / ColorMultiplier;
			var b = color.Blue / ColorMultiplier;
			return new SKColor((byte)r, (byte)g, (byte)b);
		}

		/// <summary>Converts a <see cref="T:SkiaSharp.SKColor" /> to a <see cref="T:Gdk.Color" />.</summary>
		/// <param name="color">The SkiaSharp color to convert.</param>
		/// <returns>The converted GDK color.</returns>
		/// <remarks />
		public static Color ToColor(this SKColor color)
		{
			return new Color(color.Red, color.Green, color.Blue);
		}

		// Pixbuf

		/// <summary>Converts an <see cref="T:SkiaSharp.SKPicture" /> to a <see cref="T:Gdk.Pixbuf" /> with the specified dimensions.</summary>
		/// <param name="picture">The SkiaSharp picture to convert.</param>
		/// <param name="dimensions">The dimensions for the resulting pixbuf.</param>
		/// <returns>The converted GDK pixbuf.</returns>
		/// <remarks />
		public static Pixbuf ToPixbuf(this SKPicture picture, SKSizeI dimensions)
		{
			using (var image = SKImage.FromPicture(picture, dimensions))
			{
				return image.ToPixbuf();
			}
		}

		/// <summary>Converts an <see cref="T:SkiaSharp.SKImage" /> to a <see cref="T:Gdk.Pixbuf" />.</summary>
		/// <param name="skiaImage">The SkiaSharp image to convert.</param>
		/// <returns>The converted GDK pixbuf.</returns>
		/// <remarks />
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

		/// <summary>Converts an <see cref="T:SkiaSharp.SKBitmap" /> to a <see cref="T:Gdk.Pixbuf" />.</summary>
		/// <param name="skiaBitmap">The SkiaSharp bitmap to convert.</param>
		/// <returns>The converted GDK pixbuf.</returns>
		/// <remarks />
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

		/// <summary>Converts an <see cref="T:SkiaSharp.SKPixmap" /> to a <see cref="T:Gdk.Pixbuf" />.</summary>
		/// <param name="pixmap">The SkiaSharp pixmap to convert.</param>
		/// <returns>The converted GDK pixbuf.</returns>
		/// <remarks />
		public static Pixbuf ToPixbuf(this SKPixmap pixmap)
		{
			using (var image = SKImage.FromPixels(pixmap))
			{
				return image.ToPixbuf();
			}
		}

		/// <summary>Converts a <see cref="T:Gdk.Pixbuf" /> to an <see cref="T:SkiaSharp.SKBitmap" />.</summary>
		/// <param name="pixbuf">The GDK pixbuf to convert.</param>
		/// <returns>The converted SkiaSharp bitmap.</returns>
		/// <remarks />
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

		/// <summary>Converts a <see cref="T:Gdk.Pixbuf" /> to an <see cref="T:SkiaSharp.SKImage" />.</summary>
		/// <param name="pixbuf">The GDK pixbuf to convert.</param>
		/// <returns>The converted SkiaSharp image.</returns>
		/// <remarks />
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

		/// <summary>Copies the pixel data from a <see cref="T:Gdk.Pixbuf" /> into an existing <see cref="T:SkiaSharp.SKPixmap" />.</summary>
		/// <param name="pixbuf">The GDK pixbuf to convert.</param>
		/// <param name="pixmap">The destination SkiaSharp pixmap to copy the pixel data into.</param>
		/// <remarks />
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
