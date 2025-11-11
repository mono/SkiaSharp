using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SkiaSharp;

namespace SkiaSharpSample;

internal sealed class MotionMarkScene : IDisposable
{
    private const int GridWidth = 80;
    private const int GridHeight = 40;

    private static readonly SKColor[] s_palette =
    [
        new SKColor(0x10, 0x10, 0x10),
        new SKColor(0x80, 0x80, 0x80),
        new SKColor(0xC0, 0xC0, 0xC0),
        new SKColor(0x10, 0x10, 0x10),
        new SKColor(0x80, 0x80, 0x80),
        new SKColor(0xC0, 0xC0, 0xC0),
        new SKColor(0xE0, 0x10, 0x40),
    ];

    private static readonly (int X, int Y)[] s_offsets =
    [
        (-4, 0),
        (2, 0),
        (1, -2),
        (1, 2),
    ];

    private readonly List<Element> _elements = new();
    private readonly SKPath _path = new();
    private readonly SKPaint _strokePaint = new()
    {
        IsAntialias = true,
        Style = SKPaintStyle.Fill,
        StrokeCap = SKStrokeCap.Round,
        StrokeJoin = SKStrokeJoin.Round,
    };
    private readonly SKPaint _backgroundPaint = new()
    {
        Color = new SKColor(12, 16, 24),
        Style = SKPaintStyle.Fill
    };
    private readonly Random _random = new();
    private GridPoint _lastGridPoint = new(GridWidth / 2, GridHeight / 2);
    private int _complexity = 8;
    private bool _disposed;

    public int Complexity => _complexity;
    public int ElementCount => _elements.Count;

    public void SetComplexity(int complexity)
    {
        complexity = Math.Clamp(complexity, 0, 24);
        if (_complexity == complexity)
            return;

        _complexity = complexity;
        Resize(ComputeElementCount(_complexity));
    }

    public void Render(SKCanvas canvas, float width, float height)
    {
        Resize(ComputeElementCount(_complexity));

        canvas.DrawRect(new SKRect(0, 0, width, height), _backgroundPaint);

        if (_elements.Count == 0)
            return;

        float scaleX = width / (GridWidth + 1);
        float scaleY = height / (GridHeight + 1);
        float uniformScale = MathF.Min(scaleX, scaleY);
        float offsetX = (width - uniformScale * (GridWidth + 1)) * 0.5f;
        float offsetY = (height - uniformScale * (GridHeight + 1)) * 0.5f;

        Span<Element> elements = CollectionsMarshal.AsSpan(_elements);
        _path.Reset();
        bool pathStarted = false;

        for (int i = 0; i < elements.Length; i++)
        {
            ref Element element = ref elements[i];
            if (!pathStarted)
            {
                SKPoint start = element.Start.ToPoint(uniformScale, offsetX, offsetY);
                _path.MoveTo(start);
                pathStarted = true;
            }

            switch (element.Kind)
            {
                case SegmentKind.Line:
                {
                    SKPoint end = element.End.ToPoint(uniformScale, offsetX, offsetY);
                    _path.LineTo(end);
                    break;
                }
                case SegmentKind.Quad:
                {
                    SKPoint c1 = element.Control1.ToPoint(uniformScale, offsetX, offsetY);
                    SKPoint end = element.End.ToPoint(uniformScale, offsetX, offsetY);
                    _path.QuadTo(c1, end);
                    break;
                }
                case SegmentKind.Cubic:
                {
                    SKPoint c1 = element.Control1.ToPoint(uniformScale, offsetX, offsetY);
                    SKPoint c2 = element.Control2.ToPoint(uniformScale, offsetX, offsetY);
                    SKPoint end = element.End.ToPoint(uniformScale, offsetX, offsetY);
                    _path.CubicTo(c1, c2, end);
                    break;
                }
            }

            bool finalize = element.Split || i == elements.Length - 1;
            if (finalize)
            {
                _strokePaint.Color = element.Color;
                _strokePaint.StrokeWidth = element.Width;
                canvas.DrawPath(_path, _strokePaint);
                _path.Reset();
                pathStarted = false;
            }

            if (_random.NextDouble() > 0.995)
            {
                element.Split = !element.Split;
            }
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _path.Dispose();
        _strokePaint.Dispose();
        _backgroundPaint.Dispose();
        _disposed = true;
    }

    private void Resize(int count)
    {
        int current = _elements.Count;
        if (count == current)
            return;

        if (count < current)
        {
            _elements.RemoveRange(count, current - count);
            _lastGridPoint = count > 0
                ? _elements[^1].End
                : new GridPoint(GridWidth / 2, GridHeight / 2);
            return;
        }

        _elements.Capacity = Math.Max(_elements.Capacity, count);
        if (current == 0)
        {
            _lastGridPoint = new GridPoint(GridWidth / 2, GridHeight / 2);
        }
        else
        {
            _lastGridPoint = _elements[^1].End;
        }

        for (int i = current; i < count; i++)
        {
            Element element = CreateRandomElement(_lastGridPoint);
            _elements.Add(element);
            _lastGridPoint = element.End;
        }
    }

    private Element CreateRandomElement(GridPoint last)
    {
        int segType = _random.Next(4);
        GridPoint next = RandomPoint(last);

        Element element = default;
        element.Start = last;

        if (segType < 2)
        {
            element.Kind = SegmentKind.Line;
            element.End = next;
        }
        else if (segType == 2)
        {
            GridPoint p2 = RandomPoint(next);
            element.Kind = SegmentKind.Quad;
            element.Control1 = next;
            element.End = p2;
        }
        else
        {
            GridPoint p2 = RandomPoint(next);
            GridPoint p3 = RandomPoint(next);
            element.Kind = SegmentKind.Cubic;
            element.Control1 = next;
            element.Control2 = p2;
            element.End = p3;
        }

        element.Color = s_palette[_random.Next(s_palette.Length)];
        element.Width = (float)(Math.Pow(_random.NextDouble(), 5) * 20.0 + 1.0);
        element.Split = _random.Next(2) == 0;
        return element;
    }

    private static int ComputeElementCount(int complexity)
    {
        if (complexity < 10)
        {
            return (complexity + 1) * 1_000;
        }

        int extended = (complexity - 8) * 10_000;
        return Math.Min(extended, 120_000);
    }

    private GridPoint RandomPoint(GridPoint last)
    {
        var offset = s_offsets[_random.Next(s_offsets.Length)];

        int x = last.X + offset.X;
        if (x < 0 || x > GridWidth)
        {
            x -= offset.X * 2;
        }

        int y = last.Y + offset.Y;
        if (y < 0 || y > GridHeight)
        {
            y -= offset.Y * 2;
        }

        return new GridPoint(x, y);
    }

    private enum SegmentKind : byte
    {
        Line,
        Quad,
        Cubic
    }

    private struct Element
    {
        public SegmentKind Kind;
        public GridPoint Start;
        public GridPoint Control1;
        public GridPoint Control2;
        public GridPoint End;
        public SKColor Color;
        public float Width;
        public bool Split;
    }

    private readonly struct GridPoint
    {
        public GridPoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }
        public int Y { get; }

        public SKPoint ToPoint(float scale, float offsetX, float offsetY)
        {
            float px = offsetX + (X + 0.5f) * scale;
            float py = offsetY + (Y + 0.5f) * scale;
            return new SKPoint(px, py);
        }
    }
}
