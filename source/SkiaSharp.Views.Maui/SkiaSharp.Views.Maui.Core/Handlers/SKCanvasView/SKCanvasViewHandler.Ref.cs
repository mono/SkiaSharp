using System;
using Microsoft.Maui.Handlers;

namespace SkiaSharp.Views.Maui.Handlers
{
	public partial class SKCanvasViewHandler : ViewHandler<ISKCanvasView, object>
	{
		protected override object CreatePlatformView() => throw new NotImplementedException();

		/// <param name="handler"></param>
		/// <param name="canvasView"></param>
		public static void MapIgnorePixelScaling(SKCanvasViewHandler handler, ISKCanvasView canvasView) { }

		/// <param name="handler"></param>
		/// <param name="canvasView"></param>
		public static void MapEnableTouchEvents(SKCanvasViewHandler handler, ISKCanvasView canvasView) { }

		/// <param name="handler"></param>
		/// <param name="canvasView"></param>
		/// <param name="args"></param>
		public static void OnInvalidateSurface(SKCanvasViewHandler handler, ISKCanvasView canvasView, object? args) { }
	}
}
