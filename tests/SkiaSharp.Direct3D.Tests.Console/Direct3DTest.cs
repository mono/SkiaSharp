using SkiaSharp.Tests;

namespace SkiaSharp.Direct3D.Tests;

public class Direct3DTest : SKTest
{
}

public class Direct3DTest<TContext> : Direct3DTest
	where TContext : Direct3DContext, new()
{
	protected Direct3DContext CreateDirect3DContext()
	{
		GpuPolicy.RequireOrSkip(GpuBackends.GaneshDirect3D);

		return new TContext();
	}
}
