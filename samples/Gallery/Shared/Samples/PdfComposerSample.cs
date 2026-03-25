using System;
using System.IO;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class PdfComposerSample : DocumentSampleBase
{
	private bool _isSupported = true;
	private int _pageSizeIndex;
	private float _pages = 2f;
	private string? _lastGeneratedKey;

	private static readonly string[] PageSizes = { "A4", "Letter", "A3" };

	public override string Title => "PDF Composer";

	public override string Description => "Generate multi-page PDF documents with configurable page sizes.";

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new PickerControl("pageSize", "Page Size", PageSizes, _pageSizeIndex),
		new SliderControl("pages", "Pages", 1, 5, _pages, 1),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "pageSize":
				_pageSizeIndex = (int)value;
				break;
			case "pages":
				_pages = (float)value;
				break;
		}

		// Force regeneration on next draw
		_lastGeneratedKey = null;
	}

	protected override async Task OnInit()
	{
		await base.OnInit();
		if (!string.IsNullOrEmpty(SamplesManager.TempDataPath))
		{
			var root = SamplesManager.EnsureTempDataDirectory("PdfComposerSample");
			DocumentPath = Path.Combine(root, $"{Guid.NewGuid():N}.pdf");
		}
	}

	private (int Width, int Height) GetPageDimensions() => _pageSizeIndex switch
	{
		1 => (612, 792),    // Letter
		2 => (842, 1191),   // A3
		_ => (595, 842),    // A4
	};

	protected override void OnGenerateDocument(string path)
	{
		if (!_isSupported)
			return;

		var pageCount = (int)_pages;
		var (pageWidth, pageHeight) = GetPageDimensions();
		var key = $"{_pageSizeIndex}_{pageCount}";

		if (_lastGeneratedKey == key && File.Exists(path))
			return;

		// Delete old file to regenerate
		if (File.Exists(path))
			File.Delete(path);

		var metadata = new SKDocumentPdfMetadata
		{
			Author = "Cool Developer",
			Creation = DateTime.Now,
			Creator = "Cool Developer Library",
			Keywords = "SkiaSharp, Sample, PDF, Developer, Library",
			Modified = DateTime.Now,
			Producer = "SkiaSharp",
			Subject = "SkiaSharp Sample PDF",
			Title = "Sample PDF",
		};

		using var document = SKDocument.CreatePdf(path, metadata);
		if (document == null)
		{
			_isSupported = false;
			return;
		}

		using var paint = new SKPaint
		{
			TextSize = 64.0f,
			IsAntialias = true,
			Color = 0xFF9CAFB7,
			IsStroke = true,
			StrokeWidth = 3,
			TextAlign = SKTextAlign.Center,
		};

		var sizeName = PageSizes[_pageSizeIndex];

		for (var i = 1; i <= pageCount; i++)
		{
			using var pdfCanvas = document.BeginPage(pageWidth, pageHeight);
			pdfCanvas.DrawText($"...PDF {i}/{pageCount}...", pageWidth / 2, pageHeight / 4, paint);

			using var smallPaint = new SKPaint
			{
				TextSize = 32.0f,
				IsAntialias = true,
				Color = 0xFFB0BEC5,
				TextAlign = SKTextAlign.Center,
			};
			pdfCanvas.DrawText($"{sizeName} ({pageWidth}×{pageHeight})", pageWidth / 2, pageHeight / 4 + 60, smallPaint);

			document.EndPage();
		}

		document.Close();
		_lastGeneratedKey = key;
	}
}
