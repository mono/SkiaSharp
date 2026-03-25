using System;
using System.IO;
using SkiaSharp;

namespace SkiaSharpSample;

public abstract class DocumentSampleBase : InteractiveSampleBase
{
	protected byte[]? DocumentBytes { get; set; }
	protected string? DocumentMimeType { get; set; }
	protected string? DocumentFileName { get; set; }

	public override string Category => SampleCategories.Documents;

	protected abstract void OnGenerateDocument(SKCanvas previewCanvas, int width, int height);

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		canvas.Clear(SKColors.White);
		OnGenerateDocument(canvas, width, height);
	}

	protected SKDocument? CreatePdfToMemory(SKDocumentPdfMetadata metadata, out MemoryStream stream)
	{
		stream = new MemoryStream();
		var doc = SKDocument.CreatePdf(stream, metadata);
		if (doc == null)
		{
			stream.Dispose();
			stream = null!;
		}
		return doc;
	}

	protected SKDocument? CreateXpsToMemory(out MemoryStream stream)
	{
		stream = new MemoryStream();
		var doc = SKDocument.CreateXps(stream);
		if (doc == null)
		{
			stream.Dispose();
			stream = null!;
		}
		return doc;
	}

	protected override void OnTapped()
	{
		base.OnTapped();

		if (DocumentBytes is { Length: > 0 } && !string.IsNullOrEmpty(DocumentFileName))
		{
			// Write to temp and open
			var tempPath = Path.Combine(Path.GetTempPath(), DocumentFileName);
			File.WriteAllBytes(tempPath, DocumentBytes);
			SamplesManager.OnOpenFile(tempPath);
		}
	}
}
