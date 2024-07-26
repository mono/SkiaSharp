using System;
using Xamarin.Forms;
using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Curves
{
    public class LinkedChainPage : ContentPage
    {
        const float linkRadius = 30;
        const float linkThickness = 5;

        Func<float, float, float> catenary = (float a, float x) => (float)(a * Math.Cosh(x / a));

        SKPaint linksPaint = new SKPaint
        {
            Color = SKColors.Silver
        };

        public LinkedChainPage()
        {
            Title = "Linked Chain";

            SKCanvasView canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;

            // Create the path for the individual links
            SKRect outer = new SKRect(-linkRadius, -linkRadius, linkRadius, linkRadius);
            SKRect inner = outer;
            inner.Inflate(-linkThickness, -linkThickness);

            using (SKPath linkPath = new SKPath())
            {
                linkPath.AddArc(outer, 55, 160);
                linkPath.ArcTo(inner, 215, -160, false);
                linkPath.Close();

                linkPath.AddArc(outer, 235, 160);
                linkPath.ArcTo(inner, 395, -160, false);
                linkPath.Close();

                // Set that path as the 1D path effect for linksPaint
                linksPaint.PathEffect = 
                    SKPathEffect.Create1DPath(linkPath, 1.3f * linkRadius, 0, 
                                              SKPath1DPathEffectStyle.Rotate);
            }
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear(SKColors.Black);

            // Width and height of catenary
            int width = info.Width;
            float height = info.Height - linkRadius;

            // Find the optimum 'a' for this width and height
            float optA = FindOptimumA(width, height);

            // Calculate the vertical offset for that value of 'a'
            float yOffset = catenary(optA, -width / 2);

            // Create a path for the catenary
            SKPoint[] points = new SKPoint[width];

            for (int x = 0; x < width; x++)
            {
                points[x] = new SKPoint(x, yOffset - catenary(optA, x - width / 2));
            }

            using (SKPath path = new SKPath())
            {
                path.AddPoly(points, false);

                // And render that path with the linksPaint object
                canvas.DrawPath(path, linksPaint);
            }
        }

        float FindOptimumA(float width, float height)
        {
            Func<float, float> left = (float a) => (float)Math.Cosh(width / 2 / a);
            Func<float, float> right = (float a) => 1 + height / a;

            float gtA = 1;         // starting value for left > right
            float ltA = 10000;     // starting value for left < right

            while (Math.Abs(gtA - ltA) > 0.1f)
            {
                float avgA = (gtA + ltA) / 2;

                if (left(avgA) < right(avgA))
                {
                    ltA = avgA;
                }
                else
                {
                    gtA = avgA;
                }
            }

            return (gtA + ltA) / 2;
        }
    }
}
