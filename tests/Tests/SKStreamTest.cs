using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKStreamTest : SKTest
	{
		[SkippableFact(Skip = "Windows does not support non-ASCII characters: https://github.com/mono/SkiaSharp/issues/390")]
		public void SpecialCharactersPath()
		{
			var stream = new SKFileStream(@"C:\Projects\SkiaSharp\tests\Content\images\上田jj雅美.jpg");
			Assert.NotNull(stream);
			Assert.True(stream.Length > 0);
		}
	}
}
