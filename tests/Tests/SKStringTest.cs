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
			var filename = Path.GetTempFileName ();

			bitmap.Save (filename, ImageFormat.Png);

			var filestream = new SKFileStream (filename);
			var decoder = new SKImageDecoder (filestream);
			string format1 = decoder.FormatName;
		}
	}
}
