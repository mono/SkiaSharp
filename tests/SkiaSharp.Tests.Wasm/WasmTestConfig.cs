using System.IO;
using System.Reflection;

namespace SkiaSharp.Tests.Wasm;

public class WasmTestConfig : TestConfig
{
	public WasmTestConfig()
	{
		// Use /tmp as a writable VFS directory
		PathRoot = "/tmp/tests";

		// WASM has limited system font support
		DefaultFontFamily = "sans-serif";
		UnicodeFontFamilies = new[] { "sans-serif" };

		// Extract embedded resources to VFS (same pattern as device tests)
		ExtractContent();
	}

	private void ExtractContent()
	{
		var fontsRoot = Path.Combine(PathRoot, "Content", "fonts");
		var imagesRoot = Path.Combine(PathRoot, "Content", "images");

		Directory.CreateDirectory(fontsRoot);
		Directory.CreateDirectory(imagesRoot);

		var assembly = typeof(WasmTestConfig).Assembly;
		var prefix = "SkiaSharp.Tests.Wasm.Content.";
		var fontsPrefix = "fonts.";
		var imagesPrefix = "images.";

		foreach (var name in assembly.GetManifestResourceNames())
		{
			if (!name.StartsWith(prefix))
				continue;

			var filename = name.Substring(prefix.Length);
			string root;

			if (filename.StartsWith(fontsPrefix))
			{
				filename = filename.Substring(fontsPrefix.Length);
				root = fontsRoot;
			}
			else if (filename.StartsWith(imagesPrefix))
			{
				filename = filename.Substring(imagesPrefix.Length);
				root = imagesRoot;
			}
			else
			{
				continue;
			}

			var destPath = Path.Combine(root, filename);
			using var stream = assembly.GetManifestResourceStream(name);
			using var dest = File.Create(destPath);
			stream!.CopyTo(dest);
		}
	}
}
