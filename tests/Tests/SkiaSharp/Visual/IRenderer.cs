using System;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// A backend that renders an <see cref="ISkiaScene"/> and returns the
	/// resulting pixels. Each implementation wraps one way of getting a
	/// <see cref="SKSurface"/> — pure-CPU raster, Ganesh or Graphite over
	/// OpenGL, Metal, Vulkan or Dawn.
	///
	/// <para>
	/// Implementations MUST be cheap to construct. The <see cref="RendererCatalog"/>
	/// instantiates every renderer to enumerate the test matrix, so a constructor
	/// that brings up a GPU context would pay that cost just to list tests. Do the
	/// heavy work lazily inside <see cref="RenderAsync"/>.
	/// </para>
	/// </summary>
	public interface IRenderer : IDisposable
	{
		/// <summary>
		/// Stable identifier — the golden directory name and the
		/// <see cref="GpuBackends"/> id the policy gates on (e.g. <c>"ganesh-metal"</c>).
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Renders <paramref name="scene"/> at <paramref name="info"/>'s size and
		/// returns the pixels normalized to RGBA8888 / premultiplied. The caller owns
		/// the returned array.
		///
		/// <para>
		/// Only called for a backend the policy says is required, so every exception
		/// is a real failure — never catch a failed bring-up into a skip.
		/// </para>
		/// </summary>
		Task<byte[]> RenderAsync(ISkiaScene scene, SKImageInfo info, CancellationToken cancellationToken);
	}
}
