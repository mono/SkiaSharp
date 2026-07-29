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
		/// Stable identifier, used as the per-renderer golden override directory
		/// name (e.g. <c>"raster"</c>, <c>"ganesh-gl"</c>, <c>"ganesh-metal"</c>).
		/// Matches <see cref="GpuPolicy.Id"/> for <see cref="Backend"/>.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// The backend this renderer drives. Whether it runs on the current host
		/// is decided centrally by <see cref="GpuPolicy"/> — a renderer never
		/// gates itself on the platform, so the "which OS has which API"
		/// knowledge lives in exactly one table.
		/// </summary>
		GpuBackend Backend { get; }

		/// <summary>
		/// Renders <paramref name="scene"/> at <paramref name="info"/>'s size and
		/// returns the pixel buffer normalized to RGBA8888 / premultiplied. The
		/// caller owns the returned array.
		///
		/// <para>
		/// This is only ever called for a backend the policy reports as
		/// <see cref="GpuAvailability.Required"/>, so <b>every</b> exception is a
		/// real test failure. Do not swallow a missing device, driver or context
		/// into a skip: if a host legitimately cannot run this backend, that
		/// belongs in the <see cref="GpuPolicy"/> table or in
		/// <c>SKIASHARP_TEST_SKIP_GPU</c>, never in a catch block.
		/// </para>
		/// </summary>
		Task<byte[]> RenderAsync(ISkiaScene scene, SKImageInfo info, CancellationToken cancellationToken);
	}
}
