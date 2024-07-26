using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Curves
{
    public partial class PathLengthPage : InteractivePage
    {
        const string text = "Compute length of path";

        static SKPaint textPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.Black,
            TextSize = 10,
        };

        static readonly float baseTextWidth = textPaint.MeasureText(text);

        public PathLengthPage()
        {
            touchPoints = new TouchPoint[4];

            for (int i = 0; i < 4; i++)
            {
                TouchPoint touchPoint = new TouchPoint
                {
                    Center = new SKPoint(100 + 200 * (i % 2),
                                         100 + 200 * i)
                };
                touchPoints[i] = touchPoint;
            }

            InitializeComponent();
            baseCanvasView = canvasView;
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            // Draw path with cubic Bezier curve
            using (SKPath path = new SKPath())
            {
                path.MoveTo(touchPoints[0].Center);
                path.CubicTo(touchPoints[1].Center,
                             touchPoints[2].Center,
                             touchPoints[3].Center);

                canvas.DrawPath(path, strokePaint);

                // Get path length
                SKPathMeasure pathMeasure = new SKPathMeasure(path, false, 1);

                // Find new text size
                textPaint.TextSize = pathMeasure.Length / baseTextWidth * 10;

                // Draw text on path
                canvas.DrawTextOnPath(text, path, 0, 0, textPaint);
            }

            foreach (TouchPoint touchPoint in touchPoints)
            {
                touchPoint.Paint(canvas);
            }
        }
    }
}