using System;
using Microsoft.Maui.Handlers;
using UIKit;

namespace SkiaSharp.Views.Maui.Handlers
{
	public partial class SKGLViewHandler : ViewHandler<ISKGLView, UIView>
	{
		protected override UIView CreatePlatformView() => throw new PlatformNotSupportedException("OpenGL-based views (such as SKGLView) are not supported on Mac Catalyst. Instead, use Metal-based views.");

		public static void MapIgnorePixelScaling(SKGLViewHandler handler, ISKGLView view) { }

		public static void MapHasRenderLoop(SKGLViewHandler handler, ISKGLView view) { }

		public static void MapEnableTouchEvents(SKGLViewHandler handler, ISKGLView view) { }

		public static void OnInvalidateSurface(SKGLViewHandler handler, ISKGLView view, object? args) { }
	}
}
