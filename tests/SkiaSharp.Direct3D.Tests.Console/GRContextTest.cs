using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp.Tests;
using Xunit;

namespace SkiaSharp.Direct3D.Tests;

public class GRContextTest : Direct3DTest
{
	[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
	[SkippableFact]
	public void GRContextIsNullWhenNoDirect3DContext()
	{
		var grBackendContext = new GRD3DBackendContext();

		using var grContext = GRContext.CreateDirect3D(grBackendContext);

		Assert.Null(grContext);
	}

#if ENABLE_VORTICE
	public class Vortice : GRContextTest<VorticeDirect3DContext> { }
#endif
}
	
public abstract class GRContextTest<TContext> : Direct3DTest<TContext>
	where TContext : Direct3DContext, new()
{
	[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
	[SkippableFact]
	public void CreateDirect3DContextIsValid()
	{
		using var ctx = CreateDirect3DContext();

		var grBackendContext = ctx.CreateBackendContext();

		Assert.NotNull(grBackendContext);

		using var grContext = GRContext.CreateDirect3D(grBackendContext);

		Assert.NotNull(grContext);
	}

	[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
	[SkippableFact]
	public void CreateDirect3DContextWithOptionsIsValid()
	{
		using var ctx = CreateDirect3DContext();

		var grBackendContext = ctx.CreateBackendContext();

		Assert.NotNull(grBackendContext);

		var options = new GRContextOptions();

		using var grContext = GRContext.CreateDirect3D(grBackendContext, options);

		Assert.NotNull(grContext);
	}
}
