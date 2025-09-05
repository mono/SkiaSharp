#nullable disable

using System;
using System.ComponentModel;
using System.IO;

namespace SkiaSharp
{
	/// <summary>
	/// A high-level API for creating a document-based canvas.
	/// </summary>
	/// <remarks>
	/// For each page, call <see cref="SkiaSharp.SKDocument.BeginPage" /> to get the
	/// canvas, and then complete the page with a call to
	/// <see cref="SkiaSharp.SKDocument.EndPage" />. Finally, call
	/// <see cref="SkiaSharp.SKDocument.Close" /> to complete the document.
	/// </remarks>
	public unsafe class SKDocument : SKObject, ISKReferenceCounted, ISKSkipObjectRegistration
	{
		/// <summary>
		/// Gets the default DPI for raster graphics.
		/// </summary>
		public const float DefaultRasterDpi = 72.0f;

		internal SKDocument (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		/// <summary>
		/// Stops producing the document immediately.
		/// </summary>
		public void Abort () =>
			SkiaApi.sk_document_abort (Handle);

		/// <summary>
		/// Begins a new page for the document, returning the canvas that will draw into the page.
		/// </summary>
		/// <param name="width">The width of the page.</param>
		/// <param name="height">The height of the page.</param>
		/// <returns>Returns a canvas for the new page.</returns>
		/// <remarks>
		/// The document owns this canvas, and it will go out of scope when <see cref="SKDocument.EndPage" /> or <see cref="SKDocument.Close" /> is called, or the document is deleted.
		/// </remarks>
		public SKCanvas BeginPage (float width, float height) =>
			OwnedBy (SKCanvas.GetObject (SkiaApi.sk_document_begin_page (Handle, width, height, null), false), this);

		/// <summary>
		/// Begins a new page for the document, returning the canvas that will draw into the page.
		/// </summary>
		/// <param name="width">The width of the page.</param>
		/// <param name="height">The height of the page.</param>
		/// <param name="content">The area for the page contents.</param>
		/// <returns>Returns a canvas for the new page.</returns>
		/// <remarks>
		/// The document owns this canvas, and it will go out of scope when <see cref="SKDocument.EndPage" /> or <see cref="SKDocument.Close" /> is called, or the document is deleted.
		/// </remarks>
		public SKCanvas BeginPage (float width, float height, SKRect content) =>
			OwnedBy (SKCanvas.GetObject (SkiaApi.sk_document_begin_page (Handle, width, height, &content), false), this);

		/// <summary>
		/// Completes the drawing for the current page created by <see cref="SKDocument.BeginPage(System.Single,System.Single)" />.
		/// </summary>
		public void EndPage () =>
			SkiaApi.sk_document_end_page (Handle);

		/// <summary>
		/// Closes the current file or stream holding the document's contents.
		/// </summary>
		public void Close () =>
			SkiaApi.sk_document_close (Handle);

		// CreateXps

		/// <summary>
		/// Create a XPS-backed document, writing the results into a file.
		/// </summary>
		/// <param name="path">The path of the file to write to.</param>
		/// <returns>Returns the new XPS-backed document.</returns>
		public static SKDocument CreateXps (string path) =>
			CreateXps (path, DefaultRasterDpi);

		/// <summary>
		/// Create a XPS-backed document, writing the results into a stream.
		/// </summary>
		/// <param name="stream">The stream to write to.</param>
		/// <returns>Returns the new XPS-backed document.</returns>
		public static SKDocument CreateXps (Stream stream) =>
			CreateXps (stream, DefaultRasterDpi);

		/// <summary>
		/// Create a XPS-backed document, writing the results into a stream.
		/// </summary>
		/// <param name="stream">The stream to write to.</param>
		/// <returns>Returns the new XPS-backed document.</returns>
		public static SKDocument CreateXps (SKWStream stream) =>
			CreateXps (stream, DefaultRasterDpi);

		/// <summary>
		/// Create a XPS-backed document, writing the results into a file.
		/// </summary>
		/// <param name="path">The path of the file to write to.</param>
		/// <param name="dpi">The DPI (pixels-per-inch) at which features without native XPS support will be rasterized.</param>
		/// <returns>Returns the new XPS-backed document.</returns>
		/// <remarks>
		/// XPS pages are sized in point units. 1 pt == 1/72 inch == 127/360 mm.
		/// </remarks>
		public static SKDocument CreateXps (string path, float dpi)
		{
			if (path == null) {
				throw new ArgumentNullException (nameof (path));
			}

			var stream = SKFileWStream.OpenStream (path);
			return Owned (CreateXps (stream, dpi), stream);
		}

		/// <summary>
		/// Create a XPS-backed document, writing the results into a stream.
		/// </summary>
		/// <param name="stream">The stream to write to.</param>
		/// <param name="dpi">The DPI (pixels-per-inch) at which features without native XPS support will be rasterized.</param>
		/// <returns>Returns the new XPS-backed document.</returns>
		/// <remarks>
		/// XPS pages are sized in point units. 1 pt == 1/72 inch == 127/360 mm.
		/// </remarks>
		public static SKDocument CreateXps (Stream stream, float dpi)
		{
			if (stream == null) {
				throw new ArgumentNullException (nameof (stream));
			}

			var managed = new SKManagedWStream (stream);
			return Owned (CreateXps (managed, dpi), managed);
		}

		/// <summary>
		/// Create a XPS-backed document, writing the results into a stream.
		/// </summary>
		/// <param name="stream">The stream to write to.</param>
		/// <param name="dpi">The DPI (pixels-per-inch) at which features without native XPS support will be rasterized.</param>
		/// <returns>Returns the new XPS-backed document.</returns>
		/// <remarks>
		/// XPS pages are sized in point units. 1 pt == 1/72 inch == 127/360 mm.
		/// </remarks>
		public static SKDocument CreateXps (SKWStream stream, float dpi)
		{
			if (stream == null) {
				throw new ArgumentNullException (nameof (stream));
			}

			return Referenced (GetObject (SkiaApi.sk_document_create_xps_from_stream (stream.Handle, dpi)), stream);
		}

		// CreatePdf

		/// <summary>
		/// Create a PDF-backed document, writing the results into a file.
		/// </summary>
		/// <param name="path">The path of the file to write to.</param>
		/// <returns>Returns the new PDF-backed document.</returns>
		public static SKDocument CreatePdf (string path)
		{
			if (path == null) {
				throw new ArgumentNullException (nameof (path));
			}

			var stream = SKFileWStream.OpenStream (path);
			return Owned (CreatePdf (stream), stream);
		}

		/// <summary>
		/// Create a PDF-backed document, writing the results into a stream.
		/// </summary>
		/// <param name="stream">The stream to write to.</param>
		/// <returns>Returns the new PDF-backed document.</returns>
		public static SKDocument CreatePdf (Stream stream)
		{
			if (stream == null) {
				throw new ArgumentNullException (nameof (stream));
			}

			var managed = new SKManagedWStream (stream);
			return Owned (CreatePdf (managed), managed);
		}

		/// <summary>
		/// Create a PDF-backed document, writing the results into a stream.
		/// </summary>
		/// <param name="stream">The stream to write to.</param>
		/// <returns>Returns the new PDF-backed document.</returns>
		public static SKDocument CreatePdf (SKWStream stream)
		{
			if (stream == null) {
				throw new ArgumentNullException (nameof (stream));
			}

			return Referenced (GetObject (SkiaApi.sk_document_create_pdf_from_stream (stream.Handle)), stream);
		}

		/// <summary>
		/// Create a PDF-backed document, writing the results into a file.
		/// </summary>
		/// <param name="path">The path of the file to write to.</param>
		/// <param name="dpi">The DPI (pixels-per-inch) at which features without native PDF support will be rasterized.</param>
		/// <returns>Returns the new PDF-backed document.</returns>
		/// <remarks>
		/// PDF pages are sized in point units. 1 pt == 1/72 inch == 127/360 mm.
		/// </remarks>
		public static SKDocument CreatePdf (string path, float dpi) =>
			CreatePdf (path, new SKDocumentPdfMetadata (dpi));

		/// <summary>
		/// Create a PDF-backed document, writing the results into a stream.
		/// </summary>
		/// <param name="stream">The stream to write to.</param>
		/// <param name="dpi">The DPI (pixels-per-inch) at which features without native PDF support will be rasterized.</param>
		/// <returns>Returns the new PDF-backed document.</returns>
		/// <remarks>
		/// PDF pages are sized in point units. 1 pt == 1/72 inch == 127/360 mm.
		/// </remarks>
		public static SKDocument CreatePdf (Stream stream, float dpi) =>
			CreatePdf (stream, new SKDocumentPdfMetadata (dpi));

		/// <summary>
		/// Create a PDF-backed document, writing the results into a stream.
		/// </summary>
		/// <param name="stream">The stream to write to.</param>
		/// <param name="dpi">The DPI (pixels-per-inch) at which features without native PDF support will be rasterized.</param>
		/// <returns>Returns the new PDF-backed document.</returns>
		/// <remarks>
		/// PDF pages are sized in point units. 1 pt == 1/72 inch == 127/360 mm.
		/// </remarks>
		public static SKDocument CreatePdf (SKWStream stream, float dpi) =>
			CreatePdf (stream, new SKDocumentPdfMetadata (dpi));

		/// <summary>
		/// Create a PDF-backed document with the specified metadata, writing the results into a file.
		/// </summary>
		/// <param name="path">The path of the file to write to.</param>
		/// <param name="metadata">The document metadata to include.</param>
		/// <returns>Returns the new PDF-backed document.</returns>
		public static SKDocument CreatePdf (string path, SKDocumentPdfMetadata metadata)
		{
			if (path == null) {
				throw new ArgumentNullException (nameof (path));
			}

			var stream = SKFileWStream.OpenStream (path);
			return Owned (CreatePdf (stream, metadata), stream);
		}

		/// <summary>
		/// Create a PDF-backed document with the specified metadata, writing the results into a stream.
		/// </summary>
		/// <param name="stream">The stream to write to.</param>
		/// <param name="metadata">The document metadata to include.</param>
		/// <returns>Returns the new PDF-backed document.</returns>
		public static SKDocument CreatePdf (Stream stream, SKDocumentPdfMetadata metadata)
		{
			if (stream == null) {
				throw new ArgumentNullException (nameof (stream));
			}

			var managed = new SKManagedWStream (stream);
			return Owned (CreatePdf (managed, metadata), managed);
		}

		/// <summary>
		/// Create a PDF-backed document, writing the results into a stream.
		/// </summary>
		/// <param name="stream">The stream to write to.</param>
		/// <param name="metadata">The document metadata to include.</param>
		/// <returns>Returns the new PDF-backed document.</returns>
		public static SKDocument CreatePdf (SKWStream stream, SKDocumentPdfMetadata metadata)
		{
			if (stream == null) {
				throw new ArgumentNullException (nameof (stream));
			}

			using var title = SKString.Create (metadata.Title);
			using var author = SKString.Create (metadata.Author);
			using var subject = SKString.Create (metadata.Subject);
			using var keywords = SKString.Create (metadata.Keywords);
			using var creator = SKString.Create (metadata.Creator);
			using var producer = SKString.Create (metadata.Producer);

			var cmetadata = new SKDocumentPdfMetadataInternal {
				fTitle = title?.Handle ?? IntPtr.Zero,
				fAuthor = author?.Handle ?? IntPtr.Zero,
				fSubject = subject?.Handle ?? IntPtr.Zero,
				fKeywords = keywords?.Handle ?? IntPtr.Zero,
				fCreator = creator?.Handle ?? IntPtr.Zero,
				fProducer = producer?.Handle ?? IntPtr.Zero,
				fRasterDPI = metadata.RasterDpi,
				fPDFA = metadata.PdfA ? (byte)1 : (byte)0,
				fEncodingQuality = metadata.EncodingQuality,
			};

			SKTimeDateTimeInternal creation;
			if (metadata.Creation != null) {
				creation = SKTimeDateTimeInternal.Create (metadata.Creation.Value);
				cmetadata.fCreation = &creation;
			}
			SKTimeDateTimeInternal modified;
			if (metadata.Modified != null) {
				modified = SKTimeDateTimeInternal.Create (metadata.Modified.Value);
				cmetadata.fModified = &modified;
			}

			return Referenced (GetObject (SkiaApi.sk_document_create_pdf_from_stream_with_metadata (stream.Handle, &cmetadata)), stream);
		}

		internal static SKDocument GetObject (IntPtr handle) =>
			handle == IntPtr.Zero ? null : new SKDocument (handle, true);
	}
}
