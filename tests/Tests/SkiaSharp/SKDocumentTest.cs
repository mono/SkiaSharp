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

		[Obsolete]
		[SkippableFact]
		public void CanCreatePdfWithObsoleteMetadata()
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
		public void CanCreatePdfWithMetadata()
		{
			var metadata = new SKPdfMetadata
			{
				Author = "SkiaSharp Team"
			};

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

				return SKDocument.CreatePdf(stream, new SKPdfMetadata());
			}
		}

		[SkippableFact]
		public void MetadataIsAddedToFile()
		{
			var now = DateTime.Now;
			var metadata = new SKPdfMetadata
			{
				Title = "A1",
				Author = "A2",
				Subject = "A3",
				Keywords = "A4",
				Creator = "A5",
				Creation = now,
				Modified = now,
			};

			using var pdf = new MemoryStream();
			using (var doc = SKDocument.CreatePdf(pdf, metadata))
			{
				doc.BeginPage(612.0f, 792.0f);
				doc.Close();
			}

			pdf.Position = 0;
			using var reader = new StreamReader(pdf);
			var data = reader.ReadToEnd();

			var expectations = new string[] {
				"/Title (A1)",
				"/Author (A2)",
				"/Subject (A3)",
				"/Keywords (A4)",
				"/Creator (A5)",
				"/Producer (Skia/PDF ",
				"/CreationDate (D:",
				"/ModDate (D:"
			};
			foreach (var expectation in expectations)
			{
				Assert.Contains(expectation, data);
			}
		}

		[SkippableFact]
		public void CanCreateTaggedLinkPdf()
		{
			var metadata = new SKPdfMetadata();
			metadata.Title = "Example Tagged PDF With Links";
			metadata.Creator = "Skia";
			metadata.Creation = DateTime.Now;
			metadata.Modified = DateTime.Now;

			// The document tag.
			var root = new SKPdfStructureElementNode(1, "Document");
			root.Language = "en-US";

			// A link.
			var l1 = new SKPdfStructureElementNode(2, "Link");
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
				canvas.DrawPdfNodeAnnotation(2);
				canvas.DrawText("Click to visit Google.com", 72, 72, font, paint);
				var linkRect = SKRect.Create(72, 54, 218, 24);
				canvas.DrawUrlAnnotation(linkRect, "http://www.google.com");

				doc.EndPage();
				doc.Close();
			}

			Assert.True(stream.Length > 0);
			Assert.True(stream.Position > 0);
		}

		[SkippableFact]
		public void CanCreateTaggedLinkPdfWithConstructorStructures()
		{
			var metadata = new SKPdfMetadata
			{
				Title = "Example Tagged PDF With Links",
				Creator = "Skia",
				Creation = DateTime.Now,
				Modified = DateTime.Now,
				// The document tag.
				Structure = new SKPdfStructureElementNode(1, "Document")
				{
					Language = "en-US",
					Children =
					{
						// A link.
						new SKPdfStructureElementNode(2, "Link")
					}
				}
			};

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
				canvas.DrawPdfNodeAnnotation(2);
				canvas.DrawText("Click to visit Google.com", 72, 72, font, paint);
				var linkRect = SKRect.Create(72, 54, 218, 24);
				canvas.DrawUrlAnnotation(linkRect, "http://www.google.com");

				doc.EndPage();
				doc.Close();
			}

			Assert.True(stream.Length > 0);
			Assert.True(stream.Position > 0);
		}
	}
}
