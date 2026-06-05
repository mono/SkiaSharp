#nullable disable

using System;

namespace SkiaSharp
{
	internal partial class SkiaApi
	{
#if __IOS__ || __TVOS__
		private const string SKIA = "@rpath/libSkiaSharp.framework/libSkiaSharp";
#else
		private const string SKIA = "libSkiaSharp";
#endif

		// The native-library compatibility gate. SkiaApi is the single type through which EVERY P/Invoke
		// flows, so an explicit static constructor here is guaranteed by the CLR to run before the first
		// call to any sk_* function — i.e. before the first byte crosses into native code — on every
		// interop configuration (USE_DELEGATES, USE_LIBRARY_IMPORT, direct DllImport) and every platform.
		//
		// This is deliberately NOT a [ModuleInitializer]: an assembly load must not eagerly P/Invoke, so
		// that a consumer can still configure native-library loading (a DllImportResolver, a custom search
		// path, NativeLibrary.SetDllImportResolver, …) before the first Skia call. The check is therefore
		// deferred to the first P/Invoke rather than assembly load, while still always preceding it.
		//
		// Defining this constructor strips the `beforefieldinit` flag from SkiaApi, which lets the JIT
		// require a type-init readiness check before accessing any SkiaApi static member. Benchmarking
		// showed that cost is masked by the P/Invoke transition itself (~0.35% on CoreCLR, statistically
		// zero on Mono/net48), so the hot path is unaffected.
		//
		// Re-entrancy: CheckNativeLibraryCompatible calls back into SkiaApi.sk_version_get_milestone /
		// sk_version_get_increment while this constructor is running. That is safe — the CLR's same-thread
		// type-initializer rule returns the in-progress SkiaApi type without re-running this body, and the
		// only static state the version path reads (the USE_DELEGATES Lazy<IntPtr> handle and its delegate
		// cache) is a field initializer, which the CLR runs before this body. Do NOT add a SkiaApi static
		// field that the version path depends on and that is assigned in this body AFTER the check below.
		static SkiaApi ()
		{
			SkiaSharpVersion.CheckNativeLibraryCompatible (true);
		}

#if USE_DELEGATES
		private static readonly Lazy<IntPtr> libSkiaSharpHandle =
			new Lazy<IntPtr> (() => LibraryLoader.LoadLocalLibrary<SkiaApi> (SKIA));

		private static T GetSymbol<T> (string name) where T : Delegate =>
			LibraryLoader.GetSymbolDelegate<T> (libSkiaSharpHandle.Value, name);
#endif
	}
}
