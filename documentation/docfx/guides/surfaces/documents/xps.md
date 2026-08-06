---
title: "Create an XPS document with SkiaSharp"
description: "Create and finalize a multi-page XPS document on supported Windows systems, including COM setup and point-based pages."
---

# Create an XPS document with SkiaSharp

Use [`SKDocument.CreateXps`](xref:SkiaSharp.SKDocument.CreateXps*) when a Windows workflow specifically requires XML Paper Specification (XPS) output. XPS uses the same `BeginPage`, `EndPage`, and `Close` lifecycle as PDF, but the native factory requires the Windows XPS Object Model and an initialized Component Object Model (COM) apartment.

XPS document creation is supported on desktop and server Windows where the XPS Object Model is available. It is not supported on non-Windows systems or Nano Server. `CreateXps` returns `null` when the native XPS factory is unavailable.

## Initialize COM and create the document

Keep [`SKAutoCoInitialize`](xref:SkiaSharp.SKAutoCoInitialize) alive for the complete XPS document lifetime. The following complete example writes a two-page US Letter XPS file:

```csharp
using System.IO;
using SkiaSharp;

const string outputPath = "sample.xps";

using (var com = new SKAutoCoInitialize())
{
    using var output = File.Create(outputPath);
    using var document = SKDocument.CreateXps(output);

    const float pageWidth = 612;  // 8.5 inches * 72 points
    const float pageHeight = 792; // 11 inches * 72 points

    using var paint = new SKPaint
    {
        IsAntialias = true,
        Color = SKColors.CornflowerBlue,
    };

    for (var pageNumber = 1; pageNumber <= 2; pageNumber++)
    {
        var canvas = document.BeginPage(pageWidth, pageHeight);
        canvas.Clear(SKColors.White);
        canvas.DrawRect(
            SKRect.Create(96, 96 + pageNumber * 80, 420, 180),
            paint);

        document.EndPage();
    }

    document.Close();
}
```

The `using` order keeps COM and the output stream alive while the XPS document uses them. The page canvas is invalid after `EndPage`.

## Set the raster DPI

XPS page dimensions use points, where 72 points equal one inch. Within the Windows and COM scope shown in the complete example, the optional `dpi` argument to `CreateXps` controls the resolution used when document content must be rasterized; it does not change the page coordinate system. Keep the writable `Stream output` alive for the document lifetime:

```csharp
using var document = SKDocument.CreateXps(output, dpi: 144);
```

The default raster DPI is `SKDocument.DefaultRasterDpi`, which is 72. Increase it only when rasterized content needs more detail and the additional document size and processing cost are acceptable.

## Handle unavailable or incomplete output

Production code can check the nullable result from `CreateXps` when it needs to handle systems where the XPS Object Model factory is unavailable.

If generation fails after document creation, call `Abort` and discard the stream contents. Call `Close` after the final page before reading or publishing the file.

## Verify the result

The resulting file should be non-empty and open as a two-page XPS document in the viewer or print workflow your application targets. Run this verification on the Windows versions you support.

## Related links

- [Documents](index.md)
- [Create a PDF document](pdf.md)
- [Create an SVG document](svg.md)
