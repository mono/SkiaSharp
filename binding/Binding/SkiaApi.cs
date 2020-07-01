using System;

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
#elif WINDOWS_UWP
		private const string SKIA = "libSkiaSharp.dll";
#elif __TIZEN__
		private const string SKIA = "libSkiaSharp.so";
#else
		private const string SKIA = "libSkiaSharp";
#endif

#if USE_DELEGATES
		private static readonly Lazy<IntPtr> libSkiaSharpHandle =
			new Lazy<IntPtr> (() => LibraryLoader.LoadLocalLibrary<SkiaApi> (SKIA));

		private static T GetSymbol<T> (string name) where T : Delegate =>
			LibraryLoader.GetSymbolDelegate<T> (libSkiaSharpHandle.Value, name);
#endif
	}
}
