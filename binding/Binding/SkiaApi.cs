namespace SkiaSharp
{
	internal partial class SkiaApi
	{
#if __TVOS__ && __UNIFIED__
		private const string SKIA = "@rpath/libSkiaSharp.framework/libSkiaSharp";
#elif __WATCHOS__ && __UNIFIED__
		private const string SKIA = "@rpath/libSkiaSharp.framework/libSkiaSharp";
#elif __IOS__ && __UNIFIED__
		private const string SKIA = "@rpath/libSkiaSharp.framework/libSkiaSharp";
#elif __ANDROID__
		private const string SKIA = "libSkiaSharp.so";
#elif __MACOS__
		private const string SKIA = "libSkiaSharp.dylib";
#elif __DESKTOP__
		private const string SKIA = "libSkiaSharp";
#elif WINDOWS_UWP
		private const string SKIA = "libSkiaSharp.dll";
#elif NET_STANDARD
		private const string SKIA = "libSkiaSharp";
#else
		private const string SKIA = "libSkiaSharp";
#endif
	}
}
