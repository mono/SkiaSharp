using System;

using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;

namespace DocsSamplesApp.Bitmaps
{
    public class LatticeNinePatchPage : ContentPage
    {
        SKBitmap bitmap = NinePatchDisplayPage.FiveByFiveBitmap;

        public LatticeNinePatchPage ()
        {
            Title = "Lattice Nine-Patch";

            SKCanvasView canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;
        }
        
        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            SKLattice lattice = new SKLattice();
            lattice.XDivs = new int[] { 100, 400 };
            lattice.YDivs = new int[] { 100, 400 };
            lattice.RectTypes = new SKLatticeRectType[9]; 

            canvas.DrawBitmapLattice(bitmap, lattice, info.Rect);
        }
    }
}