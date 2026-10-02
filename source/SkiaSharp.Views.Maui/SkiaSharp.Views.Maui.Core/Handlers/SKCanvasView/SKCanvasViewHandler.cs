using Microsoft.Maui;
using Microsoft.Maui.Handlers;

namespace SkiaSharp.Views.Maui.Handlers
{
	/// <summary>The .NET MAUI handler for <see cref="T:SkiaSharp.Views.Maui.ISKCanvasView" /> that maps the cross-platform control to platform-specific implementations.</summary>
	/// <remarks>This handler creates and manages the platform-specific canvas view (Android SKCanvasView, iOS SKCanvasView, or Windows SKXamlCanvas) and handles property synchronization between the cross-platform control and native view.</remarks>
	public partial class SKCanvasViewHandler
	{
		/// <summary>The property mapper that maps cross-platform properties to handler methods.</summary>
		/// <remarks>Contains mappings for EnableTouchEvents and IgnorePixelScaling properties.</remarks>
		public static PropertyMapper<ISKCanvasView, SKCanvasViewHandler> SKCanvasViewMapper =
			new PropertyMapper<ISKCanvasView, SKCanvasViewHandler>(ViewHandler.ViewMapper)
			{
				[nameof(ISKCanvasView.EnableTouchEvents)] = MapEnableTouchEvents,
				[nameof(ISKCanvasView.IgnorePixelScaling)] = MapIgnorePixelScaling,
			};

		/// <summary>The command mapper that maps cross-platform commands to handler methods.</summary>
		/// <remarks>Contains the mapping for the InvalidateSurface command.</remarks>
		public static CommandMapper<ISKCanvasView, SKCanvasViewHandler> SKCanvasViewCommandMapper =
			new CommandMapper<ISKCanvasView, SKCanvasViewHandler>(ViewHandler.ViewCommandMapper)
			{
				[nameof(ISKCanvasView.InvalidateSurface)] = OnInvalidateSurface,
			};

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Maui.Handlers.SKCanvasViewHandler" /> class with default mappers.</summary>
		/// <remarks />
		public SKCanvasViewHandler()
			: base(SKCanvasViewMapper, SKCanvasViewCommandMapper)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Maui.Handlers.SKCanvasViewHandler" /> class with custom mappers.</summary>
		/// <param name="mapper">The property mapper to use, or <see langword="null" /> to use the default.</param>
		/// <param name="commands">The command mapper to use, or <see langword="null" /> to use the default.</param>
		/// <remarks />
		public SKCanvasViewHandler(PropertyMapper? mapper, CommandMapper? commands)
			: base(mapper ?? SKCanvasViewMapper, commands ?? SKCanvasViewCommandMapper)
		{
		}
	}
}
