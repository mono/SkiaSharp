using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using SkiaSharp;

namespace SkiaSharpSample;

public sealed partial class GpuPage : Page
{
	readonly FpsCounter fpsCounter = new();

	DispatcherTimer? animationTimer;
	bool touchActive;
	SKPoint touchPos;

	public GpuPage()
	{
		InitializeComponent();
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		StartAnimation();
	}

	private void OnUnloaded(object sender, RoutedEventArgs e)
	{
		StopAnimation();
	}

	void StartAnimation()
	{
		fpsCounter.Start();
		fpsText.Text = "FPS: --";

		animationTimer = new DispatcherTimer
		{
			Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
		};
		animationTimer.Tick += OnAnimationTick;
		animationTimer.Start();
	}

	void StopAnimation()
	{
		animationTimer?.Stop();
		animationTimer = null;
		fpsCounter.Stop();
	}

	void OnAnimationTick(object? sender, object e)
	{
		skiaView.Time = fpsCounter.ElapsedSeconds;
		skiaView.TouchPos = touchPos;
		skiaView.TouchActive = touchActive;
		skiaView.Invalidate();

		if (fpsCounter.Tick() is double fps)
			fpsText.Text = $"FPS: {fps:F0}";
	}

	private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
	{
		touchActive = true;
		UpdateTouchPosition(e);
		skiaView.CapturePointer(e.Pointer);
	}

	private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
	{
		if (touchActive)
			UpdateTouchPosition(e);
	}

	private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
	{
		touchActive = false;
		skiaView.ReleasePointerCapture(e.Pointer);
	}

	void UpdateTouchPosition(PointerRoutedEventArgs e)
	{
		var point = e.GetCurrentPoint(skiaView);
		var w = skiaView.ActualWidth;
		var h = skiaView.ActualHeight;
		if (w > 0 && h > 0)
			touchPos = new SKPoint((float)(point.Position.X / w), (float)(point.Position.Y / h));
	}
}
