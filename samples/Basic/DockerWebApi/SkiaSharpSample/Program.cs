using SkiaSharp;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthChecks();
var app = builder.Build();

app.MapHealthChecks("/health");

app.MapGet("/", () => Results.Text(
	"SkiaSharp Docker Web API Sample\n\n" +
	"GET /api/images         - renders default \"SkiaSharp\" image\n" +
	"GET /api/images/{text}  - renders image with custom text\n",
	"text/plain"));

app.MapGet("/api/images/{text?}", (string? text) =>
{
	text ??= "SkiaSharp";

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

	// Encode to PNG
	using var image = surface.Snapshot();
	using var data = image.Encode(SKEncodedImageFormat.Png, 100);
	return Results.File(data.ToArray(), "image/png");
});

app.Run();
