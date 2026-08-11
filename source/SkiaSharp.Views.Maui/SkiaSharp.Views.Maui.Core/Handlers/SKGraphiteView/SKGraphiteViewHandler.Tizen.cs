using System;
using Microsoft.Maui.Handlers;

namespace SkiaSharp.Views.Maui.Handlers
{
	public partial class SKGraphiteViewHandler : ViewHandler<ISKGraphiteView, object>
	{
		protected override object CreatePlatformView () =>
			throw new PlatformNotSupportedException (
				"Graphite presentation is not supported on Tizen.");

		public static void MapIgnorePixelScaling (
			SKGraphiteViewHandler handler,
			ISKGraphiteView view)
		{
		}

		public static void MapHasRenderLoop (
			SKGraphiteViewHandler handler,
			ISKGraphiteView view)
		{
		}

		public static void MapEnableTouchEvents (
			SKGraphiteViewHandler handler,
			ISKGraphiteView view)
		{
		}

		public static void OnInvalidateSurface (
			SKGraphiteViewHandler handler,
			ISKGraphiteView view,
			object? args)
		{
		}
	}
}
