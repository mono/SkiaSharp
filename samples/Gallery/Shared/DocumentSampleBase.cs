using System;
using System.IO;
using SkiaSharp;

namespace SkiaSharpSample;

public abstract class DocumentSampleBase : SampleBase
{
	protected string DocumentPath { get; set; }

	public override string Category => SampleCategories.Documents;

	public override bool IsSupported =>
		!string.IsNullOrEmpty(SamplesManager.TempDataPath);

	protected abstract void OnGenerateDocument(string path);

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		canvas.Clear(SKColors.White);

		using var paint = new SKPaint
		{
			TextSize = 40.0f,
			IsAntialias = true,
			Color = 0xFF9CAFB7,
			StrokeWidth = 3,
			TextAlign = SKTextAlign.Center,
		};

		if (string.IsNullOrEmpty(DocumentPath))
		{
			canvas.DrawText("Documents not supported on this platform", width / 2f, height / 3, paint);
			return;
		}

		OnGenerateDocument(DocumentPath);
		canvas.DrawText("Tap to open document", width / 2f, height / 3, paint);
	}

	protected override void OnTapped()
	{
		base.OnTapped();

		if (!string.IsNullOrEmpty(DocumentPath))
			SamplesManager.OnOpenFile(DocumentPath);
	}
}
