using System;
using System.IO;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKStringTest : SKTest
	{
		[SkippableFact]
		public void StringIsMarshaledCorrectly ()
		{
			using (var typeface = SKTypeface.FromFile (Path.Combine (PathToFonts, "SpiderSymbol.ttf")))
			{
				Assert.Equal ("SpiderSymbol", typeface.FamilyName);
			}
		}
	}
}
