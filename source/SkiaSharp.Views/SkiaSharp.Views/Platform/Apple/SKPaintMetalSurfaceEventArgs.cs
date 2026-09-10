#if __IOS__ || __MACOS__ || __TVOS__
using System;

#if __IOS__
namespace SkiaSharp.Views.iOS
#elif __MACOS__
namespace SkiaSharp.Views.Mac
#elif __TVOS__
namespace SkiaSharp.Views.tvOS
#endif
{
	/// <summary>Provides data for the <c>PaintSurface</c> event.</summary>
	/// <remarks />
	public class SKPaintMetalSurfaceEventArgs : EventArgs
	{
		/// <param name="surface">The surface that is being drawn on.</param>
		/// <param name="renderTarget">The render target that is currently being drawn.</param>
		/// <summary>Initializes a new instance of the <see cref="SKPaintMetalSurfaceEventArgs" /> class.</summary>
		/// <remarks />
		public SKPaintMetalSurfaceEventArgs(SKSurface surface, GRBackendRenderTarget renderTarget)
			: this(surface, renderTarget, GRSurfaceOrigin.TopLeft, SKColorType.Rgba8888)
		{
		}

		/// <param name="surface">The surface that is being drawn on.</param>
		/// <param name="renderTarget">The render target that is currently being drawn.</param>
		/// <param name="origin">The surface origin of the render target.</param>
		/// <param name="colorType">The color type of the render target.</param>
		/// <summary>Initializes a new instance of the <see cref="SKPaintMetalSurfaceEventArgs" /> class.</summary>
		/// <remarks />
		public SKPaintMetalSurfaceEventArgs(SKSurface surface, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType)
		{
			Surface = surface;
			BackendRenderTarget = renderTarget;
			ColorType = colorType;
			Origin = origin;
			Info = new SKImageInfo(renderTarget.Width, renderTarget.Height, ColorType);
			RawInfo = Info;
		}

		/// <param name="surface">The surface that is being drawn on.</param>
		/// <param name="renderTarget">The render target that is currently being drawn.</param>
		/// <param name="origin">The surface origin of the render target.</param>
		/// <param name="info">The image information describing the surface.</param>
		/// <summary>Initializes a new instance of the <see cref="SKPaintMetalSurfaceEventArgs" /> class.</summary>
		/// <remarks />
		public SKPaintMetalSurfaceEventArgs(SKSurface surface, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKImageInfo info)
			: this(surface, renderTarget, origin, info, info)
		{
		}

		/// <param name="surface">The surface that is being drawn on.</param>
		/// <param name="renderTarget">The render target that is currently being drawn.</param>
		/// <param name="origin">The surface origin of the render target.</param>
		/// <param name="info">The image information describing the surface.</param>
		/// <param name="rawInfo">The raw image information describing the surface without any applied scaling.</param>
		/// <summary>Initializes a new instance of the <see cref="SKPaintMetalSurfaceEventArgs" /> class.</summary>
		/// <remarks />
		public SKPaintMetalSurfaceEventArgs(SKSurface surface, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKImageInfo info, SKImageInfo rawInfo)
		{
			Surface = surface;
			BackendRenderTarget = renderTarget;
			ColorType = info.ColorType;
			Origin = origin;
			Info = info;
			RawInfo = rawInfo;
		}

		/// <summary>Gets the surface that is currently being drawn on.</summary>
		/// <value>The current drawing surface.</value>
		/// <remarks />
		public SKSurface Surface { get; private set; }

		/// <summary>Gets the render target that is currently being drawn.</summary>
		/// <value>The current render target.</value>
		/// <remarks />
		public GRBackendRenderTarget BackendRenderTarget { get; private set; }

		/// <summary>Gets the color type of the render target.</summary>
		/// <value>The color type of the render target.</value>
		/// <remarks />
		public SKColorType ColorType { get; private set; }

		/// <summary>Gets the surface origin of the render target.</summary>
		/// <value>The surface origin of the render target.</value>
		/// <remarks />
		public GRSurfaceOrigin Origin { get; private set; }

		/// <summary>Gets the image information describing the surface that is currently being drawn.</summary>
		/// <value>The surface image information.</value>
		/// <remarks />
		public SKImageInfo Info { get; private set; }

		/// <summary>Gets the raw image information describing the surface without any applied scaling.</summary>
		/// <value>The raw surface image information.</value>
		/// <remarks />
		public SKImageInfo RawInfo { get; private set; }
	}
}
#endif
