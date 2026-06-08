using System;
using System.Runtime.InteropServices;
using Xunit;

namespace SkiaSharp.Tests.Wasm;

public class WasmTests
{
	[Fact]
	public void CheckVersion()
	{
		var native = SkiaSharpVersion.Native;

		Assert.True(native.Major > 0);
		Assert.True(native.Minor >= 0);
	}

	[Fact]
	public void CheckHarfBuzz()
	{
		var blob = HarfBuzzSharp.Blob.Empty;
		Assert.NotNull(blob);
		Assert.Equal(0, blob.Length);
	}

	[Fact]
	public void CanCreateSurface()
	{
		var info = new SKImageInfo(100, 100);
		using var surface = SKSurface.Create(info);

		Assert.NotNull(surface);
	}

	[Fact]
	public void CanDrawOnCanvas()
	{
		var info = new SKImageInfo(100, 100);
		using var surface = SKSurface.Create(info);
		var canvas = surface.Canvas;

		canvas.Clear(SKColors.White);

		using var paint = new SKPaint();
		paint.Color = SKColors.Red;
		paint.IsAntialias = true;

		canvas.DrawCircle(50, 50, 25, paint);

		using var image = surface.Snapshot();
		Assert.NotNull(image);
		Assert.Equal(100, image.Width);
		Assert.Equal(100, image.Height);
	}

	[Fact]
	public void CanSerializeAndDeserializePicture()
	{
		using var recorder = new SKPictureRecorder();
		using var canvas = recorder.BeginRecording(SKRect.Create(0, 0, 40, 40));
		using var picture = recorder.EndRecording();

		using var data = picture.Serialize();

		using var deserialized = SKPicture.Deserialize(data);

		Assert.NotNull(deserialized);
	}

	[Fact]
	public void CanCreatePath()
	{
		using var builder = new SKPathBuilder();
		builder.MoveTo(0, 0);
		builder.LineTo(100, 100);
		builder.LineTo(0, 100);
		builder.Close();

		using var path = builder.Detach();

		Assert.Equal(3, path.PointCount);
	}

	[Fact]
	public void CanCreateTypeface()
	{
		using var typeface = SKTypeface.Default;
		Assert.NotNull(typeface);
		Assert.NotNull(typeface.FamilyName);
	}

	[Fact]
	public void CanEncodeImage()
	{
		var info = new SKImageInfo(50, 50);
		using var surface = SKSurface.Create(info);
		surface.Canvas.Clear(SKColors.Blue);

		using var image = surface.Snapshot();
		using var data = image.Encode(SKEncodedImageFormat.Png, 100);

		Assert.NotNull(data);
		Assert.True(data.Size > 0);
	}

	[Fact]
	public void CanCreateMatrix()
	{
		var matrix = SKMatrix.CreateRotation(45);
		Assert.NotEqual(SKMatrix.Identity, matrix);
	}

	[Fact]
	public void CanCreateColorFilter()
	{
		using var filter = SKColorFilter.CreateBlendMode(SKColors.Red, SKBlendMode.Multiply);
		Assert.NotNull(filter);
	}
}
