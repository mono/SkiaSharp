using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class PaintFastBoundsSample : CanvasSampleBase
{
	private static readonly string[] blurStyles = ["Normal", "Solid", "Outer", "Inner"];

	private float sigma = 12f;
	private float strokeWidth = 6f;
	private int blurStyleIndex = 0;
	private bool showBounds = true;

	public override string Title => "Paint Fast Bounds";

	public override DateOnly? DateAdded => new DateOnly(2026, 6, 27);

	public override string Category => SampleManager.ImageFilters;

	public override string Description =>
		"Visualize SKPaint.GetFastBounds. The dashed rectangle is the conservative " +
		"device-space region the paint can draw into - the renderer uses it to quick-reject " +
		"draws and to size offscreen layers. GetFastBounds composes every effect on the " +
		"paint, so the bounds account for both the stroke width and the blur mask filter. " +
		"Increase the stroke or blur and watch the bounds grow.";

	public override IReadOnlyList<string> ApiTags =>
	[
		"SKPaint", "SKPaint.GetFastBounds",
		"SKMaskFilter.CreateBlur", "SKBlurStyle", "SKCanvas.DrawRect",
	];

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new SliderControl("sigma", "Blur Sigma", 0, 40, sigma, 0.5f),
		new SliderControl("stroke", "Stroke Width", 0, 30, strokeWidth, 0.5f),
		new PickerControl("style", "Blur Style", blurStyles, blurStyleIndex),
		new ToggleControl("bounds", "Show Fast Bounds", showBounds),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "sigma":
				sigma = (float)value;
				break;
			case "stroke":
				strokeWidth = (float)value;
				break;
			case "style":
				blurStyleIndex = (int)value;
				break;
			case "bounds":
				showBounds = (bool)value;
				break;
		}
	}

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		canvas.Clear(SKColors.White);

		var style = (SKBlurStyle)blurStyleIndex;

		// The geometry that will be drawn, centered in the canvas.
		var size = Math.Min(width, height) * 0.4f;
		var shape = new SKRect(
			(width - size) / 2f,
			(height - size) / 2f,
			(width + size) / 2f,
			(height + size) / 2f);

		using var blur = SKMaskFilter.CreateBlur(style, sigma);

		// A paint that both strokes and blurs the shape.
		using var paint = new SKPaint
		{
			IsAntialias = true,
			Style = SKPaintStyle.Stroke,
			StrokeWidth = strokeWidth,
			StrokeJoin = SKStrokeJoin.Round,
			StrokeCap = SKStrokeCap.Round,
			Color = SKColors.MediumPurple,
			MaskFilter = blur,
		};

		// The conservative bounds the paint can draw into, composing the stroke and the blur.
		paint.GetFastBounds(shape, out var fastBounds);

		// Draw the stroked, blurred shape.
		canvas.DrawRect(shape, paint);

		if (showBounds)
		{
			// The original geometry, for reference.
			using var shapeStroke = new SKPaint
			{
				IsAntialias = true,
				Style = SKPaintStyle.Stroke,
				StrokeWidth = 1,
				Color = SKColors.Gray,
			};
			canvas.DrawRect(shape, shapeStroke);

			// The fast bounds returned by GetFastBounds.
			using var dash = SKPathEffect.CreateDash([8f, 6f], 0);
			using var boundsStroke = new SKPaint
			{
				IsAntialias = true,
				Style = SKPaintStyle.Stroke,
				StrokeWidth = 2,
				Color = SKColors.OrangeRed,
				PathEffect = dash,
			};
			canvas.DrawRect(fastBounds, boundsStroke);

			// Label the bounds size.
			using var font = new SKFont { Size = 16 };
			using var label = new SKPaint { IsAntialias = true, Color = SKColors.OrangeRed };
			canvas.DrawText($"bounds {fastBounds.Width:0} x {fastBounds.Height:0}px", 12, height - 16, SKTextAlign.Left, font, label);
		}
	}
}
