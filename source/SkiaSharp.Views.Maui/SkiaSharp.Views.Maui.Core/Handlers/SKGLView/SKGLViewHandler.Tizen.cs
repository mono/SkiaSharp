using System;
using Microsoft.Maui.Handlers;
using SkiaSharp.Views.Maui.Platform;
using SkiaSharp.Views.Tizen.NUI;
using ScalingInfo = SkiaSharp.Views.Tizen.ScalingInfo;

namespace SkiaSharp.Views.Maui.Handlers
{
	public partial class SKGLViewHandler : ViewHandler<ISKGLView, SKGLSurfaceView>
	{
		protected override SKGLSurfaceView CreatePlatformView() => throw new PlatformNotSupportedException("SKGLView is not yet implemented for Tizen.");

		public static void MapIgnorePixelScaling(SKGLViewHandler handler, ISKGLView view) { }

		public static void MapHasRenderLoop(SKGLViewHandler handler, ISKGLView view) { }

		public static void MapEnableTouchEvents(SKGLViewHandler handler, ISKGLView view) { }

		public static void OnInvalidateSurface(SKGLViewHandler handler, ISKGLView view, object? args) { }
	}
}
