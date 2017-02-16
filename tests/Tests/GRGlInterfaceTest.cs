using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace SkiaSharp.Tests
{
	[Parallelizable(ParallelScope.None)]
	public class GRGlInterfaceTest : SKTest
	{
		[Test]
		public void CreateDefaultInterfaceIsValid()
		{
			using (var ctx = CreateGlContext()) {
				ctx.MakeCurrent();

				var glInterface = GRGlInterface.CreateNativeGlInterface();

				Assert.NotNull(glInterface);
				Assert.True(glInterface.Validate());
			}
		}
	}
}
