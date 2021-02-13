using System;
using System.IO;

namespace SkiaSharp.Tests
{
	public abstract class BaseTest
	{
		protected const string CategoryKey = "Category";

		protected const string GpuCategory = "GPU";
		protected const string MatchCharacterCategory = "MatchCharacter";

		protected static bool IsLinux = PlatformConfiguration.IsLinux;
		protected static bool IsMac = PlatformConfiguration.IsMac;
		protected static bool IsUnix = PlatformConfiguration.IsUnix;
		protected static bool IsWindows = PlatformConfiguration.IsWindows;

		protected static bool IsRuntimeMono;

		protected static readonly string[] UnicodeFontFamilies;
		protected static readonly string DefaultFontFamily;
		protected static readonly string PathToAssembly;
		protected static readonly string PathToFonts;
		protected static readonly string PathToImages;

		static BaseTest()
		{
			// the the base paths
#if __ANDROID__
			PathToAssembly = Xamarin.Essentials.FileSystem.CacheDirectory;
#else
			PathToAssembly = Directory.GetCurrentDirectory();
#endif
			PathToFonts = Path.Combine(PathToAssembly, "fonts");
			PathToImages = Path.Combine(PathToAssembly, "images");

			// some platforms run the tests from a temporary location, so copy the native files
#if !NET_STANDARD && !__ANDROID__
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
			DefaultFontFamily = IsLinux ? "DejaVu Sans" : "Arial";
			UnicodeFontFamilies =
				IsLinux ? new[] { "Symbola" } :
				IsMac ? new[] { "Apple Color Emoji" } :
				new[] { "Segoe UI Emoji", "Segoe UI Symbol" };
		}

		public static void CollectGarbage()
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}
	}
}
