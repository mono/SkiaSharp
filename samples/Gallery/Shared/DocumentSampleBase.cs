using System;
using System.IO;
using SkiaSharp;

namespace SkiaSharpSample;

public abstract class DocumentSampleBase : SampleBase
{
	public byte[]? DocumentBytes { get; protected set; }
	public string? DocumentMimeType { get; protected set; }
	public string? DocumentFileName { get; protected set; }

	public override string Category => SampleCategories.Documents;

	public override byte[]? DownloadBytes => DocumentBytes;
	public override string DownloadFileName => DocumentFileName ?? "document.pdf";
	public override string DownloadMimeType => DocumentMimeType ?? "application/pdf";

	protected abstract void OnGenerateDocument(SKCanvas previewCanvas, int width, int height);

	public void DrawSample(SKCanvas canvas, int width, int height)
	{
		if (IsInitialized)
		{
			canvas.Clear(SKColors.White);
			OnGenerateDocument(canvas, width, height);
		}
	}

	public override void UpdateControl(string id, object value)
	{
		OnControlChanged(id, value);
		DocumentBytes = null;
	}
}
