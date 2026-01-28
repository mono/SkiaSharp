using System;

using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;

namespace DocsSamplesApp.Bitmaps
{
    public class LatticeDisplayPage : ContentPage
    {
        SKBitmap bitmap = NinePatchDisplayPage.FiveByFiveBitmap;

        public LatticeDisplayPage()
        {
            Title = "Lattice Display";

            SKCanvasView canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;
        }

        void OnCanvasViewPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            SKLattice lattice = new SKLattice();
            lattice.XDivs = new int[] { 100, 200, 400 };
            lattice.YDivs = new int[] { 100, 300, 400 };

            int count = (lattice.XDivs.Length + 1) * (lattice.YDivs.Length + 1);
            lattice.RectTypes = new SKLatticeRectType[count];

            canvas.DrawBitmapLattice(bitmap, lattice, info.Rect);
        }
    }
}