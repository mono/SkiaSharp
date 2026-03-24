using System.Runtime.InteropServices.JavaScript;
using SkiaSharp;

Console.WriteLine("Hello, Browser!");
Console.WriteLine("Your platform color type is " + SKImageInfo.PlatformColorType);

// crate a surface
var info = new SKImageInfo(256, 256);
using var bitmap = new SKBitmap(info);

// the the canvas and properties
using var canvas = new SKCanvas(bitmap);

// make sure the canvas is blank
canvas.Clear(SKColors.White);

// draw some text
using var paint = new SKPaint
{
    Color = SKColors.Black,
    IsAntialias = true,
    Style = SKPaintStyle.Fill
};
using var font = new SKFont
{
    Size = 24
};
var coord = new SKPoint(info.Width / 2, (info.Height + font.Size) / 2);
canvas.DrawText("SkiaSharp", coord, SKTextAlign.Center, font, paint);

// render the image
Renderer.Render(info.Width, info.Height, bitmap.GetPixelSpan());

partial class Renderer
{
    [JSImport("renderer.render", "main.js")]
    internal static partial void Render(int width, int height, [JSMarshalAs<JSType.MemoryView>] Span<byte> buffer);
}
