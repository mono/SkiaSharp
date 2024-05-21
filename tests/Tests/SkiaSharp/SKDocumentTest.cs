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

			using var stream = new MemoryStream();
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
			using var reader = new StreamReader(stream);
			var contents = reader.ReadToEnd();
			Assert.Contains("/Author (SkiaSharp Team)", contents);
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

			AssertPdfMetadata(metadata, expectations);
		}

		[SkippableFact]
		public void PdfAMetadataIsAddedToFile()
		{
			var metadata = new SKPdfMetadata
			{
				Title = "test document",
				Creation = new DateTime(1999, 12, 31, 23, 59, 59, DateTimeKind.Utc),
				PdfA = true,
				Producer = "phoney library"
			};

			var expectations = new string[] {
				"sRGB IEC61966-2.1",
				"<dc:title><rdf:Alt><rdf:li xml:lang=\"x-default\">test document",
				"<xmp:CreateDate>1999-12-31T23:59:59+00:00</xmp:CreateDate>",
				"/Subtype /XML",
				"/CreationDate (D:19991231235959+00'00')>>",
				"/Producer (phoney library)",
				"<pdf:Producer>phoney library</pdf:Producer>",
			};

			AssertPdfMetadata(metadata, expectations);
		}

		[SkippableFact]
		public void UnicodeMetadataIsAddedToFile()
		{
			var metadata = new SKPdfMetadata
			{
				PdfA = true,
				Title = "𝓐𝓑𝓒𝓓𝓔 𝓕𝓖𝓗𝓘𝓙", // Out of basic multilingual plane
				Author = "ABCDE FGHIJ", // ASCII
				Subject = "αβγδε ζηθικ", // inside  basic multilingual plane
			};

			var expectations = new string[] {
				"<</Title <FEFFD835DCD0D835DCD1D835DCD2D835DCD3D835DCD40020D835DCD5D835DCD6D835DCD7D835DCD8D835DCD9>",
				"/Author (ABCDE FGHIJ)",
				"Subject <FEFF03B103B203B303B403B5002003B603B703B803B903BA>",
			};

			AssertPdfMetadata(metadata, expectations);
		}

		[SkippableTheory]
		[InlineData(1)]
		[InlineData(2)]
		[InlineData(100)]
		public void MultiplePagesWorkCorrectly(int pageCount)
		{
			var pdfData = GeneratePdf(new(), doc =>
			{
				for (var i = 0; i < pageCount; ++i)
				{
					var cnv = doc.BeginPage(612, 792);
					var color = new SKColor(0x00, (byte)(255.0f * i / (pageCount - 1)), 0x00);
					cnv.DrawColor(color);
				}
			});

			var pagesString = $"<</Type /Pages\n/Count {pageCount}";

			Assert.Contains(pagesString, pdfData);
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

		[SkippableFact]
		public void CanCreateTaggedPdf()
		{
			var pageSize = new SKSize(612, 792);  // U.S. Letter

			var metadata = new SKPdfMetadata();
			metadata.Title = "Example Tagged PDF";
			metadata.Creator = "Skia";
			var now = DateTime.Now;
			metadata.Creation = now;
			metadata.Modified = now;

			// The document tag.
			var root = new SKPdfStructureElementNode(1, "Document");

			// Heading.
			var h1 = new SKPdfStructureElementNode(2, "H1");
			root.Children.Add(h1);

			// Initial paragraph.
			var p = new SKPdfStructureElementNode(3, "P");
			root.Children.Add(p);

			// Hidden div. This is never referenced by marked content
			// so it should not appear in the resulting PDF.
			var div = new SKPdfStructureElementNode(4, "Div");
			root.Children.Add(div);

			// A bulleted list of two items.
			var l = new SKPdfStructureElementNode(5, "L");

			var lm1 = new SKPdfStructureElementNode(6, "Lbl");
			l.Children.Add(lm1);

			var li1 = new SKPdfStructureElementNode(7, "LI");
			l.Children.Add(li1);

			var lm2 = new SKPdfStructureElementNode(8, "Lbl");
			l.Children.Add(lm2);

			var li2 = new SKPdfStructureElementNode(9, "LI");
			l.Children.Add(li2);

			root.Children.Add(l);

			// Paragraph spanning two pages.
			var p2 = new SKPdfStructureElementNode(10, "P");
			root.Children.Add(p2);

			// Image with alt text.
			var img = new SKPdfStructureElementNode(11, "Figure");
			img.Alt = "Red box";
			root.Children.Add(img);

			metadata.Structure = root;

			var outputStream = new MemoryStream();
			var document = SKDocument.CreatePdf(outputStream, metadata);

			var paint = new SKPaint();
			paint.Color = SKColors.Black;

			// First page.
			{
				var canvas = document.BeginPage(pageSize.Width, pageSize.Height);

				canvas.DrawPdfNodeAnnotation(2);
				var font = new SKFont(null, 36);
				var message = "This is the title";
				canvas.Translate(72, 72);
				canvas.DrawText(message, 0, 0, font, paint);

				canvas.DrawPdfNodeAnnotation(3);
				font.Size = 14;
				message = "This is a simple paragraph.";
				canvas.Translate(0, 72);
				canvas.DrawText(message, 0, 0, font, paint);

				canvas.DrawPdfNodeAnnotation(6);
				message = "*";
				canvas.Translate(0, 72);
				canvas.DrawText(message, 0, 0, font, paint);

				canvas.DrawPdfNodeAnnotation(7);
				message = "List item 1";
				canvas.Translate(36, 0);
				canvas.DrawText(message, 0, 0, font, paint);

				canvas.DrawPdfNodeAnnotation(8);
				message = "*";
				canvas.Translate(-36, 36);
				canvas.DrawText(message, 0, 0, font, paint);

				canvas.DrawPdfNodeAnnotation(9);
				message = "List item 2";
				canvas.Translate(36, 0);
				canvas.DrawText(message, 0, 0, font, paint);

				canvas.DrawPdfNodeAnnotation(10);
				message = "This is a paragraph that starts on one page";
				canvas.Translate(-36, 6 * 72);
				canvas.DrawText(message, 0, 0, font, paint);

				document.EndPage();
			}

			// Second page.
			{
				var canvas = document.BeginPage(pageSize.Width, pageSize.Height);

				canvas.DrawPdfNodeAnnotation(10);
				var font = new SKFont(null, 14);
				var message = "and finishes on the second page.";
				canvas.Translate(72, 72);
				canvas.DrawText(message, 0, 0, font, paint);

				// Test a tagged image with alt text.
				canvas.DrawPdfNodeAnnotation(11);
				var testBitmap = new SKBitmap(new SKImageInfo(72, 72));
				testBitmap.Erase(SKColors.Red);
				canvas.Translate(72, 72);
				canvas.DrawImage(testBitmap.ToImage(), 0, 0);

				// This has a node ID but never shows up in the tag tree so it
				// won't be tagged.
				canvas.DrawPdfNodeAnnotation(999);
				message = "Page 2";
				canvas.Translate(468, -36);
				canvas.DrawText(message, 0, 0, font, paint);

				document.EndPage();
			}

			document.Close();
		}

		private static void AssertPdfMetadata(SKPdfMetadata metadata, params string[] expectations)
		{
			var pdfData = GeneratePdf(metadata);

			foreach (var expectation in expectations)
			{
				Assert.Contains(expectation, pdfData);
			}
		}

		private static string GeneratePdf(SKPdfMetadata metadata, Action<SKDocument> generate = null)
		{
			using var stream = new MemoryStream();
			using (var doc = SKDocument.CreatePdf(stream, metadata))
			{
				Assert.NotNull(doc);

				if (generate is not null)
				{
					generate.Invoke(doc);
				}
				else
				{
					var canvas = doc.BeginPage(612.0f, 792.0f);
					Assert.NotNull(canvas);

					canvas.DrawColor(SKColors.Red);

					doc.Close();
				}
			}

			Assert.True(stream.Length > 0);
			Assert.True(stream.Position > 0);

			stream.Position = 0;

			using var reader = new StreamReader(stream);
			var data = reader.ReadToEnd();

			return data;
		}
	}
}
