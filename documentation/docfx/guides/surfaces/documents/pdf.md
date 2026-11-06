---
title: "Create a PDF document with SkiaSharp"
description: "Create and finalize a multi-page PDF with SKDocument, point-based page sizes, metadata, and safe stream ownership."
---

# Create a PDF document with SkiaSharp

Use [`SKDocument`](xref:SkiaSharp.SKDocument) to draw a multi-page PDF into a file or writable stream. Create the document, begin and end each page, and call `Close` after the final page. PDF page dimensions use points, where 72 points equal one inch.

## Create and finalize the pages

The following complete example writes a two-page US Letter PDF. The page canvas is valid only until `EndPage` or `Close` is called.

```csharp
using System.IO;
using SkiaSharp;

const string outputPath = "sample.pdf";

using (var output = File.Create(outputPath))
{
    WritePdf(output);
}

static void WritePdf(Stream output)
{
    const float pageWidth = 612;  // 8.5 inches * 72 points
    const float pageHeight = 792; // 11 inches * 72 points

    var metadata = SKDocumentPdfMetadata.Default;
    metadata.Title = "SkiaSharp PDF example";
    metadata.Author = "Example application";

    using var document = SKDocument.CreatePdf(output, metadata);
    using var paint = new SKPaint
    {
        IsAntialias = true,
        Color = SKColors.CornflowerBlue,
    };

    for (var pageNumber = 1; pageNumber <= 2; pageNumber++)
    {
        var canvas = document.BeginPage(pageWidth, pageHeight);
        canvas.Clear(SKColors.White);
        canvas.DrawCircle(
            pageWidth / 2,
            220 + pageNumber * 120,
            100,
            paint);

        document.EndPage();
    }

    document.Close();
}
```

`SKDocument` owns the native page canvas and invalidates it when the page ends. Scope the managed canvas wrapper to one page and do not use it after `EndPage`.

The `Stream` overload keeps the caller's .NET stream open. `Close` finalizes the PDF into that stream; disposing the document then releases its internal stream wrapper. The caller remains responsible for disposing the .NET stream.

## Configure metadata and raster fallback

Start with `SKDocumentPdfMetadata.Default`, then change the fields you need. A newly zero-initialized `SKDocumentPdfMetadata` does not contain the documented raster DPI and encoding-quality defaults.

The most relevant options are:

- `Title`, `Author`, `Subject`, `Keywords`, `Creator`, `Producer`, `Creation`, and `Modified` set PDF metadata.
- `RasterDpi` controls the resolution used when a draw operation must be rasterized because PDF has no native representation for it. It does not change page dimensions.
- `EncodingQuality` is `101` by default, which selects lossless image encoding. Values of `100` or less allow opaque images to use JPEG at that quality.
- `PdfA` requests the additional metadata and output intent needed by Skia's PDF/A-2b path. Validate the resulting file against any conformance rules your application must meet.

## Handle incomplete output

Call `Abort` if page generation cannot complete. After an abort, discard the output stream contents; they are not a valid document. The example uses a `finally` block so the original failure still propagates while the document is abandoned.

Call `Close` before reading or publishing the result. Disposing a PDF document also closes it when needed, but an explicit `Close` makes the successful completion point clear and ensures final bytes have reached the stream.

## Verify the result

The resulting file should be non-empty, begin with the PDF header `%PDF-`, and open as a two-page US Letter document. Verify text, images, effects, and links in the PDF viewer used by your target workflow.

## Related links

- [Documents](index.md)
- [Create an XPS document](xps.md)
- [Save SkiaSharp bitmaps to files](../../bitmaps/saving.md)
