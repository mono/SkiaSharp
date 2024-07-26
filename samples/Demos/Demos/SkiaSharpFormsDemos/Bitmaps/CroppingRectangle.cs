using System;
using System.Collections.Generic;
using System.Text;

using SkiaSharp;

namespace SkiaSharpFormsDemos.Bitmaps
{
    class CroppingRectangle
    {
        const float MINIMUM = 10;   // pixels width or height

        SKRect maxRect;             // generally the size of the bitmap
        float? aspectRatio;

        public CroppingRectangle(SKRect maxRect, float? aspectRatio = null)
        {
            this.maxRect = maxRect;
            this.aspectRatio = aspectRatio;

            // Set initial cropping rectangle
            Rect = new SKRect(0.9f * maxRect.Left + 0.1f * maxRect.Right,
                              0.9f * maxRect.Top + 0.1f * maxRect.Bottom,
                              0.1f * maxRect.Left + 0.9f * maxRect.Right,
                              0.1f * maxRect.Top + 0.9f * maxRect.Bottom);

            // Adjust for aspect ratio
            if (aspectRatio.HasValue)
            {
                SKRect rect = Rect;
                float aspect = aspectRatio.Value;

                if (rect.Width > aspect * rect.Height)
                {
                    float width = aspect * rect.Height;
                    rect.Left = (maxRect.Width - width) / 2;
                    rect.Right = rect.Left + width;
                }
                else
                {
                    float height = rect.Width / aspect;
                    rect.Top = (maxRect.Height - height) / 2;
                    rect.Bottom = rect.Top + height;
                }

                Rect = rect;
            }
        }

        public SKRect Rect { set; get; }

        public SKPoint[] Corners
        {
            get
            {
                return new SKPoint[]
                {
                    new SKPoint(Rect.Left, Rect.Top),
                    new SKPoint(Rect.Right, Rect.Top),
                    new SKPoint(Rect.Right, Rect.Bottom),
                    new SKPoint(Rect.Left, Rect.Bottom)
                };
            }
        }

        public int HitTest(SKPoint point, float radius)
        {
            SKPoint[] corners = Corners;

            for (int index = 0; index < corners.Length; index++)
            {
                SKPoint diff = point - corners[index];
                
                if ((float)Math.Sqrt(diff.X * diff.X + diff.Y * diff.Y) < radius)
                {
                    return index;
                }
            }

            return -1;
        }

        public void MoveCorner(int index, SKPoint point)
        {
            SKRect rect = Rect;

            switch (index)
            {
                case 0: // upper-left
                    rect.Left = Math.Min(Math.Max(point.X, maxRect.Left), rect.Right - MINIMUM);
                    rect.Top = Math.Min(Math.Max(point.Y, maxRect.Top), rect.Bottom - MINIMUM);
                    break;

                case 1: // upper-right
                    rect.Right = Math.Max(Math.Min(point.X, maxRect.Right), rect.Left + MINIMUM);
                    rect.Top = Math.Min(Math.Max(point.Y, maxRect.Top), rect.Bottom - MINIMUM);
                    break;

                case 2: // lower-right
                    rect.Right = Math.Max(Math.Min(point.X, maxRect.Right), rect.Left + MINIMUM);
                    rect.Bottom = Math.Max(Math.Min(point.Y, maxRect.Bottom), rect.Top + MINIMUM);
                    break;

                case 3: // lower-left
                    rect.Left = Math.Min(Math.Max(point.X, maxRect.Left), rect.Right - MINIMUM);
                    rect.Bottom = Math.Max(Math.Min(point.Y, maxRect.Bottom), rect.Top + MINIMUM);
                    break;
            }

            // Adjust for aspect ratio
            if (aspectRatio.HasValue)
            {
                float aspect = aspectRatio.Value;

                if (rect.Width > aspect * rect.Height)
                {
                    float width = aspect * rect.Height;

                    switch (index)
                    {
                        case 0:
                        case 3: rect.Left = rect.Right - width; break;
                        case 1:
                        case 2: rect.Right = rect.Left + width; break;
                    }
                }
                else
                {
                    float height = rect.Width / aspect;

                    switch (index)
                    {
                        case 0:
                        case 1: rect.Top = rect.Bottom - height; break;
                        case 2:
                        case 3: rect.Bottom = rect.Top + height; break;
                    }
                }
            }

            Rect = rect;
        }
    }
}
