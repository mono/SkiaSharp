#if __TVOS__
namespace SkiaSharp.Views.tvOS
#elif __WATCHOS__
namespace SkiaSharp.Views.watchOS
#elif __IOS__
namespace SkiaSharp.Views.iOS
#elif __MACOS__
namespace SkiaSharp.Views.Mac
#endif
{
	public interface ISKGLLayerDelegate
	{
		void DrawInSurface(SKSurface surface, GRBackendRenderTargetDesc renderTarget);
	}
}
