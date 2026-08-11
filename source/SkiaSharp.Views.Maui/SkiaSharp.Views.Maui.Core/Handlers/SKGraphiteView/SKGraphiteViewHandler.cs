using Microsoft.Maui;
using Microsoft.Maui.Handlers;

namespace SkiaSharp.Views.Maui.Handlers
{
	public partial class SKGraphiteViewHandler
	{
		public static PropertyMapper<ISKGraphiteView, SKGraphiteViewHandler> SKGraphiteViewMapper =
			new PropertyMapper<ISKGraphiteView, SKGraphiteViewHandler> (ViewHandler.ViewMapper)
			{
				[nameof (ISKGraphiteView.EnableTouchEvents)] = MapEnableTouchEvents,
				[nameof (ISKGraphiteView.IgnorePixelScaling)] = MapIgnorePixelScaling,
				[nameof (ISKGraphiteView.HasRenderLoop)] = MapHasRenderLoop,
			};

		public static CommandMapper<ISKGraphiteView, SKGraphiteViewHandler> SKGraphiteViewCommandMapper =
			new CommandMapper<ISKGraphiteView, SKGraphiteViewHandler> (ViewHandler.ViewCommandMapper)
			{
				[nameof (ISKGraphiteView.InvalidateSurface)] = OnInvalidateSurface,
			};

		public SKGraphiteViewHandler ()
			: base (SKGraphiteViewMapper, SKGraphiteViewCommandMapper)
		{
		}

		public SKGraphiteViewHandler (PropertyMapper? mapper, CommandMapper? commands)
			: base (
				mapper ?? SKGraphiteViewMapper,
				commands ?? SKGraphiteViewCommandMapper)
		{
		}
	}
}
