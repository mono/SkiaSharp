namespace SkiaSharp.Views
{
	public interface ISKLayerDelegate
	{
		void DrawInSurface(SKSurface surface, SKImageInfo info);
	}
}
