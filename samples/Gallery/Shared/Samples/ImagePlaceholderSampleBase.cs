using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharpSample.Controls;
using SkiaSharpSample.ImagePlaceholders;

namespace SkiaSharpSample.Samples;

public abstract class ImagePlaceholderSampleBase : CanvasSampleBase
{
	private const int DefaultPreviewDimension = 64;

	private readonly Stopwatch clock = new();
	private PlaceholderImageView? view;
	private SKBitmap? bitmap;
	private string hash = string.Empty;
	private int imageIndex;
	private int previewWidth = DefaultPreviewDimension;
	private int previewHeight = DefaultPreviewDimension;
	private int backdrop;

	protected abstract string EncodeHash(SKBitmap source);
	protected abstract SKBitmap DecodeHash(string value, int width, int height);
	protected virtual bool SupportsAlpha => false;
	protected virtual IReadOnlyList<SampleControl> CodecControls => [];
	protected virtual bool UpdateCodecControl(string id, object value) => false;
	protected virtual bool UpdatePreviewControl(string id, object value) => false;

	public override string Category => SampleManager.BitmapDecoding;
	public override DateOnly? DateAdded => new DateOnly(2026, 9, 29);
	public override bool IsAnimated => true;
	public override IReadOnlyList<string> ApiTags =>
	[
		"SKBitmap", "SKImage", "SKCanvas.DrawImage", "SKPixmap", "SKSamplingOptions",
	];

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new PickerControl("image", "Image Source", SampleMedia.Images.DemoImageNames, imageIndex),
		..CodecControls,
		new SliderControl("previewWidth", "Preview width", 16, 128, previewWidth, 1),
		new SliderControl("previewHeight", "Preview height", 16, 128, previewHeight, 1),
		..(SupportsAlpha
			? (IReadOnlyList<SampleControl>)[new PickerControl("backdrop", "Transparency backdrop", ["White", "Checkerboard", "Dark"], backdrop)]
			: []),
		new CopyTextControl("hash", "Hash", hash),
	];

	protected override Task OnInit()
	{
		view = new PlaceholderImageView();
		clock.Restart();
		LoadImage();
		NotifyControlsChanged();
		return base.OnInit();
	}

	protected override bool OnUpdate(TimeSpan elapsed) => bitmap is not null;

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "image":
				imageIndex = (int)value;
				LoadImage();
				break;
			case "previewWidth":
				previewWidth = (int)(float)value;
				DecodePreview();
				break;
			case "previewHeight":
				previewHeight = (int)(float)value;
				DecodePreview();
				break;
			case "backdrop":
				backdrop = (int)value;
				break;
			default:
				if (UpdatePreviewControl(id, value))
					DecodePreview();
				else if (UpdateCodecControl(id, value))
					GenerateHash();
				break;
		}
		NotifyControlsChanged();
	}

	private void LoadImage()
	{
		using var stream = SampleMedia.Images.DemoImage(imageIndex);
		var next = SKBitmap.Decode(stream)
			?? throw new InvalidDataException($"Could not decode {SampleMedia.Images.DemoImageNames[imageIndex]}.");
		bitmap?.Dispose();
		bitmap = next;
		GenerateHash();
	}

	private void GenerateHash()
	{
		if (bitmap is null || view is null)
			return;
		var nextHash = EncodeHash(bitmap);
		using var preview = DecodeHash(nextHash, previewWidth, previewHeight);
		view.SetPlaceholder(SKImage.FromBitmap(preview));
		// All sources join the same sample-level cycle instead of restarting it.
		view.SetImage(SKImage.FromBitmap(bitmap), TimeSpan.Zero);
		hash = nextHash;
	}

	private void DecodePreview()
	{
		if (view is null || string.IsNullOrEmpty(hash))
			return;
		using var preview = DecodeHash(hash, previewWidth, previewHeight);
		view.SetPlaceholder(SKImage.FromBitmap(preview));
	}

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		canvas.Clear(SKColors.White);
		var gap = Math.Max(8f, Math.Min(width, height) * 0.025f);
		var cellWidth = Math.Max(0, (width - 3 * gap) / 2);
		var cellHeight = Math.Max(0, (height - 3 * gap) / 2);
		DrawPanel(canvas, SKRect.Create(gap, gap, cellWidth, cellHeight), "Hash",
			(c, r) => view?.DrawPlaceholder(c, r));
		DrawPanel(canvas, SKRect.Create(2 * gap + cellWidth, gap, cellWidth, cellHeight), "Image",
			(c, r) => view?.DrawFullImage(c, r));
		DrawPanel(canvas, SKRect.Create((width - cellWidth) / 2, 2 * gap + cellHeight, cellWidth, cellHeight),
			"Transition", (c, r) => view?.Draw(c, r, clock.Elapsed));
	}

	private void DrawPanel(SKCanvas canvas, SKRect panel, string label, Action<SKCanvas, SKRect> draw)
	{
		if (panel.Width <= 0 || panel.Height <= 0)
			return;
		using var paint = new SKPaint { IsAntialias = true };
		var dark = backdrop == 2 && SupportsAlpha;
		paint.Color = dark ? new SKColor(35, 40, 48) : SKColors.White;
		canvas.DrawRect(panel, paint);
		paint.Color = dark ? SKColors.White : new SKColor(60, 64, 70);
		using var font = new SKFont(SampleMedia.Fonts.Default, Math.Clamp(panel.Height * 0.07f, 12, 18));
		canvas.DrawText(label, panel.Left + 10, panel.Top + font.Size + 5, SKTextAlign.Left, font, paint);
		var content = SKRect.Create(panel.Left + 6, panel.Top + font.Size + 10,
			Math.Max(0, panel.Width - 12), Math.Max(0, panel.Height - font.Size - 16));
		if (backdrop == 1 && SupportsAlpha)
			DrawCheckerboard(canvas, content);
		draw(canvas, content);
		paint.Color = new SKColor(207, 213, 219);
		paint.Style = SKPaintStyle.Stroke;
		canvas.DrawRect(panel, paint);
	}

	private static void DrawCheckerboard(SKCanvas canvas, SKRect rect)
	{
		using var light = new SKPaint { Color = new SKColor(234, 234, 234) };
		using var dark = new SKPaint { Color = new SKColor(178, 178, 178) };
		const int tile = 24;
		canvas.Save();
		canvas.ClipRect(rect);
		for (var y = 0; y < rect.Height / tile; y++)
		for (var x = 0; x < rect.Width / tile; x++)
			canvas.DrawRect(rect.Left + x * tile, rect.Top + y * tile, tile, tile,
				(x + y) % 2 == 0 ? light : dark);
		canvas.Restore();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		bitmap?.Dispose();
		bitmap = null;
		view?.Dispose();
		view = null;
		clock.Stop();
	}
}
