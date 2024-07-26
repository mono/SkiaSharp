using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace SkiaSharpFormsDemos.Bitmaps
{
    public partial class PhotoPuzzlePage4 : ContentPage
    {
        // Number of tiles horizontally and vertically.
        static readonly int NUM = 4;

        // Array of tiles, and empty row & column.
        PhotoPuzzleTile[,] tiles = new PhotoPuzzleTile[NUM, NUM];
        int emptyRow = NUM - 1;
        int emptyCol = NUM - 1;

        double tileSize;
        bool isBusy;

        public PhotoPuzzlePage4 (ImageSource[] imageSources)
        {
            InitializeComponent ();

            // Loop through the rows and columns.
            for (int row = 0; row < NUM; row++)
            {
                for (int col = 0; col < NUM; col++)
                {
                    // But skip the last one!
                    if (row == NUM - 1 && col == NUM - 1)
                        break;

                    // Get the bitmap for each tile and instantiate it.
                    ImageSource imageSource = imageSources[NUM * row + col];

                    PhotoPuzzleTile tile = new PhotoPuzzleTile(row, col, imageSource);

                    // Add tap recognition.
                    TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer
                    {
                        Command = new Command(OnTileTapped),
                        CommandParameter = tile
                    };
                    tile.GestureRecognizers.Add(tapGestureRecognizer);

                    // Add it to the array and the AbsoluteLayout.
                    tiles[row, col] = tile;
                    absoluteLayout.Children.Add(tile);
                }
            }
        }

        void OnContentViewSizeChanged(object sender, EventArgs args)
        {
            ContentView contentView = (ContentView)sender;
            double width = contentView.Width;
            double height = contentView.Height;

            if (width <= 0 || height <= 0)
                return;

            // Orient StackLayout based on portrait/landscape mode.
            stackLayout.Orientation = (width < height) ? StackOrientation.Vertical :
                                                         StackOrientation.Horizontal;

            // Calculate tile size and position based on ContentView size.
            tileSize = Math.Min(width, height) / NUM;
            absoluteLayout.WidthRequest = NUM * tileSize;
            absoluteLayout.HeightRequest = NUM * tileSize;

            foreach (View view in absoluteLayout.Children)
            {
                PhotoPuzzleTile tile = (PhotoPuzzleTile)view;

                // Set tile bounds.
                AbsoluteLayout.SetLayoutBounds(tile, new Rectangle(tile.Col * tileSize,
                                                                   tile.Row * tileSize,
                                                                   tileSize,
                                                                   tileSize));
            }
        }

        async void OnTileTapped(object parameter)
        {
            if (isBusy)
                return;

            isBusy = true;
            PhotoPuzzleTile tappedTile = (PhotoPuzzleTile)parameter;
            await ShiftIntoEmpty(tappedTile.Row, tappedTile.Col);
            isBusy = false;
        }

        async Task ShiftIntoEmpty(int tappedRow, int tappedCol, uint length = 100)
        {
            // Shift columns.
            if (tappedRow == emptyRow && tappedCol != emptyCol)
            {
                int inc = Math.Sign(tappedCol - emptyCol);
                int begCol = emptyCol + inc;
                int endCol = tappedCol + inc;

                for (int col = begCol; col != endCol; col += inc)
                {
                    await AnimateTile(emptyRow, col, emptyRow, emptyCol, length);
                }
            }
            // Shift rows.
            else if (tappedCol == emptyCol && tappedRow != emptyRow)
            {
                int inc = Math.Sign(tappedRow - emptyRow);
                int begRow = emptyRow + inc;
                int endRow = tappedRow + inc;

                for (int row = begRow; row != endRow; row += inc)
                {
                    await AnimateTile(row, emptyCol, emptyRow, emptyCol, length);
                }
            }
        }

        async Task AnimateTile(int row, int col, int newRow, int newCol, uint length)
        {
            // The tile to be animated.
            PhotoPuzzleTile animaTile = tiles[row, col];

            // The destination rectangle.
            Rectangle rect = new Rectangle(emptyCol * tileSize,
                                           emptyRow * tileSize,
                                           tileSize,
                                           tileSize);

            // Animate it!
            await animaTile.LayoutTo(rect, length);

            // Set layout bounds to same Rectangle.
            AbsoluteLayout.SetLayoutBounds(animaTile, rect);

            // Set several variables and properties for new layout.
            tiles[newRow, newCol] = animaTile;
            animaTile.Row = newRow;
            animaTile.Col = newCol;
            tiles[row, col] = null;
            emptyRow = row;
            emptyCol = col;
        }

        async void OnRandomizeButtonClicked(object sender, EventArgs args)
        {
            Button button = (Button)sender;
            button.IsEnabled = false;
            Random rand = new Random();

            isBusy = true;

            // Simulate some fast crazy taps.
            for (int i = 0; i < 100; i++)
            {
                await ShiftIntoEmpty(rand.Next(NUM), emptyCol, 25);
                await ShiftIntoEmpty(emptyRow, rand.Next(NUM), 25);
            }
            button.IsEnabled = true;

            isBusy = false;
        }
    }
}
