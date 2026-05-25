using SkiaSharp;
using SkiaSharp.HarfBuzz;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class WorldTextSample : CanvasSampleBase
{
	private int scriptIndex;
	private float textSize = 36f;

	private SKTypeface? embeddedTypeface;

	private static readonly string[] ScriptNames = { "Latin", "Arabic", "Hebrew", "Emoji", "CJK" };
	private static readonly string[] ScriptTexts =
	{
		"The quick brown fox jumps",
		"مرحبا بالعالم",
		"שלום עולם",
		"Hello 🌍🎨✨🚀",
		"Hello World 你好",
	};

	// Arabic and Hebrew benefit from HarfBuzz shaping; others don't show visible difference
	private static readonly bool[] ShowComparison = { false, true, true, false, false };

	// Scripts where the embedded font can be used (Latin and Emoji)
	private static readonly bool[] UseEmbeddedFont = { true, false, false, true, false };

	public override string Title => "World Text";

	public override string Category => SampleCategories.Text;

	public override string Description => "Compare text rendering across Latin, Arabic, Hebrew, Emoji, and CJK scripts with HarfBuzz shaping.";

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new PickerControl("script", "Script", ScriptNames, scriptIndex),
		new SliderControl("textSize", "Text Size", 16, 80, textSize),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "script":
				scriptIndex = (int)value;
				break;
			case "textSize":
				textSize = (float)value;
				break;
		}
	}

	private SKTypeface GetEmbeddedTypeface()
	{
		if (embeddedTypeface == null)
		{
			using var stream = SampleMedia.Fonts.EmbeddedFont;
			if (stream != null)
				embeddedTypeface = SKTypeface.FromStream(stream);
		}
		return embeddedTypeface ?? SKTypeface.Default;
	}

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		canvas.Clear(SKColors.White);

		var text = ScriptTexts[scriptIndex];
		var scriptName = ScriptNames[scriptIndex];
		var useEmbedded = UseEmbeddedFont[scriptIndex];

		// For Latin/Emoji, use the embedded font (works on WASM without system fonts).
		// For Arabic/Hebrew/CJK, use system font matching.
		SKTypeface typeface;
		bool ownsTypeface;
		if (useEmbedded)
		{
			typeface = GetEmbeddedTypeface();
			ownsTypeface = false;
		}
		else
		{
			var firstChar = text.Length > 0 ? char.ConvertToUtf32(text, 0) : 'A';
			var matched = SKFontManager.Default.MatchCharacter(firstChar);
			typeface = matched ?? SKTypeface.Default;
			ownsTypeface = matched != null;
		}

		try
		{
			using var font = new SKFont(typeface, textSize);
			using var labelPaint = new SKPaint { IsAntialias = true, Color = SKColors.Gray };
			using var labelFont = new SKFont(GetEmbeddedTypeface(), 18);
			using var textPaint = new SKPaint { IsAntialias = true, Color = SKColors.Black };

			var x = 40f;
			var y = 60f;

			// Title
			canvas.DrawText($"Script: {scriptName}", x, y, labelFont, labelPaint);
			y += 50;

			if (!useEmbedded)
			{
				// Platform note for scripts that need system fonts
				using var warnFont = new SKFont(GetEmbeddedTypeface(), 13);
				using var warnPaint = new SKPaint { IsAntialias = true, Color = new SKColor(180, 100, 0) };
				canvas.DrawText("⚠ Requires system fonts — may not render on all platforms (e.g. WASM).", x, y, warnFont, warnPaint);
				y += 25;
			}

			if (ShowComparison[scriptIndex])
			{
				// Shaped text (primary)
				canvas.DrawText("▸ Shaped (HarfBuzz):", x, y, labelFont, labelPaint);
				y += textSize + 10;
				using var shaper = new SKShaper(typeface);
				canvas.DrawShapedText(shaper, text, x, y, font, textPaint);
				y += textSize + 20;

				// Divider
				using var divPaint = new SKPaint { Color = new SKColor(200, 200, 200), StrokeWidth = 1 };
				canvas.DrawLine(x, y, width - x, y, divPaint);
				y += 20;

				// Unshaped text (secondary)
				using var dimPaint = new SKPaint { IsAntialias = true, Color = new SKColor(0, 0, 0, 128) };
				canvas.DrawText("▸ Unshaped (no shaping):", x, y, labelFont, labelPaint);
				y += textSize + 10;
				canvas.DrawText(text, x, y, font, dimPaint);
				y += textSize + 20;

				// Note
				using var noteFont = new SKFont(GetEmbeddedTypeface(), 13);
				using var notePaint = new SKPaint { IsAntialias = true, Color = new SKColor(120, 120, 120) };
				canvas.DrawText("Note: Shaping fixes ligatures and bidirectional text layout.", x, y, noteFont, notePaint);
			}
			else
			{
				// Just show shaped version for scripts where difference is invisible
				canvas.DrawText("▸ Shaped text:", x, y, labelFont, labelPaint);
				y += textSize + 10;
				using var shaper = new SKShaper(typeface);
				canvas.DrawShapedText(shaper, text, x, y, font, textPaint);
				y += textSize + 30;

				using var noteFont = new SKFont(GetEmbeddedTypeface(), 13);
				using var notePaint = new SKPaint { IsAntialias = true, Color = new SKColor(120, 120, 120) };
				canvas.DrawText($"Shaping for {scriptName} text produces identical output to unshaped.", x, y, noteFont, notePaint);
				y += 20;
				canvas.DrawText("Select Arabic or Hebrew to see a visible difference.", x, y, noteFont, notePaint);
			}
		}
		finally
		{
			if (ownsTypeface)
				typeface.Dispose();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		embeddedTypeface?.Dispose();
		embeddedTypeface = null;
	}
}
