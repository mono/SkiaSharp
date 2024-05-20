using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace SkiaSharp
{
    public unsafe class SKDocument : SKObject, ISKReferenceCounted, ISKSkipObjectRegistration
	{
		public const float DefaultRasterDpi = 72.0f;

		internal SKDocument (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		public void Abort () =>
			SkiaApi.sk_document_abort (Handle);

		public SKCanvas BeginPage (float width, float height) =>
			OwnedBy (SKCanvas.GetObject (SkiaApi.sk_document_begin_page (Handle, width, height, null), false), this);

		public SKCanvas BeginPage (float width, float height, SKRect content) =>
			OwnedBy (SKCanvas.GetObject (SkiaApi.sk_document_begin_page (Handle, width, height, &content), false), this);

		public void EndPage () =>
			SkiaApi.sk_document_end_page (Handle);

		public void Close () =>
			SkiaApi.sk_document_close (Handle);

		// CreateXps

		public static SKDocument? CreateXps (string path) =>
			CreateXps (path, DefaultRasterDpi);

		public static SKDocument? CreateXps (Stream stream) =>
			CreateXps (stream, DefaultRasterDpi);

		public static SKDocument? CreateXps (SKWStream stream) =>
			CreateXps (stream, DefaultRasterDpi);

		public static SKDocument? CreateXps (string path, float dpi)
		{
			if (path == null) {
				throw new ArgumentNullException (nameof (path));
			}

			var stream = SKFileWStream.OpenStream (path);
			return Owned (CreateXps (stream, dpi), stream);
		}

		public static SKDocument? CreateXps (Stream stream, float dpi)
		{
			if (stream == null) {
				throw new ArgumentNullException (nameof (stream));
			}

			var managed = new SKManagedWStream (stream);
			return Owned (CreateXps (managed, dpi), managed);
		}

		public static SKDocument? CreateXps (SKWStream stream, float dpi)
		{
			if (stream == null) {
				throw new ArgumentNullException (nameof (stream));
			}

			return Referenced (GetObject (SkiaApi.sk_document_create_xps_from_stream (stream.Handle, dpi)), stream);
		}

		// CreatePdf

		public static SKDocument CreatePdf (string path)
		{
			if (path == null) {
				throw new ArgumentNullException (nameof (path));
			}

			var stream = SKFileWStream.OpenStream (path);
			return Owned (CreatePdf (stream), stream);
		}

		public static SKDocument CreatePdf (Stream stream)
		{
			if (stream == null) {
				throw new ArgumentNullException (nameof (stream));
			}

			var managed = new SKManagedWStream (stream);
			return Owned (CreatePdf (managed), managed);
		}

		public static SKDocument CreatePdf (SKWStream stream)
		{
			if (stream == null) {
				throw new ArgumentNullException (nameof (stream));
			}

			return Referenced (GetObject (SkiaApi.sk_document_create_pdf_from_stream (stream.Handle)), stream);
		}

		public static SKDocument CreatePdf (string path, float dpi) =>
			CreatePdf (path, new SKDocumentPdfMetadata (dpi));

		public static SKDocument CreatePdf (Stream stream, float dpi) =>
			CreatePdf (stream, new SKDocumentPdfMetadata (dpi));

		public static SKDocument CreatePdf (SKWStream stream, float dpi) =>
			CreatePdf (stream, new SKDocumentPdfMetadata (dpi));

		public static SKDocument CreatePdf (string path, SKDocumentPdfMetadata metadata) =>
			CreatePdf (path, new SKPdfMetadata (metadata));

		public static SKDocument CreatePdf (Stream stream, SKDocumentPdfMetadata metadata) =>
			CreatePdf (stream, new SKPdfMetadata (metadata));

		public static SKDocument CreatePdf (SKWStream stream, SKDocumentPdfMetadata metadata) =>
			CreatePdf (stream, new SKPdfMetadata (metadata));

		public static SKDocument CreatePdf (string path, SKPdfMetadata metadata)
		{
			if (path == null) {
				throw new ArgumentNullException (nameof (path));
			}

			var stream = SKFileWStream.OpenStream (path);
			return Owned (CreatePdf (stream, metadata), stream);
		}

		public static SKDocument CreatePdf (Stream stream, SKPdfMetadata metadata)
		{
			if (stream == null) {
				throw new ArgumentNullException (nameof (stream));
			}

			var managed = new SKManagedWStream (stream);
			return Owned (CreatePdf (managed, metadata), managed);
		}

		public static SKDocument CreatePdf (SKWStream stream, SKPdfMetadata metadata)
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

			SKTimeDateTimeInternal creation;
			if (metadata.Creation is not null)
				creation = SKTimeDateTimeInternal.Create (metadata.Creation.Value);

			SKTimeDateTimeInternal modified;
			if (metadata.Modified is not null)
				modified = SKTimeDateTimeInternal.Create (metadata.Modified.Value);

			var metadataHandle = SkiaApi.sk_pdf_metadata_new ();

			try {
				SkiaApi.sk_pdf_metadata_set_title(metadataHandle, title?.Handle ?? IntPtr.Zero);
				SkiaApi.sk_pdf_metadata_set_author(metadataHandle, author?.Handle ?? IntPtr.Zero);
				SkiaApi.sk_pdf_metadata_set_subject(metadataHandle, subject?.Handle ?? IntPtr.Zero);
				SkiaApi.sk_pdf_metadata_set_keywords(metadataHandle, keywords?.Handle ?? IntPtr.Zero);
				SkiaApi.sk_pdf_metadata_set_creator(metadataHandle, creator?.Handle ?? IntPtr.Zero);
				SkiaApi.sk_pdf_metadata_set_producer(metadataHandle, producer?.Handle ?? IntPtr.Zero);
				
				SkiaApi.sk_pdf_metadata_set_creation(metadataHandle, &creation);
				SkiaApi.sk_pdf_metadata_set_modified(metadataHandle, &modified);

				SkiaApi.sk_pdf_metadata_set_raster_dpi(metadataHandle, metadata.RasterDpi);
				SkiaApi.sk_pdf_metadata_set_pdfa(metadataHandle, metadata.PdfA);
				SkiaApi.sk_pdf_metadata_set_encoding_quality(metadataHandle, metadata.EncodingQuality);
				SkiaApi.sk_pdf_metadata_set_compression_level(metadataHandle, metadata.Compression);

				return Referenced (GetObject (SkiaApi.sk_document_create_pdf_from_stream_with_metadata (stream.Handle, &cmetadata)), stream);
			} finally {
				if (metadataHandle != IntPtr.Zero)
					SkiaApi.sk_pdf_metadata_delete(metadataHandle);
			}
		}

		internal static SKDocument GetObject (IntPtr handle) =>
			handle == IntPtr.Zero ? null : new SKDocument (handle, true);

		static SKPdfStructureElementNative ToNative(SKPdfStructureElement structure)
		{
			var t = new Memory<int> ();
			var x = t.Pin ();
			int* tt = (int*)x.Pointer;

			new SKPdfStructureElementInternal {
				fAdditionalNodeIds = tt,
				fAdditionalNodeIdsSize = structure.AdditionalNodeIds.Count,
			};
		}
	}
}
