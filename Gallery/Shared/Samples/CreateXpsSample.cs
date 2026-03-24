using System;
using System.IO;
using System.Threading.Tasks;

using SkiaSharp;

namespace SkiaSharpSample.Samples
{
	[Preserve(AllMembers = true)]
	public class CreateXpsSample : SampleBase
	{
		private string path;
		private bool isSupported = true;

		[Preserve]
		public CreateXpsSample()
		{
		}

		protected override async Task OnInit()
		{
			await base.OnInit();

			// create the folder for this sample
			var root = SamplesManager.EnsureTempDataDirectory("CreateXpsSample");
			path = Path.Combine(root, $"{Guid.NewGuid():N}.xps");
		}

		public override string Title => "Create XPS Document";

		public override SampleCategories Category => SampleCategories.Documents;

		public override SamplePlatforms SupportedPlatform => SamplePlatforms.AllWindows;

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

			canvas.DrawText(isSupported ? "tap to open XPS" : "Oops! No XPS support!", width / 2f, height / 3, paint);
		}

		private void GenerateDocument()
		{
			if (!isSupported || (isSupported && File.Exists(path)))
				return;

			using var document = SKDocument.CreateXps(path);

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
			using (var xpsCanvas = document.BeginPage(pageWidth, pageHeight))
			{
				// draw contents
				xpsCanvas.DrawText("...XPS 1/2...", pageWidth / 2, pageHeight / 4, paint);
				document.EndPage();
			}

			// draw page 2
			using (var xpsCanvas = document.BeginPage(pageWidth, pageHeight))
			{
				// draw contents
				xpsCanvas.DrawText("...XPS 2/2...", pageWidth / 2, pageHeight / 4, paint);
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
