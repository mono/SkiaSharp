using System;

namespace SkiaSharp.Views.Forms
{
	public sealed partial class SKImageImageSource
	{
		static SKImageImageSource()
		{
			SKImageSourceHandler.EnsureRegistered(typeof(SKImageImageSource));
		}
	}

	public sealed partial class SKBitmapImageSource
	{
		static SKBitmapImageSource()
		{
			SKImageSourceHandler.EnsureRegistered(typeof(SKBitmapImageSource));
		}
	}

	public sealed partial class SKPixmapImageSource
	{
		static SKPixmapImageSource()
		{
			SKImageSourceHandler.EnsureRegistered(typeof(SKPixmapImageSource));
		}
	}

	public sealed partial class SKPictureImageSource
	{
		static SKPictureImageSource()
		{
			SKImageSourceHandler.EnsureRegistered(typeof(SKPictureImageSource));
		}
	}

	public sealed partial class SKImageSourceHandler
	{
		internal static void EnsureRegistered(Type type)
		{
			var registered = Xamarin.Forms.Internals.Registrar.Registered;
			if (registered.GetHandlerType(type) != null)
				return;
			registered.Register(type, typeof(SKImageSourceHandler));
		}
	}
}
