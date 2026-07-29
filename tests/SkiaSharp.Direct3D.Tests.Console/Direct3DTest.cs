using SkiaSharp.Tests;

namespace SkiaSharp.Direct3D.Tests;

public class Direct3DTest : SKTest
{
}

public class Direct3DTest<TContext> : Direct3DTest
	where TContext : Direct3DContext, new()
{
	// No catch and no inline platform check: GpuPolicy marks Direct3D as
	// Unsupported off Windows (it is a Windows-only API), so this only ever runs
	// where D3D must work. A context we cannot create there is a real failure.
	protected Direct3DContext CreateDirect3DContext()
	{
		GpuPolicy.RequireOrSkip(GpuBackend.GaneshDirect3D);

		return new TContext();
	}
}
