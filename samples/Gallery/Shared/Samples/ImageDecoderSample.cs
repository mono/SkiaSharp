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

	private static readonly string[] ImageSources = { "Baboon", "Color Wheel", "HDR PNG (CICP)", "DNG", "WebP", "GIF" };

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
		2 => SampleMedia.Images.CicpPq,
		3 => SampleMedia.Images.AdobeDng,
		4 => SampleMedia.Images.BabyTux,
		5 => SampleMedia.Images.AnimatedHeartGif,
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
		var lines = BuildMetadataLines(codec, info);

		var fontSize = Math.Max(7f, height / 60f);
		var padding = Math.Max(8f, fontSize * 0.85f);
		var cornerRadius = Math.Max(8f, fontSize);

		using var bgPaint = new SKPaint
		{
			Color = new SKColor(20, 24, 33, 210),
			Style = SKPaintStyle.Fill,
			IsAntialias = true,
		};
		using var borderPaint = new SKPaint
		{
			Color = new SKColor(255, 255, 255, 45),
			Style = SKPaintStyle.Stroke,
			StrokeWidth = 1,
			IsAntialias = true,
		};
		using var font = new SKFont(SampleMedia.Fonts.Default, fontSize);
		using var textPaint = new SKPaint
		{
			Color = SKColors.White,
			IsAntialias = true,
		};

		var lineHeight = fontSize * 1.2f;
		var maxTextWidth = 0f;
		foreach (var line in lines)
			maxTextWidth = Math.Max(maxTextWidth, font.MeasureText(line, textPaint));

		var boxWidth = Math.Min(width * 0.5f, maxTextWidth + padding * 2f);
		var boxHeight = lineHeight * lines.Count + padding * 2f;
		var boxLeft = width - boxWidth - 12f;
		var boxTop = height - boxHeight - 12f;
		var boxRect = new SKRoundRect(new SKRect(boxLeft, boxTop, boxLeft + boxWidth, boxTop + boxHeight), cornerRadius, cornerRadius);

		canvas.DrawRoundRect(boxRect, bgPaint);
		canvas.DrawRoundRect(boxRect, borderPaint);

		var y = boxTop + padding + fontSize;
		foreach (var line in lines)
		{
			canvas.DrawText(line, boxLeft + padding, y, font, textPaint);
			y += lineHeight;
		}
	}

	private static IReadOnlyList<string> BuildMetadataLines(SKCodec codec, SKImageInfo info)
	{
		var lines = new List<string>
		{
			$"Format: {codec.EncodedFormat}",
			$"Size: {info.Width} x {info.Height}",
			$"Pixels: {info.ColorType} / {info.AlphaType}",
			$"Origin: {codec.EncodedOrigin}",
			$"Frames: {GetFrameDescription(codec)}",
		};

		AddColorSpaceLines(lines, info.ColorSpace);
		return lines;
	}

	private static void AddColorSpaceLines(List<string> lines, SKColorSpace? colorSpace)
	{
		if (colorSpace is null)
		{
			lines.Add("Color: Unspecified");
			return;
		}

		var xyz = colorSpace.ToColorSpaceXyz();
		lines.Add($"Color: {GetColorSpaceName(colorSpace, xyz)}");
		lines.Add($"Transfer: {GetTransferName(colorSpace)}");

		using var profile = colorSpace.ToProfile();
		if (profile.Size > 0)
			lines.Add($"ICC: {FormatByteCount(profile.Size)}");

		if (!xyz.Equals(SKColorSpaceXyz.Empty))
			lines.Add($"XYZ D50[0]: {FormatMatrixRow(xyz)}");
	}

	private static string GetColorSpaceName(SKColorSpace colorSpace, SKColorSpaceXyz xyz)
	{
		if (colorSpace.IsSrgb || xyz.Equals(SKColorSpaceXyz.Srgb))
			return "sRGB / Rec709";
		if (xyz.Equals(SKColorSpaceXyz.DisplayP3))
			return "Display P3";
		if (xyz.Equals(SKColorSpaceXyz.Rec2020))
			return "Rec2020";
		if (xyz.Equals(SKColorSpaceXyz.AdobeRgb))
			return "Adobe RGB";
		if (xyz.Equals(SKColorSpaceXyz.Xyz))
			return "XYZ";

		return "Custom / wide gamut";
	}

	private static string GetTransferName(SKColorSpace colorSpace)
	{
		if (colorSpace.GammaIsLinear)
			return "Linear";

		if (colorSpace.GetNumericalTransferFunction(out var fn))
		{
			if (fn.Equals(SKColorSpaceTransferFn.Srgb))
				return "sRGB";
			if (fn.Equals(SKColorSpaceTransferFn.TwoDotTwo))
				return "Gamma 2.2";
			if (fn.Equals(SKColorSpaceTransferFn.Rec2020))
				return "Rec2020";
			if (fn.Equals(SKColorSpaceTransferFn.Pq))
				return "PQ (ST 2084)";
			if (fn.Equals(SKColorSpaceTransferFn.Hlg))
				return "HLG";

			var values = fn.Values;
			return $"Custom (G={values[0]:0.###})";
		}

		return colorSpace.GammaIsCloseToSrgb ? "sRGB-like" : "Unknown";
	}

	private static string FormatMatrixRow(SKColorSpaceXyz xyz)
	{
		var values = xyz.Values;
		return $"{values[0]:0.###}, {values[1]:0.###}, {values[2]:0.###}";
	}

	private static string GetFrameDescription(SKCodec codec) =>
		codec.FrameCount > 0 ? codec.FrameCount.ToString() : "Static image";

	private static string FormatByteCount(long size) =>
		size >= 1024 ? $"{size / 1024d:0.#} KB" : $"{size} B";

	private static void DrawErrorText(SKCanvas canvas, int width, int height, string message)
	{
		using var font = new SKFont(SampleMedia.Fonts.Default, 24);
		using var paint = new SKPaint
		{
			Color = SKColors.Red,
			IsAntialias = true,
		};
		canvas.DrawText(message, width / 2f, height / 2f, SKTextAlign.Center, font, paint);
	}
}
