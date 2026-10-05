using System;
using SkiaSharp;
using SkiaSharpSample.Samples;
using Xunit;

public class PlaceholderImageViewTests
{
    [Fact]
    public void TransitionIsDeterministicAcrossBoundariesAndLateStart()
    {
        var start = TimeSpan.FromSeconds(30);
        Assert.Equal(0, PlaceholderTransition.FullOpacity(TimeSpan.FromSeconds(20), start));
        Assert.Equal(0, PlaceholderTransition.FullOpacity(start + TimeSpan.FromSeconds(2), start));
        Assert.InRange(PlaceholderTransition.FullOpacity(start + TimeSpan.FromMilliseconds(2500), start), 0.49f, 0.51f);
        Assert.Equal(1, PlaceholderTransition.FullOpacity(start + TimeSpan.FromSeconds(3), start));
        Assert.Equal(1, PlaceholderTransition.FullOpacity(start + TimeSpan.FromMilliseconds(4999), start));
        Assert.Equal(1, PlaceholderTransition.FullOpacity(start + TimeSpan.FromSeconds(5), start));
        Assert.InRange(PlaceholderTransition.FullOpacity(start + TimeSpan.FromMilliseconds(5500), start), 0.49f, 0.51f);
        Assert.Equal(0, PlaceholderTransition.FullOpacity(start + TimeSpan.FromSeconds(6), start));
        Assert.Equal(0, PlaceholderTransition.FullOpacity(start + TimeSpan.FromSeconds(1_000_008), start));
    }

    [Fact]
    public void ViewRendersInsideAnyRectangleAndRetainsHashOnly()
    {
        using var view = new PlaceholderImageView();
        using var placeholderBitmap = Solid(SKColors.Red);
        using var fullBitmap = Solid(SKColors.Blue);
        var placeholder = SKImage.FromBitmap(placeholderBitmap);
        var full = SKImage.FromBitmap(fullBitmap);
        view.SetPlaceholder(placeholder);
        using var target = new SKBitmap(100, 100);
        using var canvas = new SKCanvas(target);
        canvas.Clear(SKColors.Green);
        var rect = SKRect.Create(25, 30, 40, 20);
        view.Draw(canvas, rect, TimeSpan.FromDays(100));
        Assert.Equal(SKColors.Green, target.GetPixel(0, 0));
        Assert.Equal(SKColors.Red, target.GetPixel(45, 40));
        view.SetImage(full, TimeSpan.FromSeconds(20));
        view.Draw(canvas, rect, TimeSpan.FromSeconds(22.5));
        Assert.NotEqual(IntPtr.Zero, placeholder.Handle);
        Assert.NotEqual(IntPtr.Zero, full.Handle);
        var mixed = target.GetPixel(45, 40);
        Assert.InRange((int)mixed.Red, 115, 140);
        Assert.InRange((int)mixed.Blue, 115, 140);
        view.Draw(canvas, rect, TimeSpan.FromSeconds(23));
        Assert.Equal(SKColors.Blue, target.GetPixel(45, 40));
        view.Draw(canvas, rect, TimeSpan.FromSeconds(25.5));
        mixed = target.GetPixel(45, 40);
        Assert.InRange((int)mixed.Red, 115, 140);
        Assert.InRange((int)mixed.Blue, 115, 140);
        view.Draw(canvas, rect, TimeSpan.FromSeconds(26));
        Assert.Equal(SKColors.Red, target.GetPixel(45, 40));
        view.DrawFullImage(canvas, rect);
        Assert.Equal(SKColors.Blue, target.GetPixel(45, 40));
        view.DrawPlaceholder(canvas, rect);
        Assert.Equal(SKColors.Red, target.GetPixel(45, 40));
        view.Clear();
        Assert.Equal(IntPtr.Zero, placeholder.Handle);
        Assert.Equal(IntPtr.Zero, full.Handle);
        Assert.Equal(SKColors.Green, target.GetPixel(0, 0));
    }

    [Fact]
    public void ViewTransfersEachUniqueImageExactlyOnce()
    {
        using var view = new PlaceholderImageView();
        using var bitmap = Solid(SKColors.Red);
        var first = SKImage.FromBitmap(bitmap);
        var second = SKImage.FromBitmap(bitmap);
        view.SetPlaceholder(first);
        view.SetImage(first, TimeSpan.Zero);
        view.SetPlaceholder(second);
        Assert.NotEqual(IntPtr.Zero, first.Handle);
        view.SetImage(second, TimeSpan.Zero);
        Assert.Equal(IntPtr.Zero, first.Handle);
        view.Dispose();
        view.Dispose();
        Assert.Equal(IntPtr.Zero, second.Handle);
        Assert.Throws<ObjectDisposedException>(() => view.SetImage(null, TimeSpan.Zero));
    }

    [Fact]
    public void ReplacingImageCanKeepTheSharedTransitionPhase()
    {
        using var view = new PlaceholderImageView();
        using var bitmap = Solid(SKColors.Red);
        using var firstBitmap = Solid(SKColors.Blue);
        using var nextBitmap = Solid(SKColors.Green);
        view.SetPlaceholder(SKImage.FromBitmap(bitmap));
        var first = SKImage.FromBitmap(firstBitmap);
        view.SetImage(first, TimeSpan.Zero);
        using var target = Solid(SKColors.White);
        using var canvas = new SKCanvas(target);
        var bounds = SKRect.Create(0, 0, 2, 2);
        view.Draw(canvas, bounds, TimeSpan.FromSeconds(3));
        Assert.Equal(SKColors.Blue, target.GetPixel(1, 1));
        view.SetImage(SKImage.FromBitmap(nextBitmap), TimeSpan.Zero);
        Assert.Equal(IntPtr.Zero, first.Handle);
        view.Draw(canvas, bounds, TimeSpan.FromSeconds(3));
        Assert.Equal(SKColors.Green, target.GetPixel(1, 1));
    }

    private static SKBitmap Solid(SKColor color)
    {
        var bitmap = new SKBitmap(2, 2);
        bitmap.Erase(color);
        return bitmap;
    }
}
