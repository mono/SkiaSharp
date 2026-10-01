using System;
using SkiaSharp;

namespace SkiaSharpSample.Samples;

/// <summary>Draws the Gallery's placeholder, source image, and looping comparison.</summary>
internal sealed class PlaceholderImageView : IDisposable
{
    private SKImage? placeholder;
    private SKImage? image;
    private TimeSpan started;
    private bool disposed;

    public void SetPlaceholder(SKImage? image)
    {
        CheckDisposed();
        if (ReferenceEquals(placeholder, image))
            return;
        var old = placeholder;
        placeholder = image;
        if (!ReferenceEquals(old, this.image))
            old?.Dispose();
    }

    public void SetImage(SKImage? image, TimeSpan now)
    {
        CheckDisposed();
        if (ReferenceEquals(this.image, image))
            return;
        var old = this.image;
        this.image = image;
        started = now;
        if (!ReferenceEquals(old, placeholder))
            old?.Dispose();
    }

    public void Draw(SKCanvas canvas, SKRect rectangle, TimeSpan now)
    {
        CheckDisposed();
        ArgumentNullException.ThrowIfNull(canvas);
        if (rectangle.Width <= 0 || rectangle.Height <= 0)
            return;
        canvas.Save();
        try
        {
            canvas.ClipRect(rectangle);
            var opacity = image is null ? 0 : placeholder is null ? 1 : PlaceholderTransition.FullOpacity(now, started);
            if (placeholder is not null && opacity < 1)
                DrawFitted(canvas, placeholder, rectangle, 1);
            if (image is not null && opacity > 0)
                DrawFitted(canvas, image, rectangle, opacity);
        }
        finally
        {
            canvas.Restore();
        }
    }

    public void DrawPlaceholder(SKCanvas canvas, SKRect rectangle) =>
        DrawSingle(canvas, placeholder, rectangle);

    public void DrawFullImage(SKCanvas canvas, SKRect rectangle) =>
        DrawSingle(canvas, image, rectangle);

    private void DrawSingle(SKCanvas canvas, SKImage? source, SKRect rectangle)
    {
        CheckDisposed();
        ArgumentNullException.ThrowIfNull(canvas);
        if (source is null || rectangle.Width <= 0 || rectangle.Height <= 0)
            return;
        canvas.Save();
        try
        {
            canvas.ClipRect(rectangle);
            DrawFitted(canvas, source, rectangle, 1);
        }
        finally
        {
            canvas.Restore();
        }
    }

    private static void DrawFitted(SKCanvas canvas, SKImage source, SKRect bounds, float opacity)
    {
        var scale = Math.Min(bounds.Width / source.Width, bounds.Height / source.Height);
        var width = source.Width * scale;
        var height = source.Height * scale;
        var dest = SKRect.Create(bounds.MidX - width / 2, bounds.MidY - height / 2, width, height);
        using var paint = new SKPaint { Color = SKColors.White.WithAlpha((byte)Math.Clamp((int)Math.Round(opacity * 255), 0, 255)), IsAntialias = true };
        canvas.DrawImage(source, dest, new SKSamplingOptions(SKFilterMode.Linear), paint);
    }

    public void Clear()
    {
        CheckDisposed();
        if (!ReferenceEquals(placeholder, image))
            placeholder?.Dispose();
        image?.Dispose();
        placeholder = null;
        image = null;
        started = default;
    }

    public void Dispose()
    {
        if (disposed)
            return;
        Clear();
        disposed = true;
    }

    private void CheckDisposed()
    {
        if (disposed)
            throw new ObjectDisposedException(nameof(PlaceholderImageView));
    }
}

/// <summary>Defines the Gallery's continuous hold-and-cross-fade timeline.</summary>
internal static class PlaceholderTransition
{
    private static readonly long HoldTicks = TimeSpan.FromSeconds(2).Ticks;
    private static readonly long FadeTicks = TimeSpan.FromSeconds(1).Ticks;
    private static readonly long CycleTicks = 2 * (HoldTicks + FadeTicks);

    public static float FullOpacity(TimeSpan now, TimeSpan started)
    {
        var elapsed = now - started;
        if (elapsed.Ticks < HoldTicks)
            return 0;
        var position = elapsed.Ticks % CycleTicks;
        if (position < HoldTicks)
            return 0;
        if (position < HoldTicks + FadeTicks)
            return (float)((position - HoldTicks) / (double)FadeTicks);
        if (position < 2 * HoldTicks + FadeTicks)
            return 1;
        return (float)((CycleTicks - position) / (double)FadeTicks);
    }
}
