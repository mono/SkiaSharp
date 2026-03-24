using System.IO;
using System.Linq;
using System.Reflection;

using SkiaSharp;

namespace SkiaSharpSample
{
	public static class SampleMedia
	{
		public static class Colors
		{
			public static readonly SKColor XamarinLightBlue = 0xFF3498DB;
			public static readonly SKColor XamarinGreen = 0xFF77D065;
			public static readonly SKColor XamarinDarkBlue = 0xFF2C3E50;
			public static readonly SKColor XamarinPurple = 0xFFB455B6;
		}

		public static class Images
		{
			public static Stream AdobeDng => Embedded.Load("adobe-dng.dng");
			public static Stream Baboon => Embedded.Load("baboon.png");
			public static Stream ColorWheel => Embedded.Load("color-wheel.png");
			public static Stream NinePatch => Embedded.Load("nine-patch.png");
			public static Stream BabyTux => Embedded.Load("baby_tux.webp");
			public static Stream LogosSvg => Embedded.Load("logos.svg");
			public static Stream AnimatedHeartGif => Embedded.Load("animated-heart.gif");
			public static Stream OpacitySvg => Embedded.Load("opacity.svg");
			public static Stream LottieLogo => Embedded.Load("LottieLogo1.json");
		}

		public static class Fonts
		{
			public static Stream EmbeddedFont => Embedded.Load("embedded-font.ttf");

			public static string ContentFontPath = string.Empty;
		}

		public static class Embedded
		{
			private static readonly Assembly assembly;
			private static readonly string[] resources;

			static Embedded()
			{
				assembly = typeof(SampleMedia.Embedded).GetTypeInfo().Assembly;
				resources = assembly.GetManifestResourceNames();
			}

			public static Stream Load(string name)
			{
				name = $".Media.{name}";
				name = resources.FirstOrDefault(n => n.EndsWith(name));

				Stream stream = null;
				if (name != null)
				{
					stream = assembly.GetManifestResourceStream(name);
				}
				return stream;
			}
		}
	}
}
