#if __IOS__ || __MACOS__ || __TVOS__
using System;

#if __IOS__
namespace SkiaSharp.Views.iOS
#elif __MACOS__
namespace SkiaSharp.Views.Mac
#elif __TVOS__
namespace SkiaSharp.Views.tvOS
#endif
{
	public class SKPaintGraphiteSurfaceEventArgs : EventArgs
	{
		public SKPaintGraphiteSurfaceEventArgs (
			SKSurface surface,
			SKGraphiteBackendTexture backendTexture,
			SKGraphiteContext context,
			SKImageInfo info)
			: this (surface, backendTexture, context, info, info)
		{
		}

		public SKPaintGraphiteSurfaceEventArgs (
			SKSurface surface,
			SKGraphiteBackendTexture backendTexture,
			SKGraphiteContext context,
			SKImageInfo info,
			SKImageInfo rawInfo)
		{
			Surface = surface ?? throw new ArgumentNullException (nameof (surface));
			BackendTexture = backendTexture ?? throw new ArgumentNullException (nameof (backendTexture));
			Context = context ?? throw new ArgumentNullException (nameof (context));
			Info = info;
			RawInfo = rawInfo;
		}

		public SKSurface Surface { get; }

		public SKGraphiteBackendTexture BackendTexture { get; }

		public SKGraphiteContext Context { get; }

		public SKColorType ColorType => Info.ColorType;

		public SKImageInfo Info { get; }

		public SKImageInfo RawInfo { get; }
	}
}
#endif
