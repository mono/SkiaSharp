#nullable enable

using Microsoft.Maui;
using Microsoft.Maui.Handlers;

namespace SkiaSharp.Views.Maui.Handlers
{
	/// <summary>The .NET MAUI handler for <see cref="T:SkiaSharp.Views.Maui.ISKGaneshView" /> that maps the cross-platform control to platform-specific Ganesh GPU-accelerated implementations.</summary>
	/// <remarks>This handler creates and manages the platform-specific Ganesh GPU view and handles property synchronization between the cross-platform control and native view.</remarks>
	public partial class SKGaneshViewHandler
	{
		/// <summary>The property mapper that maps cross-platform properties to handler methods.</summary>
		/// <remarks>Contains mappings for EnableTouchEvents, HasRenderLoop, and IgnorePixelScaling properties.</remarks>
		public static PropertyMapper<ISKGaneshView, SKGaneshViewHandler> SKGaneshViewMapper =
			new PropertyMapper<ISKGaneshView, SKGaneshViewHandler>(ViewHandler.ViewMapper)
			{
				[nameof(ISKGaneshView.EnableTouchEvents)] = MapEnableTouchEvents,
				[nameof(ISKGaneshView.IgnorePixelScaling)] = MapIgnorePixelScaling,
				[nameof(ISKGaneshView.HasRenderLoop)] = MapHasRenderLoop,
#if WINDOWS
				[nameof(ISKGaneshView.Background)] = MapBackground,
#endif
			};

		/// <summary>The command mapper that maps cross-platform commands to handler methods.</summary>
		/// <remarks>Contains the mapping for the InvalidateSurface command.</remarks>
		public static CommandMapper<ISKGaneshView, SKGaneshViewHandler> SKGaneshViewCommandMapper =
			new CommandMapper<ISKGaneshView, SKGaneshViewHandler>(ViewHandler.ViewCommandMapper)
			{
				[nameof(ISKGaneshView.InvalidateSurface)] = OnInvalidateSurface,
			};

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Maui.Handlers.SKGaneshViewHandler" /> class with default mappers.</summary>
		/// <remarks />
		public SKGaneshViewHandler()
			: base(SKGaneshViewMapper, SKGaneshViewCommandMapper)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Maui.Handlers.SKGaneshViewHandler" /> class with custom mappers.</summary>
		/// <param name="mapper">The property mapper to use, or <see langword="null" /> to use the default.</param>
		/// <param name="commands">The command mapper to use, or <see langword="null" /> to use the default.</param>
		/// <remarks />
		public SKGaneshViewHandler(PropertyMapper? mapper, CommandMapper? commands)
			: base(mapper ?? SKGaneshViewMapper, commands ?? SKGaneshViewCommandMapper)
		{
		}
	}
}
