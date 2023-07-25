namespace SkiaSharp.Tests
{
	public class DevicesTestConfig : TestConfig
	{
		public DevicesTestConfig()
		{
			// the the base paths
			PathRoot = Microsoft.Maui.Storage.FileSystem.CacheDirectory;

			// set the test fields
#if ANDROID
			DefaultFontFamily = "sans-serif";
			UnicodeFontFamilies = new[] { "Noto Color Emoji" };
#elif IOS || MACCATALYST
			DefaultFontFamily = "Arial";
			UnicodeFontFamilies = new[] { "Apple Color Emoji" };
#elif WINDOWS
			DefaultFontFamily = "Arial";
			UnicodeFontFamilies = new[] { "Segoe UI Emoji", "Segoe UI Symbol" };
#else
#error Missing platform variation
#endif
		}

	}
}
