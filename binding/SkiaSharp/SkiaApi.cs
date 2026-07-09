#nullable disable

using System;

namespace SkiaSharp
{
	internal partial class SkiaApi
	{
		// The native-library compatibility gate.
		//
		// SkiaApi is the single internal type through which EVERY P/Invoke into libSkiaSharp flows.
		// Defining an explicit static constructor strips this type's `beforefieldinit` flag, so the CLR
		// guarantees this body runs before the first access to any SkiaApi static member — i.e. before
		// the first byte crosses into native code — on every interop configuration (USE_DELEGATES,
		// USE_LIBRARY_IMPORT, direct DllImport) and every platform. That covers ALL consumers, including
		// code that only touches non-singleton wrappers (SKBitmap, SKCanvas), structs (SKMatrix), or
		// static utility classes (SKGraphics): they all reach native only through SkiaApi. The check runs
		// once; a throw becomes a cached TypeInitializationException (inner = the InvalidOperationException
		// carrying the supported-version-range message) — the desired one-shot poison for an unusable lib.
		//
		// This is deliberately NOT a [ModuleInitializer]. A module initializer runs at ASSEMBLY LOAD, which
		// would force an eager P/Invoke before a consumer can configure native-library loading (registering
		// a DllImportResolver / NativeLibrary.SetDllImportResolver, or adjusting the search path). Placing
		// the gate in SkiaApi's static constructor defers it to the first P/Invoke — as late as possible —
		// while still always preceding any native call, so "set up the native binary, then use Skia later"
		// keeps working.
		//
		// Re-entrancy is safe. CheckNativeLibraryCompatible reaches native through
		// SkiaApi.sk_version_get_milestone / sk_version_get_increment while this constructor is still
		// running. The CLR's same-thread type-initializer rule returns the in-progress SkiaApi type without
		// re-running this body, and the only static state the version path reads (the USE_DELEGATES
		// Lazy<IntPtr> library handle and its delegate cache) is a field initializer, which the CLR runs
		// BEFORE this body. The version path touches only SkiaApi and LibraryLoader — never an SKObject — so
		// it cannot reintroduce the #3817 static-initializer cycle.
		//
		// Guard for future edits: do NOT add a SkiaApi static field that the version path depends on and
		// that is assigned in this body AFTER the check below — it would be read as null during the
		// re-entrant version call.
		static SkiaApi ()
		{
			SkiaSharpVersion.CheckNativeLibraryCompatible (true);
		}

#if __IOS__ || __TVOS__
		private const string SKIA = "@rpath/libSkiaSharp.framework/libSkiaSharp";
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
