using System;
using HarfBuzzSharp;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class ColorFontSample : CanvasSampleBase
{
private int paletteIndex;
private float textSize = 72f;
private int textIndex;

private SKTypeface? baseTypeface;
private int paletteCount;

private static readonly string[] TextOptions =
{
"Color SkiaSharp",
"The Quick Brown Fox",
"Hello World!",
"AaBbCcDdEeFf",
"0123456789",
};

public override string Title => "Color Fonts";

public override string Description =>
"Explore color font palettes with Nabla — a Google Font with 7 built-in color palettes and variable axes.";

public override string Category => SampleCategories.Text;

public override IReadOnlyList<SampleControl> Controls =>
[
new PickerControl("text", "Text", TextOptions, textIndex),
new SliderControl("palette", "Palette", 0, Math.Max(0, paletteCount - 1), paletteIndex, 1),
new SliderControl("textSize", "Text Size", 24, 120, textSize),
];

protected override void OnControlChanged(string id, object value)
{
switch (id)
{
case "text": textIndex = (int)value; break;
case "palette": paletteIndex = (int)(float)value; break;
case "textSize": textSize = (float)value; break;
}
}

protected override void OnDrawSample(SKCanvas canvas, int width, int height)
{
canvas.Clear(SKColors.White);

if (baseTypeface == null)
return;



using var typeface = baseTypeface.Clone(paletteIndex);
if (typeface == null)
return;

using var font = new SKFont(typeface, textSize);
using var paint = new SKPaint { IsAntialias = true };

var text = TextOptions[textIndex];
var x = width / 2f;
var y = height * 0.35f;

// Draw the sample text centered
font.MeasureText(text, out var bounds, paint);
canvas.DrawText(text, x - bounds.MidX, y, font, paint);

// Draw palette info below
using var infoFont = new SKFont(SKTypeface.Default, 14);
using var infoPaint = new SKPaint { Color = new SKColor(0x88, 0x88, 0x88), IsAntialias = true };

var info = $"Palette {paletteIndex + 1} of {paletteCount}";
infoFont.MeasureText(info, out var infoBounds, infoPaint);
canvas.DrawText(info, x - infoBounds.MidX, height * 0.45f, infoFont, infoPaint);

// Draw palette spectrum at the bottom
DrawPaletteSpectrum(canvas, width, height);
}

private void DrawPaletteSpectrum(SKCanvas canvas, int width, int height)
{
var spectrumY = height * 0.65f;
var spectrumText = "Aa";
var spectrumSize = Math.Max(20, textSize * 0.5f);
var cols = Math.Min(paletteCount, 5);
var spacing = width / (float)(cols + 1);


using var labelPaint = new SKPaint { Color = new SKColor(0xAA, 0xAA, 0xAA), IsAntialias = true };
using var labelFont = new SKFont(SKTypeface.Default, 11);
using var paint = new SKPaint { IsAntialias = true };

for (int i = 0; i < paletteCount; i++)
{
var col = i % cols;
var row = i / cols;
var cx = spacing * (col + 1);
var cy = spectrumY + row * (spectrumSize + 28);

using var pTypeface = baseTypeface!.Clone(i);
if (pTypeface == null)
continue;

using var pFont = new SKFont(pTypeface, spectrumSize);
pFont.MeasureText(spectrumText, out var b, paint);
canvas.DrawText(spectrumText, cx - b.MidX, cy, pFont, paint);

var label = $"{i + 1}";
labelFont.MeasureText(label, out var lb, labelPaint);
using var namePaint = i == paletteIndex
? new SKPaint { Color = SKColors.Black, IsAntialias = true }
: new SKPaint { Color = new SKColor(0xAA, 0xAA, 0xAA), IsAntialias = true };
canvas.DrawText(label, cx - lb.MidX, cy + b.Height + 16, labelFont, namePaint);
}
}

protected override System.Threading.Tasks.Task OnInit()
{
using var stream = SampleMedia.Fonts.Nabla;
using var data = SKData.Create(stream);
baseTypeface = SKTypeface.FromData(data);

using var hbStream = SampleMedia.Fonts.Nabla;
using var blob = Blob.FromStream(hbStream);
using var face = new Face(blob, 0);
paletteCount = face.PaletteCount;

return base.OnInit();
}

protected override void OnDestroy()
{
baseTypeface?.Dispose();
baseTypeface = null;
base.OnDestroy();
}
}
