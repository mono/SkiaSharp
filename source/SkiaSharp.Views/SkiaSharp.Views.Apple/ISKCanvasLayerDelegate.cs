namespace SkiaSharp.Views
{
	public interface ISKCanvasLayerDelegate
	{
		void DrawInSurface(SKSurface surface, SKImageInfo info);
	}
}
