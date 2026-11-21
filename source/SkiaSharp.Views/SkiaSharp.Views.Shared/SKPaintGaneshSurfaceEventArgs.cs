#nullable enable

using System;

#if HAS_UNO_WINUI
namespace SkiaSharp.Views.Windows
#elif __ANDROID__
namespace SkiaSharp.Views.Android
#elif __TVOS__
namespace SkiaSharp.Views.tvOS
#elif __IOS__
namespace SkiaSharp.Views.iOS
#elif __DESKTOP__
namespace SkiaSharp.Views.Desktop
#elif __MACOS__
namespace SkiaSharp.Views.Mac
#elif __TIZEN__
namespace SkiaSharp.Views.Tizen
#elif WINDOWS
namespace SkiaSharp.Views.Windows
#elif __BLAZOR__
namespace SkiaSharp.Views.Blazor
#endif
{
	/// <summary>Provides data for the Ganesh GPU <c>PaintSurface</c> event.</summary>
	/// <remarks />
	public class SKPaintGaneshSurfaceEventArgs : SKPaintSurfaceEventArgs
	{
		/// <summary>Initializes a new instance of the <see cref="SKPaintGaneshSurfaceEventArgs" /> class.</summary>
		/// <param name="surface">The surface that is being drawn on.</param>
		/// <param name="context">The Ganesh GPU context used to render the surface.</param>
		/// <param name="renderTarget">The backend render target that is currently being drawn.</param>
		/// <param name="origin">The surface origin of the render target.</param>
		/// <param name="info">The image information describing the surface.</param>
		/// <remarks />
		public SKPaintGaneshSurfaceEventArgs(
			SKSurface surface,
			GRContext? context,
			GRBackendRenderTarget? renderTarget,
			GRSurfaceOrigin origin,
			SKImageInfo info)
			: this(surface, context, renderTarget, origin, info, info)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="SKPaintGaneshSurfaceEventArgs" /> class.</summary>
		/// <param name="surface">The surface that is being drawn on.</param>
		/// <param name="context">The Ganesh GPU context used to render the surface.</param>
		/// <param name="renderTarget">The backend render target that is currently being drawn.</param>
		/// <param name="origin">The surface origin of the render target.</param>
		/// <param name="info">The image information describing the surface.</param>
		/// <param name="rawInfo">The raw image information of the underlying surface.</param>
		/// <remarks />
		public SKPaintGaneshSurfaceEventArgs(
			SKSurface surface,
			GRContext? context,
			GRBackendRenderTarget? renderTarget,
			GRSurfaceOrigin origin,
			SKImageInfo info,
			SKImageInfo rawInfo)
			: base(surface, info, rawInfo)
		{
			Context = context;
			BackendRenderTarget = renderTarget;
			Origin = origin;
		}

		/// <summary>Gets the Ganesh GPU context used to render the surface.</summary>
		/// <value>The Ganesh GPU context, or <see langword="null" /> if unavailable.</value>
		/// <remarks />
		public GRContext? Context { get; }

		/// <summary>Gets the backend render target that is currently being drawn.</summary>
		/// <value>The current backend render target, or <see langword="null" /> if unavailable.</value>
		/// <remarks />
		public GRBackendRenderTarget? BackendRenderTarget { get; }

		/// <summary>Gets the surface origin of the render target.</summary>
		/// <value>The surface origin of the render target.</value>
		/// <remarks />
		public GRSurfaceOrigin Origin { get; }
	}
}
