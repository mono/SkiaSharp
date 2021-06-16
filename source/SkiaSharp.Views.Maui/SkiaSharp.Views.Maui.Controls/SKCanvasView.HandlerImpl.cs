#nullable enable

using System;

namespace SkiaSharp.Views.Maui.Controls
{
	public partial class SKCanvasView : ISKCanvasView
	{
		private SKSizeI lastCanvasSize;

		public SKCanvasView()
		{
			var controller = (ISKCanvasViewController)this;

			controller.GetCanvasSize += OnGetCanvasSize;
			controller.SurfaceInvalidated += OnSurfaceInvalidated;

			void OnGetCanvasSize(object? sender, GetPropertyValueEventArgs<SKSize> e)
			{
				e.Value = lastCanvasSize;
			}

			void OnSurfaceInvalidated(object? sender, EventArgs e)
			{
				Handler.UpdateValue(nameof(ISKCanvasView.InvalidateSurface));
			}
		}

		void ISKCanvasView.OnCanvasSizeChanged(SKSizeI size) =>
			lastCanvasSize = size;

		void ISKCanvasView.OnPaintSurface(SKPaintSurfaceEventArgs e) =>
			OnPaintSurface(e);

		void ISKCanvasView.OnTouch(SKTouchEventArgs e) =>
			OnTouch(e);
	}
}
