using System;
using System.IO;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class ImageDecoderSample : CanvasSampleBase
{
	private int _imageIndex;
	private bool _showInfo;
	private bool _subset;

	private static readonly string[] ImageSources = { "Baboon", "Color Wheel", "DNG", "WebP" };

	public override string Title => "Image Decoder";

	public override string Description => "Decode images in various formats (PNG, WebP, DNG) with metadata inspection and subset decoding.";

	public override string Category => SampleCategories.BitmapDecoding;

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new PickerControl("image", "Image Source", ImageSources, _imageIndex),
		new ToggleControl("subset", "Decode Subset", _subset),
		new ToggleControl("showInfo", "Show Metadata", _showInfo),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "image":
				_imageIndex = (int)value;
				break;
			case "showInfo":
				_showInfo = (bool)value;
				break;
			case "subset":
				_subset = (bool)value;
				break;
		}
	}

	private Stream GetImageStream() => _imageIndex switch
	{
		1 => SampleMedia.Images.ColorWheel,
		2 => SampleMedia.Images.AdobeDng,
		3 => SampleMedia.Images.BabyTux,
		_ => SampleMedia.Images.Baboon,
	};

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		canvas.Clear(SKColors.White);

		using var stream = GetImageStream();
		if (stream == null)
		{
			DrawErrorText(canvas, width, height, "Image not available");
			return;
		}

		using var data = SKData.Create(stream);
		using var codec = SKCodec.Create(data);
		if (codec == null)
		{
			DrawErrorText(canvas, width, height, "Unable to decode image");
			return;
		}

		var info = codec.Info;
		SKBitmap bitmap;

		if (_subset)
		{
			// Decode full image then extract center 50%
			using var fullBitmap = SKBitmap.Decode(codec);
			if (fullBitmap == null)
			{
				DrawErrorText(canvas, width, height, "Failed to decode bitmap");
				return;
			}
			var subsetWidth = Math.Max(1, info.Width / 2);
			var subsetHeight = Math.Max(1, info.Height / 2);
			var subsetLeft = (info.Width - subsetWidth) / 2;
			var subsetTop = (info.Height - subsetHeight) / 2;
			var subsetRect = new SKRectI(subsetLeft, subsetTop, subsetLeft + subsetWidth, subsetTop + subsetHeight);

			bitmap = new SKBitmap(subsetWidth, subsetHeight);
			if (!fullBitmap.ExtractSubset(bitmap, subsetRect))
			{
				bitmap.Dispose();
				DrawErrorText(canvas, width, height, "Failed to extract subset");
				return;
			}
		}
		else
		{
			bitmap = SKBitmap.Decode(codec);
		}

		if (bitmap == null)
		{
			DrawErrorText(canvas, width, height, "Failed to decode bitmap");
			return;
		}

		using (bitmap)
		{
			var destRect = CalculateDestRect(bitmap.Width, bitmap.Height, width, height);

			// Draw checkered background for transparency
			DrawCheckerboard(canvas, destRect);

			canvas.DrawBitmap(bitmap, destRect);

			if (_showInfo)
				DrawMetadata(canvas, width, height, codec, info);
		}
	}

	private static void DrawCheckerboard(SKCanvas canvas, SKRect rect)
	{
		var checkSize = 8f;
		using var light = new SKPaint { Color = new SKColor(230, 230, 230) };
		using var dark = new SKPaint { Color = SKColors.White };

		canvas.Save();
		canvas.ClipRect(rect);
		for (var y = rect.Top; y < rect.Bottom; y += checkSize)
		{
			for (var x = rect.Left; x < rect.Right; x += checkSize)
			{
				var col = (int)((x - rect.Left) / checkSize);
				var row = (int)((y - rect.Top) / checkSize);
				var paint = (col + row) % 2 == 0 ? light : dark;
				canvas.DrawRect(x, y, checkSize, checkSize, paint);
			}
		}
		canvas.Restore();
	}

	private static SKRect CalculateDestRect(int bmpWidth, int bmpHeight, int canvasWidth, int canvasHeight)
	{
		var padding = 10f;
		var availWidth = canvasWidth - padding * 2;
		var availHeight = canvasHeight - padding * 2;
		var scale = Math.Min(availWidth / bmpWidth, availHeight / bmpHeight);
		var dw = bmpWidth * scale;
		var dh = bmpHeight * scale;
		return new SKRect(
			(canvasWidth - dw) / 2f,
			(canvasHeight - dh) / 2f,
			(canvasWidth + dw) / 2f,
			(canvasHeight + dh) / 2f);
	}

	private static void DrawMetadata(SKCanvas canvas, int width, int height, SKCodec codec, SKImageInfo info)
	{
		var lines = new[]
		{
			$"Format: {codec.EncodedFormat}",
			$"Dimensions: {info.Width} × {info.Height}",
			$"Color Type: {info.ColorType}",
			$"Alpha Type: {info.AlphaType}",
		};

		using var bgPaint = new SKPaint
		{
			Color = new SKColor(0, 0, 0, 180),
			Style = SKPaintStyle.Fill,
		};
		var fontSize = Math.Max(14f, height / 30f);
		using var font = new SKFont { Size = fontSize };
		using var textPaint = new SKPaint
		{
			Color = SKColors.White,
			IsAntialias = true,
		};

		var lineHeight = fontSize * 1.4f;
		var boxHeight = lineHeight * lines.Length + 20;
		var boxTop = height - boxHeight;
		canvas.DrawRect(0, boxTop, width, boxHeight, bgPaint);

		var y = boxTop + lineHeight;
		foreach (var line in lines)
		{
			canvas.DrawText(line, 12, y, font, textPaint);
			y += lineHeight;
		}
	}

	private static void DrawErrorText(SKCanvas canvas, int width, int height, string message)
	{
		using var font = new SKFont { Size = 24 };
		using var paint = new SKPaint
		{
			Color = SKColors.Red,
			IsAntialias = true,
		};
		canvas.DrawText(message, width / 2f, height / 2f, SKTextAlign.Center, font, paint);
	}
}
