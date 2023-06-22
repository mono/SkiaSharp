using System;
using System.ComponentModel;

#if __TVOS__
namespace SkiaSharp.Views.tvOS
#elif __IOS__
namespace SkiaSharp.Views.iOS
#elif __MACOS__
namespace SkiaSharp.Views.Mac
#endif
{
	[EditorBrowsable (EditorBrowsableState.Never)]
	[Obsolete("Use SKCanvasLayer.PaintSurface instead.")]
	public interface ISKCanvasLayerDelegate
	{
		void DrawInSurface(SKSurface surface, SKImageInfo info);
	}
}
