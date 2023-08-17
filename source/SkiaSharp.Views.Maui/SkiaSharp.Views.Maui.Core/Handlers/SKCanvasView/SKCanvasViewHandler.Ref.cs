using System;
using Microsoft.Maui.Handlers;

namespace SkiaSharp.Views.Maui.Handlers
{
	public partial class SKCanvasViewHandler : ViewHandler<ISKCanvasView, object>
	{
		protected override object CreatePlatformView() => throw new NotImplementedException();

		public static void MapIgnorePixelScaling(SKCanvasViewHandler handler, ISKCanvasView canvasView) { }

		public static void MapEnableTouchEvents(SKCanvasViewHandler handler, ISKCanvasView canvasView) { }

		public static void OnInvalidateSurface(SKCanvasViewHandler handler, ISKCanvasView canvasView, object? args) { }
	}
}
