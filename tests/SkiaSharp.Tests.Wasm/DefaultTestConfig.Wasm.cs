namespace SkiaSharp.Tests
{
	public partial class DefaultTestConfig : TestConfig
	{
		public DefaultTestConfig()
		{
			PathRoot = "/wasm-no-filesystem";

			DefaultFontFamily = "Arial";
			UnicodeFontFamilies = new[] { "Noto Color Emoji" };
		}
	}
}
