using System;

namespace SkiaSharp.Views.Forms
{
	internal class Registrar
	{
		internal static void EnsureRegistered(Type type, Type handler)
		{
			var registered = Xamarin.Forms.Internals.Registrar.Registered;
			if (registered.GetHandlerType(type) != null)
				return;
			registered.Register(type, handler);
		}
	}

	public sealed partial class SKImageImageSource
	{
		static SKImageImageSource() =>
			Registrar.EnsureRegistered(typeof(SKImageImageSource), typeof(SKImageSourceHandler));
	}

	public sealed partial class SKBitmapImageSource
	{
		static SKBitmapImageSource() =>
			Registrar.EnsureRegistered(typeof(SKBitmapImageSource), typeof(SKImageSourceHandler));
	}

	public sealed partial class SKPixmapImageSource
	{
		static SKPixmapImageSource() =>
			Registrar.EnsureRegistered(typeof(SKPixmapImageSource), typeof(SKImageSourceHandler));
	}

	public sealed partial class SKPictureImageSource
	{
		static SKPictureImageSource() =>
			Registrar.EnsureRegistered(typeof(SKPictureImageSource), typeof(SKImageSourceHandler));
	}

	public partial class SKCanvasView
	{
		static SKCanvasView() =>
			Registrar.EnsureRegistered(typeof(SKCanvasView), typeof(SKCanvasViewRenderer));
	}

	public partial class SKGLView
	{
		static SKGLView() =>
			Registrar.EnsureRegistered(typeof(SKGLView), typeof(SKGLViewRenderer));
	}
}
