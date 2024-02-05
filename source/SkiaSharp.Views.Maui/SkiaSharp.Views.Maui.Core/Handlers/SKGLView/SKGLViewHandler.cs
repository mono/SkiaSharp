using Microsoft.Maui;
using Microsoft.Maui.Handlers;

namespace SkiaSharp.Views.Maui.Handlers
{
	public partial class SKGLViewHandler
	{
		public static PropertyMapper<ISKGLView, SKGLViewHandler> SKGLViewMapper =
			new PropertyMapper<ISKGLView, SKGLViewHandler>(ViewHandler.ViewMapper)
			{
				[nameof(ISKGLView.EnableTouchEvents)] = MapEnableTouchEvents,
				[nameof(ISKGLView.IgnorePixelScaling)] = MapIgnorePixelScaling,
				[nameof(ISKGLView.HasRenderLoop)] = MapHasRenderLoop,
#if WINDOWS
				[nameof(ISKGLView.Background)] = MapBackground,
#endif
			};

		public static CommandMapper<ISKGLView, SKGLViewHandler> SKGLViewCommandMapper =
			new CommandMapper<ISKGLView, SKGLViewHandler>(ViewHandler.ViewCommandMapper)
			{
				[nameof(ISKGLView.InvalidateSurface)] = OnInvalidateSurface,
			};

		public SKGLViewHandler()
			: base(SKGLViewMapper, SKGLViewCommandMapper)
		{
		}

		public SKGLViewHandler(PropertyMapper? mapper, CommandMapper? commands)
			: base(mapper ?? SKGLViewMapper, commands ?? SKGLViewCommandMapper)
		{
		}
	}
}
