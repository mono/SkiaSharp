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
			if (await local.CheckExistsAsync("SkiaSharpSample") == ExistenceCheckResult.NotFound)
			{
				local = await local.CreateFolderAsync("SkiaSharpSample", CreationCollisionOption.OpenIfExists);
			}
			if (await local.CheckExistsAsync("CreatePdfSample") == ExistenceCheckResult.NotFound)
			{
				local = await local.CreateFolderAsync("CreatePdfSample", CreationCollisionOption.OpenIfExists);
			}
			root = local.Path;
		}

		public override string Title => "Create PDF";

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

			using (var stream = new SKFileWStream(path))
			using (var document = SKDocument.CreatePdf(stream))
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
					pdfCanvas.DrawText("...PDF 1/2...", width / 2, height / 4, paint);
					document.EndPage();
				}

				// draw page 2
				using (var pdfCanvas = document.BeginPage(width, height))
				{
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
