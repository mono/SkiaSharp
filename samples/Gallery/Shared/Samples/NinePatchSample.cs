using System;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class NinePatchSample : CanvasSampleBase
{
	private float _width = 400f;
	private float _height = 300f;
	private bool _showGrid = true;
	private SKBitmap? cachedBitmap;

	public override string Title => "Nine-Patch Scaler";

	public override DateOnly? DateAdded => new DateOnly(2026, 3, 27);

	public override string Description => "Interactively resize 9-patch bitmaps with adjustable dimensions and lattice grid overlay.";

	public override string Category => SampleManager.General;

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new SliderControl("width", "Width", 100, 800, _width, 10),
		new SliderControl("height", "Height", 100, 600, _height, 10),
		new ToggleControl("showGrid", "Show Grid Lines", _showGrid),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "width":
				_width = (float)value;
				break;
			case "height":
				_height = (float)value;
				break;
			case "showGrid":
				_showGrid = (bool)value;
				break;
		}
	}

	protected override Task OnInit()
	{
		using var stream = new SKManagedStream(SampleMedia.Images.NinePatch);
		cachedBitmap = SKBitmap.Decode(stream);
		return base.OnInit();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		cachedBitmap?.Dispose();
		cachedBitmap = null;
	}

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		canvas.Clear(SKColors.White);

		if (cachedBitmap == null) return;

		var patchCenter = new SKRectI(33, 33, 256 - 33, 256 - 33);

		// Clamp dest rect to canvas
		var destW = Math.Min(_width, width - 20);
		var destH = Math.Min(_height, height - 20);
		var destX = (width - destW) / 2f;
		var destY = (height - destH) / 2f;
		var destRect = new SKRect(destX, destY, destX + destW, destY + destH);

		canvas.DrawBitmapNinePatch(cachedBitmap, patchCenter, destRect);

		if (_showGrid)
			DrawGridLines(canvas, cachedBitmap, patchCenter, destRect);
	}

	private static void DrawGridLines(SKCanvas canvas, SKBitmap bitmap, SKRectI patchCenter, SKRect dest)
	{
		using var dashEffect = SKPathEffect.CreateDash(new float[] { 6, 4 }, 0);
		using var paint = new SKPaint
		{
			Color = new SKColor(255, 0, 0, 128),
			Style = SKPaintStyle.Stroke,
			StrokeWidth = 1,
			IsAntialias = true,
			PathEffect = dashEffect,
		};

		// Map patch boundaries to dest rect
		var bw = (float)bitmap.Width;
		var bh = (float)bitmap.Height;
		var leftPatch = patchCenter.Left / bw;
		var rightPatch = patchCenter.Right / bw;
		var topPatch = patchCenter.Top / bh;
		var bottomPatch = patchCenter.Bottom / bh;

		// In 9-patch, the corners are fixed-size, so compute pixel offsets
		var leftEdge = patchCenter.Left;
		var rightEdge = bitmap.Width - patchCenter.Right;
		var topEdge = patchCenter.Top;
		var bottomEdge = bitmap.Height - patchCenter.Bottom;

		var x1 = dest.Left + leftEdge;
		var x2 = dest.Right - rightEdge;
		var y1 = dest.Top + topEdge;
		var y2 = dest.Bottom - bottomEdge;

		// Vertical lines
		canvas.DrawLine(x1, dest.Top, x1, dest.Bottom, paint);
		canvas.DrawLine(x2, dest.Top, x2, dest.Bottom, paint);

		// Horizontal lines
		canvas.DrawLine(dest.Left, y1, dest.Right, y1, paint);
		canvas.DrawLine(dest.Left, y2, dest.Right, y2, paint);

		// Outer border
		paint.Color = new SKColor(0, 100, 255, 128);
		canvas.DrawRect(dest, paint);
	}
}
