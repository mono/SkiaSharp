using System;
using System.IO;
using System.Runtime.InteropServices;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKDocumentTest : SKTest
	{
		[SkippableFact]
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
		public void PdfFileWithNonASCIIPathIsClosed()
		{
			var path = Path.Combine(PathToImages, Guid.NewGuid().ToString("D") + "上田雅美.pdf");

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
		public void XpsFileIsClosed()
		{
			var path = Path.Combine(PathToImages, Guid.NewGuid().ToString("D") + ".xps");

			using (new SKAutoCoInitialize())
			using (var doc = SKDocument.CreateXps(path))
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
			{
				using (var doc = SKDocument.CreatePdf(stream))
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
		public void CanCreatePdfWithMetadata()
		{
			var metadata = SKDocumentPdfMetadata.Default;
			metadata.Author = "SkiaSharp Team";

			using (var stream = new MemoryStream())
			{
				using (var doc = SKDocument.CreatePdf(stream, metadata))
				{
					Assert.NotNull(doc);
					Assert.NotNull(doc.BeginPage(100, 100));

					doc.EndPage();
					doc.Close();
				}

				Assert.True(stream.Length > 0);
				Assert.True(stream.Position > 0);

				stream.Position = 0;
				using (var reader = new StreamReader(stream))
				{
					var contents = reader.ReadToEnd();
					Assert.Contains("/Author (SkiaSharp Team)", contents);
				}
			}
		}

		[SkippableFact]
		public void ManagedStreamDisposeOrder()
		{
			using (var stream = new MemoryStream())
			using (var doc = SKDocument.CreatePdf(stream))
			{
				Assert.NotNull(doc);
				Assert.NotNull(doc.BeginPage(100, 100));

				doc.EndPage();
				doc.Close();
				doc.Dispose();

				stream.Flush();

				Assert.True(stream.Length > 0);
				Assert.True(stream.Position > 0);
			}
		}

		[SkippableFact]
		public void CanCreateXps()
		{
			// XPS is only supported on Windows

			using (var stream = new MemoryStream())
			{
				using (new SKAutoCoInitialize())
				using (var doc = SKDocument.CreateXps(stream))
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

		[SkippableFact]
		public void StreamIsNotCollectedPrematurely()
		{
			DoWork(out var handle);

			CollectGarbage();

			Assert.False(SKObject.GetInstance<SKDynamicMemoryWStream>(handle, out _));

			static void DoWork(out IntPtr streamHandle)
			{
				using (var document = CreateDocument(out streamHandle))
				{
					using (var pageCanvas = document.BeginPage(792, 842))
					{
						document.EndPage();
					}

					CollectGarbage();

					Assert.True(SKObject.GetInstance<SKDynamicMemoryWStream>(streamHandle, out _));

					document.Close();
				}

				Assert.True(SKObject.GetInstance<SKDynamicMemoryWStream>(streamHandle, out _));
			}

			static SKDocument CreateDocument(out IntPtr streamHandle)
			{
				var stream = new SKDynamicMemoryWStream();
				streamHandle = stream.Handle;

				return SKDocument.CreatePdf(stream, new SKDocumentPdfMetadata());
			}
		}
	
		[SkippableFact]
		public void CanCreateTaggedPdf()
		{
			var metadata = new SKPdfMetadata();
			metadata.Title = "Example Tagged PDF With Links";
			metadata.Creator = "Skia";
			metadata.Creation = DateTime.Now;
			metadata.Modified = DateTime.Now;

			// The document tag.
			var root = new SKPdfStructureElement();
			root.NodeId = 1;
			root.Type = "Document";
			root.Language = "en-US";

			// A link.
			var l1 = new SKPdfStructureElement();
			l1.NodeId = 2;
			l1.Type = "Link";
			root.Children.Add(l1);

			metadata.Structure = root;

            using var stream = new MemoryStream();
            using (var doc = SKDocument.CreatePdf(stream, metadata))
            {
                Assert.NotNull(doc);

				var canvas = doc.BeginPage(612, 792);  // U.S. Letter
                Assert.NotNull(canvas);

				var paint = new SKPaint();
				paint.Color = SKColors.Blue;

				var font = new SKFont();
				font.Size = 20;

				// The node ID should cover both the text and the annotation.
				SkPDF::SetNodeId(canvas, 2);
				canvas->drawString("Click to visit Google.com", 72, 72, font, paint);
				SkRect linkRect = SkRect::MakeXYWH(
					SkIntToScalar(72), SkIntToScalar(54),
					SkIntToScalar(218), SkIntToScalar(24));
				sk_sp<SkData> url(SkData::MakeWithCString("http://www.google.com"));
				SkAnnotateRectWithURL(canvas, linkRect, url.get());

				document->endPage();
				document->close();
				outputStream.flush();


                doc.EndPage();
                doc.Close();
            }

            Assert.True(stream.Length > 0);
            Assert.True(stream.Position > 0);
        }
	}
}
