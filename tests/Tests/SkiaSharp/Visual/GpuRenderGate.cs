namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Serializes all GPU rendering across the visual matrix. xUnit may run
	/// theory cells in parallel, but GPU drivers (and especially mixing GL and
	/// Metal on the same machine) do not reliably tolerate concurrent context
	/// creation/use. Each GPU renderer holds this lock for the duration of a
	/// single <c>RenderAsync</c>. The scenes are tiny, so the throughput cost is
	/// negligible and the determinism win is large.
	///
	/// <para>
	/// Public because it is shared across assembly boundaries: the Direct3D
	/// satellite host, the Graphite renderers, and — on the MAUI Devices host —
	/// the Vulkan renderers (which are compiled into the app assembly while this
	/// gate lives in the referenced <c>SkiaSharp.Tests</c> assembly) all hold
	/// the same lock as the shared GL/Metal renderers.
	/// </para>
	/// </summary>
	public static class GpuRenderGate
	{
		public static readonly object Sync = new();
	}
}
