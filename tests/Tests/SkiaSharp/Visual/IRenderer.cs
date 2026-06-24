using System;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// A backend that renders an <see cref="ISkiaScene"/> and returns the
	/// resulting pixels. Each implementation wraps one way of getting a
	/// <see cref="SKSurface"/> — pure-CPU raster, Ganesh over OpenGL, Ganesh over
	/// Metal, Ganesh over Vulkan, and (in a later change) Graphite over Vulkan or
	/// Metal.
	///
	/// <para>
	/// Implementations MUST be cheap to construct. The <see cref="RendererCatalog"/>
	/// instantiates every renderer to enumerate the test matrix, so a constructor
	/// that brings up a GPU context would pay that cost just to list tests. Do the
	/// heavy work lazily inside <see cref="RenderAsync"/>; keep
	/// <see cref="IsAvailable"/> a cheap, side-effect-free probe.
	/// </para>
	/// </summary>
	public interface IRenderer : IDisposable
	{
		/// <summary>
		/// Stable identifier, used as the per-renderer golden override directory
		/// name (e.g. <c>"raster"</c>, <c>"ganesh-gl"</c>, <c>"ganesh-metal"</c>).
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Cheap probe: <see langword="true"/> when this renderer can run on the
		/// current host. Must not allocate GPU resources — only check whether the
		/// OS, driver, or SDK is reachable. When this is <see langword="false"/>
		/// the matrix skips the renderer's cells with <see cref="UnavailableReason"/>.
		/// </summary>
		bool IsAvailable { get; }

		/// <summary>
		/// Human-readable explanation for why <see cref="IsAvailable"/> is
		/// <see langword="false"/>; <see langword="null"/> when available.
		/// </summary>
		string UnavailableReason { get; }

		/// <summary>
		/// Renders <paramref name="scene"/> at <paramref name="info"/>'s size and
		/// returns the pixel buffer normalized to RGBA8888 / premultiplied. The
		/// caller owns the returned array.
		///
		/// <para>
		/// Throw <see cref="RendererUnavailableException"/> if the renderer
		/// determines at runtime that this host genuinely cannot run it (driver
		/// feature missing, no device attached); the matrix converts that to a
		/// skip. Any other exception is treated as a real test failure.
		/// </para>
		/// </summary>
		Task<byte[]> RenderAsync(ISkiaScene scene, SKImageInfo info, CancellationToken cancellationToken);
	}
}
