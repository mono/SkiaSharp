using System;
using System.Collections.Generic;
using Xunit;
using System.IO;

namespace SkiaSharp.Tests
{
	public class AnimationTest : SKTest
	{
		[Trait(CategoryKey, GpuCategory)]
		[SkippableFact]
		public void When_Default_Make()
		{
			var path = Path.Combine(PathToImages, "LottieLogo1.json");

			var animation = SkiaSharp.Skottie.Animation.Make(File.ReadAllText(path));
			Assert.NotEqual(IntPtr.Zero, animation?.Handle);
		}
	}
}
