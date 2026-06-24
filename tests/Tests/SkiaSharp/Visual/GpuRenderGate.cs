namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Serializes all GPU rendering across the visual matrix. xUnit may run
	/// theory cells in parallel, but GPU drivers (and especially mixing GL and
	/// Metal on the same machine) do not reliably tolerate concurrent context
	/// creation/use. Each GPU renderer holds this lock for the duration of a
	/// single <c>RenderAsync</c>. The scenes are tiny, so the throughput cost is
	/// negligible and the determinism win is large.
	/// </summary>
	internal static class GpuRenderGate
	{
		public static readonly object Sync = new();
	}
}
