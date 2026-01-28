using System;
using SkiaSharp;

namespace DocsSamplesApp
{
    class MatrixDisplay
    {
        public SKPaint MatrixPaint { set; get; } = new SKPaint
                                                   {
                                                       Color = SKColors.Black,
                                                       StrokeWidth = 2
                                                   };

        public SKFont MatrixFont { set; get; } = new SKFont
                                                   {
                                                       Size = 48
                                                   };

        public string PerspectiveFormat { set; get; } = "F0";

        public SKSize Measure(SKMatrix matrix)
        {
            return MeasureAndPaint(null, matrix, new SKPoint(), false);
        }

        public void Paint(SKCanvas canvas, SKMatrix matrix, SKPoint location)
        {
            MeasureAndPaint(canvas, matrix, location, true);
        }

        SKSize MeasureAndPaint(SKCanvas? canvas, SKMatrix matrix, SKPoint location, bool doPaint)
        {
            float[] values = matrix.Values;
            string[] texts = new string[9];
            SKRect[] bounds = new SKRect[9];
            float[] widths = new float[3]; 

            for (int i = 0; i < 9; i++)
            {
                int row = i % 3;
                int col = i / 3;

                // Format string differently based on row
                texts[i] = values[i].ToString(row == 2 ? "F0" : (col == 2 ? PerspectiveFormat : "F2"));

                // Measure string with a '-' even if one is not present
                bool isNegative = texts[i][0] == '-';
                string text = (isNegative ? "" : "-") + texts[i];
                MatrixFont.MeasureText(text, out bounds[i]);

                // Get maximum width for each column
                widths[col] = Math.Max(widths[col], bounds[i].Width);

                // Measure the text again without the '-' in front
                MatrixFont.MeasureText(texts[i], out bounds[i]);
            }

            // Some formatting constants 
            float horzGap = MatrixFont.Size;
            float horzMarg = MatrixFont.Size;
            float vertMarg = MatrixFont.Spacing / 4;

            // Calculate the total width and height of the matrix display
            float totalWidth = widths[0] + widths[1] + widths[2] + 2 * horzGap + 2 * horzMarg;
            float totalHeight = 3 * MatrixFont.Spacing + 2 * vertMarg;

            if (doPaint && canvas is not null)
            {
                SKPaintStyle saveStyle = MatrixPaint.Style;

                for (int i = 0; i < 9; i++)
                {
                    int row = i % 3;
                    int col = i / 3;

                    // Find x, y of upper-left corner of text
                    float x = location.X + horzMarg;

                    for (int c = 0; c < col; c++)
                    {
                        x += widths[c] + horzGap;
                    }

                    float y = location.Y + vertMarg + row * MatrixFont.Spacing;

                    // Adjust for right-justified text
                    x += widths[col] - bounds[i].Width;
                    y += (MatrixFont.Spacing - bounds[i].Height) / 2 - bounds[i].Top;

                    // Draw the text
                    MatrixPaint.Style = SKPaintStyle.Fill;
                    canvas.DrawText(texts[i], x, y, SKTextAlign.Left, MatrixFont, MatrixPaint);
                }

                // Display vertical lines at the sides of the matrix
                MatrixPaint.Style = SKPaintStyle.Stroke;
                canvas.DrawLine(location.X + horzMarg / 2, location.Y + vertMarg, 
                                location.X + horzMarg / 2, location.Y + totalHeight - vertMarg, MatrixPaint);
                canvas.DrawLine(location.X + totalWidth - horzMarg / 2, location.Y + vertMarg, 
                                location.X + totalWidth - horzMarg / 2, location.Y + totalHeight - vertMarg, MatrixPaint);

                MatrixPaint.Style = saveStyle;
            }
            return new SKSize(totalWidth, totalHeight);
        }
    }
}
