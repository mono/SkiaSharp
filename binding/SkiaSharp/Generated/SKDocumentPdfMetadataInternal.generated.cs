using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_document_pdf_metadata_t
	[StructLayout (LayoutKind.Sequential)]
	internal unsafe partial struct SKDocumentPdfMetadataInternal : IEquatable<SKDocumentPdfMetadataInternal> {
		// public sk_string_t* fTitle
		public IntPtr fTitle;

		// public sk_string_t* fAuthor
		public IntPtr fAuthor;

		// public sk_string_t* fSubject
		public IntPtr fSubject;

		// public sk_string_t* fKeywords
		public IntPtr fKeywords;

		// public sk_string_t* fCreator
		public IntPtr fCreator;

		// public sk_string_t* fProducer
		public IntPtr fProducer;

		// public sk_document_pdf_datetime_t* fCreation
		public SKTimeDateTimeInternal* fCreation;

		// public sk_document_pdf_datetime_t* fModified
		public SKTimeDateTimeInternal* fModified;

		// public float fRasterDPI
		public Single fRasterDPI;

		// public bool fPDFA
		public Byte fPDFA;

		// public int fEncodingQuality
		public Int32 fEncodingQuality;

		public readonly bool Equals (SKDocumentPdfMetadataInternal obj) =>
#pragma warning disable CS8909
			fTitle == obj.fTitle && fAuthor == obj.fAuthor && fSubject == obj.fSubject && fKeywords == obj.fKeywords && fCreator == obj.fCreator && fProducer == obj.fProducer && fCreation == obj.fCreation && fModified == obj.fModified && fRasterDPI == obj.fRasterDPI && fPDFA == obj.fPDFA && fEncodingQuality == obj.fEncodingQuality;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKDocumentPdfMetadataInternal f && Equals (f);

		public static bool operator == (SKDocumentPdfMetadataInternal left, SKDocumentPdfMetadataInternal right) =>
			left.Equals (right);

		public static bool operator != (SKDocumentPdfMetadataInternal left, SKDocumentPdfMetadataInternal right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fTitle);
			hash.Add (fAuthor);
			hash.Add (fSubject);
			hash.Add (fKeywords);
			hash.Add (fCreator);
			hash.Add (fProducer);
			hash.Add (fCreation);
			hash.Add (fModified);
			hash.Add (fRasterDPI);
			hash.Add (fPDFA);
			hash.Add (fEncodingQuality);
			return hash.ToHashCode ();
		}

	}
}
