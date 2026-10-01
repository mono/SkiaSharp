using System;
using System.Collections.Generic;
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
	private readonly SKCanvasView cpu = new() { WidthRequest = 150, HeightRequest = 150, EnableTouchEvents = true };
	private readonly SKGLView gpu = new() { WidthRequest = 150, HeightRequest = 150, EnableTouchEvents = true };
	private readonly List<SKTouchEventArgs> cpuTouches = new();
	private readonly List<SKTouchEventArgs> gpuTouches = new();
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
	private bool reattached;
	private bool touchesVerified;
	private DateTime requestedResizeAt;
	private DateTime requestedNativeLayoutAt;
	private int gpuFramesAtLoopStart;
	private int cpuFramesAtReattach;
	private int gpuFramesAtReattach;
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
		cpu.Touch += (_, e) => { e.Handled = true; cpuTouches.Add(e); };
		gpu.Touch += (_, e) => { e.Handled = true; gpuTouches.Add(e); };

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
			touchesVerified = VerifyTouch(cpu, cpuTouches) && VerifyTouch(gpu, gpuTouches);
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
			sawRawInfo && imagesConverted && touchesVerified)
		{
			if (!renderLoopStarted)
			{
				renderLoopStarted = true;
				gpuFramesAtLoopStart = gpuFrames;
				gpu.HasRenderLoop = true;
			}
			else if (gpuFrames - gpuFramesAtLoopStart >= 3)
			{
				if (!reattached)
				{
					reattached = true;
					gpu.HasRenderLoop = false;
					layout.Remove(cpu);
					layout.Remove(gpu);
					Console.WriteLine($"Removed from layout: CPU handler={cpu.Handler is not null} Metal handler={gpu.Handler is not null} Metal context={gpu.GRContext is not null}");
					cpuFramesAtReattach = cpuFrames;
					gpuFramesAtReattach = gpuFrames;
					layout.Add(cpu);
					layout.Add(gpu);
					var nativeLayout = (NSView)layout.Handler!.PlatformView!;
					nativeLayout.NeedsLayout = true;
					nativeLayout.LayoutSubtreeIfNeeded();
					cpu.InvalidateSurface();
					gpu.InvalidateSurface();
				}
				else if (cpuFrames > cpuFramesAtReattach && gpuFrames > gpuFramesAtReattach)
				{
					cpuTouches.Clear();
					gpuTouches.Clear();
					var touchesReattached = VerifyTouch(cpu, cpuTouches) && VerifyTouch(gpu, gpuTouches);
					var hadMetalContext = gpu.GRContext is not null;
					cpu.Handler!.DisconnectHandler();
					gpu.Handler!.DisconnectHandler();
					if (touchesReattached && hadMetalContext && gpu.GRContext is null)
					{
						Console.WriteLine($"PASS AppKit CPU={cpuFrames} Metal={gpuFrames} resized CPU={resizedCpuFrames} Metal={resizedGpuFrames}; mouse, reattach, disconnect, Metal loop and all image sources converted");
						Finish(0);
					}
					else
					{
						Console.Error.WriteLine($"FAIL AppKit reattach/disconnect: mouse={touchesReattached} previousMetalContext={hadMetalContext} cleared={gpu.GRContext is null}");
						Finish(1);
					}
				}
			}
		}
		if (DateTime.UtcNow - started > TimeSpan.FromSeconds(20))
		{
			Console.Error.WriteLine($"FAIL AppKit CPU={cpuFrames} Metal={gpuFrames} resize CPU={resizedCpuFrames} Metal={resizedGpuFrames} context={sawMetal} rawInfo={sawRawInfo} images={imagesConverted} touch={touchesVerified} reattach={reattached} reattachedFrames={cpuFrames - cpuFramesAtReattach},{gpuFrames - gpuFramesAtReattach}");
			Finish(1);
		}
	}

	private static bool VerifyTouch(Microsoft.Maui.Controls.View control, List<SKTouchEventArgs> touches)
	{
		var nativeView = (NSView)control.Handler!.PlatformView!;
		using var graphics = NSGraphicsContext.FromWindow(nativeView.Window!);
		var point = new CGPoint(12, 18);
		var windowPoint = nativeView.ConvertPointToView(point, null);
		var ignoreScaling = control switch
		{
			SKCanvasView canvas => canvas.IgnorePixelScaling,
			SKGLView metal => metal.IgnorePixelScaling,
			_ => false,
		};
		var scale = ignoreScaling ? 1 : (float)nativeView.Window!.BackingScaleFactor;

		NSEvent MouseEvent(NSEventType type) =>
			NSEvent.MouseEvent(type, windowPoint, (NSEventModifierMask)0,
				0, nativeView.Window.WindowNumber, graphics, 0, 1, 1);

		using (var down = MouseEvent(NSEventType.LeftMouseDown))
			nativeView.MouseDown(down);
		using (var drag = MouseEvent(NSEventType.LeftMouseDragged))
			nativeView.MouseDragged(drag);
		using (var up = MouseEvent(NSEventType.LeftMouseUp))
			nativeView.MouseUp(up);
		using (var move = MouseEvent(NSEventType.MouseMoved))
			nativeView.MouseMoved(move);

		var expectedY = nativeView.IsFlipped ? (float)point.Y : (float)(nativeView.Bounds.Height - point.Y);
		var valid = touches.Count == 4 &&
			touches[0].ActionType == SKTouchAction.Pressed && touches[0].InContact &&
			touches[0].MouseButton == SKMouseButton.Left &&
			touches[1].ActionType == SKTouchAction.Moved && touches[1].InContact &&
			touches[2].ActionType == SKTouchAction.Released && !touches[2].InContact &&
			touches[3].ActionType == SKTouchAction.Moved && !touches[3].InContact &&
			touches[3].MouseButton == SKMouseButton.Unknown &&
			Math.Abs(touches[0].Location.X - point.X * scale) < 1 &&
			Math.Abs(touches[0].Location.Y - expectedY * scale) < 1;

		using (var down = MouseEvent(NSEventType.LeftMouseDown))
			nativeView.MouseDown(down);
		switch (control)
		{
			case SKCanvasView canvas:
				canvas.EnableTouchEvents = false;
				break;
			case SKGLView metal:
				metal.EnableTouchEvents = false;
				break;
		}
		valid &= touches.Count == 6 &&
			touches[4].ActionType == SKTouchAction.Pressed &&
			touches[5].ActionType == SKTouchAction.Cancelled &&
			!touches[5].InContact;
		switch (control)
		{
			case SKCanvasView canvas:
				canvas.EnableTouchEvents = true;
				break;
			case SKGLView metal:
				metal.EnableTouchEvents = true;
				break;
		}
		using (var up = MouseEvent(NSEventType.LeftMouseUp))
			nativeView.MouseUp(up);
		valid &= touches.Count == 6;
		using (var down = MouseEvent(NSEventType.LeftMouseDown))
			nativeView.MouseDown(down);
		using (var repeatedDown = MouseEvent(NSEventType.LeftMouseDown))
			nativeView.MouseDown(repeatedDown);
		using (var up = MouseEvent(NSEventType.LeftMouseUp))
			nativeView.MouseUp(up);
		using (var move = MouseEvent(NSEventType.MouseMoved))
			nativeView.MouseMoved(move);
		valid &= touches.Count == 11 &&
			touches[7].ActionType == SKTouchAction.Cancelled &&
			!touches[7].InContact &&
			touches[8].ActionType == SKTouchAction.Pressed &&
			touches[8].InContact &&
			touches[9].ActionType == SKTouchAction.Released &&
			!touches[9].InContact &&
			touches[10].MouseButton == SKMouseButton.Unknown;
		Console.WriteLine($"Mouse {control.GetType().Name} received={touches.Count} expected position={point.X * scale},{expectedY * scale} actual={touches[0].Location} valid={valid}");
		return valid;
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
