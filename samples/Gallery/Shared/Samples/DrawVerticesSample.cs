using System;

using SkiaSharp;

namespace SkiaSharpSample.Samples
{
	[Preserve(AllMembers = true)]
	public class DrawVerticesSample : SampleBase
	{
		[Preserve]
		public DrawVerticesSample()
		{
		}

		public override string Title => "Draw Vertices";

		public override SampleCategories Category => SampleCategories.General;

		protected override void OnDrawSample(SKCanvas canvas, int width, int height)
		{
			canvas.Clear(SKColors.White);
			var paint = new SKPaint
			{
				IsAntialias = true
			};

			var vertices = new[] { new SKPoint(110, 20), new SKPoint(160, 200), new SKPoint(10, 200) };
			var colors = new[] { SKColors.Red, SKColors.Green, SKColors.Blue };

			canvas.DrawVertices(SKVertexMode.Triangles, vertices, colors, paint);
		}
	}
}
