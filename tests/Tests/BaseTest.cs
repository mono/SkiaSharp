using System;

namespace SkiaSharp.Tests
{
	public abstract class BaseTest
	{
		protected const string CategoryKey = "Category";

		protected const string ApiCategory = "API";
		protected const string GpuCategory = "GPU";
		protected const string MatchCharacterCategory = "MatchCharacter";

		protected static bool IsLinux = TestConfig.Current.IsLinux;
		protected static bool IsMac = TestConfig.Current.IsMac;
		protected static bool IsUnix = TestConfig.Current.IsUnix;
		protected static bool IsWindows = TestConfig.Current.IsWindows;

		protected static bool IsRuntimeMono => TestConfig.Current.IsRuntimeMono;

		protected static string[] UnicodeFontFamilies => TestConfig.Current.UnicodeFontFamilies;
		protected static string DefaultFontFamily => TestConfig.Current.DefaultFontFamily;

		public static string PathRoot => TestConfig.Current.PathRoot;
		public static string PathToFonts => TestConfig.Current.PathToFonts;
		public static string PathToImages => TestConfig.Current.PathToImages;

		public static void CollectGarbage()
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}
	}
}
