using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows.Media.Imaging;
using Microsoft.Maui.Controls;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui.Controls.Hosting;
using SkiaSharp.Views.Maui.Controls.WPF.Handlers;
using SkiaSharp.Views.Maui.Controls.WPF.Platform;
using Xunit;

namespace SkiaSharp.Views.Maui.Controls.WPF.Tests;

public class WpfHandlerTests
{
	[Fact]
	public void WpfRegistrationSharesStableHostingNamespace()
	{
		Assert.Equal(typeof(AppHostBuilderExtensions).Namespace, typeof(WpfAppHostBuilderExtensions).Namespace);
	}

	[Fact]
	public void CanvasMapperIncludesAllCanvasPropertiesAndCommands()
	{
		Assert.NotNull(WpfSKCanvasViewHandler.Mapper[nameof(ISKCanvasView.EnableTouchEvents)]);
		Assert.NotNull(WpfSKCanvasViewHandler.Mapper[nameof(ISKCanvasView.IgnorePixelScaling)]);
		Assert.NotNull(WpfSKCanvasViewHandler.CommandMapper[nameof(ISKCanvasView.InvalidateSurface)]);
	}

	[Fact]
	public void GlMapperIncludesAllGpuPropertiesAndCommands()
	{
		Assert.NotNull(WpfSKGLViewHandler.Mapper[nameof(ISKGLView.EnableTouchEvents)]);
		Assert.NotNull(WpfSKGLViewHandler.Mapper[nameof(ISKGLView.IgnorePixelScaling)]);
		Assert.NotNull(WpfSKGLViewHandler.Mapper[nameof(ISKGLView.HasRenderLoop)]);
		Assert.NotNull(WpfSKGLViewHandler.CommandMapper[nameof(ISKGLView.InvalidateSurface)]);
	}

	[Theory]
	[InlineData(false, 50, 75)]
	[InlineData(true, 25, 30)]
	public void TouchCoordinatesFollowCanvasScaling(bool ignorePixelScaling, float expectedX, float expectedY)
	{
		var point = WpfCanvasMetrics.GetTouchLocation(25, 30, 2, 2.5, ignorePixelScaling);
		Assert.Equal(expectedX, point.X);
		Assert.Equal(expectedY, point.Y);
	}

	[Fact]
	public void GlLogicalSizeMatchesWpfSoftwareCanvasRounding()
	{
		Assert.Equal(new SKSizeI(100, 200), WpfCanvasMetrics.GetLogicalSize(100.8, 200.9));
	}

	[Fact]
	public void ImageMapperLeavesOrdinarySourcesToBackend()
	{
		Assert.False(WpfSkiaImageMapper.TryCreateBitmap(new FileImageSource { File = "test.png" }, out var bitmap));
		Assert.Null(bitmap);
	}

	[Fact]
	public void ImageMapperSelectsSkiaThenOrdinarySourceBranches()
	{
		Assert.True(WpfSkiaImageMapper.TryCreateBitmap(new SKBitmapImageSource(), out var bitmap));
		Assert.Null(bitmap);
		Assert.False(WpfSkiaImageMapper.TryCreateBitmap(new FileImageSource { File = "test.png" }, out bitmap));
		Assert.Null(bitmap);
		Assert.True(WpfSkiaImageMapper.TryCreateBitmap(new SKImageImageSource(), out bitmap));
		Assert.Null(bitmap);
	}

	[Fact]
	public void ImageMapperConvertsSkiaBitmapWithCopiedPixels()
	{
		RunOnWpfThread(() =>
		{
			WriteableBitmap? bitmap;
			using (var source = new SKBitmap(1, 1))
			{
				source.SetPixel(0, 0, SKColors.Red);
				var imageSource = new SKBitmapImageSource { Bitmap = source };
				Assert.True(WpfSkiaImageMapper.TryCreateBitmap(imageSource, out bitmap));
			}

			Assert.NotNull(bitmap);
			Assert.Equal(1, bitmap.PixelWidth);
			Assert.Equal(1, bitmap.PixelHeight);

			var pixels = new byte[4];
			bitmap.CopyPixels(pixels, 4, 0);
			Assert.Equal(new byte[] { 0, 0, 255, 255 }, pixels);
		});
	}

	private static void RunOnWpfThread(Action test)
	{
		Exception? error = null;
		var thread = new Thread(() =>
		{
			try
			{
				test();
			}
			catch (Exception e)
			{
				error = e;
			}
		});
		thread.SetApartmentState(ApartmentState.STA);
		thread.Start();
		thread.Join();
		if (error is not null)
			ExceptionDispatchInfo.Capture(error).Throw();
	}
}
