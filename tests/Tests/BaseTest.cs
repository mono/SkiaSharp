using System;
using Xunit;

namespace SkiaSharp.Tests
{
	public abstract class BaseTest
	{
		protected static bool IsLinux = TestConfig.Current.IsLinux;
		protected static bool IsMac = TestConfig.Current.IsMac;
		protected static bool IsUnix = TestConfig.Current.IsUnix;
		protected static bool IsWindows = TestConfig.Current.IsWindows;

		protected static string[] UnicodeFontFamilies => TestConfig.Current.UnicodeFontFamilies;
		protected static string DefaultFontFamily => TestConfig.Current.DefaultFontFamily;

		public static string PathRoot => TestConfig.Current.PathRoot;
		public static string PathToFonts => TestConfig.Current.PathToFonts;
		public static string PathToImages => TestConfig.Current.PathToImages;

		private readonly ITestOutputHelper _output;

		public BaseTest()
		{
		}

		public BaseTest(ITestOutputHelper output)
		{
			_output = output;
		}

		protected void WriteOutput(string message)
		{
			Assert.True(_output is not null, "Output writer was null, you should be using the constructor that accepts an ITestOutputHelper.");

			_output.WriteLine(message);
		}

		public static void CollectGarbage()
		{
			// Loop so multi-level finalization chains fully drain. Collecting one
			// wrapper can release a managed reference it held to another (e.g. an
			// SKDocument releasing its SKWStream only when the document itself is
			// finalized); that second object becomes collectable *during* the first
			// round's finalization, so it survives a single Collect/WaitForPending-
			// Finalizers and is only reclaimed on a subsequent round. A single pass
			// therefore makes GC-lifetime tests (e.g. StreamIsNotCollectedPrematurely)
			// intermittently fail depending on when an earlier implicit GC happened.
			for (var i = 0; i < 4; i++)
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();
			}
		}

		protected static bool IsAndroid =>
#if NET5_0_OR_GREATER
			OperatingSystem.IsAndroid();
#else
			false;
#endif

		protected static bool IsIOS =>
#if NET5_0_OR_GREATER
			OperatingSystem.IsIOS();
#else
			false;
#endif

		protected static bool IsMacCatalyst =>
#if NET5_0_OR_GREATER
			OperatingSystem.IsMacCatalyst();
#else
			false;
#endif

		protected static bool IsBrowser =>
#if NET5_0_OR_GREATER
			OperatingSystem.IsBrowser();
#else
			false;
#endif

		protected static void SkipOnMono(string reason = "Mono does not guarantee finalizers are invoked immediately")
		{
			Assert.SkipWhen(IsAndroid || IsIOS || IsMacCatalyst || IsBrowser, reason);
		}

		protected static void SkipOnNonWindows(string reason = "Exceptions cannot be thrown in native delegates on non-Windows platforms")
		{
			Assert.SkipWhen(!IsWindows, reason);
		}

		protected static void SkipOnPlatform(bool condition, string reason)
		{
			Assert.SkipWhen(condition, reason);
		}

		// True when the platform has no system font manager that can enumerate or match fonts.
		// WASM has no font manager; the NoDependencies Linux builds (skia_use_fontconfig=false) and
		// Windows Nano Server have an "empty" font manager that reports one empty family and matches
		// nothing. Probing a basic Latin character ('a') detects all three (guard IsBrowser first so
		// we never call into a non-existent manager on WASM).
		protected static bool HasNoSystemFontManager =>
			IsBrowser || SkiaSharp.SKFontManager.Default.MatchCharacter('a') is null;

		// True when there is no usable default typeface. The NoDependencies and Nano Server builds
		// default to an empty typeface (no glyphs). WASM keeps a single embedded default font, so it
		// is NOT considered fontless here.
		protected static bool HasNoDefaultFont =>
			SkiaSharp.SKTypeface.Default.IsEmpty;

		// Skip tests that need the system font manager to enumerate or match fonts.
		protected static void SkipWhenNoSystemFontManager(string reason = "This platform has no system font manager to enumerate or match fonts.")
		{
			Assert.SkipWhen(HasNoSystemFontManager, reason);
		}

		// Skip tests that need a usable default typeface (system fonts installed and enumerable).
		protected static void SkipWhenNoDefaultFont(string reason = "This platform has no usable default typeface (no system fonts).")
		{
			Assert.SkipWhen(HasNoDefaultFont, reason);
		}
	}
}
