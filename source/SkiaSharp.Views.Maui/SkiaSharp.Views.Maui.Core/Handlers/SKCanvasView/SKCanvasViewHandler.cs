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
			};

		public static CommandMapper<ISKCanvasView, SKCanvasViewHandler> SKCanvasViewCommandMapper =
			new CommandMapper<ISKCanvasView, SKCanvasViewHandler>(ViewHandler.ViewCommandMapper)
			{
				[nameof(ISKCanvasView.InvalidateSurface)] = OnInvalidateSurface,
			};

		public SKCanvasViewHandler()
			: base(SKCanvasViewMapper, SKCanvasViewCommandMapper)
		{
		}

		public SKCanvasViewHandler(PropertyMapper? mapper, CommandMapper? commands)
			: base(mapper ?? SKCanvasViewMapper, commands ?? SKCanvasViewCommandMapper)
		{
		}
	}
}
