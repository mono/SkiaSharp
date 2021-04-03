using System;

namespace HarfBuzzSharp
{
	internal unsafe partial class HarfBuzzApi
	{
#if __TVOS__ && __UNIFIED__
		private const string HARFBUZZ = "@rpath/libHarfBuzzSharp.framework/libHarfBuzzSharp";
#elif __WATCHOS__ && __UNIFIED__
		private const string HARFBUZZ = "@rpath/libHarfBuzzSharp.framework/libHarfBuzzSharp";
#elif __IOS__ && __UNIFIED__
		private const string HARFBUZZ = "@rpath/libHarfBuzzSharp.framework/libHarfBuzzSharp";
#elif __ANDROID__
		private const string HARFBUZZ = "libHarfBuzzSharp.so";
#elif __MACOS__
		private const string HARFBUZZ = "libHarfBuzzSharp.dylib";
#elif __DESKTOP__
		private const string HARFBUZZ = "libHarfBuzzSharp";
#elif WINDOWS_UWP
		private const string HARFBUZZ = "libHarfBuzzSharp.dll";
#elif NET_STANDARD
		private const string HARFBUZZ = "libHarfBuzzSharp";
#else
		private const string HARFBUZZ = "libHarfBuzzSharp";
#endif

#if USE_DELEGATES
		private static readonly Lazy<IntPtr> libHarfBuzzSharpHandle =
			new Lazy<IntPtr> (() => LibraryLoader.LoadLocalLibrary<HarfBuzzApi> (HARFBUZZ));

		private static T GetSymbol<T> (string name) where T : Delegate =>
			LibraryLoader.GetSymbolDelegate<T> (libHarfBuzzSharpHandle.Value, name);
#endif
	}
}
