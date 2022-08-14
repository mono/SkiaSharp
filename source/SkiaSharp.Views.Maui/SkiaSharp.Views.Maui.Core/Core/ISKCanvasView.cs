using Microsoft.Maui;

namespace SkiaSharp.Views.Maui
{
	public interface ISKCanvasView : IView
	{
		SKSize CanvasSize { get; }

		bool IgnorePixelScaling { get; }

		bool EnableTouchEvents { get; }

		void InvalidateSurface();

		void OnCanvasSizeChanged(SKSizeI size);

		void OnPaintSurface(SKPaintSurfaceEventArgs e);

		void OnTouch(SKTouchEventArgs e);
	}
}
