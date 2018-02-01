using System;
using System.IO;
using System.Runtime.InteropServices;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKDocumentTest : SKTest
	{
		[SkippableFact]
		[Obsolete]
		public void PdfFileIsClosed()
		{
			var path = Path.Combine(PathToImages, Guid.NewGuid().ToString("D") + ".pdf");

			using (var doc = SKDocument.CreatePdf(path))
			{
				Assert.NotNull(doc);
				Assert.NotNull(doc.BeginPage(100, 100));

				doc.EndPage();
				doc.Close();
			}

			File.Delete(path);
		}

		[SkippableFact]
		[Obsolete]
		public void XpsFileIsClosed()
		{
			var path = Path.Combine(PathToImages, Guid.NewGuid().ToString("D") + ".pdf");

			using (var doc = SKDocument.CreatePdf(path))
			{
				if (IsWindows)
				{
					Assert.NotNull(doc);
					Assert.NotNull(doc.BeginPage(100, 100));

					doc.EndPage();
					doc.Close();
				}
			}

			File.Delete(path);
		}

		[SkippableFact]
		public void CanCreatePdf()
		{
			using (var stream = new MemoryStream())
			using (var managed = new SKManagedWStream(stream, false))
			{
				using (var doc = SKDocument.CreatePdf(managed))
				{
					Assert.NotNull(doc);
					Assert.NotNull(doc.BeginPage(100, 100));

					doc.EndPage();
					doc.Close();
				}

				Assert.True(stream.Length > 0);
				Assert.True(stream.Position > 0);
			}
		}

		[SkippableFact]
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
						Assert.NotNull(doc);
						Assert.NotNull(doc.BeginPage(100, 100));

						doc.EndPage();
						doc.Close();
					}
					else
					{
						Assert.Null(doc);
					}
				}

				if (IsWindows)
				{
					Assert.True(stream.Length > 0);
					Assert.True(stream.Position > 0);
				}
				else
				{
					Assert.True(stream.Length == 0);
					Assert.True(stream.Position == 0);
				}
			}
		}
	}
}
