using System;
using System.Collections.Generic;
using System.IO;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class PdfComposerSample : DocumentSampleBase
{
	private bool isSupported = true;
	private int pageSizeIndex;
	private float pages = 2f;

	private static readonly string[] PageSizes = { "A4", "Letter", "A3" };

	public override string Title => "PDF Composer";

	public override string Description => "Generate multi-page PDF documents with configurable page sizes.";

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new PickerControl("pageSize", "Page Size", PageSizes, pageSizeIndex),
		new SliderControl("pages", "Pages", 1, 5, pages, 1),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "pageSize": pageSizeIndex = (int)value; break;
			case "pages": pages = (float)value; break;
		}
		DocumentBytes = null; // force regeneration
	}

	private (int Width, int Height) GetPageDimensions() => pageSizeIndex switch
	{
		1 => (612, 792),
		2 => (842, 1191),
		_ => (595, 842),
	};

	protected override void OnGenerateDocument(SKCanvas previewCanvas, int width, int height)
	{
		var pageCount = (int)pages;
		var (pageWidth, pageHeight) = GetPageDimensions();
		var sizeName = PageSizes[pageSizeIndex];

		// Generate PDF to memory if needed
		if (DocumentBytes == null && isSupported)
		{
			var metadata = new SKDocumentPdfMetadata
			{
				Author = "SkiaSharp Gallery",
				Creation = DateTime.Now,
				Producer = "SkiaSharp",
				Title = "Sample PDF",
			};

			using var stream = new MemoryStream();
			using var document = SKDocument.CreatePdf(stream, metadata);
			if (document == null)
			{
				isSupported = false;
			}
			else
			{
				using var docPaint = new SKPaint
				{
					TextSize = 64.0f,
					IsAntialias = true,
					Color = 0xFF9CAFB7,
					IsStroke = true,
					StrokeWidth = 3,
					TextAlign = SKTextAlign.Center,
				};

				for (var i = 1; i <= pageCount; i++)
				{
					using var pdfCanvas = document.BeginPage(pageWidth, pageHeight);
					pdfCanvas.DrawText($"...PDF {i}/{pageCount}...", pageWidth / 2, pageHeight / 4, docPaint);

					using var smallPaint = new SKPaint
					{
						TextSize = 32.0f,
						IsAntialias = true,
						Color = 0xFFB0BEC5,
						TextAlign = SKTextAlign.Center,
					};
					pdfCanvas.DrawText($"{sizeName} ({pageWidth}\u00d7{pageHeight})", pageWidth / 2, pageHeight / 4 + 60, smallPaint);
					document.EndPage();
				}

				document.Close();
				DocumentBytes = stream.ToArray();
				DocumentFileName = "SkiaSharp-Sample.pdf";
				DocumentMimeType = "application/pdf";
			}
		}

		// Draw preview
		using var font = new SKFont { Size = 40 };
		using var paint = new SKPaint { IsAntialias = true, Color = 0xFF9CAFB7 };

		if (!isSupported)
		{
			previewCanvas.DrawText("PDF generation not supported", width / 2f, height / 3, SKTextAlign.Center, font, paint);
			return;
		}

		previewCanvas.DrawText("Tap to open PDF", width / 2f, height / 3, SKTextAlign.Center, font, paint);

		font.Size = 20;
		paint.Color = 0xFFB0BEC5;
		previewCanvas.DrawText($"{pageCount} pages \u2022 {sizeName} ({pageWidth}\u00d7{pageHeight})", width / 2f, height / 3 + 50, SKTextAlign.Center, font, paint);

		if (DocumentBytes != null)
		{
			previewCanvas.DrawText($"Size: {DocumentBytes.Length / 1024.0:F1} KB", width / 2f, height / 3 + 80, SKTextAlign.Center, font, paint);
		}
	}
}
