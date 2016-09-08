#if __IOS__ || __TVOS__

namespace SkiaSharp.Views
{
	public interface ISKGLLayerDelegate
	{
		void DrawInSurface(SKSurface surface, GRBackendRenderTargetDesc renderTarget);
	}
}
#endif
