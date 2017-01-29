using System;
using System.IO;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKStringTest : SKTest
	{
		[Fact]
		public void StringIsMarshaledCorrectly ()
		{
			using (var typeface = SKTypeface.FromFile (Path.Combine (PathToFonts, "SpiderSymbol.ttf")))
			{
				if (IsLinux) // see issue #225
					Assert.Equal("", typeface.FamilyName);
				else
					Assert.Equal ("SpiderSymbol", typeface.FamilyName);
			}
		}
	}
}
