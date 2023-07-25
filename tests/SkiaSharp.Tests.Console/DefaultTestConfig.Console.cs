using System.IO;

namespace SkiaSharp.Tests
{
	public partial class DefaultTestConfig : TestConfig
	{
		public DefaultTestConfig()
		{
			// the the base paths
			PathRoot = Directory.GetCurrentDirectory();

			// some platforms run the tests from a temporary location, so copy the native files
			var skiaRoot = Path.GetDirectoryName(typeof(SkiaSharp.SKImageInfo).Assembly.Location);
			var harfRoot = Path.GetDirectoryName(typeof(HarfBuzzSharp.Buffer).Assembly.Location);

			foreach (var file in Directory.GetFiles(PathRoot))
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

			// set the test fields
			DefaultFontFamily = IsLinux ? "DejaVu Sans" : "Arial";
			UnicodeFontFamilies =
				IsLinux ? new[] { "Symbola" } :
				IsMac ? new[] { "Apple Color Emoji" } :
				new[] { "Segoe UI Emoji", "Segoe UI Symbol" };
		}

		public override GlContext CreateGlContext()
		{
			if (IsLinux)
				return new GlxContext();
			else if (IsMac)
				return new CglContext();
			else if (IsWindows)
				return new WglContext();

			return base.CreateGlContext();
		}
	}
}
