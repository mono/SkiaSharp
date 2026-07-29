using SkiaSharp.Tests;

namespace SkiaSharp.Direct3D.Tests;

public class Direct3DTest : SKTest
{
}

public class Direct3DTest<TContext> : Direct3DTest
	where TContext : Direct3DContext, new()
{
	// GpuPolicy marks Direct3D Unsupported off Windows, so this only runs where it
	// must work — no catch, no inline platform check.
	protected Direct3DContext CreateDirect3DContext()
	{
		GpuPolicy.RequireOrSkip(GpuBackend.GaneshDirect3D);

		return new TContext();
	}
}
