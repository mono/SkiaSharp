using SkiaSharp;

// Parse arguments: [text] [--output path]
var text = "SkiaSharp";
var output = "output.png";

for (int i = 0; i < args.Length; i++)
{
	if (args[i] is "--output" or "-o" && i + 1 < args.Length)
		output = args[++i];
	else if (!args[i].StartsWith('-'))
		text = args[i];
}

Console.WriteLine($"Rendering \"{text}\" to {output}");
Console.WriteLine($"Platform: {System.Runtime.InteropServices.RuntimeInformation.OSDescription}");
Console.WriteLine($"Architecture: {System.Runtime.InteropServices.RuntimeInformation.OSArchitecture}");
Console.WriteLine($"Runtime: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
Console.WriteLine("Platform Color Type: " + SKImageInfo.PlatformColorType);

// Create the image
var info = new SKImageInfo(800, 600);
using var surface = SKSurface.Create(info);
var canvas = surface.Canvas;
int width = info.Width, height = info.Height;
var center = new SKPoint(width / 2f, height / 2f);
var radius = Math.Max(width, height) / 2f;

canvas.Clear(SKColors.White);

// Background gradient
SKColor[] gradientColors = { new(0x44, 0x88, 0xFF), new(0x88, 0x33, 0xCC) };
using var shader = SKShader.CreateRadialGradient(center, radius, gradientColors, SKShaderTileMode.Clamp);
using var bgPaint = new SKPaint { IsAntialias = true, Shader = shader };
canvas.DrawRect(0, 0, width, height, bgPaint);

// Circles
(float X, float Y, float R, SKColor Color)[] circles =
{
	(0.20f, 0.30f, 0.10f, new(0xFF, 0x4D, 0x66, 0xCC)),
	(0.75f, 0.25f, 0.08f, new(0x4D, 0xB3, 0xFF, 0xCC)),
	(0.15f, 0.70f, 0.07f, new(0xFF, 0x99, 0x1A, 0xCC)),
	(0.80f, 0.70f, 0.12f, new(0x66, 0xFF, 0xB3, 0xCC)),
	(0.50f, 0.15f, 0.06f, new(0xB3, 0x4D, 0xFF, 0xCC)),
	(0.40f, 0.80f, 0.09f, new(0xFF, 0xE6, 0x33, 0xCC)),
};
using var circlePaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
foreach (var (x, y, r, color) in circles)
{
	circlePaint.Color = color;
	canvas.DrawCircle(x * width, y * height, r * Math.Min(width, height), circlePaint);
}

// Centered text
using var textPaint = new SKPaint { Color = SKColors.White, IsAntialias = true };
using var font = new SKFont { Size = width * 0.10f };
canvas.DrawText(text, center.X, center.Y + font.Size / 3f, SKTextAlign.Center, font, textPaint);

// Save
using var image = surface.Snapshot();
using var data = image.Encode(SKEncodedImageFormat.Png, 100);
using var stream = File.OpenWrite(output);
data.SaveTo(stream);

Console.WriteLine($"Saved {new FileInfo(output).Length:N0} bytes to {Path.GetFullPath(output)}");
