using SkiaSharp;
using SkiaSharp.HarfBuzz;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class WorldTextSample : InteractiveSampleBase
{
	private int scriptIndex;
	private bool shapingEnabled = true;
	private float textSize = 36f;

	private static readonly string[] ScriptNames = { "Latin", "Arabic", "Hebrew", "Emoji", "CJK", "Mixed" };
	private static readonly string[] ScriptTexts =
	{
		"The quick brown fox jumps over the lazy dog",
		"مرحبا بالعالم - SkiaSharp",
		"שלום עולם - SkiaSharp",
		"Hello 🌍🎨✨ SkiaSharp 🚀",
		"你好世界 - スキアシャープ",
		"Hello مرحبا 你好 🌍",
	};

	public override string Title => "World Text";

	public override string Category => SampleCategories.Text;

	public override string Description => "Compare text rendering with and without HarfBuzz shaping for multiple scripts.";

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new PickerControl("script", "Script", ScriptNames, scriptIndex),
		new ToggleControl("shaping", "HarfBuzz Shaping", shapingEnabled),
		new SliderControl("textSize", "Text Size", 16, 80, textSize),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "script":
				scriptIndex = (int)value;
				break;
			case "shaping":
				shapingEnabled = (bool)value;
				break;
			case "textSize":
				textSize = (float)value;
				break;
		}
	}

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		canvas.Clear(SKColors.White);

		var text = ScriptTexts[scriptIndex];
		var scriptName = ScriptNames[scriptIndex];

		// Find a typeface that can render the chosen script
		var firstChar = text.Length > 0 ? char.ConvertToUtf32(text, 0) : 'A';
		using var typeface = SKFontManager.Default.MatchCharacter(firstChar) ?? SKTypeface.Default;
		using var font = new SKFont(typeface, textSize);

		using var labelPaint = new SKPaint { IsAntialias = true, Color = SKColors.Gray };
		using var labelFont = new SKFont(SKTypeface.Default, 18);
		using var textPaint = new SKPaint { IsAntialias = true, Color = SKColors.Black };

		var x = 40f;
		var y = 60f;

		// Title
		canvas.DrawText($"Script: {scriptName}", x, y, labelFont, labelPaint);
		y += 50;

		if (shapingEnabled)
		{
			// Shaped (prominent)
			canvas.DrawText("Shaped:", x, y, labelFont, labelPaint);
			y += textSize + 10;
			using var shaper = new SKShaper(typeface);
			canvas.DrawShapedText(shaper, text, x, y, font, textPaint);
			y += textSize + 30;

			// Unshaped (secondary, dimmer)
			using var dimPaint = new SKPaint { IsAntialias = true, Color = new SKColor(0, 0, 0, 128) };
			canvas.DrawText("Unshaped:", x, y, labelFont, labelPaint);
			y += textSize + 10;
			canvas.DrawText(text, x, y, font, dimPaint);
		}
		else
		{
			// Unshaped (prominent)
			canvas.DrawText("Unshaped:", x, y, labelFont, labelPaint);
			y += textSize + 10;
			canvas.DrawText(text, x, y, font, textPaint);
			y += textSize + 30;

			// Shaped (secondary, dimmer)
			using var dimPaint = new SKPaint { IsAntialias = true, Color = new SKColor(0, 0, 0, 128) };
			canvas.DrawText("Shaped:", x, y, labelFont, labelPaint);
			y += textSize + 10;
			using var shaper = new SKShaper(typeface);
			canvas.DrawShapedText(shaper, text, x, y, font, dimPaint);
		}
	}
}
