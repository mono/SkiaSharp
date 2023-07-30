using System;

namespace HarfBuzzSharp
{
	internal unsafe partial class HarfBuzzApi
	{
#if __IOS__ || __TVOS__
		private const string HARFBUZZ = "@rpath/libHarfBuzzSharp.framework/libHarfBuzzSharp";
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
