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
private SampleBackends currentBackend = SampleBackends.Memory;

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

// Disable GPU button if sample doesn't support OpenGL
gpuButton.IsEnabled = sample?.SupportedBackends.HasFlag(SampleBackends.OpenGL) ?? false;

// Default to CPU; fall back to GPU if CPU not supported
if (sample is not null && !sample.SupportedBackends.HasFlag(SampleBackends.Memory))
SwitchToBackend(SampleBackends.OpenGL);
else
SwitchToBackend(SampleBackends.Memory);

if (sample is not null)
{
sample.RefreshRequested += OnRefreshRequested;
sample.Init();
UpdateInfoOverlay();
}
}

private void SwitchToBackend(SampleBackends backend)
{
currentBackend = backend;
var isCpu = backend == SampleBackends.Memory;

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
if (currentBackend == SampleBackends.Memory)
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
SwitchToBackend(SampleBackends.Memory);

private void OnGpuBackendClicked(object? sender, EventArgs e) =>
SwitchToBackend(SampleBackends.OpenGL);

private void OnTapSample(object sender, TappedEventArgs e)
{
sample?.Tap();
InvalidateActiveView();
}

private void OnPanSample(object sender, PanUpdatedEventArgs e)
{
var activeSize = currentBackend == SampleBackends.Memory
? skiaView.CanvasSize : skiaGLView.CanvasSize;
var activeWidth = currentBackend == SampleBackends.Memory
? skiaView.Width : skiaGLView.Width;
var scale = activeWidth > 0 ? activeSize.Width / (float)activeWidth : 1f;

sample?.Pan(
(GestureState)(int)e.StatusType,
new SKPoint((float)e.TotalX * scale, (float)e.TotalY * scale));
InvalidateActiveView();
}

private void OnPinchSample(object sender, PinchGestureUpdatedEventArgs e)
{
var size = currentBackend == SampleBackends.Memory
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
