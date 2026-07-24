using Xunit;

namespace SkiaSharp.Vulkan.Tests
{
	// Serializes the GPU work in the Vulkan test assembly. xUnit runs test collections in parallel, so
	// without this every Vulkan-context-creating class (the backend-context tests plus the visual matrix)
	// could bring a Vulkan device up concurrently. This is the Vulkan-assembly counterpart to the base
	// test library's GpuRenderingCollection: xUnit collection definitions are per-assembly, so the Vulkan
	// tests -- which live in their own assembly (SkiaSharp.Vulkan.Tests, compiled into both the desktop
	// Console satellite and the device library) -- need their own definition here rather than referencing
	// the base one. The scenes and contexts are tiny, so the throughput cost of serializing is negligible.
	[CollectionDefinition(VulkanGpuRenderingCollection.Name, DisableParallelization = true)]
	public sealed class VulkanGpuRenderingCollection
	{
		public const string Name = "Vulkan GPU rendering (serialized)";
	}
}
