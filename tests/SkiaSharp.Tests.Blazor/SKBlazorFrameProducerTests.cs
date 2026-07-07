using SkiaSharp;
using SkiaSharp.Views.Blazor;
using SkiaSharp.Views.Blazor.Internal;
using Xunit;

namespace SkiaSharp.Tests.Blazor;

public class SKBlazorFrameProducerTests
{
	private static SKImage CreateImage(int width = 8, int height = 8, SKColor? color = null)
	{
		var info = new SKImageInfo(width, height, SKBlazorFrameProducer.RgbaColorType, SKAlphaType.Premul);
		using var surface = SKSurface.Create(info);
		surface.Canvas.Clear(color ?? SKColors.Red);
		return surface.Snapshot();
	}

	[Fact]
	public void PngProducesPngSignature()
	{
		using var image = CreateImage();

		var bytes = SKBlazorFrameProducer.Produce(image, SKBlazorTransferFormat.Png, 100);

		Assert.True(bytes.Length > 8);
		// PNG signature: 89 50 4E 47
		Assert.Equal(0x89, bytes[0]);
		Assert.Equal(0x50, bytes[1]);
		Assert.Equal(0x4E, bytes[2]);
		Assert.Equal(0x47, bytes[3]);
		Assert.Equal("image/png", SKBlazorFrameProducer.GetContentType(SKBlazorTransferFormat.Png));
	}

	[Fact]
	public void JpegProducesJpegSignature()
	{
		using var image = CreateImage();

		var bytes = SKBlazorFrameProducer.Produce(image, SKBlazorTransferFormat.Jpeg, 80);

		Assert.True(bytes.Length > 3);
		// JPEG start-of-image marker: FF D8
		Assert.Equal(0xFF, bytes[0]);
		Assert.Equal(0xD8, bytes[1]);
		Assert.Equal("image/jpeg", SKBlazorFrameProducer.GetContentType(SKBlazorTransferFormat.Jpeg));
	}

	[Fact]
	public void PutProducesRawRgbaOfExpectedLength()
	{
		using var image = CreateImage(4, 3, SKColors.Red);

		var bytes = SKBlazorFrameProducer.Produce(image, SKBlazorTransferFormat.Put, 0);

		Assert.Equal(4 * 3 * 4, bytes.Length);
		// first pixel is opaque red in RGBA order (directly usable by ImageData/texImage2D)
		Assert.Equal(255, bytes[0]);
		Assert.Equal(0, bytes[1]);
		Assert.Equal(0, bytes[2]);
		Assert.Equal(255, bytes[3]);
		Assert.Equal("application/octet-stream", SKBlazorFrameProducer.GetContentType(SKBlazorTransferFormat.Put));
	}

	[Fact]
	public void PutPreservesTransparency()
	{
		using var image = CreateImage(2, 2, SKColors.Transparent);

		var bytes = SKBlazorFrameProducer.Produce(image, SKBlazorTransferFormat.Put, 0);

		// fully transparent => alpha 0
		Assert.Equal(0, bytes[3]);
	}

	[Theory]
	[InlineData(-50)]
	[InlineData(50)]
	[InlineData(500)]
	public void JpegQualityIsClampedAndAlwaysProducesData(int quality)
	{
		using var image = CreateImage();

		var bytes = SKBlazorFrameProducer.Produce(image, SKBlazorTransferFormat.Jpeg, quality);

		Assert.NotEmpty(bytes);
	}
}
