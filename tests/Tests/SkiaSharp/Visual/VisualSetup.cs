using System;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// A graphics setup is a (backend × API) combination — e.g. <c>raster</c>,
	/// <c>ganesh-vulkan</c>, <c>graphite-vulkan</c>. Each one knows how to vend
	/// an offscreen render target (<see cref="VisualSurface"/>) and identifies
	/// itself for golden-image lookup. Setups that can't initialize on the
	/// current host (missing backend, missing driver, etc.) report
	/// <c>IsAvailable = false</c> and are skipped by visual tests.
	///
	/// Setups are process-singletons — they're cheap to query and the GPU
	/// resources they hold (VkContext, GRContext, etc.) are reused across every
	/// test in the run.
	/// </summary>
	public abstract class VisualSetup : IDisposable
	{
		/// <summary>Stable identifier — used as a directory under <c>Goldens/</c>.</summary>
		public abstract string Name { get; }

		/// <summary>
		/// True if this setup can run on the current host. Cheap; no surface
		/// allocation. If false, <see cref="UnavailableReason"/> explains why
		/// (used in <c>Skip.IfNot</c> messages).
		/// </summary>
		public abstract bool IsAvailable { get; }

		public virtual string UnavailableReason => null;

		/// <summary>
		/// Allocate a fresh offscreen surface of the requested size. Caller
		/// owns the returned <see cref="VisualSurface"/> and must dispose it.
		/// May return null if allocation failed (rare; treat as a test failure).
		/// </summary>
		public abstract VisualSurface CreateSurface (SKImageInfo info);

		public override string ToString () => Name;

		public virtual void Dispose () { }
	}
}
