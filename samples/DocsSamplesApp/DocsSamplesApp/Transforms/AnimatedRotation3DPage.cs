using System;

using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace DocsSamplesApp.Transforms
{
    public class AnimatedRotation3DPage : ContentPage
    {
        SKCanvasView canvasView;
        float xRotationDegrees, yRotationDegrees, zRotationDegrees;
        string text = "SkiaSharp"; 
        SKPaint textPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Black,
            StrokeWidth = 3,
        };
        SKFont textFont = new SKFont { Size = 100 };
        SKRect textBounds;

        public AnimatedRotation3DPage()
        {
            Title = "Animated Rotation 3D";

            canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;

            // Measure the text
            textFont.MeasureText(text, out textBounds);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            new Animation((value) => xRotationDegrees = 360 * (float)value).
                Commit(this, "xRotationAnimation", length: 5000, repeat: () => true);

            new Animation((value) => yRotationDegrees = 360 * (float)value).
                Commit(this, "yRotationAnimation", length: 7000, repeat: () => true);

            new Animation((value) =>
            {
                zRotationDegrees = 360 * (float)value;
                canvasView.InvalidateSurface();
            }).Commit(this, "zRotationAnimation", length: 11000, repeat: () => true);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            this.AbortAnimation("xRotationAnimation");
            this.AbortAnimation("yRotationAnimation");
            this.AbortAnimation("zRotationAnimation");
        }

        void OnCanvasViewPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            // Find center of canvas
            float xCenter = info.Width / 2;
            float yCenter = info.Height / 2;

            // Translate center to origin
            SKMatrix matrix = SKMatrix.CreateTranslation(-xCenter, -yCenter);

            // Scale so text fits
            float scale = Math.Min(info.Width / textBounds.Width, 
                                   info.Height / textBounds.Height);
            matrix = matrix.PostConcat(SKMatrix.CreateScale(scale, scale));

            // Calculate composite 3D transforms
            float depth = 0.75f * scale * textBounds.Width;

            SKMatrix44 matrix44 = SKMatrix44.CreateIdentity();
            matrix44 *= SKMatrix44.CreateRotationDegrees(1, 0, 0, xRotationDegrees);
            matrix44 *= SKMatrix44.CreateRotationDegrees(0, 1, 0, yRotationDegrees);
            matrix44 *= SKMatrix44.CreateRotationDegrees(0, 0, 1, zRotationDegrees);

            SKMatrix44 perspectiveMatrix = SKMatrix44.CreateIdentity();
            perspectiveMatrix[2, 3] = -1 / depth;
            matrix44 *= perspectiveMatrix;

            // Concatenate with 2D matrix
            matrix = matrix.PostConcat(matrix44.Matrix);

            // Translate back to center
            matrix = matrix.PostConcat(
                SKMatrix.CreateTranslation(xCenter, yCenter));

            // Set the matrix and display the text
            canvas.SetMatrix(matrix);
            float xText = xCenter - textBounds.MidX;
            float yText = yCenter - textBounds.MidY;
            canvas.DrawText(text, xText, yText, SKTextAlign.Left, textFont, textPaint);
        }
    }
}