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
	public sealed class SKGraphiteRenderFailedEventArgs : EventArgs
	{
		public SKGraphiteRenderFailedEventArgs (Exception exception)
		{
			Exception = exception ?? throw new ArgumentNullException (nameof (exception));
		}

		public Exception Exception { get; }
	}
}
#endif
