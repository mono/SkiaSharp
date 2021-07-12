using Microsoft.Maui;
using Microsoft.Maui.Handlers;

namespace SkiaSharp.Views.Maui.Handlers
{
	public partial class SKCanvasViewHandler
	{
		public static PropertyMapper<ISKCanvasView, SKCanvasViewHandler> SKCanvasViewMapper =
			new PropertyMapper<ISKCanvasView, SKCanvasViewHandler>(ViewHandler.ViewMapper)
			{
				[nameof(ISKCanvasView.EnableTouchEvents)] = MapEnableTouchEvents,
				[nameof(ISKCanvasView.IgnorePixelScaling)] = MapIgnorePixelScaling,

				Actions =
				{
					[nameof(ISKCanvasView.InvalidateSurface)] = OnInvalidateSurface,
				}
			};

		public SKCanvasViewHandler()
			: base(SKCanvasViewMapper)
		{
		}

		public SKCanvasViewHandler(PropertyMapper? mapper)
			: base(mapper ?? SKCanvasViewMapper)
		{
		}
	}
}
