using System;
using Xamarin.Forms;

namespace SkiaSharpFormsDemos.Bitmaps
{
    class PhotoPuzzleTile : ContentView
    {
        public PhotoPuzzleTile (int row, int col, ImageSource imageSource)
        {
            Row = row;
            Col = col;

            Padding = new Thickness(1);
            Content = new Image
            {
                Source = imageSource
            };
        }

        public int Row { set; get; }

        public int Col { set; get; }
    }
}
