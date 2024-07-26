using System;

using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace DocsSamplesApp.Bitmaps
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
