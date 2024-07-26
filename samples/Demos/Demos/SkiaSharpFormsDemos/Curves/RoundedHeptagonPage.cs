using System;
using Xamarin.Forms;
using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Curves
{
    public class RoundedHeptagonPage : ContentPage
    {
        public RoundedHeptagonPage()
        {
            Title = "Rounded Heptagon";

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

            float cornerRadius = 100;
            int numVertices = 7;
            float radius = 0.45f * Math.Min(info.Width, info.Height);

            SKPoint[] vertices = new SKPoint[numVertices];
            SKPoint[] midPoints = new SKPoint[numVertices];

            double vertexAngle = -0.5f * Math.PI;       // straight up

            // Coordinates of the vertices of the polygon
            for (int vertex = 0; vertex < numVertices; vertex++)
            {
                vertices[vertex] = new SKPoint(radius * (float)Math.Cos(vertexAngle),
                                               radius * (float)Math.Sin(vertexAngle));
                vertexAngle += 2 * Math.PI / numVertices;
            }

            // Coordinates of the midpoints of the sides connecting the vertices
            for (int vertex = 0; vertex < numVertices; vertex++)
            {
                int prevVertex = (vertex + numVertices - 1) % numVertices;
                midPoints[vertex] = new SKPoint((vertices[prevVertex].X + vertices[vertex].X) / 2,
                                                (vertices[prevVertex].Y + vertices[vertex].Y) / 2);
            }

            // Create the path
            using (SKPath path = new SKPath())
            {
                // Begin at the first midpoint
                path.MoveTo(midPoints[0]);

                for (int vertex = 0; vertex < numVertices; vertex++)
                {
                    SKPoint nextMidPoint = midPoints[(vertex + 1) % numVertices];

                    // Draws a line from the current point, and then the arc
                    path.ArcTo(vertices[vertex], nextMidPoint, cornerRadius);

                    // Connect the arc with the next midpoint
                    path.LineTo(nextMidPoint);
                }
                path.Close();

                // Render the path in the center of the screen
                using (SKPaint paint = new SKPaint())
                {
                    paint.Style = SKPaintStyle.Stroke;
                    paint.Color = SKColors.Blue;
                    paint.StrokeWidth = 10;

                    canvas.Translate(info.Width / 2, info.Height / 2);
                    canvas.DrawPath(path, paint);
                }
            }
        }
    }
}