using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Curves
{
    public class RegionOperationsPage : ContentPage
    {
        SKPaint textPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.Black,
            TextSize = 40,
            TextAlign = SKTextAlign.Center
        };

        SKPaint fillPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.Pink
        };

        public RegionOperationsPage()
        {
            Title = "Region Operations";

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

            float x = 0;
            float y = 0;
            float width = info.Height > info.Width ? info.Width / 2 : info.Width / 3;
            float height = info.Height > info.Width ? info.Height / 3 : info.Height / 2;
            
            foreach (SKRegionOperation regionOp in Enum.GetValues(typeof(SKRegionOperation)))
            {
                DisplayClipOp(canvas, new SKRect(x, y, x + width, y + height), regionOp);

                if ((x += width) >= info.Width)
                {
                    x = 0;
                    y += height;
                }
            }
        }

        void DisplayClipOp(SKCanvas canvas, SKRect rect, SKRegionOperation regionOp)
        {
            float textSize = textPaint.TextSize;
            canvas.DrawText(regionOp.ToString(), rect.MidX, rect.Top + textSize, textPaint);
            rect.Top += textSize;

            float radius = 0.9f * Math.Min(rect.Width / 3, rect.Height / 2);
            float xCenter = rect.MidX;
            float yCenter = rect.MidY;

            SKRectI recti = new SKRectI((int)rect.Left, (int)rect.Top, 
                                        (int)rect.Right, (int)rect.Bottom);

            using (SKRegion wholeRectRegion = new SKRegion())
            {
                wholeRectRegion.SetRect(recti);

                using (SKRegion region1 = new SKRegion(wholeRectRegion))
                using (SKRegion region2 = new SKRegion(wholeRectRegion))
                {
                    using (SKPath path1 = new SKPath())
                    {
                        path1.AddCircle(xCenter - radius / 2, yCenter, radius);
                        region1.SetPath(path1);
                    }

                    using (SKPath path2 = new SKPath())
                    {
                        path2.AddCircle(xCenter + radius / 2, yCenter, radius);
                        region2.SetPath(path2);
                    }

                    region1.Op(region2, regionOp);

                    canvas.Save();
                    canvas.ClipRegion(region1);
                    canvas.DrawPaint(fillPaint);
                    canvas.Restore();
                }
            }
        }
    }
}
