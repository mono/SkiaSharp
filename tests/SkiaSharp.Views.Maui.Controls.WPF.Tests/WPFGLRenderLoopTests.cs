using System;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui.Handlers;
using SkiaSharp.Views.WPF;
using Xunit;
using WPFWindow = System.Windows.Window;

namespace SkiaSharp.Views.Maui.Controls.WPF.Tests;

[Collection("WPF handlers")]
public class WPFGLRenderLoopTests
{
	[Fact]
	public void RenderLoopMapperWaitsForPlatformViewCreation() => WPFTestThread.Run(() =>
	{
		var handler = new WPFSKGLViewHandler();
		WPFSKGLViewHandler.MapHasRenderLoop(handler, new SKGLView { HasRenderLoop = true });
		Assert.Null(((IElementHandler)handler).PlatformView);
	});

	[Fact]
	public void RenderLoopFollowsLoadingAndPropertyChanges() => WPFTestThread.RunAsync(async () =>
	{
		using var host = new GLTestHost(hasRenderLoop: true);
		Assert.False(host.NativeView.RenderContinuously);

		await host.WaitForPaintAsync(host.Window.Show);
		Assert.True(host.NativeView.RenderContinuously);
		var firstContext = host.View.GRContext;
		Assert.NotNull(firstContext);

		host.View.HasRenderLoop = false;
		Assert.False(host.NativeView.RenderContinuously);
		host.View.HasRenderLoop = true;
		Assert.True(host.NativeView.RenderContinuously);

		host.Window.Content = null;
		await Dispatcher.Yield(DispatcherPriority.Background);
		Assert.False(host.NativeView.IsLoaded);
		Assert.False(host.NativeView.RenderContinuously);
		Assert.Null(host.View.GRContext);

		await host.WaitForPaintAsync(() => host.Window.Content = host.NativeView);
		Assert.True(host.NativeView.RenderContinuously);
		Assert.NotNull(host.View.GRContext);
		Assert.NotSame(firstContext, host.View.GRContext);
	});

	[Fact]
	public void DisconnectStopsRenderLoopAndClearsContext() => WPFTestThread.RunAsync(async () =>
	{
		using var host = new GLTestHost(hasRenderLoop: true);
		await host.WaitForPaintAsync(host.Window.Show);
		Assert.True(host.NativeView.RenderContinuously);
		Assert.NotNull(host.View.GRContext);

		host.Disconnect();

		Assert.False(host.NativeView.RenderContinuously);
		Assert.Null(host.View.GRContext);
	});

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void ContinuousAndOnDemandFramesPreservePixelsAndScaling(bool ignorePixelScaling) => WPFTestThread.RunAsync(async () =>
	{
		using var host = new GLTestHost(hasRenderLoop: false, ignorePixelScaling);
		var onDemand = await host.CapturePaintAsync(host.Window.Show);
		Assert.False(host.NativeView.RenderContinuously);
		var invalidated = await host.CapturePaintAsync(host.View.InvalidateSurface);
		var continuous = await host.CapturePaintAsync(() => host.View.HasRenderLoop = true);
		Assert.True(host.NativeView.RenderContinuously);
		host.View.HasRenderLoop = false;
		Assert.False(host.NativeView.RenderContinuously);

		Assert.Equal(onDemand, invalidated);
		Assert.Equal(onDemand, continuous);
		Assert.Equal(SKColors.Red, onDemand.Pixel);
		Assert.Equal(GRSurfaceOrigin.BottomLeft, onDemand.Origin);

		var dpi = VisualTreeHelper.GetDpi(host.NativeView);
		var logicalSize = new SKSizeI((int)host.NativeView.ActualWidth, (int)host.NativeView.ActualHeight);
		var pixelSize = new SKSizeI(
			(int)(host.NativeView.ActualWidth * dpi.DpiScaleX),
			(int)(host.NativeView.ActualHeight * dpi.DpiScaleY));
		Assert.Equal(pixelSize, onDemand.RawInfo.Size);
		Assert.Equal(ignorePixelScaling ? logicalSize : pixelSize, onDemand.Info.Size);
		Assert.Equal(onDemand.Info.Size, host.View.CanvasSize);
	});

	private sealed record PaintedFrame(SKImageInfo Info, SKImageInfo RawInfo, GRSurfaceOrigin Origin, SKColor Pixel);

	private sealed class GLTestHost : IDisposable
	{
		private readonly MauiApp app = WPFTestThread.CreateApp();
		private readonly WPFSKGLViewHandler handler = new();
		private bool connected = true;

		public SKGLView View { get; }
		public SKGLElement NativeView { get; }
		public WPFWindow Window { get; }

		public GLTestHost(bool hasRenderLoop, bool ignorePixelScaling = false)
		{
			View = new SKGLView
			{
				HasRenderLoop = hasRenderLoop,
				IgnorePixelScaling = ignorePixelScaling,
				WidthRequest = 96,
				HeightRequest = 64,
			};
			var mauiWindow = new Window(new ContentPage { Content = View });
			Assert.Same(mauiWindow, View.Window);
			handler.SetMauiContext(new MauiContext(app.Services));
			handler.SetVirtualView(View);
			NativeView = Assert.IsType<WPFSKGLViewHandler.MauiSKGLElement>(handler.PlatformView);
			NativeView.Width = 96;
			NativeView.Height = 64;
			Window = new WPFWindow
			{
				Content = NativeView,
				Width = 200,
				Height = 180,
				ShowActivated = false,
				ShowInTaskbar = false,
			};
		}

		public async Task WaitForPaintAsync(Action action)
		{
			var painted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
			void OnPaint(object? sender, SKPaintGLSurfaceEventArgs args) => painted.TrySetResult();
			View.PaintSurface += OnPaint;
			try
			{
				action();
				await painted.Task.WaitAsync(TimeSpan.FromSeconds(10));
			}
			finally
			{
				View.PaintSurface -= OnPaint;
			}
		}

		public async Task<PaintedFrame> CapturePaintAsync(Action action)
		{
			var painted = new TaskCompletionSource<PaintedFrame>(TaskCreationOptions.RunContinuationsAsynchronously);
			void OnPaint(object? sender, SKPaintGLSurfaceEventArgs args)
			{
				try
				{
					args.Surface.Canvas.Clear(SKColors.Red);
					using var bitmap = new SKBitmap(new SKImageInfo(1, 1));
					Assert.True(args.Surface.ReadPixels(bitmap.Info, bitmap.GetPixels(), bitmap.RowBytes, 0, 0));
					painted.TrySetResult(new PaintedFrame(args.Info, args.RawInfo, args.Origin, bitmap.GetPixel(0, 0)));
				}
				catch (Exception exception)
				{
					painted.TrySetException(exception);
				}
			}
			View.PaintSurface += OnPaint;
			try
			{
				action();
				return await painted.Task.WaitAsync(TimeSpan.FromSeconds(10));
			}
			finally
			{
				View.PaintSurface -= OnPaint;
			}
		}

		public void Disconnect()
		{
			if (!connected)
				return;
			((IElementHandler)handler).DisconnectHandler();
			connected = false;
		}

		public void Dispose()
		{
			Window.Content = null;
			Disconnect();
			Window.Close();
			app.Dispose();
		}
	}
}
