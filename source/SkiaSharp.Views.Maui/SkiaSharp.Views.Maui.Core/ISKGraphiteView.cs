using System;
using Microsoft.Maui;

namespace SkiaSharp.Views.Maui
{
	public interface ISKGraphiteView : IView
	{
		SKSize CanvasSize { get; }

		SKGraphiteContext? GraphiteContext { get; }

		SKGraphiteBackend Backend { get; }

		bool HasRenderLoop { get; }

		bool IgnorePixelScaling { get; }

		bool EnableTouchEvents { get; }

		void InvalidateSurface ();

		void OnCanvasSizeChanged (SKSizeI size);

		void OnGraphiteContextChanged (SKGraphiteContext? context);

		void OnPaintSurface (SKPaintGraphiteSurfaceEventArgs e);

		void OnRenderFailed (Exception exception);

		void OnTouch (SKTouchEventArgs e);
	}
}
