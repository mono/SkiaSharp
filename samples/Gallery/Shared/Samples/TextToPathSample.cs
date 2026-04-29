using System;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class TextToPathSample : CanvasSampleBase
{
	private int textIndex;
	private float textSize = 64f;
	private float strokeWidth = 3f;
	private bool showFillPath = true;
	private bool showOutline = true;
	private bool showOriginalText;

	private SKTypeface? typeface;

	private static readonly string[] TextOptions =
	{
		"SkiaSharp",
		"The Quick Brown Fox",
		"Hello World!",
		"AaBbCcDdEeFf",
		"0123456789",
	};

	public override string Title => "Text to Path";

	public override DateOnly? DateAdded => new DateOnly(2026, 4, 27);

	public override string Description =>
		"Convert text to an SKPath using SKFont.GetTextPath, then compute the stroked outline with GetFillPath.";

	public override string Category => SampleManager.Text;

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new PickerControl("text", "Text", TextOptions, textIndex),
		new SliderControl("textSize", "Text Size", 16, 120, textSize),
		new SliderControl("strokeWidth", "Stroke Width", 0.5f, 20, strokeWidth, 0.5f),
		new ToggleControl("showFillPath", "Show Stroke Outline", showFillPath),
		new ToggleControl("showOutline", "Show Glyph Outlines", showOutline),
		new ToggleControl("showOriginalText", "Show Original Text", showOriginalText),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "text":
				textIndex = (int)value;
				break;
			case "textSize":
				textSize = (float)value;
				break;
			case "strokeWidth":
				strokeWidth = (float)value;
				break;
			case "showFillPath":
				showFillPath = (bool)value;
				break;
			case "showOutline":
				showOutline = (bool)value;
				break;
			case "showOriginalText":
				showOriginalText = (bool)value;
				break;
		}
	}

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		canvas.Clear(SKColors.White);

		if (typeface == null)
			return;

		var text = TextOptions[textIndex];

		using var font = new SKFont(typeface, textSize);
		var textWidth = font.MeasureText(text);
		var metrics = font.Metrics;
		var textHeight = metrics.Descent - metrics.Ascent;

		var x = (width - textWidth) / 2f;
		var y = height * 0.45f;

		using var textPath = font.GetTextPath(text, new SKPoint(x, y));
		if (textPath == null || textPath.PointCount == 0)
			return;

		// Show the original rendered text faintly for reference
		if (showOriginalText)
		{
			using var textPaint = new SKPaint
			{
				Color = SKColors.Black.WithAlpha(40),
				IsAntialias = true,
			};
			canvas.DrawText(text, x, y, font, textPaint);
		}

		// Draw the glyph outlines (the raw text path)
		if (showOutline)
		{
			using var outlinePaint = new SKPaint
			{
				Color = SKColors.Red,
				IsStroke = true,
				StrokeWidth = 1,
				IsAntialias = true,
			};
			canvas.DrawPath(textPath, outlinePaint);
		}

		// Compute and draw the stroked fill path
		if (showFillPath)
		{
			using var strokePaint = new SKPaint
			{
				IsStroke = true,
				StrokeWidth = strokeWidth,
				StrokeCap = SKStrokeCap.Round,
				StrokeJoin = SKStrokeJoin.Round,
				IsAntialias = true,
			};

			using var fillPath = strokePaint.GetFillPath(textPath);
			if (fillPath != null)
			{
				using var fillBrush = new SKPaint
				{
					Color = SKColors.CornflowerBlue.WithAlpha(80),
					IsAntialias = true,
				};
				canvas.DrawPath(fillPath, fillBrush);

				using var fillOutline = new SKPaint
				{
					Color = SKColors.CornflowerBlue,
					IsStroke = true,
					StrokeWidth = 1,
					IsAntialias = true,
				};
				canvas.DrawPath(fillPath, fillOutline);
			}
		}

		// Draw info
		using var infoFont = new SKFont(typeface, 13);
		using var infoPaint = new SKPaint
		{
			Color = new SKColor(0x99, 0x99, 0x99),
			IsAntialias = true,
		};
		var info = $"Path points: {textPath.PointCount}   Text size: {textSize:F0}   Stroke: {strokeWidth:F1}";
		canvas.DrawText(info, 12, height - 12, infoFont, infoPaint);
	}

	protected override System.Threading.Tasks.Task OnInit()
	{
		using var stream = SampleMedia.Fonts.InterVariable;
		using var data = SKData.Create(stream);
		typeface = SKTypeface.FromData(data);
		return base.OnInit();
	}

	protected override void OnDestroy()
	{
		typeface?.Dispose();
		typeface = null;
		base.OnDestroy();
	}
}
