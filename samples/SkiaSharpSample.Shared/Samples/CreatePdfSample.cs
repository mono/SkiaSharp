using System;
using System.IO;
using System.Threading.Tasks;
using PCLStorage;

using SkiaSharp;

namespace SkiaSharpSample.Samples
{
	[Preserve(AllMembers = true)]
	public class CreatePdfSample : SampleBase
	{
		private string root;

		[Preserve]
		public CreatePdfSample()
		{
		}

		protected override async Task OnInit()
		{
			await base.OnInit();

			// create the folder for this sample
			var local = FileSystem.Current.LocalStorage;
			local = await local.CreateFolderAsync("SkiaSharpSample", CreationCollisionOption.OpenIfExists);
			local = await local.CreateFolderAsync("CreatePdfSample", CreationCollisionOption.OpenIfExists);
			root = local.Path;
		}

		public override string Title => "Create PDF Document";

		public override SampleCategories Category => SampleCategories.Documents;

		protected override void OnDrawSample(SKCanvas canvas, int width, int height)
		{
			canvas.Clear(SKColors.White);

			using (var paint = new SKPaint())
			{
				paint.TextSize = 60.0f;
				paint.IsAntialias = true;
				paint.Color = (SKColor)0xFF9CAFB7;
				paint.StrokeWidth = 3;
				paint.TextAlign = SKTextAlign.Center;

				canvas.DrawText("tap to open PDF", width / 2f, height / 3, paint);
			}
		}

		protected override void OnTapped()
		{
			base.OnTapped();

			var path = Path.Combine(root, $"{Guid.NewGuid().ToString("N")}.pdf");

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

			using (var stream = new SKFileWStream(path))
			using (var document = SKDocument.CreatePdf(stream, metadata))
			using (var paint = new SKPaint())
			{
				paint.TextSize = 64.0f;
				paint.IsAntialias = true;
				paint.Color = (SKColor)0xFF9CAFB7;
				paint.IsStroke = true;
				paint.StrokeWidth = 3;
				paint.TextAlign = SKTextAlign.Center;

				var width = 840;
				var height = 1188;

				// draw page 1
				using (var pdfCanvas = document.BeginPage(width, height))
				{
					// draw button
					var nextPagePaint = new SKPaint
					{
						IsAntialias = true,
						TextSize = 16,
						Color = SKColors.OrangeRed
					};
					var nextText = "Next Page >>";
					var btn = new SKRect(width - nextPagePaint.MeasureText(nextText) - 24, 0, width, nextPagePaint.TextSize + 24);
					pdfCanvas.DrawText(nextText, btn.Left + 12, btn.Bottom - 12, nextPagePaint);
					// make button link
					pdfCanvas.DrawLinkDestinationAnnotation(btn, "next-page");

					// draw contents
					pdfCanvas.DrawText("...PDF 1/2...", width / 2, height / 4, paint);
					document.EndPage();
				}

				// draw page 2
				using (var pdfCanvas = document.BeginPage(width, height))
				{
					// draw link destintion
					pdfCanvas.DrawNamedDestinationAnnotation(SKPoint.Empty, "next-page");

					// draw contents
					pdfCanvas.DrawText("...PDF 2/2...", width / 2, height / 4, paint);
					document.EndPage();
				}

				// end the doc
				document.Close();
			}

			// display to the user
			SamplesManager.OnOpenFile(path);
		}
	}
}
