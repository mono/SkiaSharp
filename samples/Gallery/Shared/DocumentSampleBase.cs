using System;
using System.IO;
using SkiaSharp;

namespace SkiaSharpSample
{
	public abstract class DocumentSampleBase : SampleBase
	{
		protected string DocumentPath { get; set; }

		public override string Category => SampleCategories.Documents;

		protected abstract void OnGenerateDocument(string path);

		protected override void OnDrawSample(SKCanvas canvas, int width, int height)
		{
			canvas.Clear(SKColors.White);

			if (!string.IsNullOrEmpty(DocumentPath))
				OnGenerateDocument(DocumentPath);

			using var paint = new SKPaint
			{
				TextSize = 60.0f,
				IsAntialias = true,
				Color = 0xFF9CAFB7,
				StrokeWidth = 3,
				TextAlign = SKTextAlign.Center,
			};

			canvas.DrawText("Tap to open document", width / 2f, height / 3, paint);
		}

		protected override void OnTapped()
		{
			base.OnTapped();

			if (!string.IsNullOrEmpty(DocumentPath))
				SamplesManager.OnOpenFile(DocumentPath);
		}
	}
}
