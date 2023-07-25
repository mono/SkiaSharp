using System;
using System.IO;
using SkiaSharp.Internals;

namespace SkiaSharp.Tests
{
	public abstract class BaseTest
	{
		protected const string CategoryKey = "Category";

		protected const string ApiCategory = "API";
		protected const string GpuCategory = "GPU";
		protected const string MatchCharacterCategory = "MatchCharacter";

		protected static bool IsLinux = PlatformConfiguration.IsLinux;
		protected static bool IsMac = PlatformConfiguration.IsMac;
		protected static bool IsUnix = PlatformConfiguration.IsUnix;
		protected static bool IsWindows = PlatformConfiguration.IsWindows;

		protected static bool IsRuntimeMono;

		protected static readonly string[] UnicodeFontFamilies;
		protected static readonly string DefaultFontFamily;

		public static readonly string PathToAssembly;
		public static readonly string PathToFonts;
		public static readonly string PathToImages;

		static BaseTest()
		{
			// the the base paths
#if __ANDROID__ || __IOS__
			PathToAssembly = Microsoft.Maui.Storage.FileSystem.CacheDirectory;
#else
			PathToAssembly = Directory.GetCurrentDirectory();
#endif
			PathToFonts = Path.Combine(PathToAssembly, "fonts");
			PathToImages = Path.Combine(PathToAssembly, "images");

			// some platforms run the tests from a temporary location, so copy the native files
#if !NETCOREAPP && !__ANDROID__ && !__IOS__
			var skiaRoot = Path.GetDirectoryName(typeof(SkiaSharp.SKImageInfo).Assembly.Location);
			var harfRoot = Path.GetDirectoryName(typeof(HarfBuzzSharp.Buffer).Assembly.Location);

			foreach (var file in Directory.GetFiles(PathToAssembly))
			{
				var fname = Path.GetFileNameWithoutExtension(file);

				var skiaDest = Path.Combine(skiaRoot, Path.GetFileName(file));
				if (fname == "libSkiaSharp" && !File.Exists(skiaDest))
				{
					File.Copy(file, skiaDest, true);
				}

				var harfDest = Path.Combine(harfRoot, Path.GetFileName(file));
				if (fname == "libHarfBuzzSharp" && !File.Exists(harfDest))
				{
					File.Copy(file, harfDest, true);
				}
			}
#endif

			IsRuntimeMono = Type.GetType("Mono.Runtime") != null;

			// set the test fields
#if __ANDROID__
			DefaultFontFamily = "sans-serif";
			UnicodeFontFamilies = new[] { "Noto Color Emoji" };
#elif __IOS__
			DefaultFontFamily = "Arial";
			UnicodeFontFamilies = new[] { "Apple Color Emoji" };
#else
			DefaultFontFamily = IsLinux ? "DejaVu Sans" : "Arial";
			UnicodeFontFamilies =
				IsLinux ? new[] { "Symbola" } :
				IsMac ? new[] { "Apple Color Emoji" } :
				new[] { "Segoe UI Emoji", "Segoe UI Symbol" };
#endif
		}

		public static void CollectGarbage()
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}
	}
}
