---
title: "Create an SVG document with SkiaSharp"
description: "Create a single SVG document with SKSvgCanvas, draw through SKCanvas, and finalize valid XML without closing the caller-owned stream."
---

# Create an SVG document with SkiaSharp

Use [`SKSvgCanvas`](xref:SkiaSharp.SKSvgCanvas) to translate `SKCanvas` draw calls into a Scalable Vector Graphics (SVG) stream. Unlike PDF and XPS, SVG creation returns the canvas directly and has no page lifecycle. Dispose the canvas to complete the XML.

## Create and complete the SVG

The bounds passed to `SKSvgCanvas.Create` define the initial SVG viewport. This complete example writes a 640 by 480 SVG:

```csharp
using System;
using System.IO;
using SkiaSharp;

const string outputPath = "drawing.svg";
var bounds = SKRect.Create(640, 480);

using (var output = File.Create(outputPath))
using (var canvas = SKSvgCanvas.Create(bounds, output)
    ?? throw new InvalidOperationException("Unable to create the SVG canvas."))
using (var backgroundPaint = new SKPaint { Color = SKColors.White })
using (var paint = new SKPaint
{
    IsAntialias = true,
    Color = SKColors.CornflowerBlue,
})
{
    canvas.DrawRect(bounds, backgroundPaint);
    canvas.DrawCircle(bounds.MidX, bounds.MidY, 140, paint);
}

var xml = File.ReadAllText(outputPath);
if (!xml.Contains("<svg", StringComparison.Ordinal))
    throw new InvalidOperationException("The output does not contain an SVG root element.");
```

The SVG canvas may buffer output. Its closing XML is not guaranteed to be present until the canvas is disposed. Keep the output stream alive for the full canvas lifetime and dispose the canvas before reading, sending, or closing the stream.

The `Stream` overload does not dispose the caller's .NET stream. In the example, the nested `using` statements dispose the canvas first and the file stream second.

## Reuse drawing code

`SKSvgCanvas.Create` returns an ordinary `SKCanvas`, so drawing helpers that accept `SKCanvas` can target SVG without an `SKSurface`. Keep the SVG bounds and stream lifecycle in the caller:

```csharp
static void DrawBadge(SKCanvas canvas, SKRect bounds, SKPaint paint)
{
    canvas.DrawCircle(bounds.MidX, bounds.MidY, 64, paint);
}
```

The SVG output represents drawing commands rather than a pixel snapshot. Open the result in the browsers or SVG renderers your application supports. If you require exact raster pixels instead, [render to a raster surface](../raster/index.md) and encode the resulting image.

## Verify the result

The file should parse as XML with an `svg` root element whose width and height are `640` and `480`. It should display a blue circle on a white background.

## Related links

- [Documents](index.md)
- [Create a PDF document](pdf.md)
- [SVG path data in SkiaSharp](../../curves/path-data.md)
