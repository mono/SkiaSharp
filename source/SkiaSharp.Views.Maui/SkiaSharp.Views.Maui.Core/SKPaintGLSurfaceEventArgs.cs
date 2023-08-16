using System;
using System.ComponentModel;

using Microsoft.Maui;

namespace SkiaSharp.Views.Maui
{
	public class SKPaintGLSurfaceEventArgs : EventArgs
	{
		public SKPaintGLSurfaceEventArgs(SKSurface surface, GRBackendRenderTarget renderTarget)
			: this(surface, renderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888)
		{
		}

		public SKPaintGLSurfaceEventArgs(SKSurface surface, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType)
		{
			Surface = surface;
			BackendRenderTarget = renderTarget;
			ColorType = colorType;
			Origin = origin;
		}

		public SKSurface Surface { get; private set; }

		public GRBackendRenderTarget BackendRenderTarget { get; private set; }

		public SKColorType ColorType { get; private set; }

		public GRSurfaceOrigin Origin { get; private set; }
	}
}
