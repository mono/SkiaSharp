using Microsoft.Maui;

namespace SkiaSharp.Views.Maui
{
	public interface ISKGLView : IView
	{
		SKSize CanvasSize { get; }

		GRContext? GRContext { get; }

		bool HasRenderLoop { get; }

		bool IgnorePixelScaling { get; }

		bool EnableTouchEvents { get; }

		void InvalidateSurface();

		void OnCanvasSizeChanged(SKSizeI size);

		void OnGRContextChanged(GRContext? context);

		void OnPaintSurface(SKPaintGLSurfaceEventArgs e);

		void OnTouch(SKTouchEventArgs e);
	}
}
