using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Xunit;

namespace SkiaSharp.Tests
{
	public class GGlInterfaceTest : SKTest
	{
		[Fact]
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
