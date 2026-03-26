using System;
using System.IO;
using SkiaSharp;

namespace SkiaSharpSample;

public abstract class DocumentSampleBase : InteractiveSampleBase
{
	public byte[]? DocumentBytes { get; protected set; }
	public string? DocumentMimeType { get; protected set; }
	public string? DocumentFileName { get; protected set; }

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
}
