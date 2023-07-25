using System.IO;

namespace SkiaSharp.Tests
{
	internal static class AssetCopier
	{
		public static void CopyAssets()
		{
			var fontsRoot = BaseTest.PathToFonts;
			var imagesRoot = BaseTest.PathToImages;

			var assembly = typeof(AssetCopier).Assembly;
			var prefix = "SkiaSharp.Tests.Content.";
			var fontsPrefix = "fonts.";
			var imagesPrefix = "images.";

			var names = assembly.GetManifestResourceNames();

			foreach (var name in names)
			{
				if (!name.StartsWith(prefix))
					continue;

				var filename = name.Substring(prefix.Length);
				var root = "";

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

				Directory.CreateDirectory(root);

				using var stream = assembly.GetManifestResourceStream(name);
				using var dest = File.Create(Path.Combine(root, filename));
				stream.CopyTo(dest);
			}
		}
	}
}
