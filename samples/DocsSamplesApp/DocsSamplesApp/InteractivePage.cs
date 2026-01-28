using System;

using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace DocsSamplesApp
{
    public class InteractivePage : ContentPage
    {
        protected SKCanvasView baseCanvasView;
        protected TouchPoint[] touchPoints;

        protected SKPaint strokePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Black,
            StrokeWidth = 3
        };

        protected SKPaint redStrokePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Red,
            StrokeWidth = 15
        };

        protected SKPaint dottedStrokePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Black,
            StrokeWidth = 3,
            PathEffect = SKPathEffect.CreateDash(new float[] { 7, 7 }, 0)
        };

        protected void OnTouch(object sender, SKTouchEventArgs e)
        {
            bool touchPointMoved = false;

            foreach (TouchPoint touchPoint in touchPoints)
            {
                // Location is already in pixels with built-in touch
                touchPointMoved |= touchPoint.ProcessTouchEvent(e.Id, e.ActionType, e.Location);
            }

            if (touchPointMoved)
            {
                baseCanvasView.InvalidateSurface();
            }

            e.Handled = true;
        }
    }
}
