using System;
using System.IO;
using SkiaSharp;

namespace SkiaSharpSample.Samples;

public class CreateXpsSample : DocumentSampleBase
{
	private bool isSupported = true;

	public override string Title => "Create XPS Document";

	public override string Description => "Generate multi-page XPS documents using SkiaSharp's document API. Windows only.";

	public override bool IsSupported => OperatingSystem.IsWindows();

	protected override void OnGenerateDocument(SKCanvas previewCanvas, int width, int height)
	{
		if (DocumentBytes == null && isSupported)
		{
			using var stream = new MemoryStream();
			using var document = SKDocument.CreateXps(stream);
			if (document == null)
			{
				isSupported = false;
			}
			else
			{
				using var paint = new SKPaint
				{
					IsAntialias = true,
					Color = 0xFF9CAFB7,
					IsStroke = true,
					StrokeWidth = 3,
				};
				using var docFont = new SKFont { Size = 64.0f };

				var pageWidth = 840;
				var pageHeight = 1188;

				for (var i = 1; i <= 2; i++)
				{
					using var xpsCanvas = document.BeginPage(pageWidth, pageHeight);
					xpsCanvas.DrawText($"...XPS {i}/2...", pageWidth / 2, pageHeight / 4, SKTextAlign.Center, docFont, paint);
					document.EndPage();
				}

				document.Close();
				DocumentBytes = stream.ToArray();
				DocumentFileName = "SkiaSharp-Sample.xps";
				DocumentMimeType = "application/oxps";
			}
		}

		using var font = new SKFont { Size = 40 };
		using var previewPaint = new SKPaint { IsAntialias = true, Color = 0xFF9CAFB7 };

		previewCanvas.DrawText(
			isSupported ? "Tap to open XPS" : "XPS not supported on this platform",
			width / 2f, height / 3, SKTextAlign.Center, font, previewPaint);
	}
}
