using System;
using System.IO;
using System.Reflection;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Curves
{
    public class MonkeyThroughKeyholePage : ContentPage
    {
        SKBitmap bitmap;
        SKPath keyholePath = SKPath.ParseSvgPathData(
            "M 300 130 L 250 350 L 450 350 L 400 130 A 70 70 0 1 0 300 130 Z");

        public MonkeyThroughKeyholePage()
        {
            Title = "Monkey through Keyhole";

            SKCanvasView canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;

            string resourceID = "SkiaSharpFormsDemos.Media.SeatedMonkey.jpg";
            Assembly assembly = GetType().GetTypeInfo().Assembly;

            using (Stream stream = assembly.GetManifestResourceStream(resourceID))
            {
                bitmap = SKBitmap.Decode(stream);
            }
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            // Set transform to center and enlarge clip path to window height
            SKRect bounds;
            keyholePath.GetTightBounds(out bounds);

            canvas.Translate(info.Width / 2, info.Height / 2);
            canvas.Scale(0.98f * info.Height / bounds.Height);
            canvas.Translate(-bounds.MidX, -bounds.MidY);

            // Set the clip path
            canvas.ClipPath(keyholePath);

            // Reset transforms
            canvas.ResetMatrix();

            // Display monkey to fill height of window but maintain aspect ratio
            canvas.DrawBitmap(bitmap, 
                new SKRect((info.Width - info.Height) / 2, 0,
                           (info.Width + info.Height) / 2, info.Height));
        }
    }
}
