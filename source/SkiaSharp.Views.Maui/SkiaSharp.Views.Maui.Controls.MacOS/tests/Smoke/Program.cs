using System;
using AppKit;
using CoreGraphics;
using Foundation;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platforms.MacOS.Hosting;
using Microsoft.Maui.Platforms.MacOS.Platform;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui.Controls.Hosting;
using SkiaSharp;
using MauiImage = Microsoft.Maui.Controls.Image;
using MauiWindow = Microsoft.Maui.Controls.Window;

public static class MainClass
{
	public static void Main(string[] args)
	{
		NSApplication.Init();
		NSApplication.SharedApplication.Delegate = new SmokeHost();
		NSApplication.Main(args);
	}
}

[Register("SkiaSharpMauiAppKitSmoke")]
sealed class SmokeHost : MacOSMauiApplication
{
	protected override MauiApp CreateMauiApp() => MauiApp.CreateBuilder()
		.UseMauiAppMacOS<SmokeApp>()
		.UseSkiaSharpMacOS()
		.Build();
}

sealed class SmokeApp : Application
{
	private readonly SKCanvasView cpu = new() { WidthRequest = 150, HeightRequest = 150 };
	private readonly SKGLView gpu = new() { WidthRequest = 150, HeightRequest = 150 };
	private readonly VerticalStackLayout layout = new();
	private readonly MauiImage bitmapImage = new() { WidthRequest = 20, HeightRequest = 20 };
	private readonly MauiImage skImage = new() { WidthRequest = 20, HeightRequest = 20 };
	private readonly MauiImage pixmapImage = new() { WidthRequest = 20, HeightRequest = 20 };
	private readonly MauiImage pictureImage = new() { WidthRequest = 20, HeightRequest = 20 };
	private readonly MauiImage fontImage = new() { WidthRequest = 20, HeightRequest = 20 };
	private readonly ImageButton skImageButton = new() { WidthRequest = 20, HeightRequest = 20 };
	private readonly SKBitmap bitmap = new(2, 2);
	private SKImage? image;
	private SKPixmap? pixmap;
	private SKPicture? picture;
	private int cpuFrames;
	private int gpuFrames;
	private int resizedCpuFrames;
	private int resizedGpuFrames;
	private bool sawMetal;
	private bool sawRawInfo;
	private bool resized;
	private bool requestedResize;
	private bool requestedNativeLayout;
	private bool renderLoopStarted;
	private DateTime requestedResizeAt;
	private DateTime requestedNativeLayoutAt;
	private int gpuFramesAtLoopStart;
	private DateTime started;
	private NSTimer? timer;

	protected override MauiWindow CreateWindow(IActivationState? activationState)
	{
		bitmap.Erase(SKColors.CornflowerBlue);
		image = SKImage.FromBitmap(bitmap);
		pixmap = bitmap.PeekPixels();
		using (var recorder = new SKPictureRecorder())
		{
			recorder.BeginRecording(SKRect.Create(2, 2)).Clear(SKColors.CornflowerBlue);
			picture = recorder.EndRecording();
		}
		bitmapImage.Source = new SKBitmapImageSource { Bitmap = bitmap };
		skImage.Source = new SKImageImageSource { Image = image };
		pixmapImage.Source = new SKPixmapImageSource { Pixmap = pixmap };
		pictureImage.Source = new SKPictureImageSource { Picture = picture, Dimensions = new SKSizeI(2, 2) };
		skImageButton.Source = new SKImageImageSource { Image = image };
		fontImage.Source = new FontImageSource { Glyph = "A", Size = 16 };
		cpu.PaintSurface += (_, e) =>
		{
			e.Surface.Canvas.Clear(SKColors.SteelBlue);
			cpuFrames++;
			if (resized && e.Info.Width >= 220)
				resizedCpuFrames++;
			sawRawInfo |= e.RawInfo.Width >= e.Info.Width && e.RawInfo.Height >= e.Info.Height;
		};
		gpu.PaintSurface += (_, e) =>
		{
			e.Surface.Canvas.Clear(SKColors.Coral);
			gpuFrames++;
			sawMetal |= gpu.GRContext is not null && e.BackendRenderTarget.IsValid;
			if (resized && e.Info.Width >= 220)
				resizedGpuFrames++;
			sawRawInfo |= e.RawInfo.Width >= e.Info.Width && e.RawInfo.Height >= e.Info.Height;
		};

		layout.Add(cpu);
		layout.Add(gpu);
		layout.Add(bitmapImage);
		layout.Add(skImage);
		layout.Add(pixmapImage);
		layout.Add(pictureImage);
		layout.Add(fontImage);
		layout.Add(skImageButton);
		var page = new ContentPage { Content = layout };
		var window = new MauiWindow(page);
		window.Created += (_, _) =>
		{
			started = DateTime.UtcNow;
			timer = NSTimer.CreateRepeatingScheduledTimer(TimeSpan.FromMilliseconds(100), _ => Check());
		};
		return window;
	}

	private void Check()
	{
		if (!requestedResize && cpuFrames > 0 && gpuFrames > 0)
		{
			requestedResize = true;
			requestedResizeAt = DateTime.UtcNow;
			cpu.WidthRequest = 220;
			gpu.WidthRequest = 220;
			cpu.InvalidateMeasure();
			gpu.InvalidateMeasure();
			layout.InvalidateMeasure();
		}
		else if (requestedResize && !requestedNativeLayout && DateTime.UtcNow - requestedResizeAt > TimeSpan.FromSeconds(2))
		{
			requestedNativeLayout = true;
			requestedNativeLayoutAt = DateTime.UtcNow;
			Console.WriteLine($"MAUI layout parent={ReferenceEquals(cpu.Parent, layout) && ReferenceEquals(gpu.Parent, layout)} requested 220 native CPU={((NSView)cpu.Handler!.PlatformView!).Frame.Width} Metal={((NSView)gpu.Handler!.PlatformView!).Frame.Width}");
			var nativeLayout = (NSView)layout.Handler!.PlatformView!;
			nativeLayout.NeedsLayout = true;
			nativeLayout.LayoutSubtreeIfNeeded();
		}
		else if (requestedNativeLayout && !resized && DateTime.UtcNow - requestedNativeLayoutAt > TimeSpan.FromSeconds(1))
		{
			resized = true;
			var cpuView = (NSView)cpu.Handler!.PlatformView!;
			var gpuView = (NSView)gpu.Handler!.PlatformView!;
			Console.WriteLine($"Native layout invalidated requested 220 CPU={cpuView.Frame.Width} Metal={gpuView.Frame.Width}");
			cpu.IgnorePixelScaling = true;
			gpu.IgnorePixelScaling = true;
			cpuView.SetFrameSize(new CGSize(220, 150));
			gpuView.SetFrameSize(new CGSize(220, 150));
			cpu.InvalidateSurface();
			gpu.InvalidateSurface();
		}

		var imagesConverted =
			bitmapImage.Handler?.PlatformView is NSImageView { Image: not null } &&
			skImage.Handler?.PlatformView is NSImageView { Image: not null } &&
			pixmapImage.Handler?.PlatformView is NSImageView { Image: not null } &&
			pictureImage.Handler?.PlatformView is NSImageView { Image: not null } &&
			fontImage.Handler?.PlatformView is NSImageView { Image: not null } &&
			skImageButton.Handler?.PlatformView is NSButton { Image: not null };

		if (resizedCpuFrames > 0 && resizedGpuFrames > 0 && sawMetal &&
			sawRawInfo && imagesConverted)
		{
			if (!renderLoopStarted)
			{
				renderLoopStarted = true;
				gpuFramesAtLoopStart = gpuFrames;
				gpu.HasRenderLoop = true;
			}
			else if (gpuFrames - gpuFramesAtLoopStart >= 3)
			{
				gpu.HasRenderLoop = false;
				Console.WriteLine($"PASS AppKit CPU={cpuFrames} Metal={gpuFrames} resized CPU={resizedCpuFrames} Metal={resizedGpuFrames}; Metal render loop and all image sources, glyph and image button converted");
				Finish(0);
			}
		}
		else if (DateTime.UtcNow - started > TimeSpan.FromSeconds(20))
		{
			Console.Error.WriteLine($"FAIL AppKit CPU={cpuFrames} Metal={gpuFrames} resize CPU={resizedCpuFrames} Metal={resizedGpuFrames} context={sawMetal} rawInfo={sawRawInfo} images={imagesConverted}");
			Finish(1);
		}
	}

	private void Finish(int exitCode)
	{
		timer?.Invalidate();
		timer?.Dispose();
		pixmap?.Dispose();
		picture?.Dispose();
		image?.Dispose();
		bitmap.Dispose();
		Environment.Exit(exitCode);
	}
}
