﻿using System;
using System.ComponentModel;
using System.IO;

namespace SkiaSharp
{
	public unsafe class SKDocument : SKObject, ISKReferenceCounted
	{
		public const float DefaultRasterDpi = 72.0f;

		// keep the stream alive for as long as the document exists
		private SKWStream underlyingStream;

		[Preserve]
		internal SKDocument (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		public void Abort () =>
			SkiaApi.sk_document_abort (Handle);

		public SKCanvas BeginPage (float width, float height) =>
			GetObject<SKCanvas> (SkiaApi.sk_document_begin_page (Handle, width, height, null), false);

		public SKCanvas BeginPage (float width, float height, SKRect content) =>
			GetObject<SKCanvas> (SkiaApi.sk_document_begin_page (Handle, width, height, &content), false);

		public void EndPage () =>
			SkiaApi.sk_document_end_page (Handle);

		public void Close () =>
			SkiaApi.sk_document_close (Handle);

		// CreateXps

		public static SKDocument CreateXps (string path) =>
			CreateXps (path, DefaultRasterDpi);

		public static SKDocument CreateXps (Stream stream) =>
			CreateXps (stream, DefaultRasterDpi);

		public static SKDocument CreateXps (SKWStream stream) =>
			CreateXps (stream, DefaultRasterDpi);

		public static SKDocument CreateXps (string path, float dpi)
		{
			if (path == null) {
				throw new ArgumentNullException (nameof (path));
			}

			var stream = SKFileWStream.OpenStream (path);
			return Owned (CreateXps (stream, dpi), stream);
		}

		public static SKDocument CreateXps (Stream stream, float dpi)
		{
			if (stream == null) {
				throw new ArgumentNullException (nameof (stream));
			}

			var managed = new SKManagedWStream (stream);
			return Owned (CreateXps (managed, dpi), managed);
		}

		public static SKDocument CreateXps (SKWStream stream, float dpi)
		{
			if (stream == null) {
				throw new ArgumentNullException (nameof (stream));
			}

			return Referenced (GetObject<SKDocument> (SkiaApi.sk_document_create_xps_from_stream (stream.Handle, dpi)), stream);
		}

		// CreatePdf

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use CreatePdf(SKWStream, SKDocumentPdfMetadata) instead.")]
		public static SKDocument CreatePdf (SKWStream stream, SKDocumentPdfMetadata metadata, float dpi)
		{
			metadata.RasterDpi = dpi;
			return CreatePdf (stream, metadata);
		}

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

			return GetObject<SKDocument> (SkiaApi.sk_document_create_pdf_from_stream (stream.Handle));
		}

		public static SKDocument CreatePdf (string path, float dpi) =>
			CreatePdf (path, new SKDocumentPdfMetadata (dpi));

		public static SKDocument CreatePdf (Stream stream, float dpi) =>
			CreatePdf (stream, new SKDocumentPdfMetadata (dpi));

		public static SKDocument CreatePdf (SKWStream stream, float dpi) =>
			CreatePdf (stream, new SKDocumentPdfMetadata (dpi));

		public static SKDocument CreatePdf (string path, SKDocumentPdfMetadata metadata)
		{
			if (path == null) {
				throw new ArgumentNullException (nameof (path));
			}

			var stream = SKFileWStream.OpenStream (path);
			return Owned (CreatePdf (stream, metadata), stream);
		}

		public static SKDocument CreatePdf (Stream stream, SKDocumentPdfMetadata metadata)
		{
			if (stream == null) {
				throw new ArgumentNullException (nameof (stream));
			}

			var managed = new SKManagedWStream (stream);
			return Owned (CreatePdf (managed, metadata), managed);
		}

		public static SKDocument CreatePdf (SKWStream stream, SKDocumentPdfMetadata metadata)
		{
			if (stream == null) {
				throw new ArgumentNullException (nameof (stream));
			}

			using (var title = SKString.Create (metadata.Title))
			using (var author = SKString.Create (metadata.Author))
			using (var subject = SKString.Create (metadata.Subject))
			using (var keywords = SKString.Create (metadata.Keywords))
			using (var creator = SKString.Create (metadata.Creator))
			using (var producer = SKString.Create (metadata.Producer)) {
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

				if (metadata.Creation != null) {
					var creation = SKTimeDateTimeInternal.Create (metadata.Creation.Value);
					cmetadata.fCreation = &creation;
				}
				if (metadata.Modified != null) {
					var modified = SKTimeDateTimeInternal.Create (metadata.Modified.Value);
					cmetadata.fModified = &modified;
				}

				return Referenced (GetObject<SKDocument> (SkiaApi.sk_document_create_pdf_from_stream_with_metadata (stream.Handle, &cmetadata)), stream);
			}
		}

		private static SKDocument Owned (SKDocument doc, SKWStream stream)
		{
			if (stream != null) {
				if (doc != null)
					doc.SetDisposeChild (stream);
				else
					stream.Dispose ();
			}

			return doc;
		}

		private static SKDocument Referenced (SKDocument doc, SKWStream stream)
		{
			if (stream != null && doc != null)
				doc.underlyingStream = stream;

			return doc;
		}
	}
}
