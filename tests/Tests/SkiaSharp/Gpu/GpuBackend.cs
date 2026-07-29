namespace SkiaSharp.Tests
{
	/// <summary>
	/// A rendering backend the test suite can drive. Each value has a stable
	/// string id (see <see cref="GpuPolicy.Id"/>) that matches the corresponding
	/// visual-matrix renderer name, so a golden folder, a
	/// <c>SKIASHARP_TEST_SKIP_GPU</c> directive and a test failure message all
	/// use the same word.
	/// </summary>
	public enum GpuBackend
	{
		/// <summary>Pure CPU raster. Always required, everywhere.</summary>
		Cpu,

		GaneshGl,
		GaneshVulkan,

		/// <summary>
		/// The legacy SharpVk Vulkan vehicle, kept only for the SharpVk-specific
		/// tests. Tracked separately from <see cref="GaneshVulkan"/> (Silk.NET)
		/// so it can be opted out on its own.
		/// </summary>
		GaneshVulkanSharpVk,

		GaneshMetal,
		GaneshDirect3D,

		GraphiteVulkan,
		GraphiteMetal,
		GraphiteDawn,
	}

	/// <summary>
	/// Why a backend will or will not run on this host. Exactly one of the four
	/// states can produce a test failure.
	///
	/// <list type="table">
	/// <listheader><term>State</term><description>Can fail? / Needs configuration?</description></listheader>
	/// <item><term><see cref="Required"/></term><description>yes / no</description></item>
	/// <item><term><see cref="Disabled"/></term><description>no / yes</description></item>
	/// <item><term><see cref="Unsupported"/></term><description>no / no</description></item>
	/// <item><term><see cref="NotBuilt"/></term><description>no / no</description></item>
	/// </list>
	/// </summary>
	public enum GpuAvailability
	{
		/// <summary>
		/// Built for this platform and expected to work. Failing to bring the
		/// backend up is a <b>test failure</b>, never a skip.
		/// </summary>
		Required,

		/// <summary>
		/// Explicitly opted out for this host via <c>SKIASHARP_TEST_SKIP_GPU</c>
		/// (or the <c>SkiaSharpTestSkipGpu</c> MSBuild property on device and
		/// browser hosts). The only state driven by configuration — it describes
		/// an <i>agent</i>, never a platform.
		/// </summary>
		Disabled,

		/// <summary>
		/// The API does not exist on this platform and never will — Metal off
		/// Apple, Direct3D off Windows, Vulkan in the browser. Compiled in,
		/// never configured.
		/// </summary>
		Unsupported,

		/// <summary>
		/// The API exists here, but SkiaSharp does not build or wire it up on
		/// this platform today — Vulkan on Apple (no MoltenVK), Dawn off the
		/// browser, OpenGL on the device hosts. May flip in the future; the
		/// reason string says what would have to change.
		/// </summary>
		NotBuilt,
	}
}
