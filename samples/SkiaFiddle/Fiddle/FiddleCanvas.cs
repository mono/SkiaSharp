using System;
using System.Diagnostics;
using Microsoft.UI.Xaml;
using SkiaSharp;
using Uno.WinUI.Graphics2DSK;
using Windows.Foundation;

namespace SkiaFiddle.Fiddle;

public class FiddleCanvas : SKCanvasElement
{
    public delegate void FiddleDelegate(SKCanvas canvas, int width, int height, double t);

    private FiddleDelegate? _drawDelegate;
    private string? _errorMessage;
    private readonly Stopwatch _clock = new();
    private DispatcherTimer? _frameTimer;
    private int _frameCount;
    private double _lastFpsSampleTime;
    private double _fps;

    public bool IsAnimating { get; private set; }

    public double Fps => _fps;

    public event EventHandler<double>? FpsUpdated;

    public void SetDrawDelegate(FiddleDelegate? draw)
    {
        _drawDelegate = draw;
        _errorMessage = null;
        if (draw is not null)
            StartAnimation();
        else
            StopAnimation();
        Invalidate();
    }

    public void SetError(string message)
    {
        _drawDelegate = null;
        _errorMessage = message;
        StopAnimation();
        Invalidate();
    }

    public void StartAnimation()
    {
        if (IsAnimating)
            return;
        IsAnimating = true;
        _clock.Restart();
        _frameCount = 0;
        _lastFpsSampleTime = 0;
        _fps = 0;
        _frameTimer ??= new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _frameTimer.Tick -= OnFrameTick;
        _frameTimer.Tick += OnFrameTick;
        _frameTimer.Start();
    }

    public void StopAnimation()
    {
        if (!IsAnimating)
            return;
        IsAnimating = false;
        _clock.Stop();
        _frameTimer?.Stop();
    }

    public void TogglePause()
    {
        if (IsAnimating) StopAnimation();
        else if (_drawDelegate is not null) StartAnimation();
    }

    private void OnFrameTick(object? sender, object e) => Invalidate();

    protected override void RenderOverride(SKCanvas canvas, Size area)
    {
        var width = (int)area.Width;
        var height = (int)area.Height;

        canvas.Clear(new SKColor(0x1E, 0x1E, 0x1E));

        if (_errorMessage is not null)
        {
            DrawError(canvas, width, height);
            return;
        }

        if (_drawDelegate is null)
        {
            DrawPlaceholder(canvas, width, height);
            return;
        }

        var t = _clock.Elapsed.TotalSeconds;

        try
        {
            _drawDelegate(canvas, width, height, t);
        }
        catch (Exception ex)
        {
            _errorMessage = $"Runtime error: {ex.Message}";
            StopAnimation();
            canvas.Clear(new SKColor(0x1E, 0x1E, 0x1E));
            DrawError(canvas, width, height);
            return;
        }

        // FPS tracking — sample once per second.
        _frameCount++;
        if (t - _lastFpsSampleTime >= 1.0)
        {
            _fps = _frameCount / (t - _lastFpsSampleTime);
            _lastFpsSampleTime = t;
            _frameCount = 0;
            FpsUpdated?.Invoke(this, _fps);
        }
    }

    private static void DrawPlaceholder(SKCanvas canvas, int width, int height)
    {
        using var fill = new SKPaint { Color = new SKColor(0x46, 0x46, 0x46), IsAntialias = true };
        using var stroke = new SKPaint
        {
            Color = new SKColor(0x6A, 0x6A, 0x6A),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2,
        };
        var rect = SKRect.Create(20, 20, width - 40, height - 40);
        canvas.DrawRoundRect(rect, 16, 16, fill);
        canvas.DrawRoundRect(rect, 16, 16, stroke);

        using var font = new SKFont(SKTypeface.Default, 22);
        using var textPaint = new SKPaint { Color = new SKColor(0xCD, 0xCD, 0xCD), IsAntialias = true };
        const string msg = "Press Run to see the result";
        var metrics = font.Metrics;
        var textWidth = font.MeasureText(msg);
        var x = (width - textWidth) / 2;
        var y = (height - (metrics.Ascent + metrics.Descent)) / 2;
        canvas.DrawText(msg, x, y, font, textPaint);
    }

    private void DrawError(SKCanvas canvas, int width, int height)
    {
        using var bgPaint = new SKPaint { Color = new SKColor(0x40, 0x14, 0x14), IsAntialias = true };
        canvas.DrawRect(SKRect.Create(0, 0, width, height), bgPaint);

        using var titleFont = new SKFont(SKTypeface.Default, 18) { Embolden = true };
        using var titlePaint = new SKPaint { Color = new SKColor(0xFF, 0x6B, 0x6B), IsAntialias = true };
        canvas.DrawText("Error", 24, 40, titleFont, titlePaint);

        using var msgFont = new SKFont(SKTypeface.Default, 13);
        using var msgPaint = new SKPaint { Color = new SKColor(0xFF, 0xC8, 0xC8), IsAntialias = true };
        var lines = (_errorMessage ?? string.Empty).Split('\n');
        var y = 70f;
        foreach (var line in lines)
        {
            WrapAndDraw(canvas, line, 24, y, width - 48, msgFont, msgPaint);
            y += 22;
        }
    }

    private static void WrapAndDraw(SKCanvas canvas, string text, float x, float y, float maxWidth, SKFont font, SKPaint paint)
    {
        if (font.MeasureText(text) <= maxWidth)
        {
            canvas.DrawText(text, x, y, font, paint);
            return;
        }

        var words = text.Split(' ');
        var line = string.Empty;
        foreach (var word in words)
        {
            var trial = line.Length == 0 ? word : line + " " + word;
            if (font.MeasureText(trial) > maxWidth && line.Length > 0)
            {
                canvas.DrawText(line, x, y, font, paint);
                y += 22;
                line = word;
            }
            else
            {
                line = trial;
            }
        }
        if (line.Length > 0)
            canvas.DrawText(line, x, y, font, paint);
    }
}
