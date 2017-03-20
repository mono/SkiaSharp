using System;
using System.IO;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace SkiaSharp.Tests
{
	public class SKDocumentTest : SKTest
	{
		[Test]
		public void CanCreatePdf()
		{
			using (var stream = new MemoryStream())
			using (var managed = new SKManagedWStream(stream, false))
			{
				using (var doc = SKDocument.CreatePdf(managed))
				{
					Assert.IsNotNull(doc);
					Assert.IsNotNull(doc.BeginPage(100, 100));

					doc.EndPage();
					doc.Close();
				}

				Assert.IsTrue(stream.Length > 0);
				Assert.IsTrue(stream.Position > 0);
			}
		}

		[Test]
		public void CanCreateXps()
		{
			// XPS is only supported on Windows

			using (var stream = new MemoryStream())
			using (var managed = new SKManagedWStream(stream, false))
			{
				using (var doc = SKDocument.CreateXps(managed))
				{
					if (IsWindows)
					{
						Assert.IsNotNull(doc);
						Assert.IsNotNull(doc.BeginPage(100, 100));

						doc.EndPage();
						doc.Close();
					}
					else
					{
						Assert.IsNull(doc);
					}
				}

				if (IsWindows)
				{
					Assert.IsTrue(stream.Length > 0);
					Assert.IsTrue(stream.Position > 0);
				}
				else
				{
					Assert.IsTrue(stream.Length == 0);
					Assert.IsTrue(stream.Position == 0);
				}
			}
		}
	}
}
