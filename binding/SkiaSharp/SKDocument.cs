using System;
using System.ComponentModel;
using System.IO;
using System.Collections.Generic;
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

			var structureHandle = ToNative (metadata.Structure);

			var metadataHandle = SkiaApi.sk_pdf_metadata_new ();

			try {
				if (metadata.Title is not null) {
					using var title = SKString.CreateRaw (metadata.Title);
					SkiaApi.sk_pdf_metadata_set_title (metadataHandle, title.Handle);
				}
				using var author = SKString.CreateRaw (metadata.Author);
				SkiaApi.sk_pdf_metadata_set_author (metadataHandle, author.Handle);
				using var subject = SKString.CreateRaw (metadata.Subject);
				SkiaApi.sk_pdf_metadata_set_subject (metadataHandle, subject.Handle);
				using var keywords = SKString.CreateRaw (metadata.Keywords);
				SkiaApi.sk_pdf_metadata_set_keywords (metadataHandle, keywords.Handle);
				using var creator = SKString.CreateRaw (metadata.Creator);
				SkiaApi.sk_pdf_metadata_set_creator (metadataHandle, creator.Handle);
				using var producer = SKString.CreateRaw (metadata.Producer);
				SkiaApi.sk_pdf_metadata_set_producer (metadataHandle, producer.Handle);

				if (metadata.Creation is not null) {
					var creation = SKTimeDateTimeInternal.Create (metadata.Creation.Value);
					SkiaApi.sk_pdf_metadata_set_creation (metadataHandle, &creation);
				}

				if (metadata.Modified is not null) {
					var modified = SKTimeDateTimeInternal.Create (metadata.Modified.Value);
					SkiaApi.sk_pdf_metadata_set_modified (metadataHandle, &modified);
				}

				SkiaApi.sk_pdf_metadata_set_raster_dpi (metadataHandle, metadata.RasterDpi);
				SkiaApi.sk_pdf_metadata_set_pdfa (metadataHandle, metadata.PdfA);
				SkiaApi.sk_pdf_metadata_set_encoding_quality (metadataHandle, metadata.EncodingQuality);
				SkiaApi.sk_pdf_metadata_set_compression_level (metadataHandle, metadata.Compression);

				SkiaApi.sk_pdf_metadata_set_structure_element_tree_root (metadataHandle, structureHandle);

				return Referenced (GetObject (SkiaApi.sk_document_create_pdf_from_stream_with_metadata (stream.Handle, &cmetadata)), stream);
			} finally {
				if (metadataHandle != IntPtr.Zero)
					SkiaApi.sk_pdf_metadata_delete (metadataHandle);

				// dispose because the root node is not owned
				if (structureHandle != IntPtr.Zero)
					SkiaApi.sk_pdf_structure_element_delete (structureHandle);
			}
		}

		internal static SKDocument GetObject (IntPtr handle) =>
			handle == IntPtr.Zero ? null : new SKDocument (handle, true);

		static IntPtr ToNative(SKPdfStructureElement? structure)
		{
			if (structure is null)
				return IntPtr.Zero;

			var handle = SkiaApi.sk_pdf_structure_element_new ();

			SkiaApi.sk_pdf_structure_element_set_node_id (handle, structure.Id);
			
#if NET5_0_OR_GREATER
			if (structure.AdditionalNodeIds is List<int> nodes) {
				var span = CollectionsMarshal.AsSpan (nodes);
				SkiaApi.sk_pdf_structure_element_add_additional_node_ids (handle, &span, span.Length);
			} else
#endif
			{
				using var nodes = Utils.ToRentedArray (structure.AdditionalNodeIds);
				SkiaApi.sk_pdf_structure_element_add_additional_node_ids (handle, &nodes, nodes.Length);
			}

			using var type = SKString.CreateRaw (structure.Type);
			SkiaApi.sk_pdf_structure_element_set_type_string (handle, type.Handle);

			using var alt = SKString.CreateRaw (structure.Alt);
			SkiaApi.sk_pdf_structure_element_set_alt (handle, alt.Handle);

			using var lang = SKString.CreateRaw (structure.Language);
			SkiaApi.sk_pdf_structure_element_set_lang (handle, lang.Handle);

			if (structure.Attributes.Inner.Count > 0) {
				foreach (var attr in structure.Attributes.Inner) {
					switch (attr.Value) {
						case int intValue:
							SkiaApi.sk_pdf_structure_element_add_int_attribute (handle, attr.Owner, attr.Name, intValue);
							break;
						case float floatValue:
							SkiaApi.sk_pdf_structure_element_add_float_attribute (handle, attr.Owner, attr.Name, floatValue);
							break;
						case string stringValue:
							SkiaApi.sk_pdf_structure_element_add_int_attribute (handle, attr.Owner, attr.Name, stringValue);
							break;
						case int[] intArray:
							SkiaApi.sk_pdf_structure_element_add_node_id_array_attribute (handle, attr.Owner, attr.Name, &intArray, intArray.Length);
							break;
						case float[] floatArray:
							SkiaApi.sk_pdf_structure_element_add_float_array_attribute (handle, attr.Owner, attr.Name, &floatArray, floatArray.Length);
							break;
						default:
							// TODO
							break;
					}
				}
			}

			if (structure.Children.Count > 0) {
				foreach (var child in structure.Children) {
					// do not dispose as the parent takes ownership
					var childHandle = ToNative (child);
					if (childHandle == IntPtr.Zero)
						continue;
					SkiaApi.sk_pdf_structure_element_add_child (handle, childHandle);
				}
			}
		}
	}
}
