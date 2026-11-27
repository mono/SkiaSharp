#nullable enable

using Microsoft.Maui;
using Microsoft.Maui.Handlers;

namespace SkiaSharp.Views.Maui.Handlers
{
	/// <summary>The .NET MAUI handler for <see cref="T:SkiaSharp.Views.Maui.ISKGraphiteView" /> that maps the cross-platform control to platform-specific Graphite GPU-accelerated implementations.</summary>
	/// <remarks>This handler creates and manages the platform-specific Graphite GPU view and handles property synchronization between the cross-platform control and native view.</remarks>
	public partial class SKGraphiteViewHandler
	{
		/// <summary>The property mapper that maps cross-platform properties to handler methods.</summary>
		/// <remarks>Contains mappings for EnableTouchEvents, HasRenderLoop, and IgnorePixelScaling properties.</remarks>
		public static PropertyMapper<ISKGraphiteView, SKGraphiteViewHandler> SKGraphiteViewMapper =
			new PropertyMapper<ISKGraphiteView, SKGraphiteViewHandler>(ViewHandler.ViewMapper)
			{
				[nameof(ISKGraphiteView.EnableTouchEvents)] = MapEnableTouchEvents,
				[nameof(ISKGraphiteView.IgnorePixelScaling)] = MapIgnorePixelScaling,
				[nameof(ISKGraphiteView.HasRenderLoop)] = MapHasRenderLoop,
			};

		/// <summary>The command mapper that maps cross-platform commands to handler methods.</summary>
		/// <remarks>Contains the mapping for the InvalidateSurface command.</remarks>
		public static CommandMapper<ISKGraphiteView, SKGraphiteViewHandler> SKGraphiteViewCommandMapper =
			new CommandMapper<ISKGraphiteView, SKGraphiteViewHandler>(ViewHandler.ViewCommandMapper)
			{
				[nameof(ISKGraphiteView.InvalidateSurface)] = OnInvalidateSurface,
			};

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Maui.Handlers.SKGraphiteViewHandler" /> class with default mappers.</summary>
		/// <remarks />
		public SKGraphiteViewHandler()
			: base(SKGraphiteViewMapper, SKGraphiteViewCommandMapper)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Maui.Handlers.SKGraphiteViewHandler" /> class with custom mappers.</summary>
		/// <param name="mapper">The property mapper to use, or <see langword="null" /> to use the default.</param>
		/// <param name="commands">The command mapper to use, or <see langword="null" /> to use the default.</param>
		/// <remarks />
		public SKGraphiteViewHandler(PropertyMapper? mapper, CommandMapper? commands)
			: base(mapper ?? SKGraphiteViewMapper, commands ?? SKGraphiteViewCommandMapper)
		{
		}
	}
}
