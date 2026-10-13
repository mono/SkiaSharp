$ErrorActionPreference = 'Stop'
$repository = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../../..'))
$source = Get-Content -LiteralPath (Join-Path $repository 'source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKGaneshView.cs') -Raw
$start = $source.IndexOf('public unsafe void Render ()')
if ($start -lt 0) { throw 'The Windows Ganesh render method was not found.' }
$end = $source.IndexOf('public void Disconnect ()', $start)
if ($end -lt 0) { throw 'The Windows Ganesh render-method boundary was not found.' }
$method = $source.Substring($start, $end - $start)

# Exercise backend selection in the actual render method without loading GPU libraries.
$harness = @'
#nullable enable
using System;

public enum Visibility { Visible }
public enum SKColorType { Rgba8888 }
public enum SKAlphaType { Premul }
public enum GRSurfaceOrigin { BottomLeft }
public static class ColorTypeExtensions
{
    public static uint ToGlSizedFormat(this SKColorType type) => 0x8058;
}
public struct SKSize { }
public readonly struct SKSizeI
{
    private readonly int width, height;
    public SKSizeI(int width, int height) { this.width = width; this.height = height; }
    public static bool operator ==(SKSizeI left, SKSizeI right) =>
        left.width == right.width && left.height == right.height;
    public static bool operator !=(SKSizeI left, SKSizeI right) => !(left == right);
    public override bool Equals(object? value) => value is SKSizeI other && this == other;
    public override int GetHashCode() => HashCode.Combine(width, height);
}
public readonly struct SKImageInfo
{
    public int Width { get; }
    public int Height { get; }
    public SKSize Size => default;
    public SKImageInfo(int width, int height, SKColorType color, SKAlphaType alpha)
    {
        Width = width;
        Height = height;
    }
    public SKImageInfo WithSize(int width, int height) =>
        new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
}
public sealed class GlesContext
{
    public static bool Current;
    public bool HasSurface { get; private set; }
    public void CreateSurface(object panel, object? size, float scale) { HasSurface = true; }
    public void MakeCurrent() { Current = true; }
    public void GetSurfaceDimensions(out int width, out int height) { width = 64; height = 64; }
    public void SetViewportSize(int width, int height) { }
    public bool SwapBuffers() => true;
}
public static unsafe class Gles
{
    public const uint GL_FRAMEBUFFER_BINDING = 1, GL_STENCIL_BITS = 2, GL_SAMPLES = 3;
    public static void glGetIntegerv(uint name, int* value) { *value = 0; }
}
public sealed class GRGlInterface
{
    public static int NativeProbes, AngleInterfaces;
    public static GRGlInterface Create()
    {
        NativeProbes++;
        throw new InvalidOperationException("An ANGLE presenter entered the native WGL probe.");
    }
    public static GRGlInterface CreateAngle()
    {
        if (!GlesContext.Current) throw new InvalidOperationException("ANGLE was not made current.");
        AngleInterfaces++;
        return new GRGlInterface();
    }
}
public sealed class GRContext
{
    public static GRContext CreateGl(GRGlInterface gl) => new GRContext();
    public int GetMaxSurfaceSampleCount(SKColorType color) => 1;
    public void Flush() { }
}
public readonly struct GRGlFramebufferInfo
{
    public GRGlFramebufferInfo(uint framebuffer, uint format) { }
}
public sealed class GRBackendRenderTarget : IDisposable
{
    public GRBackendRenderTarget(int width, int height, int samples, int stencil, GRGlFramebufferInfo info) { }
    public void Dispose() { }
}
public sealed class Canvas
{
    public void Clear(int color) { }
    public void Scale(float x, float y) { }
}
public static class SKColors { public const int Transparent = 0; }
public sealed class SKSurface : IDisposable
{
    public Canvas Canvas { get; } = new Canvas();
    public static SKSurface Create(GRContext context, GRBackendRenderTarget target, GRSurfaceOrigin origin, SKColorType color) =>
        new SKSurface();
    public void Flush() { }
    public void Dispose() { }
}
public sealed class SKAutoCanvasRestore : IDisposable
{
    public SKAutoCanvasRestore(Canvas canvas, bool save) { }
    public void Dispose() { }
}
public sealed class SKPaintGaneshSurfaceEventArgs : EventArgs
{
    public SKPaintGaneshSurfaceEventArgs(SKSurface surface, GRContext context, GRBackendRenderTarget target,
        GRSurfaceOrigin origin, SKImageInfo info, SKImageInfo raw) { }
}
public sealed class Owner
{
    public Visibility Visibility = Visibility.Visible;
    public bool IgnorePixelScaling = true;
    public SKSize CanvasSize;
    public int Paints;
    public Exception? Failure;
    public EventHandler? ContextCreated;
    public EventHandler<SKPaintGaneshSurfaceEventArgs>? PaintSurface;
    public Owner() { PaintSurface = (_, _) => Paints++; }
    public void Fail(Exception exception) { Failure = exception; }
}
public sealed class Timer { public void Stop() { } }
public sealed class WindowsGaneshBackendFixture
{
    private readonly Owner owner = new Owner();
    private readonly Timer timer = new Timer();
    private GlesContext? backend;
    private GRGlInterface? glInterface;
    private GRBackendRenderTarget? target;
    private SKSurface? surface;
    private SKSizeI size;
    private bool failed;
    public bool IsLoaded = true;
    public double ActualWidth = 64, ActualHeight = 64;
    public float CompositionScaleX = 1;
    public GRContext? Context;
    private void UpdateLoop() { if (failed) timer.Stop(); }
    private void ReleaseContext() { Context = null; surface = null; target = null; backend = null; }
    public int Paints => owner.Paints;
    public Exception? Failure => owner.Failure;
__METHOD__
}
'@
Add-Type -TypeDefinition $harness.Replace('__METHOD__', $method) -CompilerOptions '/unsafe'

foreach ($generation in 1..16) {
    [GlesContext]::Current = $false
    $presenter = [WindowsGaneshBackendFixture]::new()
    $presenter.Render()
    $presenter.Render()
    if ($presenter.Failure -or $presenter.Paints -ne 2 -or
        [GRGlInterface]::NativeProbes -ne 0 -or [GRGlInterface]::AngleInterfaces -ne $generation) {
        throw "Windows Ganesh backend selection failed for generation ${generation}: $($presenter.Failure)"
    }
}
'Windows Ganesh creates only ANGLE interfaces after making its backend current, once per context generation.'
