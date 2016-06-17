using System;
using System.Drawing.Imaging;
using System.IO;
using NUnit.Framework;

namespace SkiaSharp.Tests
{
	[TestFixture]
	public class SKStringTest : SKTest
	{
		[Test]
		public void StringIsMarshaledCorrectly ()
		{
			using (var typeface = SKTypeface.FromFile (Path.Combine ("fonts", "SpiderSymbol.ttf")))
			{
				Assert.AreEqual ("SpiderSymbol", typeface.FamilyName, "Family name must be SpiderSymbol");
			}
		}
	}
}
