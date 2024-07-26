using System;
using Xamarin.Forms;
using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Curves
{
    public class AnotherRoundedHeptagonPage : ContentPage
    {
        public AnotherRoundedHeptagonPage()
        {
            Title = "Another Rounded Heptagon";

            SKCanvasView canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            int numVertices = 7;
            float radius = 0.45f * Math.Min(info.Width, info.Height);
            SKPoint[] vertices = new SKPoint[numVertices];
            double vertexAngle = -0.5f * Math.PI;       // straight up

            // Coordinates of the vertices of the polygon
            for (int vertex = 0; vertex < numVertices; vertex++)
            {
                vertices[vertex] = new SKPoint(radius * (float)Math.Cos(vertexAngle),
                                               radius * (float)Math.Sin(vertexAngle));
                vertexAngle += 2 * Math.PI / numVertices;
            }

            float cornerRadius = 100;

            // Create the path
            using (SKPath path = new SKPath())
            {
                path.AddPoly(vertices, true);

                // Render the path in the center of the screen
                using (SKPaint paint = new SKPaint())
                {
                    paint.Style = SKPaintStyle.Stroke;
                    paint.Color = SKColors.Blue;
                    paint.StrokeWidth = 10;

                    // Set argument to half the desired corner radius!
                    paint.PathEffect = SKPathEffect.CreateCorner(cornerRadius / 2);

                    canvas.Translate(info.Width / 2, info.Height / 2);
                    canvas.DrawPath(path, paint);

                    // Uncomment DrawCircle call to verify corner radius
                    float offset = cornerRadius / (float)Math.Sin(Math.PI * (numVertices - 2) / numVertices / 2);
                    paint.Color = SKColors.Green;
                    // canvas.DrawCircle(vertices[0].X, vertices[0].Y + offset, cornerRadius, paint);
                }
            }
        }
    }
}
