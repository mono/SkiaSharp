using System;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class VariableFontSample : CanvasSampleBase
{
	private float weight = 400f;
	private float opticalSize = 14f;
	private float textSize = 48f;
	private int textIndex;

	private SKTypeface? baseTypeface;

	private static readonly string[] TextOptions =
	{
		"Variable SkiaSharp",
		"The Quick Brown Fox",
		"Hello World!",
		"AaBbCcDdEeFf",
		"0123456789",
	};

	public override string Title => "Variable Fonts";

	public override string Description =>
		"Explore OpenType variable font axes — adjust weight and optical size in real time using Inter.";

	public override string Category => SampleCategories.Text;

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new PickerControl("text", "Text", TextOptions, textIndex),
		new SliderControl("weight", "Weight (wght)", 100, 900, weight, 1),
		new SliderControl("opticalSize", "Optical Size (opsz)", 14, 32, opticalSize, 1),
		new SliderControl("textSize", "Text Size", 16, 120, textSize),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "text": textIndex = (int)value; break;
			case "weight": weight = (float)value; break;
			case "opticalSize": opticalSize = (float)value; break;
			case "textSize": textSize = (float)value; break;
		}
	}

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		canvas.Clear(SKColors.White);

		if (baseTypeface == null)
			return;

		var position = new[]
		{
			new SKFontVariationPositionCoordinate { Axis = SKFourByteTag.Parse("wght"), Value = weight },
			new SKFontVariationPositionCoordinate { Axis = SKFourByteTag.Parse("opsz"), Value = opticalSize },
		};

		using var typeface = baseTypeface.Clone(position);
		if (typeface == null)
			return;

		using var font = new SKFont(typeface, textSize);
		using var paint = new SKPaint { Color = SKColors.Black, IsAntialias = true };

		var text = TextOptions[textIndex];
		var x = width / 2f;
		var y = height * 0.35f;

		// Draw the sample text centered
		font.MeasureText(text, out var bounds, paint);
		canvas.DrawText(text, x - bounds.MidX, y, font, paint);

		// Draw axis info below
		using var infoFont = new SKFont(typeface, Math.Max(14, textSize * 0.35f));
		using var infoPaint = new SKPaint { Color = new SKColor(0x88, 0x88, 0x88), IsAntialias = true };

		var info = $"wght: {weight:F0}   opsz: {opticalSize:F0}";
		infoFont.MeasureText(info, out var infoBounds, infoPaint);
		canvas.DrawText(info, x - infoBounds.MidX, y + bounds.Height + 40, infoFont, infoPaint);

		// Draw weight spectrum at the bottom
		DrawWeightSpectrum(canvas, width, height, typeface);
	}

	private static readonly float[] SpectrumWeights = { 100, 200, 300, 400, 500, 600, 700, 800, 900 };

	private void DrawWeightSpectrum(SKCanvas canvas, int width, int height, SKTypeface currentTypeface)
	{
		var spectrumY = height * 0.65f;
		var spectrumText = "Aa";
		var spectrumSize = Math.Max(20, textSize * 0.6f);
		var spacing = width / (float)(SpectrumWeights.Length + 1);

		using var labelPaint = new SKPaint { Color = new SKColor(0xAA, 0xAA, 0xAA), IsAntialias = true };
		using var labelFont = new SKFont(currentTypeface, 11);

		for (int i = 0; i < SpectrumWeights.Length; i++)
		{
			var w = SpectrumWeights[i];
			var cx = spacing * (i + 1);

			var pos = new[]
			{
				new SKFontVariationPositionCoordinate { Axis = SKFourByteTag.Parse("wght"), Value = w },
				new SKFontVariationPositionCoordinate { Axis = SKFourByteTag.Parse("opsz"), Value = opticalSize },
			};

			using var tf = baseTypeface!.Clone(pos);
			if (tf == null)
				continue;

			using var f = new SKFont(tf, spectrumSize);
			using var p = new SKPaint
			{
				Color = Math.Abs(w - weight) < 1 ? new SKColor(0x33, 0x99, 0xDD) : SKColors.Black,
				IsAntialias = true,
			};

			f.MeasureText(spectrumText, out var b, p);
			canvas.DrawText(spectrumText, cx - b.MidX, spectrumY, f, p);

			// Weight label
			var label = $"{w:F0}";
			labelFont.MeasureText(label, out var lb, labelPaint);
			canvas.DrawText(label, cx - lb.MidX, spectrumY + b.Height + 16, labelFont, labelPaint);
		}
	}

	protected override System.Threading.Tasks.Task OnInit()
	{
		using var stream = SampleMedia.Fonts.InterVariable;
		using var data = SKData.Create(stream);
		baseTypeface = SKTypeface.FromData(data);
		return base.OnInit();
	}

	protected override void OnDestroy()
	{
		baseTypeface?.Dispose();
		baseTypeface = null;
		base.OnDestroy();
	}
}
