using System;
using System.IO;
using System.Threading.Tasks;
using SkiaSharp;

namespace SkiaSharpSample.Samples
{
	public class CreatePdfSample : DocumentSampleBase
	{
		private bool isSupported = true;

		public CreatePdfSample()
		{
		}

		protected override async Task OnInit()
		{
			await base.OnInit();
			var root = SamplesManager.EnsureTempDataDirectory("CreatePdfSample");
			DocumentPath = Path.Combine(root, $"{Guid.NewGuid():N}.pdf");
		}

		public override string Title => "Create PDF Document";

		protected override void OnGenerateDocument(string path)
		{
			if (!isSupported || File.Exists(path))
				return;

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
				isSupported = false;
				return;
			}

			using var paint = new SKPaint
			{
				TextSize = 64.0f,
				IsAntialias = true,
				Color = 0xFF9CAFB7,
				IsStroke = true,
				StrokeWidth = 3,
				TextAlign = SKTextAlign.Center
			};

			var pageWidth = 840;
			var pageHeight = 1188;

			using (var pdfCanvas = document.BeginPage(pageWidth, pageHeight))
			{
				pdfCanvas.DrawText("...PDF 1/2...", pageWidth / 2, pageHeight / 4, paint);
				document.EndPage();
			}

			using (var pdfCanvas = document.BeginPage(pageWidth, pageHeight))
			{
				pdfCanvas.DrawText("...PDF 2/2...", pageWidth / 2, pageHeight / 4, paint);
				document.EndPage();
			}

			document.Close();
		}
	}
}
