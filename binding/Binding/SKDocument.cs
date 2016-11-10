//
// Bindings for SKDocument
//
// Author:
//   Matthew Leibowitz
//
// Copyright 2016 Xamarin Inc
//
using System;

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

		public static SKDocument CreatePdf (string path, float dpi = DefaultRasterDpi)
		{
			return GetObject<SKDocument> (SkiaApi.sk_document_create_pdf_from_filename (path, dpi));
		}

		public static SKDocument CreatePdf (SKWStream stream, float dpi = DefaultRasterDpi)
		{
			if (stream == null) {
				throw new ArgumentNullException (nameof(stream));
			}

			return GetObject<SKDocument> (SkiaApi.sk_document_create_pdf_from_stream(stream.Handle, dpi));
		}
	}
}

