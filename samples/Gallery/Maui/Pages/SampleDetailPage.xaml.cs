using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace SkiaSharpSample.Pages
{
[QueryProperty(nameof(SampleTitle), "sampleTitle")]
public partial class SampleDetailPage : ContentPage
{
private SampleBase? sample;
private bool useGpu;

public SampleDetailPage()
{
InitializeComponent();
}

public string? SampleTitle
{
set
{
var title = Uri.UnescapeDataString(value ?? string.Empty);
var newSample = SamplesManager.GetSample(title);
SetSample(newSample);
}
}

private void SetSample(SampleBase? newSample)
{
if (sample is not null)
{
sample.RefreshRequested -= OnRefreshRequested;
sample.Destroy();
}

sample = newSample;
Title = sample?.Title ?? "Sample";

gpuButton.IsEnabled = true;

// Default to CPU
SwitchToBackend(false);

if (sample is not null)
{
sample.RefreshRequested += OnRefreshRequested;
sample.Init();
UpdateInfoOverlay();
}
}

private void SwitchToBackend(bool gpu)
{
useGpu = gpu;
var isCpu = !gpu;

skiaView.IsVisible = isCpu;
skiaGLView.IsVisible = !isCpu;

cpuButton.BackgroundColor = isCpu
? (Color)Application.Current!.Resources["AccentColor"]
: Colors.Transparent;
cpuButton.TextColor = isCpu ? Colors.White : Color.FromArgb("#aaaaaa");

gpuButton.BackgroundColor = !isCpu
? (Color)Application.Current!.Resources["AccentColor"]
: Colors.Transparent;
gpuButton.TextColor = !isCpu ? Colors.White : Color.FromArgb("#aaaaaa");

InvalidateActiveView();
}

private void InvalidateActiveView()
{
if (!useGpu)
skiaView.InvalidateSurface();
else
skiaGLView.InvalidateSurface();
}

private void UpdateInfoOverlay()
{
if (sample is null) return;
infoLabel.Text =
$"SkiaSharp: {SamplesManager.SkiaSharpVersion}  |  " +
$"HarfBuzz: {SamplesManager.HarfBuzzSharpVersion}";
infoOverlay.IsVisible = true;
}

private void OnRefreshRequested(object? sender, EventArgs e) =>
InvalidateActiveView();

private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
{
e.Surface.Canvas.Clear(SKColors.White);
sample?.DrawSample(e.Surface.Canvas, e.Info.Width, e.Info.Height);
}

private void OnPaintGLSurface(object sender, SKPaintGLSurfaceEventArgs e)
{
e.Surface.Canvas.Clear(SKColors.White);
sample?.DrawSample(e.Surface.Canvas, e.BackendRenderTarget.Width, e.BackendRenderTarget.Height);
}

private void OnCpuBackendClicked(object? sender, EventArgs e) =>
SwitchToBackend(false);

private void OnGpuBackendClicked(object? sender, EventArgs e) =>
SwitchToBackend(true);

private void OnTapSample(object sender, TappedEventArgs e)
{
sample?.Tap();
InvalidateActiveView();
}

private void OnPanSample(object sender, PanUpdatedEventArgs e)
{
var activeSize = !useGpu
? skiaView.CanvasSize : skiaGLView.CanvasSize;
var activeWidth = !useGpu
? skiaView.Width : skiaGLView.Width;
var scale = activeWidth > 0 ? activeSize.Width / (float)activeWidth : 1f;

sample?.Pan(
(GestureState)(int)e.StatusType,
new SKPoint((float)e.TotalX * scale, (float)e.TotalY * scale));
InvalidateActiveView();
}

private void OnPinchSample(object sender, PinchGestureUpdatedEventArgs e)
{
var size = !useGpu
? skiaView.CanvasSize : skiaGLView.CanvasSize;

sample?.Pinch(
(GestureState)(int)e.Status,
(float)e.Scale,
new SKPoint((float)e.ScaleOrigin.X * size.Width, (float)e.ScaleOrigin.Y * size.Height));
InvalidateActiveView();
}

private void OnResetMatrixClicked(object? sender, EventArgs e)
{
sample?.ResetMatrix();
InvalidateActiveView();
}

protected override void OnDisappearing()
{
base.OnDisappearing();
SetSample(null);
}
}
}
