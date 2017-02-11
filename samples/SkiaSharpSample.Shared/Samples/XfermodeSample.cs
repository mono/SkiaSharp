using System;
using System.Linq;

using SkiaSharp;

namespace SkiaSharpSample.Samples
{
	[Preserve(AllMembers = true)]
	public class XfermodeSample : SampleBase
	{
		[Preserve]
		public XfermodeSample()
		{
		}

		public override string Title => "Blend Mode";

		public override SampleCategories Category => SampleCategories.Shaders;

		protected override void OnDrawSample(SKCanvas canvas, int width, int height)
		{
			var modes = Enum.GetValues(typeof(SKBlendMode)).Cast<SKBlendMode>().ToArray();

			var cols = width < height ? 3 : 5;
			var rows = (modes.Length - 1) / cols + 1;

			var w = (float)width / cols;
			var h = (float)height / rows;
			var rect = SKRect.Create(w, h);
			var srcPoints = new[] {
				new SKPoint (0.0f, 0.0f),
				new SKPoint (w, 0.0f)
			};
			var srcColors = new[] {
				SKColors.Magenta.WithAlpha (0),
				SKColors.Magenta
			};
			var dstPoints = new[] {
				new SKPoint (0.0f, 0.0f),
				new SKPoint (0.0f, h)
			};
			var dstColors = new[] {
				SKColors.Cyan.WithAlpha (0),
				SKColors.Cyan
			};

			using (var text = new SKPaint())
			using (var stroke = new SKPaint())
			using (var src = new SKPaint())
			using (var dst = new SKPaint())
			using (var srcShader = SKShader.CreateLinearGradient(srcPoints[0], srcPoints[1], srcColors, null, SKShaderTileMode.Clamp))
			using (var dstShader = SKShader.CreateLinearGradient(dstPoints[0], dstPoints[1], dstColors, null, SKShaderTileMode.Clamp))
			{
				text.TextSize = 12.0f;
				text.IsAntialias = true;
				text.TextAlign = SKTextAlign.Center;
				stroke.IsStroke = true;
				src.Shader = srcShader;
				dst.Shader = dstShader;

				canvas.Clear(SKColors.White);

				for (var i = 0; i < modes.Length; ++i)
				{
					using (new SKAutoCanvasRestore(canvas, true))
					{
						canvas.Translate(w * (i / rows), h * (i % rows));

						canvas.ClipRect(rect);
						canvas.DrawColor(SKColors.LightGray);

						canvas.SaveLayer(null);
						canvas.Clear(SKColors.Transparent);
						canvas.DrawPaint(dst);

						src.BlendMode = modes[i];
						canvas.DrawPaint(src);
						canvas.DrawRect(rect, stroke);

						var desc = modes[i].ToString();
						canvas.DrawText(desc, w / 2f, h / 2f, text);
					}
				}
			}
		}
	}
}
