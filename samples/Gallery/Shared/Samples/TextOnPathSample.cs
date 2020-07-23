using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;

namespace SkiaSharpSample.Samples
{
	[Preserve(AllMembers = true)]
	public class TextOnPathSample : AnimatedSampleBase
	{
		private int animationIndex = 0;

		[Preserve]
		public TextOnPathSample()
		{
		}

		public override string Title => "Text on Path";

		public override SampleCategories Category => SampleCategories.Text;


		protected override void OnDrawSample(SKCanvas canvas, int width, int height)
		{
			const float textSize = 40;

			var alignments = new[] { SKTextAlign.Left, SKTextAlign.Center, SKTextAlign.Right };
			var warpings = new[] { false, true };
			var hOffsets = new[] { 0f, -textSize, textSize };
			var vOffsets = new[] { 0f, textSize / 2, textSize };
			var text = @"The quick brown fox jumps over the lazy dog!";

			canvas.Clear(SKColors.White);

			var index = animationIndex;

			// create a circular path
			using var path = SKPath.ParseSvgPathData("M 32 128 A 64 64 0 1 1 224 128 A 64 64 0 1 1 32 128");
			using var paint = new SKPaint
			{
				IsAntialias = true,
				TextAlign = SKTextAlign.Center,
				TextSize = textSize,
				StrokeWidth = 2
			};

			// Fit path in window.
			path.Transform(SKMatrix.CreateScale(width / 256f, height / 256f));

			// Pick text-on-path parameters.
			var alignment = Pick(alignments, ref index);
			var hOffset = Pick(hOffsets, ref index);
			var vOffset = Pick(vOffsets, ref index);
			var warping = Pick(warpings, ref index);

			// make sure the canvas is blank
			canvas.Clear(SKColors.SkyBlue);

			// draw the parameters
			paint.TextSize = 16;
			paint.Color = SKColors.White;
			paint.TextAlign = SKTextAlign.Left;
			canvas.DrawText($" Alignment: {alignment}", 0, paint.TextSize, paint);
			paint.TextAlign = SKTextAlign.Center;
			canvas.DrawText($"Warping: {(warping ? "on" : "off")}", width / 2f, paint.TextSize, paint);
			paint.TextAlign = SKTextAlign.Right;
			canvas.DrawText($"Offset: ({hOffset}, {vOffset}) ", width, paint.TextSize, paint);

			// draw the path
			paint.Color = SKColors.Blue;
			paint.Style = SKPaintStyle.Stroke;
			paint.TextSize = textSize;
			canvas.DrawPath(path, paint);

			// draw the text on the path
			paint.TextAlign = alignment;
			paint.Color = SKColors.Black;
			paint.Style = SKPaintStyle.Fill;
			canvas.DrawTextOnPath(text, path, new SKPoint(hOffset, vOffset), warping, paint);
		}

		protected override async Task OnUpdate(CancellationToken token)
		{
			await Task.Delay(1000, token);

			animationIndex += 1;
		}

		private T Pick<T>(T[] items, ref int index)
		{
			var item = items[index % items.Length];
			index /= items.Length;
			return item;
		}
	}
}
