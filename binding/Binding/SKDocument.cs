﻿using System;

namespace SkiaSharp
{
	public class SKDocument : SKObject
	{
		public const float DefaultRasterDpi = 72.0f;

		[Preserve]
		internal SKDocument(IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.sk_document_unref (Handle);
			}

			base.Dispose (disposing);
		}

		public void Abort ()
		{
			SkiaApi.sk_document_abort (Handle);
		}

		public SKCanvas BeginPage (float width, float height)
		{
			return GetObject<SKCanvas> (SkiaApi.sk_document_begin_page (Handle, width, height, IntPtr.Zero), false);
		}

		public SKCanvas BeginPage (float width, float height, SKRect content)
		{
			return GetObject<SKCanvas> (SkiaApi.sk_document_begin_page (Handle, width, height, ref content), false);
		}

		public void EndPage ()
		{
			SkiaApi.sk_document_end_page (Handle);
		}

		public void Close ()
		{
			SkiaApi.sk_document_close (Handle);
		}

		[Obsolete ("Use CreateXps(SKWStream, float) instead.")]
		public static SKDocument CreateXps (string path, float dpi = DefaultRasterDpi)
		{
			if (path == null) {
				throw new ArgumentNullException (nameof (path));
			}

			var stream = SKFileWStream.OpenStream (path);
			var doc = CreateXps (stream, dpi);
			if (doc != null)
				doc.SetDisposeChild (stream);
			else
				stream.Dispose();
			return doc;
		}

		public static SKDocument CreateXps (SKWStream stream, float dpi = DefaultRasterDpi)
		{
			if (stream == null) {
				throw new ArgumentNullException (nameof(stream));
			}

			return GetObject<SKDocument> (SkiaApi.sk_document_create_xps_from_stream (stream.Handle, dpi));
		}

		[Obsolete ("Use CreatePdf(SKWStream) instead.")]
		public static SKDocument CreatePdf (string path, float dpi = DefaultRasterDpi)
		{
			if (path == null) {
				throw new ArgumentNullException (nameof (path));
			}

			var stream = SKFileWStream.OpenStream (path);
			var doc = CreatePdf (stream, dpi);
			if (doc != null)
				doc.SetDisposeChild (stream);
			else
				stream.Dispose();
			return doc;
		}

		[Obsolete("Use CreatePdf(SKWStream) instead.")]
		public static SKDocument CreatePdf (SKWStream stream, float dpi)
		{
			return CreatePdf (stream);
		}

		public static SKDocument CreatePdf (SKWStream stream)
		{
			if (stream == null) {
				throw new ArgumentNullException (nameof(stream));
			}

			return GetObject<SKDocument> (SkiaApi.sk_document_create_pdf_from_stream (stream.Handle));
		}

		[Obsolete("Use CreatePdf(SKWStream, SKDocumentPdfMetadata) instead.")]
		public static SKDocument CreatePdf (SKWStream stream, SKDocumentPdfMetadata metadata, float dpi)
		{
			return CreatePdf (stream, metadata);
		}

		public static SKDocument CreatePdf (SKWStream stream, SKDocumentPdfMetadata metadata)
		{
			if (stream == null) {
				throw new ArgumentNullException (nameof(stream));
			}

			using (var title = SKString.Create (metadata.Title))
			using (var author = SKString.Create (metadata.Author))
			using (var subject = SKString.Create (metadata.Subject))
			using (var keywords = SKString.Create (metadata.Keywords))
			using (var creator = SKString.Create (metadata.Creator))
			using (var producer = SKString.Create (metadata.Producer)) {

				var cmetadata = new SKDocumentPdfMetadataInternal {
					Title = title?.Handle ?? IntPtr.Zero,
					Author = author?.Handle ?? IntPtr.Zero,
					Subject = subject?.Handle ?? IntPtr.Zero,
					Keywords = keywords?.Handle ?? IntPtr.Zero,
					Creator = creator?.Handle ?? IntPtr.Zero,
					Producer = producer?.Handle ?? IntPtr.Zero
				};

				unsafe {
					if (metadata.Creation != null) {
						var creation = SKTimeDateTimeInternal.Create (metadata.Creation.Value);
						cmetadata.Creation = &creation;
					}
					if (metadata.Modified != null) {
						var modified = SKTimeDateTimeInternal.Create (metadata.Modified.Value);
						cmetadata.Modified = &modified;
					}

					return GetObject<SKDocument> (SkiaApi.sk_document_create_pdf_from_stream_with_metadata (stream.Handle, ref cmetadata));
				}
			}
		}
	}
}

