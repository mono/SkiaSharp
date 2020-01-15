using System;
using System.IO;

namespace SkiaSharp
{
	public unsafe class SKTypeface : SKObject, ISKReferenceCounted
	{
		private static readonly Lazy<SKTypeface> defaultTypeface;

		static SKTypeface ()
		{
			defaultTypeface = new Lazy<SKTypeface> (() => new SKTypefaceStatic (SkiaApi.sk_typeface_ref_default ()));
		}

		internal static void EnsureStaticInstanceAreInitialized ()
		{
			// IMPORTANT: do not remove to ensure that the static instances
			//            are initialized before any access is made to them
		}

		[Preserve]
		internal SKTypeface (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		// Default

		public static SKTypeface Default =>
			defaultTypeface.Value;

		public static SKTypeface CreateDefault () =>
			GetObject<SKTypeface> (SkiaApi.sk_typeface_create_default ());

		// FromXxx

		public static SKTypeface FromFamilyName (string familyName, int weight, int width, SKFontStyleSlant slant) =>
			FromFamilyName (familyName, new SKFontStyle (weight, width, slant));

		public static SKTypeface FromFamilyName (string familyName) =>
			FromFamilyName (familyName, SKFontStyle.Normal);

		public static SKTypeface FromFamilyName (string familyName, SKFontStyle style)
		{
			if (style == null)
				throw new ArgumentNullException (nameof (style));

			return GetObject<SKTypeface> (SkiaApi.sk_typeface_create_from_name (familyName, style.Handle));
		}

		public static SKTypeface FromFamilyName (string familyName, SKFontStyleWeight weight, SKFontStyleWidth width, SKFontStyleSlant slant) =>
			FromFamilyName (familyName, (int)weight, (int)width, slant);

		public static SKTypeface FromFile (string path, int index = 0)
		{
			if (path == null)
				throw new ArgumentNullException (nameof (path));

			var utf8path = StringUtilities.GetEncodedText (path, SKTextEncoding.Utf8);
			fixed (byte* u = utf8path) {
				return GetObject<SKTypeface> (SkiaApi.sk_typeface_create_from_file (u, index));
			}
		}

		public static SKTypeface FromStream (Stream stream, int index = 0)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			return FromStream (new SKManagedStream (stream, true), index);
		}

		public static SKTypeface FromStream (SKStreamAsset stream, int index = 0)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			if (stream is SKManagedStream managed) {
				stream = managed.ToMemoryStream ();
				managed.Dispose ();
			}

			var typeface = GetObject<SKTypeface> (SkiaApi.sk_typeface_create_from_stream (stream.Handle, index));
			stream.RevokeOwnership (typeface);
			return typeface;
		}

		public static SKTypeface FromData (SKData data, int index = 0)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));

			return GetObject<SKTypeface> (SkiaApi.sk_typeface_create_from_data (data.Handle, index));
		}

		// Properties

		public string FamilyName => (string)GetObject<SKString> (SkiaApi.sk_typeface_get_family_name (Handle));

		public SKFontStyle FontStyle => GetObject<SKFontStyle> (SkiaApi.sk_typeface_get_fontstyle (Handle));

		public int FontWeight => SkiaApi.sk_typeface_get_font_weight (Handle);

		public int FontWidth => SkiaApi.sk_typeface_get_font_width (Handle);

		public SKFontStyleSlant FontSlant => SkiaApi.sk_typeface_get_font_slant (Handle);

		public bool IsBold => FontStyle.Weight >= (int)SKFontStyleWeight.SemiBold;

		public bool IsItalic => FontStyle.Slant != SKFontStyleSlant.Upright;

		public bool IsFixedPitch => SkiaApi.sk_typeface_is_fixed_pitch (Handle);

		public int UnitsPerEm => SkiaApi.sk_typeface_get_units_per_em (Handle);

		public int GlyphsCount => SkiaApi.sk_typeface_count_glyphs (Handle);

		public int TableTagsCount => SkiaApi.sk_typeface_count_tables (Handle);

		// GetTableTags

		public UInt32[] GetTableTags ()
		{
			if (!TryGetTableTags (out var result)) {
				throw new Exception ("Unable to read the tables for the file.");
			}
			return result;
		}

		public bool TryGetTableTags (out UInt32[] tags)
		{
			var buffer = new UInt32[TableTagsCount];
			if (TryGetTableTags (buffer)) {
				tags = null;
				return false;
			}
			tags = buffer;
			return true;
		}

		public bool TryGetTableTags (Span<UInt32> tags)
		{
			fixed (UInt32* b = tags) {
				return SkiaApi.sk_typeface_get_table_tags (Handle, b) != 0;
			}
		}

		// GetTableSize

		public int GetTableSize (UInt32 tag) =>
			(int)SkiaApi.sk_typeface_get_table_size (Handle, tag);

		// GetTableData

		public byte[] GetTableData (UInt32 tag)
		{
			if (!TryGetTableData (tag, out var result)) {
				throw new Exception ("Unable to read the data table.");
			}
			return result;
		}

		public bool TryGetTableData (UInt32 tag, out byte[] tableData)
		{
			var length = GetTableSize (tag);
			var buffer = new byte[length];
			if (!TryGetTableData (tag, buffer)) {
				tableData = null;
				return false;
			}
			tableData = buffer;
			return true;
		}

		public bool TryGetTableData (UInt32 tag, Span<byte> tableData) =>
			TryGetTableData (tag, 0, tableData.Length, tableData);

		public bool TryGetTableData (UInt32 tag, int start, int length, Span<byte> tableData)
		{
			fixed (byte* b = tableData) {
				return SkiaApi.sk_typeface_get_table_data (Handle, tag, (IntPtr)start, (IntPtr)length, b) != IntPtr.Zero;
			}
		}

		// GetGlyph

		public ushort GetGlyph (int codepoint) =>
			SkiaApi.sk_typeface_unichar_to_glyph (Handle, codepoint);

		// GetGlyphs

		public ushort[] GetGlyphs (ReadOnlySpan<int> codepoints)
		{
			var glyphs = new ushort[codepoints.Length];
			GetGlyphs (codepoints, glyphs);
			return glyphs;
		}

		public void GetGlyphs (ReadOnlySpan<int> codepoints, Span<ushort> glyphs)
		{
			fixed (int* up = codepoints)
			fixed (ushort* gp = glyphs) {
				SkiaApi.sk_typeface_unichars_to_glyphs (Handle, up, codepoints.Length, gp);
			}
		}

		// OpenStream

		public SKStreamAsset OpenStream () =>
			OpenStream (out _);

		public SKStreamAsset OpenStream (out int ttcIndex)
		{
			fixed (int* ttc = &ttcIndex) {
				return GetObject<SKStreamAssetImplementation> (SkiaApi.sk_typeface_open_stream (Handle, ttc));
			}
		}

		// GetKerningPairAdjustments

		public int[] GetKerningPairAdjustments (ReadOnlySpan<ushort> glyphs)
		{
			var adjustments = new int[glyphs.Length];
			GetKerningPairAdjustments (glyphs, adjustments);
			return adjustments;
		}

		public void GetKerningPairAdjustments (ReadOnlySpan<ushort> glyphs, Span<int> adjustments)
		{
			fixed (ushort* gp = glyphs)
			fixed (int* ap = adjustments) {
				SkiaApi.sk_typeface_get_kerning_pair_adjustments (Handle, gp, glyphs.Length, ap);
			}
		}

		private sealed class SKTypefaceStatic : SKTypeface
		{
			internal SKTypefaceStatic (IntPtr x)
				: base (x, false)
			{
				IgnorePublicDispose = true;
			}

			protected override void Dispose (bool disposing)
			{
				// do not dispose
			}
		}
	}
}
