---
title: "Documents"
description: "Choose PDF, SVG, or XPS output and learn how each SkiaSharp document API supplies a canvas and finalizes its stream safely."
---

# Documents

Use a SkiaSharp document canvas when drawing should be serialized to a file or stream instead of rendered into an [`SKSurface`](xref:SkiaSharp.SKSurface). You still draw with an [`SKCanvas`](xref:SkiaSharp.SKCanvas), but the document canvas encodes those draw calls as PDF, SVG, or XPS output in a caller-supplied stream.

Start with PDF for multi-page output unless the consumer specifically requires XPS. Choose SVG when you need one scalable vector graphic represented as XML.

## Choose a format

| Format | Use when | Canvas lifecycle | Main constraint |
| --- | --- | --- | --- |
| PDF | You need portable, multi-page document output | Create an `SKDocument`, call `BeginPage` and `EndPage` for each page, then call `Close` | Page dimensions use points; close the document before reading the completed output |
| SVG | You need one scalable vector graphic or SVG markup | Create an `SKCanvas` with `SKSvgCanvas.Create`, draw your content, then dispose the canvas | There is no page lifecycle; the SVG is not complete until the canvas is disposed |
| XPS | A Windows workflow specifically requires XPS output | Use the same `SKDocument` page lifecycle as PDF | Requires the Windows XPS Object Model and COM; creation returns `null` where unavailable |

PDF and XPS page sizes use point units, where 72 points equal one inch. Their raster DPI options control how drawing operations without a native document representation are rasterized; DPI does not change the page coordinate system.

## How document canvases differ from surfaces

An `SKSurface` exposes pixels in CPU memory or a GPU resource. You can snapshot it, read its pixels, or present it through a view. A document canvas instead serializes draw calls to an output stream. The public document workflow has no surface to snapshot or read back.

This distinction lets you reuse drawing code. Put the drawing itself in a method that accepts `SKCanvas`, then call it with a surface canvas, a document page canvas, or an SVG canvas as appropriate. Keep destination-specific setup and finalization outside that method.

## In this section

- [Create a PDF document](pdf.md) - write and finalize a multi-page PDF, set metadata, and handle incomplete output.
- [Create an SVG document](svg.md) - draw one bounded SVG graphic and complete its XML by disposing the canvas.
- [Create an XPS document](xps.md) - initialize COM and write a multi-page XPS file on supported Windows systems.

## Related output paths

- To produce PNG, JPEG, WebP, or another raster image, [render and encode bitmap pixels](../../bitmaps/saving.md).
- To record Skia drawing commands for replay or Skia-specific serialization, use [`SKPictureRecorder`](xref:SkiaSharp.SKPictureRecorder) and [`SKPicture`](xref:SkiaSharp.SKPicture). An `SKPicture` is not a standard document format.
- To render into pixels rather than a document stream, [choose a SkiaSharp surface](../index.md).
