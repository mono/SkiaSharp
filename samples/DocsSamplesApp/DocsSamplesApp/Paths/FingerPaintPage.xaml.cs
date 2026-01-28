using System.Collections.Generic;

using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;

namespace DocsSamplesApp.Paths
{
    public partial class FingerPaintPage : ContentPage
    {
        Dictionary<long, SKPath> inProgressPaths = new Dictionary<long, SKPath>();
        List<SKPath> completedPaths = new List<SKPath>();

        SKPaint paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Blue,
            StrokeWidth = 10,
            StrokeCap = SKStrokeCap.Round,
            StrokeJoin = SKStrokeJoin.Round
        };

        public FingerPaintPage()
        {
            InitializeComponent();
        }

        void OnTouch(object sender, SKTouchEventArgs e)
        {
            switch (e.ActionType)
            {
                case SKTouchAction.Pressed:
                    if (!inProgressPaths.ContainsKey(e.Id))
                    {
                        SKPath path = new SKPath();
                        path.MoveTo(e.Location);
                        inProgressPaths.Add(e.Id, path);
                        canvasView.InvalidateSurface();
                    }
                    break;

                case SKTouchAction.Moved:
                    if (inProgressPaths.ContainsKey(e.Id))
                    {
                        SKPath path = inProgressPaths[e.Id];
                        path.LineTo(e.Location);
                        canvasView.InvalidateSurface();
                    }
                    break;

                case SKTouchAction.Released:
                    if (inProgressPaths.ContainsKey(e.Id))
                    {
                        completedPaths.Add(inProgressPaths[e.Id]);
                        inProgressPaths.Remove(e.Id);
                        canvasView.InvalidateSurface();
                    }
                    break;

                case SKTouchAction.Cancelled:
                    if (inProgressPaths.ContainsKey(e.Id))
                    {
                        inProgressPaths.Remove(e.Id);
                        canvasView.InvalidateSurface();
                    }
                    break;
            }

            e.Handled = true;
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKCanvas canvas = args.Surface.Canvas;
            canvas.Clear();

            foreach (SKPath path in completedPaths)
            {
                canvas.DrawPath(path, paint);
            }

            foreach (SKPath path in inProgressPaths.Values)
            {
                canvas.DrawPath(path, paint);
            }
        }
    }
}
