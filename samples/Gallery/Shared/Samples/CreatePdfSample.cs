using System;
using System.IO;
using System.Threading.Tasks;

using SkiaSharp;

namespace SkiaSharpSample.Samples
{
	[Preserve(AllMembers = true)]
	public class CreatePdfSample : SampleBase
	{
		private string path;
		private bool isSupported = true;

		[Preserve]
		public CreatePdfSample()
		{
		}

		protected override async Task OnInit()
		{
			await base.OnInit();

			// create the folder for this sample
			var root = SamplesManager.EnsureTempDataDirectory("CreatePdfSample");
			path = Path.Combine(root, $"{Guid.NewGuid():N}.pdf");
		}

		public override string Title => "Create PDF Document";

		public override SampleCategories Category => SampleCategories.Documents;

		protected override void OnDrawSample(SKCanvas canvas, int width, int height)
		{
			canvas.Clear(SKColors.White);

			GenerateDocument();

			using var paint = new SKPaint
			{
				TextSize = 60.0f,
				IsAntialias = true,
				Color = 0xFF9CAFB7,
				StrokeWidth = 3,
				TextAlign = SKTextAlign.Center
			};

			canvas.DrawText(isSupported ? "tap to open PDF" : "Oops! No PDF support!", width / 2f, height / 3, paint);
		}

		private void GenerateDocument()
		{
			if (!isSupported || (isSupported && File.Exists(path)))
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

			// draw page 1
			using (var pdfCanvas = document.BeginPage(pageWidth, pageHeight))
			{
				// draw button
				using var nextPagePaint = new SKPaint
				{
					IsAntialias = true,
					TextSize = 16,
					Color = SKColors.OrangeRed
				};
				var nextText = "Next Page >>";
				var btn = new SKRect(pageWidth - nextPagePaint.MeasureText(nextText) - 24, 0, pageWidth, nextPagePaint.TextSize + 24);
				pdfCanvas.DrawText(nextText, btn.Left + 12, btn.Bottom - 12, nextPagePaint);
				// make button link
				pdfCanvas.DrawLinkDestinationAnnotation(btn, "next-page");

				// draw contents
				pdfCanvas.DrawText("...PDF 1/2...", pageWidth / 2, pageHeight / 4, paint);
				document.EndPage();
			}

			// draw page 2
			using (var pdfCanvas = document.BeginPage(pageWidth, pageHeight))
			{
				// draw link destintion
				pdfCanvas.DrawNamedDestinationAnnotation(SKPoint.Empty, "next-page");

				// draw contents
				pdfCanvas.DrawText("...PDF 2/2...", pageWidth / 2, pageHeight / 4, paint);
				document.EndPage();
			}

			// end the doc
			document.Close();
		}

		protected override void OnTapped()
		{
			base.OnTapped();

			// display to the user
			SamplesManager.OnOpenFile(path);
		}
	}
}
