using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Curves
{
    public partial class QuadraticCurvePage : InteractivePage
    {
        public QuadraticCurvePage()
        {
            touchPoints = new TouchPoint[3];

            for (int i = 0; i < 3; i++)
            {
                TouchPoint touchPoint = new TouchPoint
                {
                    Center = new SKPoint(100 + 200 * i,
                                         100 + (i == 1 ? 300 : 0))
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

            // Draw path with quadratic Bezier
            using (SKPath path = new SKPath())
            {
                path.MoveTo(touchPoints[0].Center);
                path.QuadTo(touchPoints[1].Center,
                            touchPoints[2].Center);

                canvas.DrawPath(path, strokePaint);
            }

            // Draw tangent lines
            canvas.DrawLine(touchPoints[0].Center.X,
                            touchPoints[0].Center.Y,
                            touchPoints[1].Center.X,
                            touchPoints[1].Center.Y, dottedStrokePaint);

            canvas.DrawLine(touchPoints[1].Center.X,
                            touchPoints[1].Center.Y,
                            touchPoints[2].Center.X,
                            touchPoints[2].Center.Y, dottedStrokePaint);

            foreach (TouchPoint touchPoint in touchPoints)
            {
                touchPoint.Paint(canvas);
            }
        }
    }
}
