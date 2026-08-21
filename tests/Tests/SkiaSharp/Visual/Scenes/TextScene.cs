using System;
using System.IO;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Text rendered with a font bundled under <c>tests/Content/fonts</c>.
	///
	/// <para>
	/// Glyph rasterization and antialiasing diverge more across platforms and
	/// backends than any other primitive (different font scalers — CoreText,
	/// FreeType, DirectWrite — and subpixel strategies), which makes this both the
	/// most valuable regression canary and the scene most likely to need
	/// per-platform golden overrides. Hinting and subpixel positioning are turned
	/// off to keep the output as portable as possible.
	/// </para>
	/// </summary>
	public sealed class TextScene : ISkiaScene
	{
		private const string FontFileName = "Roboto2-Regular_NoEmbed.ttf";

		public string Name => "Text";

		public SKImageInfo Info => new(256, 256, SKColorType.Rgba8888, SKAlphaType.Premul);

		public void Draw(SKCanvas canvas)
		{
			canvas.Clear(SKColors.White);

			using var typeface = LoadTypeface();
			using var font = new SKFont(typeface, 44)
			{
				Hinting = SKFontHinting.None,
				Subpixel = false,
				Edging = SKFontEdging.Antialias,
			};
			using var paint = new SKPaint { IsAntialias = true, Color = SKColors.Black };

			// The bundled Roboto2-Regular_NoEmbed.ttf is a tiny subset that only maps
			// the glyphs "!,DEHLORW" — text using any other letters (e.g. "Skia")
			// renders nothing, producing a blank golden that asserts nothing. Keep
			// this string within the subset so the scene actually exercises glyph
			// rasterization.
			canvas.DrawText("HELLO", 28, 110, font, paint);

			paint.Color = SKColors.OrangeRed;
			canvas.DrawText("WORLD!", 28, 170, font, paint);
		}

		private static SKTypeface LoadTypeface()
		{
			// Determinism is the whole point of this scene: it must render with the
			// bundled font on every host. Silently falling back to the system
			// default font (CreateDefault) would capture a host-dependent,
			// non-portable golden and hide the real "font not bundled" failure, so
			// a missing or unloadable bundled font is a hard error, not a fallback.
			var path = Path.Combine(TestConfig.Current.PathToFonts, FontFileName);
			if (!File.Exists(path))
				throw new FileNotFoundException(
					$"The bundled font '{FontFileName}' was not found at '{path}'. The Text scene must " +
					"render with this font to stay deterministic across hosts; refusing to fall back to a " +
					"system font.", path);

			return SKTypeface.FromFile(path)
				?? throw new InvalidOperationException($"Failed to load the bundled font '{path}'.");
		}
	}
}
