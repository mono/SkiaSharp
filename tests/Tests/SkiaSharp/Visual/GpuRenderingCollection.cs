using Xunit;

namespace SkiaSharp.Tests.Visual
{
	// Serializes GPU rendering in the visual matrix. xUnit runs test collections in parallel but the tests
	// inside one collection sequentially, so pinning every renderer-driving class to this single
	// DisableParallelization collection keeps all GL/Metal/Vulkan context creation and drawing off the
	// parallel path -- GPU drivers (especially mixing backends on one machine) do not reliably tolerate
	// concurrent use. The serialization point is the test class, not the renderer, so this holds no matter
	// which assembly a renderer type lives in. The scenes are tiny, so the throughput cost is negligible.
	//
	// Today VisualMatrixTests is the only renderer-driving class in this assembly. Any future class that
	// drives the GPU renderers should join this collection so they never render concurrently.
	[CollectionDefinition(GpuRenderingCollection.Name, DisableParallelization = true)]
	public sealed class GpuRenderingCollection
	{
		public const string Name = "GPU rendering (serialized)";
	}
}
