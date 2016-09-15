using System;

namespace SkiaSharp.Views.Forms
{
	public class SKPaintGLSurfaceEventArgs : EventArgs
	{
		public SKPaintGLSurfaceEventArgs(SKSurface surface, GRBackendRenderTargetDesc renderTarget)
		{
			Surface = surface;
			RenderTarget = renderTarget;
		}

		public SKSurface Surface { get; private set; }

		public GRBackendRenderTargetDesc RenderTarget { get; private set; }
	}
}
