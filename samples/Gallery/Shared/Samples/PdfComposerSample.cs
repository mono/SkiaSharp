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

	private static readonly string[] PageSizes = { "A4", "Letter", "A3" };

	private static readonly SKColor HeadingColor = new SKColor(0x1A, 0x23, 0x7E);
	private static readonly SKColor BodyColor = new SKColor(0x42, 0x42, 0x42);
	private static readonly SKColor LinkColor = new SKColor(0x15, 0x65, 0xC0);
	private static readonly SKColor AccentColor = new SKColor(0xE9, 0x1E, 0x63);
	private static readonly SKColor LightGray = new SKColor(0xE0, 0xE0, 0xE0);
	private static readonly SKColor SubtitleColor = new SKColor(0x75, 0x75, 0x75);

	public override string Title => "PDF Composer";

	public override string Description => "Generate rich multi-page PDFs with shapes, text, images, and clickable annotations.";

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new PickerControl("pageSize", "Page Size", PageSizes, pageSizeIndex),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "pageSize": pageSizeIndex = (int)value; break;
		}
		DocumentBytes = null;
	}

	private (int Width, int Height) GetPageDimensions() => pageSizeIndex switch
	{
		1 => (612, 792),
		2 => (842, 1191),
		_ => (595, 842),
	};

	protected override void OnGenerateDocument(SKCanvas previewCanvas, int width, int height)
	{
		const int pageCount = 5;
		var (pageWidth, pageHeight) = GetPageDimensions();

		if (DocumentBytes == null && isSupported)
		{
			var metadata = new SKDocumentPdfMetadata
			{
				Author = "SkiaSharp Gallery",
				Creation = DateTime.Now,
				Producer = "SkiaSharp",
				Title = "SkiaSharp PDF Demo",
			};

			using var stream = new MemoryStream();
			using var document = SKDocument.CreatePdf(stream, metadata);
			if (document == null)
			{
				isSupported = false;
			}
			else
			{
				// Page 1: Cover / TOC
				using (var c = document.BeginPage(pageWidth, pageHeight))
					DrawCoverPage(c, pageWidth, pageHeight, pageCount);
				document.EndPage();

				// Page 2: Shapes & Paths
				using (var c = document.BeginPage(pageWidth, pageHeight))
					DrawShapesPage(c, pageWidth, pageHeight);
				document.EndPage();

				// Page 3: Text & Typography
				using (var c = document.BeginPage(pageWidth, pageHeight))
					DrawTextPage(c, pageWidth, pageHeight);
				document.EndPage();

				// Page 4: Images
				using (var c = document.BeginPage(pageWidth, pageHeight))
					DrawImagesPage(c, pageWidth, pageHeight);
				document.EndPage();

				// Page 5: Effects & Patterns
				using (var c = document.BeginPage(pageWidth, pageHeight))
					DrawEffectsPage(c, pageWidth, pageHeight, 5, pageCount);
				document.EndPage();

				document.Close();
				DocumentBytes = stream.ToArray();
				DocumentFileName = "SkiaSharp-Demo.pdf";
				DocumentMimeType = "application/pdf";
			}
		}

		// Draw preview
		using var font = new SKFont(SampleMedia.Fonts.Default, 40);
		using var paint = new SKPaint { IsAntialias = true, Color = 0xFF9CAFB7 };

		if (!isSupported)
		{
			previewCanvas.DrawText("PDF generation not supported", width / 2f, height / 3, SKTextAlign.Center, font, paint);
			return;
		}

		previewCanvas.DrawText("Tap to open PDF", width / 2f, height / 3, SKTextAlign.Center, font, paint);

		font.Size = 20;
		paint.Color = 0xFFB0BEC5;
		previewCanvas.DrawText($"5 pages \u2022 {PageSizes[pageSizeIndex]}", width / 2f, height / 3 + 50, SKTextAlign.Center, font, paint);

		if (DocumentBytes != null)
		{
			previewCanvas.DrawText($"Size: {DocumentBytes.Length / 1024.0:F1} KB", width / 2f, height / 3 + 80, SKTextAlign.Center, font, paint);
		}
	}

	private static void DrawBackToTocLink(SKCanvas canvas, float pageWidth, float pageHeight)
	{
		using var font = new SKFont(SampleMedia.Fonts.Default, 12);
		using var paint = new SKPaint { IsAntialias = true, Color = LinkColor };
		var text = "\u2190 Back to Contents";
		var textWidth = font.MeasureText(text);
		var x = (pageWidth - textWidth) / 2;
		var y = pageHeight - 40;
		canvas.DrawText(text, x, y, font, paint);

		using var underline = new SKPaint { IsAntialias = true, Color = LinkColor, StrokeWidth = 0.5f, IsStroke = true };
		canvas.DrawLine(x, y + 2, x + textWidth, y + 2, underline);

		var linkRect = new SKRect(x, y - 14, x + textWidth, y + 4);
		canvas.DrawLinkDestinationAnnotation(linkRect, "toc")?.Dispose();
	}

	private static void DrawPageTitle(SKCanvas canvas, float pageWidth, string title)
	{
		using var font = new SKFont(SampleMedia.Fonts.Default, 28);
		using var paint = new SKPaint { IsAntialias = true, Color = HeadingColor };
		canvas.DrawText(title, pageWidth / 2, 60, SKTextAlign.Center, font, paint);

		using var line = new SKPaint { IsAntialias = true, Color = AccentColor, StrokeWidth = 2, IsStroke = true };
		canvas.DrawLine(60, 75, pageWidth - 60, 75, line);
	}

	private static void DrawCoverPage(SKCanvas canvas, float pageWidth, float pageHeight, int pageCount)
	{
		canvas.DrawNamedDestinationAnnotation(new SKPoint(0, 0), "toc")?.Dispose();

		// Title
		using var titleFont = new SKFont(SampleMedia.Fonts.Default, 48);
		using var titlePaint = new SKPaint { IsAntialias = true, Color = HeadingColor };
		canvas.DrawText("SkiaSharp PDF Demo", pageWidth / 2, 120, SKTextAlign.Center, titleFont, titlePaint);

		// Subtitle
		using var subtitleFont = new SKFont(SampleMedia.Fonts.Default, 20);
		using var subtitlePaint = new SKPaint { IsAntialias = true, Color = SubtitleColor };
		canvas.DrawText("Generated with SkiaSharp", pageWidth / 2, 155, SKTextAlign.Center, subtitleFont, subtitlePaint);

		// Horizontal rule
		using var rulePaint = new SKPaint { IsAntialias = true, Color = LightGray, StrokeWidth = 2, IsStroke = true };
		canvas.DrawLine(80, 185, pageWidth - 80, 185, rulePaint);

		// TOC header
		using var tocHeaderFont = new SKFont(SampleMedia.Fonts.Default, 22);
		using var tocHeaderPaint = new SKPaint { IsAntialias = true, Color = HeadingColor };
		canvas.DrawText("Table of Contents", pageWidth / 2, 230, SKTextAlign.Center, tocHeaderFont, tocHeaderPaint);

		// TOC entries
		using var tocFont = new SKFont(SampleMedia.Fonts.Default, 16);
		using var tocPaint = new SKPaint { IsAntialias = true, Color = LinkColor };
		using var tocUnderline = new SKPaint { IsAntialias = true, Color = LinkColor, StrokeWidth = 0.5f, IsStroke = true };

		var entries = new List<(string Text, string Dest)>
		{
			("1. Shapes & Paths", "page-shapes"),
			("2. Text & Typography", "page-text"),
			("3. Images", "page-images"),
			("4. Effects & Patterns", "page-effects"),
		};

		var y = 270f;
		var x = 100f;
		foreach (var (text, dest) in entries)
		{
			canvas.DrawText(text, x, y, tocFont, tocPaint);
			var tw = tocFont.MeasureText(text);
			canvas.DrawLine(x, y + 2, x + tw, y + 2, tocUnderline);
			canvas.DrawLinkDestinationAnnotation(new SKRect(x, y - 16, x + tw, y + 4), dest)?.Dispose();
			y += 32;
		}

		// Footer URL
		using var footerFont = new SKFont(SampleMedia.Fonts.Default, 11);
		using var footerPaint = new SKPaint { IsAntialias = true, Color = LinkColor };
		var url = "https://github.com/mono/SkiaSharp";
		var urlW = footerFont.MeasureText(url);
		var urlX = (pageWidth - urlW) / 2;
		var urlY = pageHeight - 50;
		canvas.DrawText(url, urlX, urlY, footerFont, footerPaint);
		canvas.DrawUrlAnnotation(new SKRect(urlX, urlY - 12, urlX + urlW, urlY + 4), url)?.Dispose();
	}

	private static void DrawShapesPage(SKCanvas canvas, float pageWidth, float pageHeight)
	{
		canvas.DrawNamedDestinationAnnotation(new SKPoint(0, 0), "page-shapes")?.Dispose();
		DrawPageTitle(canvas, pageWidth, "Shapes & Paths");

		using var labelFont = new SKFont(SampleMedia.Fonts.Default, 12);
		using var labelPaint = new SKPaint { IsAntialias = true, Color = BodyColor };

		// Star
		using var starPath = new SKPath();
		var scx = pageWidth / 4;
		var scy = 200f;
		var outerR = 60f;
		var innerR = 25f;
		for (var i = 0; i < 10; i++)
		{
			var r = i % 2 == 0 ? outerR : innerR;
			var angle = (float)(Math.PI / 2 + i * Math.PI / 5);
			var px = scx + r * (float)Math.Cos(angle);
			var py = scy - r * (float)Math.Sin(angle);
			if (i == 0) starPath.MoveTo(px, py);
			else starPath.LineTo(px, py);
		}
		starPath.Close();

		using var starFill = new SKPaint { IsAntialias = true, Color = new SKColor(0xFF, 0xC1, 0x07) };
		canvas.DrawPath(starPath, starFill);
		using var starStroke = new SKPaint { IsAntialias = true, Color = new SKColor(0xFF, 0x8F, 0x00), IsStroke = true, StrokeWidth = 2 };
		canvas.DrawPath(starPath, starStroke);
		canvas.DrawText("Filled Star", scx, scy + outerR + 20, SKTextAlign.Center, labelFont, labelPaint);

		// Rounded rect with gradient
		var rrx = pageWidth / 2 + 20;
		var rrRect = new SKRect(rrx, 140, rrx + 160, 260);
		using var rrShader = SKShader.CreateLinearGradient(
			new SKPoint(rrRect.Left, rrRect.Top), new SKPoint(rrRect.Right, rrRect.Bottom),
			new SKColor[] { new SKColor(0x42, 0xA5, 0xF5), new SKColor(0x0D, 0x47, 0xA1) },
			null, SKShaderTileMode.Clamp);
		using var rrPaint = new SKPaint { IsAntialias = true, Shader = rrShader };
		canvas.DrawRoundRect(rrRect, 16, 16, rrPaint);
		canvas.DrawText("Gradient Rounded Rect", rrRect.MidX, rrRect.Bottom + 20, SKTextAlign.Center, labelFont, labelPaint);

		// Circle with radial gradient
		var ccx = pageWidth / 4;
		var ccy = 400f;
		var cr = 55f;
		using var radShader = SKShader.CreateRadialGradient(
			new SKPoint(ccx - 15, ccy - 15), cr * 1.2f,
			new SKColor[] { new SKColor(0xEF, 0x53, 0x50), new SKColor(0xB7, 0x1C, 0x1C) },
			null, SKShaderTileMode.Clamp);
		using var circlePaint = new SKPaint { IsAntialias = true, Shader = radShader };
		canvas.DrawCircle(ccx, ccy, cr, circlePaint);
		canvas.DrawText("Radial Gradient Circle", ccx, ccy + cr + 20, SKTextAlign.Center, labelFont, labelPaint);

		// Bezier curves
		using var bezierPath = new SKPath();
		var bx = pageWidth / 2 + 20;
		bezierPath.MoveTo(bx, 350);
		bezierPath.CubicTo(bx + 40, 310, bx + 120, 310, bx + 160, 350);
		bezierPath.CubicTo(bx + 120, 390, bx + 40, 430, bx, 400);
		bezierPath.CubicTo(bx + 60, 370, bx + 100, 380, bx + 160, 400);

		using var bezierPaint = new SKPaint
		{
			IsAntialias = true,
			Color = new SKColor(0x7B, 0x1F, 0xA2),
			IsStroke = true,
			StrokeWidth = 3,
			StrokeCap = SKStrokeCap.Round,
		};
		canvas.DrawPath(bezierPath, bezierPaint);
		canvas.DrawText("B\u00e9zier Curves", bx + 80, 450, SKTextAlign.Center, labelFont, labelPaint);

		DrawBackToTocLink(canvas, pageWidth, pageHeight);
	}

	private static void DrawTextPage(SKCanvas canvas, float pageWidth, float pageHeight)
	{
		canvas.DrawNamedDestinationAnnotation(new SKPoint(0, 0), "page-text")?.Dispose();
		DrawPageTitle(canvas, pageWidth, "Text & Typography");

		var margin = 60f;
		var y = 110f;

		using var sectionFont = new SKFont(SampleMedia.Fonts.Default, 14);
		using var sectionPaint = new SKPaint { IsAntialias = true, Color = AccentColor };
		using var bodyPaint = new SKPaint { IsAntialias = true, Color = BodyColor };

		// Different sizes
		canvas.DrawText("Text Sizes", margin, y, sectionFont, sectionPaint);
		y += 10;
		foreach (var size in new[] { 12, 24, 36, 48 })
		{
			using var sizedFont = new SKFont(SampleMedia.Fonts.Default, size);
			y += size + 8;
			canvas.DrawText($"{size}pt Sample Text", margin, y, sizedFont, bodyPaint);
		}
		y += 40;

		// Text styles
		canvas.DrawText("Text Styles", margin, y, sectionFont, sectionPaint);
		y += 30;

		using var styleFont = new SKFont(SampleMedia.Fonts.Default, 20);
		canvas.DrawText("Normal text style", margin, y, styleFont, bodyPaint);
		y += 30;

		using var boldPaint = new SKPaint
		{
			IsAntialias = true,
			Color = BodyColor,
			Style = SKPaintStyle.StrokeAndFill,
			StrokeWidth = 1.0f,
		};
		canvas.DrawText("Bold text style (stroke)", margin, y, styleFont, boldPaint);
		y += 30;

		using var italicFont = new SKFont(SampleMedia.Fonts.Default, 20, 1, -0.25f);
		canvas.DrawText("Italic text style (skew)", margin, y, italicFont, bodyPaint);
		y += 50;

		// Alignment
		canvas.DrawText("Text Alignment", margin, y, sectionFont, sectionPaint);
		y += 30;

		using var alignFont = new SKFont(SampleMedia.Fonts.Default, 16);
		using var guidePaint = new SKPaint { IsAntialias = true, Color = LightGray, StrokeWidth = 1, IsStroke = true };
		canvas.DrawLine(pageWidth / 2, y - 16, pageWidth / 2, y + 60, guidePaint);

		canvas.DrawText("Left-aligned text", margin, y, alignFont, bodyPaint);
		y += 24;
		canvas.DrawText("Center-aligned text", pageWidth / 2, y, SKTextAlign.Center, alignFont, bodyPaint);
		y += 24;
		canvas.DrawText("Right-aligned text", pageWidth - margin, y, SKTextAlign.Right, alignFont, bodyPaint);

		DrawBackToTocLink(canvas, pageWidth, pageHeight);
	}

	private static void DrawImagesPage(SKCanvas canvas, float pageWidth, float pageHeight)
	{
		canvas.DrawNamedDestinationAnnotation(new SKPoint(0, 0), "page-images")?.Dispose();

		using var whiteBg = new SKPaint { Color = SKColors.White };
		canvas.DrawRect(0, 0, pageWidth, pageHeight, whiteBg);
		DrawPageTitle(canvas, pageWidth, "Images");

		var margin = 60f;
		var y = 100f;

		// Description
		using var descFont = new SKFont(SampleMedia.Fonts.Default, 13);
		using var descPaint = new SKPaint { IsAntialias = true, Color = BodyColor };
		canvas.DrawText("SkiaSharp can decode and embed bitmap images in PDF documents.", margin, y, descFont, descPaint);
		y += 30;

		// Load and draw baboon image
		using var baboonStream = SampleMedia.Images.Baboon;
		if (baboonStream != null)
		{
			using var baboonBitmap = SKBitmap.Decode(baboonStream);
			if (baboonBitmap != null)
			{
				var imgSize = Math.Min(pageWidth - margin * 2, 300);
				var imgRect = SKRect.Create(margin, y, imgSize, imgSize);
				canvas.DrawBitmap(baboonBitmap, imgRect);

				// Label
				using var labelFont = new SKFont(SampleMedia.Fonts.Default, 11);
				using var labelPaint = new SKPaint { IsAntialias = true, Color = SubtitleColor };
				canvas.DrawText($"baboon.png ({baboonBitmap.Width}×{baboonBitmap.Height})", margin, y + imgSize + 16, labelFont, labelPaint);

				// Draw the same image with effects on the right side
				var rightX = margin + imgSize + 30;
				var smallSize = imgSize * 0.45f;

				// Grayscale version
				using var grayFilter = SKColorFilter.CreateColorMatrix(new float[]
				{
					0.21f, 0.72f, 0.07f, 0, 0,
					0.21f, 0.72f, 0.07f, 0, 0,
					0.21f, 0.72f, 0.07f, 0, 0,
					0,     0,     0,     1, 0,
				});
				using var grayPaint = new SKPaint { ColorFilter = grayFilter };
				canvas.DrawBitmap(baboonBitmap, SKRect.Create(rightX, y, smallSize, smallSize), grayPaint);
				canvas.DrawText("Grayscale filter", rightX, y + smallSize + 16, labelFont, labelPaint);

				// Sepia version
				using var sepiaFilter = SKColorFilter.CreateColorMatrix(new float[]
				{
					0.393f, 0.769f, 0.189f, 0, 0,
					0.349f, 0.686f, 0.168f, 0, 0,
					0.272f, 0.534f, 0.131f, 0, 0,
					0,      0,      0,      1, 0,
				});
				using var sepiaPaint = new SKPaint { ColorFilter = sepiaFilter };
				canvas.DrawBitmap(baboonBitmap, SKRect.Create(rightX, y + smallSize + 30, smallSize, smallSize), sepiaPaint);
				canvas.DrawText("Sepia filter", rightX, y + smallSize * 2 + 46, labelFont, labelPaint);

				y += imgSize + 40;
			}
		}

		// Draw color wheel
		using var cwStream = SampleMedia.Images.ColorWheel;
		if (cwStream != null)
		{
			using var cwBitmap = SKBitmap.Decode(cwStream);
			if (cwBitmap != null)
			{
				var cwSize = 150f;
				canvas.DrawBitmap(cwBitmap, SKRect.Create(margin, y, cwSize, cwSize));

				using var cwLabel = new SKFont(SampleMedia.Fonts.Default, 11);
				using var cwPaint = new SKPaint { IsAntialias = true, Color = SubtitleColor };
				canvas.DrawText($"color-wheel.png ({cwBitmap.Width}×{cwBitmap.Height})", margin, y + cwSize + 16, cwLabel, cwPaint);
			}
		}

		// Page number
		using var pageFont = new SKFont(SampleMedia.Fonts.Default, 11);
		using var pagePaint = new SKPaint { IsAntialias = true, Color = BodyColor };
		canvas.DrawText("Page 4 of 5", pageWidth / 2, pageHeight - 60, SKTextAlign.Center, pageFont, pagePaint);

		DrawBackToTocLink(canvas, pageWidth, pageHeight);
	}

	private static void DrawEffectsPage(SKCanvas canvas, float pageWidth, float pageHeight, int pageNum, int totalPages)
	{
		canvas.DrawNamedDestinationAnnotation(new SKPoint(0, 0), "page-effects")?.Dispose();

		// Gradient background
		using var bgShader = SKShader.CreateLinearGradient(
			new SKPoint(0, 0), new SKPoint(pageWidth, pageHeight),
			new SKColor[] { new SKColor(0xE8, 0xEA, 0xF6), new SKColor(0xFC, 0xE4, 0xEC) },
			null, SKShaderTileMode.Clamp);
		using var bgPaint = new SKPaint { Shader = bgShader };
		canvas.DrawRect(0, 0, pageWidth, pageHeight, bgPaint);

		DrawPageTitle(canvas, pageWidth, "Effects & Patterns");

		// Concentric rings
		var cx = pageWidth / 2;
		var cy = 250f;
		for (var i = 5; i >= 0; i--)
		{
			var radius = 30 + i * 20;
			var alpha = (byte)(40 + i * 35);
			using var ringPaint = new SKPaint
			{
				IsAntialias = true,
				Color = new SKColor(HeadingColor.Red, HeadingColor.Green, HeadingColor.Blue, alpha),
				IsStroke = true,
				StrokeWidth = 3,
			};
			canvas.DrawCircle(cx, cy, radius, ringPaint);
		}

		// Dot grid pattern
		using var dotPaint = new SKPaint { IsAntialias = true, Color = new SKColor(0xE9, 0x1E, 0x63, 0x60) };
		for (var gx = 80f; gx < pageWidth - 80; gx += 30)
		{
			for (var gy = 400f; gy < 550; gy += 30)
			{
				var dist = (float)Math.Sqrt(Math.Pow(gx - cx, 2) + Math.Pow(gy - 475, 2));
				var dotR = Math.Max(1.5f, 5 - dist / 80);
				canvas.DrawCircle(gx, gy, dotR, dotPaint);
			}
		}

		// Diamond row
		using var diamondPath = new SKPath();
		var dy = 620f;
		for (var d = 0; d < 5; d++)
		{
			var dx = 100f + d * 90;
			diamondPath.MoveTo(dx, dy - 25);
			diamondPath.LineTo(dx + 25, dy);
			diamondPath.LineTo(dx, dy + 25);
			diamondPath.LineTo(dx - 25, dy);
			diamondPath.Close();
		}
		using var diaShader = SKShader.CreateLinearGradient(
			new SKPoint(80, dy), new SKPoint(pageWidth - 80, dy),
			new SKColor[] { new SKColor(0x42, 0xA5, 0xF5), new SKColor(0xAB, 0x47, 0xBC), new SKColor(0xEF, 0x53, 0x50) },
			null, SKShaderTileMode.Clamp);
		using var diaPaint = new SKPaint { IsAntialias = true, Shader = diaShader };
		canvas.DrawPath(diamondPath, diaPaint);

		// Page number
		using var pageFont = new SKFont(SampleMedia.Fonts.Default, 11);
		using var pagePaint = new SKPaint { IsAntialias = true, Color = BodyColor };
		canvas.DrawText($"Page {pageNum} of {totalPages}", pageWidth / 2, pageHeight - 60, SKTextAlign.Center, pageFont, pagePaint);

		DrawBackToTocLink(canvas, pageWidth, pageHeight);
	}
}
