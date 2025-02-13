using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp.Tests;
using Xunit;

namespace SkiaSharp.Direct3D.Tests
{
	public class GRContextTest : Direct3DTest
	{
		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[SkippableFact]
		public void CreateDirect3DContextIsValid()
		{
			using var ctx = CreateDirect3DContext();

			var grBackendContext = new GRDirect3DBackendContext
			{
				Adapter = ctx.Adapter,
				Device = ctx.Device,
				Queue = ctx.Queue,
			};

			Assert.NotNull(grBackendContext);

			using var grContext = GRContext.CreateDirect3D(grBackendContext);

			Assert.NotNull(grContext);
		}

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[SkippableFact]
		public void CreateDirect3DContextWithOptionsIsValid()
		{
			using var ctx = CreateDirect3DContext();

			var grBackendContext = new GRDirect3DBackendContext
			{
				Adapter = ctx.Adapter,
				Device = ctx.Device,
				Queue = ctx.Queue,
			};

			Assert.NotNull(grBackendContext);

			var options = new GRContextOptions();

			using var grContext = GRContext.CreateDirect3D(grBackendContext, options);

			Assert.NotNull(grContext);
		}
	}
}
