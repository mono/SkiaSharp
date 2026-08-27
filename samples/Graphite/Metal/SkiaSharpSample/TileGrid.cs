using SkiaSharp;

namespace SkiaSharpSample;

internal readonly record struct TileGrid(
	int Columns,
	int Rows,
	int TileWidth,
	int TileHeight,
	int Gap)
{
	public static TileGrid Create(int width, int height, int tileCount)
	{
		const int gap = 4;
		var aspect = Math.Max(0.5, width / (double)Math.Max(height, 1));
		var columns = Math.Clamp(
			(int)Math.Ceiling(Math.Sqrt(tileCount * aspect)),
			1,
			tileCount);
		var rows = (int)Math.Ceiling(tileCount / (double)columns);
		var tileWidth = Math.Max(1, (width - gap * (columns + 1)) / columns);
		var tileHeight = Math.Max(1, (height - gap * (rows + 1)) / rows);
		return new TileGrid(columns, rows, tileWidth, tileHeight, gap);
	}

	public SKRect Destination(int index)
	{
		var column = index % Columns;
		var row = index / Columns;
		var left = Gap + column * (TileWidth + Gap);
		var top = Gap + row * (TileHeight + Gap);
		return new SKRect(left, top, left + TileWidth, top + TileHeight);
	}
}
