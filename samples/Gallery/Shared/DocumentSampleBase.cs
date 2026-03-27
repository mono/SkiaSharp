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
}
