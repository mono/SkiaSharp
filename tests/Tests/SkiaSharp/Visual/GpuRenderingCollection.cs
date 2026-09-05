using Xunit;

namespace SkiaSharp.Tests.Visual
{
	// Serializes GPU rendering. xUnit runs test collections in parallel but the tests inside one
	// collection sequentially, and DisableParallelization additionally keeps this collection off the
	// parallel path entirely -- so pinning every class that touches a GPU context to it means no two
	// of them ever run at once. GPU drivers (especially mixing backends on one machine) do not
	// reliably tolerate concurrent use: X11/GLX in particular crashed the Linux test host outright
	// with concurrent context creation (#4590). The serialization point is the test class, not the
	// renderer, so this holds no matter which assembly a renderer type lives in.
	//
	// Membership is "does this class create a GPU context", which is the visual matrix plus every
	// class calling SKTest.CreateGlContext(). Where only a nested class does GPU work (SKBlenderTest,
	// SKRuntimeEffectTest) only that nested class joins, so its raster sibling stays parallel.
	[CollectionDefinition(GpuRenderingCollection.Name, DisableParallelization = true)]
	public sealed class GpuRenderingCollection
	{
		public const string Name = "GPU rendering (serialized)";
	}
}
