using System;
using System.IO;
using System.Threading.Tasks;
using SkiaSharp;

namespace SkiaSharpSample.Samples
{
	public class CreateXpsSample : DocumentSampleBase
	{
		private bool isSupported = true;

		public CreateXpsSample()
		{
		}

		protected override async Task OnInit()
		{
			await base.OnInit();
			var root = SamplesManager.EnsureTempDataDirectory("CreateXpsSample");
			DocumentPath = Path.Combine(root, $"{Guid.NewGuid():N}.xps");
		}

		public override string Title => "Create XPS Document";

		public override bool IsSupported => OperatingSystem.IsWindows();

		protected override void OnGenerateDocument(string path)
		{
			if (!isSupported || File.Exists(path))
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

			using (var xpsCanvas = document.BeginPage(pageWidth, pageHeight))
			{
				xpsCanvas.DrawText("...XPS 1/2...", pageWidth / 2, pageHeight / 4, paint);
				document.EndPage();
			}

			using (var xpsCanvas = document.BeginPage(pageWidth, pageHeight))
			{
				xpsCanvas.DrawText("...XPS 2/2...", pageWidth / 2, pageHeight / 4, paint);
				document.EndPage();
			}

			document.Close();
		}
	}
}
