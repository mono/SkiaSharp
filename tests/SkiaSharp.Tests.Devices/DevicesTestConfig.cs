namespace SkiaSharp.Tests
{
	public class DevicesTestConfig : TestConfig
	{
#if WINDOWS
		public override GlContext CreateGlContext() => new WglContext();
#endif

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
