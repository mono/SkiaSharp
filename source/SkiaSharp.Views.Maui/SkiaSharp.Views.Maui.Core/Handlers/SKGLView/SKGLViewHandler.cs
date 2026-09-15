using Microsoft.Maui;
using Microsoft.Maui.Handlers;

namespace SkiaSharp.Views.Maui.Handlers
{
	/// <summary>The .NET MAUI handler for <see cref="T:SkiaSharp.Views.Maui.ISKGLView" /> that maps the cross-platform control to platform-specific GPU-accelerated implementations.</summary>
	/// <remarks>This handler creates and manages the platform-specific GPU view (Android SKGLTextureView using OpenGL ES, Mac Catalyst SKMetalView using Metal, or Windows SKSwapChainPanel using DirectX via ANGLE) and handles property synchronization between the cross-platform control and native view.</remarks>
	public partial class SKGLViewHandler
	{
		/// <summary>The property mapper that maps cross-platform properties to handler methods.</summary>
		/// <remarks>Contains mappings for EnableTouchEvents, HasRenderLoop, and IgnorePixelScaling properties.</remarks>
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

		/// <summary>The command mapper that maps cross-platform commands to handler methods.</summary>
		/// <remarks>Contains the mapping for the InvalidateSurface command.</remarks>
		public static CommandMapper<ISKGLView, SKGLViewHandler> SKGLViewCommandMapper =
			new CommandMapper<ISKGLView, SKGLViewHandler>(ViewHandler.ViewCommandMapper)
			{
				[nameof(ISKGLView.InvalidateSurface)] = OnInvalidateSurface,
			};

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Maui.Handlers.SKGLViewHandler" /> class with default mappers.</summary>
		/// <remarks />
		public SKGLViewHandler()
			: base(SKGLViewMapper, SKGLViewCommandMapper)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Maui.Handlers.SKGLViewHandler" /> class with custom mappers.</summary>
		/// <param name="mapper">The property mapper to use, or <see langword="null" /> to use the default.</param>
		/// <param name="commands">The command mapper to use, or <see langword="null" /> to use the default.</param>
		/// <remarks />
		public SKGLViewHandler(PropertyMapper? mapper, CommandMapper? commands)
			: base(mapper ?? SKGLViewMapper, commands ?? SKGLViewCommandMapper)
		{
		}
	}
}
