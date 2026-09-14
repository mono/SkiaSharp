using System;
using System.ComponentModel;

using Microsoft.Maui;

namespace SkiaSharp.Views.Maui
{
	/// <summary>Provides data for the <see cref="E:SkiaSharp.Views.Maui.Controls.SKGLView.PaintSurface" /> event.</summary>
	/// <remarks>This class provides access to the GPU-backed drawing surface, render target information, and image metadata needed for hardware-accelerated drawing operations.</remarks>
	public class SKPaintGLSurfaceEventArgs : EventArgs
	{
		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Maui.SKPaintGLSurfaceEventArgs" /> class.</summary>
		/// <param name="surface">The GPU-backed surface to draw on.</param>
		/// <param name="renderTarget">The backend render target information.</param>
		/// <remarks />
		public SKPaintGLSurfaceEventArgs(SKSurface surface, GRBackendRenderTarget renderTarget)
			: this(surface, renderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Maui.SKPaintGLSurfaceEventArgs" /> class with origin and color type.</summary>
		/// <param name="surface">The GPU-backed surface to draw on.</param>
		/// <param name="renderTarget">The backend render target information.</param>
		/// <param name="origin">The surface origin (top-left or bottom-left).</param>
		/// <param name="colorType">The color type of the surface.</param>
		/// <remarks />
		public SKPaintGLSurfaceEventArgs(SKSurface surface, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType)
		{
			Surface = surface;
			BackendRenderTarget = renderTarget;
			ColorType = colorType;
			Origin = origin;
			Info = new SKImageInfo(renderTarget.Width, renderTarget.Height, ColorType);
			RawInfo = Info;
		}

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Maui.SKPaintGLSurfaceEventArgs" /> class with image info.</summary>
		/// <param name="surface">The GPU-backed surface to draw on.</param>
		/// <param name="renderTarget">The backend render target information.</param>
		/// <param name="origin">The surface origin (top-left or bottom-left).</param>
		/// <param name="info">The image information describing the surface.</param>
		/// <remarks />
		public SKPaintGLSurfaceEventArgs(SKSurface surface, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKImageInfo info)
			: this(surface, renderTarget, origin, info, info)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Maui.SKPaintGLSurfaceEventArgs" /> class with image info and raw info.</summary>
		/// <param name="surface">The GPU-backed surface to draw on.</param>
		/// <param name="renderTarget">The backend render target information.</param>
		/// <param name="origin">The surface origin (top-left or bottom-left).</param>
		/// <param name="info">The image information describing the surface (scaled for pixel density).</param>
		/// <param name="rawInfo">The raw image information describing the actual surface size.</param>
		/// <remarks />
		public SKPaintGLSurfaceEventArgs(SKSurface surface, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKImageInfo info, SKImageInfo rawInfo)
		{
			Surface = surface;
			BackendRenderTarget = renderTarget;
			ColorType = info.ColorType;
			Origin = origin;
			Info = info;
			RawInfo = rawInfo;
		}

		/// <summary>Gets the GPU-backed surface to draw on.</summary>
		/// <value>The <see cref="T:SkiaSharp.SKSurface" /> that provides the canvas for GPU-accelerated drawing.</value>
		/// <remarks>Use <c>Surface.Canvas</c> to access the <see cref="T:SkiaSharp.SKCanvas" /> for drawing operations. The surface is backed by a GPU texture or render target.</remarks>
		public SKSurface Surface { get; private set; }

		/// <summary>Gets the backend render target for the GPU surface.</summary>
		/// <value>The <see cref="T:SkiaSharp.GRBackendRenderTarget" /> that describes the GPU render target.</value>
		/// <remarks>This provides information about the underlying GPU framebuffer, including its dimensions and sample count.</remarks>
		public GRBackendRenderTarget BackendRenderTarget { get; private set; }

		/// <summary>Gets the color type of the surface.</summary>
		/// <value>The <see cref="T:SkiaSharp.SKColorType" /> that describes the pixel format of the surface.</value>
		/// <remarks />
		public SKColorType ColorType { get; private set; }

		/// <summary>Gets the origin of the surface coordinate system.</summary>
		/// <value>The <see cref="T:SkiaSharp.GRSurfaceOrigin" /> indicating whether the origin is at the top-left or bottom-left corner.</value>
		/// <remarks>OpenGL typically uses bottom-left origin, while other backends may use top-left.</remarks>
		public GRSurfaceOrigin Origin { get; private set; }

		/// <summary>Gets the image information describing the surface.</summary>
		/// <value>The image information that describes the size and format of the surface.</value>
		/// <remarks>When <see cref="P:SkiaSharp.Views.Maui.Controls.SKGLView.IgnorePixelScaling" /> is <see langword="true" />, this returns the user-visible size (logical pixels). Otherwise, it returns the same as <see cref="P:SkiaSharp.Views.Maui.SKPaintGLSurfaceEventArgs.RawInfo" />.</remarks>
		public SKImageInfo Info { get; private set; }

		/// <summary>Gets the raw image information describing the actual surface size in physical pixels.</summary>
		/// <value>The raw image information representing the actual surface dimensions in physical device pixels.</value>
		/// <remarks>This is the actual pixel buffer size regardless of the <see cref="P:SkiaSharp.Views.Maui.Controls.SKGLView.IgnorePixelScaling" /> setting. On high-DPI devices, this will be larger than the logical view size.</remarks>
		public SKImageInfo RawInfo { get; private set; }
	}
}
