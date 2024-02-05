using System;
using Microsoft.Maui.Handlers;

namespace SkiaSharp.Views.Maui.Handlers
{
	public partial class SKGLViewHandler : ViewHandler<ISKGLView, object>
	{
		protected override object CreatePlatformView() => throw new NotImplementedException();

		public static void MapIgnorePixelScaling(SKGLViewHandler handler, ISKGLView view) { }

		public static void MapHasRenderLoop(SKGLViewHandler handler, ISKGLView view) { }

		public static void MapEnableTouchEvents(SKGLViewHandler handler, ISKGLView view) { }

		public static void OnInvalidateSurface(SKGLViewHandler handler, ISKGLView view, object? args) { }
	}
}
