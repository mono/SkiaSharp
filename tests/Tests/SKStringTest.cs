using System;
using System.IO;
using NUnit.Framework;

namespace SkiaSharp.Tests
{
	public class SKStringTest : SKTest
	{
		[Test]
		public void StringIsMarshaledCorrectly ()
		{
			using (var typeface = SKTypeface.FromFile (Path.Combine (PathToFonts, "SpiderSymbol.ttf")))
			{
				Assert.AreEqual ("SpiderSymbol", typeface.FamilyName);
			}
		}
	}
}
