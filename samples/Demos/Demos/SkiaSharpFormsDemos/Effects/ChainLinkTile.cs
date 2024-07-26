using System;
using SkiaSharp;

namespace SkiaSharpFormsDemos.Effects
{
    // This class is not used. 
    // It was replaced by much shorter code in ChainLinkFencePage.cs, but this file 
    //      demonstrates the more complex logic when the result is not rotated.
    class ChainLinkTile
    {
        readonly int tileSize;
        readonly float wireThickness;
        readonly float cornerOffset;
        readonly SKColor[] shadeGradientColors = new SKColor[] { SKColors.Silver, SKColors.Black };
        readonly float[] shadeGradientOffsets = new float[] { 0.4f, 0.6f };

        public ChainLinkTile(int tileSize)
        {
            this.tileSize = tileSize;
            wireThickness = tileSize / 16;

            // Where the wires cross the edge near the corner
            cornerOffset = wireThickness / (float)Math.Sqrt(2);

            SKBitmap bitmap = new SKBitmap(tileSize, tileSize);

            using (SKCanvas canvas = new SKCanvas(bitmap))
            using (SKPaint paint = new SKPaint())
            using (SKPath path = new SKPath())
            {
                canvas.Clear();
                paint.IsAntialias = true;

                // Divide bitmap into upper-Left and lower-right quadrants
                LinkCrossQuadrant(canvas, path, paint, new SKRect(0, 0, tileSize / 2, tileSize / 2));
                LinkCrossQuadrant(canvas, path, paint, new SKRect(tileSize / 2, tileSize / 2, tileSize, tileSize));

                // Convert SKPaint object for drawing lines
                paint.Style = SKPaintStyle.Stroke;
                paint.StrokeWidth = wireThickness;

                // Divide bitmap into upper-right and lower-left quadrants
                MostlyEmptyQuadrant(canvas, paint, new SKRect(tileSize / 2, 0, tileSize, tileSize / 2));
                MostlyEmptyQuadrant(canvas, paint, new SKRect(0, tileSize / 2, tileSize / 2, tileSize));
            }

            // Set public property
            Bitmap = bitmap;
        }

        public SKBitmap Bitmap { private set; get; }

        void LinkCrossQuadrant(SKCanvas canvas, SKPath path, SKPaint paint, SKRect rect)
        {
            SKPoint center = new SKPoint(rect.MidX, rect.MidY);

            paint.Shader = SKShader.CreateLinearGradient(new SKPoint(rect.Right, rect.Top),
                                                         new SKPoint(rect.Left, rect.Bottom),
                                                         shadeGradientColors,
                                                         shadeGradientOffsets, SKShaderTileMode.Clamp);

            // Create and draw path
            StraightWire(path, center, new SKPoint(rect.Left, rect.Top),                 // the corner point
                                       new SKPoint(rect.Left, rect.Top + cornerOffset),  // the point near the corner that goes to center
                                       new SKPoint(rect.Left + cornerOffset, rect.Top)); // the other point near the corner
            canvas.DrawPath(path, paint);

            StraightWire(path, center, new SKPoint(rect.Right, rect.Bottom), 
                                       new SKPoint(rect.Right, rect.Bottom - cornerOffset), 
                                       new SKPoint(rect.Right - cornerOffset, rect.Bottom));
            canvas.DrawPath(path, paint);

            paint.Shader = SKShader.CreateLinearGradient(new SKPoint(rect.Right, rect.Top),
                                                         new SKPoint(rect.Left, rect.Bottom),
                                                         new SKColor[] { SKColors.Silver, SKColors.Black },
                                                         null, SKShaderTileMode.Clamp);

            CurvedWire(path, center, new SKPoint(rect.Right, rect.Top), 
                                     new SKPoint(rect.Right, rect.Top + cornerOffset), 
                                     new SKPoint(rect.Right - cornerOffset, rect.Top));

            canvas.DrawPath(path, paint);

            paint.Shader = SKShader.CreateLinearGradient(new SKPoint(rect.Right, rect.Top),
                                                         new SKPoint(rect.Left, rect.Bottom),
                                                         new SKColor[] { SKColors.White, SKColors.Silver },
                                                         null, SKShaderTileMode.Clamp);

            CurvedWire(path, center, new SKPoint(rect.Left, rect.Bottom), 
                                     new SKPoint(rect.Left, rect.Bottom - cornerOffset), 
                                     new SKPoint(rect.Left + cornerOffset, rect.Bottom));

            canvas.DrawPath(path, paint);
        }

        void MostlyEmptyQuadrant(SKCanvas canvas, SKPaint paint, SKRect rect)
        {
            using (new SKAutoCanvasRestore(canvas))
            {
                canvas.ClipRect(rect, SKClipOperation.Intersect);

                // Fill in the lines on the upper-left and lower-right
                paint.Shader = null;
                paint.Color = SKColors.Silver;

                canvas.DrawLine(rect.Left - tileSize, rect.Top + tileSize, 
                                rect.Left + tileSize, rect.Top - tileSize, paint);

                canvas.DrawLine(rect.Right - tileSize, rect.Bottom + tileSize, 
                                rect.Right + tileSize, rect.Bottom - tileSize, paint);

                // Fill in the line on the lower-left
                paint.Shader = SKShader.CreateLinearGradient(new SKPoint(rect.MidX, rect.MidY), 
                                                             new SKPoint(rect.Left - rect.Width / 2, rect.Bottom + rect.Height / 2),
                                                             shadeGradientColors,
                                                             shadeGradientOffsets,
                                                             SKShaderTileMode.Clamp);

                canvas.DrawLine(rect.Left - tileSize, rect.Bottom - tileSize, 
                                rect.Left + tileSize, rect.Bottom + tileSize, paint);


                // Fill in the line on the upper-right
                paint.Shader = SKShader.CreateLinearGradient(new SKPoint(rect.Right + rect.Width / 2, rect.Top - rect.Height / 2), 
                                                             new SKPoint(rect.MidX, rect.MidY),
                                                             shadeGradientColors,
                                                             shadeGradientOffsets,
                                                             SKShaderTileMode.Clamp);

                canvas.DrawLine(rect.Right - tileSize, rect.Top - tileSize, 
                                rect.Right + tileSize, rect.Top + tileSize, paint);
            }
        }

        void StraightWire(SKPath path, SKPoint center, SKPoint corner, SKPoint pt1, SKPoint pt2)
        {
            path.Reset();

            SKPoint vector = center - pt1;
            path.MoveTo(pt1);
            path.LineTo(pt1 + vector);
            path.LineTo(pt2 + vector);
            path.LineTo(pt2);
            path.LineTo(corner);
            path.Close();
        }

        void CurvedWire(SKPath path, SKPoint center, SKPoint corner, SKPoint pt1, SKPoint pt2)
        {
            path.Reset();

            SKPoint vector = center - pt1;

            // Vector that goes beyond the center to the end of the curve
            SKPoint vector2 = vector;
            float length = (float)Math.Sqrt(Math.Pow(vector2.X, 2) + Math.Pow(vector2.Y, 2));
            vector2.X /= length;
            vector2.Y /= length;
            vector2.X *= (length + wireThickness);
            vector2.Y *= (length + wireThickness);

            path.MoveTo(pt2);
            path.LineTo(pt2 + vector);
            path.ArcTo(new SKPoint(wireThickness, wireThickness), 
                       0, 
                       SKPathArcSize.Small, 
                       SKPathDirection.CounterClockwise, 
                       pt1 + vector2);
            path.LineTo(pt1);
            path.LineTo(corner);
            path.Close();
        }
    }
}
