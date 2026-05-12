using System;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Transport-agnostic renderer. Hands a scene to whatever lives behind the
	/// interface — could be an in-process surface, a TCP-attached device, a
	/// browser tab — and returns the resulting RGBA8888/Premul pixel buffer.
	///
	/// Implementations MUST be cheap to construct: discovery enumerates all
	/// renderers, and a heavy constructor would bring up every GPU context
	/// just to list tests. Move that work into <see cref="RenderAsync"/>
	/// (lazily) or into a first-call probe gated by <see cref="IsAvailable"/>.
	/// </summary>
	public interface IRenderer : IDisposable
	{
		/// <summary>
		/// Stable identifier. Used as a Goldens/ subdirectory name for
		/// per-renderer overrides (<c>"linux-graphite-vulkan"</c>, etc.).
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Cheap probe — true if this renderer can run on the current host.
		/// Must not allocate GPU resources; just check whether the loader, driver,
		/// or SDK is reachable.
		/// </summary>
		bool IsAvailable { get; }

		/// <summary>Why <see cref="IsAvailable"/> is false (for Skip messages).</summary>
		string UnavailableReason { get; }

		/// <summary>
		/// Render the scene at <paramref name="info"/>'s size and return the
		/// pixel buffer in RGBA8888/Premul. Caller owns the returned array.
		/// </summary>
		Task<byte[]> RenderAsync (ISkiaScene scene, SKImageInfo info, CancellationToken ct);
	}
}
