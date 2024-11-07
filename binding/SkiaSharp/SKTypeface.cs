#nullable disable

using System;
using System.ComponentModel;
using System.IO;

namespace SkiaSharp
{
	public unsafe class SKTypeface : SKObject, ISKReferenceCounted
	{
		private static readonly SKTypeface defaultTypeface;

		private SKFont font;

		static SKTypeface ()
		{
			// TODO: This is not the best way to do this as it will create a lot of objects that
			//       might not be needed, but it is the only way to ensure that the static
			//       instances are created before any access is made to them.
			//       See more info: SKObject.EnsureStaticInstanceAreInitialized()

			defaultTypeface = new SKTypefaceStatic (SkiaApi.sk_typeface_ref_default ());
		}

		internal static void EnsureStaticInstanceAreInitialized ()
		{
			// IMPORTANT: do not remove to ensure that the static instances
			//            are initialized before any access is made to them
		}

		internal SKTypeface (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		// Default

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		public static SKTypeface Default => defaultTypeface;

		public static SKTypeface CreateDefault ()
		{
			return GetObject (SkiaApi.sk_typeface_create_default ());
		}

		// FromFamilyName

		public static SKTypeface FromFamilyName (string familyName, int weight, int width, SKFontStyleSlant slant)
		{
			return FromFamilyName (familyName, new SKFontStyle (weight, width, slant));
		}

		public static SKTypeface FromFamilyName (string familyName)
		{
			return FromFamilyName (familyName, SKFontStyle.Normal);
		}

		public static SKTypeface FromFamilyName (string familyName, SKFontStyle style)
		{
			if (style == null)
				throw new ArgumentNullException (nameof (style));

			var familyNameUtf8ByteList = StringUtilities.GetEncodedText (familyName, SKTextEncoding.Utf8, addNull:true);
			fixed (byte* familyNamePointer = familyNameUtf8ByteList)
			{
				var tf = GetObject (SkiaApi.sk_typeface_create_from_name (new IntPtr (familyNamePointer), style.Handle));
				tf?.PreventPublicDisposal ();
				return tf;
			}
		}

		public static SKTypeface FromFamilyName (string familyName, SKFontStyleWeight weight, SKFontStyleWidth width, SKFontStyleSlant slant)
		{
			return FromFamilyName (familyName, (int)weight, (int)width, slant);
		}

		// From*

		public static SKTypeface FromFile (string path, int index = 0)
		{
			if (path == null)
				throw new ArgumentNullException (nameof (path));

			var utf8path = StringUtilities.GetEncodedText (path, SKTextEncoding.Utf8, true);
			fixed (byte* u = utf8path) {
				return GetObject (SkiaApi.sk_typeface_create_from_file (u, index));
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

			var typeface = GetObject (SkiaApi.sk_typeface_create_from_stream (stream.Handle, index));
			stream.RevokeOwnership (typeface);
			return typeface;
		}

		public static SKTypeface FromData (SKData data, int index = 0)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));

			return GetObject (SkiaApi.sk_typeface_create_from_data (data.Handle, index));
		}

		// Properties

		public string FamilyName => (string)SKString.GetObject (SkiaApi.sk_typeface_get_family_name (Handle));

		public SKFontStyle FontStyle => SKFontStyle.GetObject (SkiaApi.sk_typeface_get_fontstyle (Handle));

		public int FontWeight => SkiaApi.sk_typeface_get_font_weight (Handle);

		public int FontWidth => SkiaApi.sk_typeface_get_font_width (Handle);

		public SKFontStyleSlant FontSlant => SkiaApi.sk_typeface_get_font_slant (Handle);

		public bool IsBold => FontStyle.Weight >= (int)SKFontStyleWeight.SemiBold;

		public bool IsItalic => FontStyle.Slant != SKFontStyleSlant.Upright;

		public bool IsFixedPitch => SkiaApi.sk_typeface_is_fixed_pitch (Handle);

		public int UnitsPerEm => SkiaApi.sk_typeface_get_units_per_em (Handle);

		public int GlyphCount => SkiaApi.sk_typeface_count_glyphs (Handle);

		// GetTableTags

		public int TableCount => SkiaApi.sk_typeface_count_tables (Handle);

		public UInt32[] GetTableTags ()
		{
			if (!TryGetTableTags (out var result)) {
				throw new Exception ("Unable to read the tables for the file.");
			}
			return result;
		}

		public bool TryGetTableTags (out UInt32[] tags)
		{
			var buffer = new UInt32[TableCount];
			fixed (UInt32* b = buffer) {
				if (SkiaApi.sk_typeface_get_table_tags (Handle, b) == 0) {
					tags = null;
					return false;
				}
			}
			tags = buffer;
			return true;
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
			fixed (byte* b = buffer) {
				if (!TryGetTableData (tag, 0, length, (IntPtr)b)) {
					tableData = null;
					return false;
				}
			}
			tableData = buffer;
			return true;
		}

		public bool TryGetTableData (UInt32 tag, int offset, int length, IntPtr tableData)
		{
			var actual = SkiaApi.sk_typeface_get_table_data (Handle, tag, (IntPtr)offset, (IntPtr)length, (byte*)tableData);
			return actual != IntPtr.Zero;
		}

		// CountGlyphs

		public int CountGlyphs (string str) =>
			GetFont ().CountGlyphs (str);

		public int CountGlyphs (ReadOnlySpan<char> str) =>
			GetFont ().CountGlyphs (str);

		public int CountGlyphs (byte[] str, SKTextEncoding encoding) =>
			GetFont ().CountGlyphs (str, encoding);

		public int CountGlyphs (ReadOnlySpan<byte> str, SKTextEncoding encoding) =>
			GetFont ().CountGlyphs (str, encoding);

		public int CountGlyphs (IntPtr str, int strLen, SKTextEncoding encoding) =>
			GetFont ().CountGlyphs (str, strLen * encoding.GetCharacterByteSize (), encoding);

		// GetGlyph

		public ushort GetGlyph (int codepoint) =>
			GetFont ().GetGlyph (codepoint);

		// GetGlyphs

		public ushort[] GetGlyphs (ReadOnlySpan<int> codepoints) =>
			GetFont ().GetGlyphs (codepoints);

		public ushort[] GetGlyphs (string text) =>
			GetGlyphs (text.AsSpan ());

		public ushort[] GetGlyphs (ReadOnlySpan<char> text) =>
			GetFont ().GetGlyphs (text);

		public ushort[] GetGlyphs (ReadOnlySpan<byte> text, SKTextEncoding encoding) =>
			GetFont ().GetGlyphs (text, encoding);

		public ushort[] GetGlyphs (IntPtr text, int length, SKTextEncoding encoding) =>
			GetFont ().GetGlyphs (text, length * encoding.GetCharacterByteSize (), encoding);

		// ContainsGlyph

		public bool ContainsGlyph (int codepoint) =>
			GetFont ().ContainsGlyph (codepoint);

		// ContainsGlyphs

		public bool ContainsGlyphs (ReadOnlySpan<int> codepoints) =>
			GetFont ().ContainsGlyphs (codepoints);

		public bool ContainsGlyphs (string text) =>
			GetFont ().ContainsGlyphs (text);

		public bool ContainsGlyphs (ReadOnlySpan<char> text) =>
			GetFont ().ContainsGlyphs (text);

		public bool ContainsGlyphs (ReadOnlySpan<byte> text, SKTextEncoding encoding) =>
			GetFont ().ContainsGlyphs (text, encoding);

		public bool ContainsGlyphs (IntPtr text, int length, SKTextEncoding encoding) =>
			GetFont ().ContainsGlyphs (text, length * encoding.GetCharacterByteSize (), encoding);

		// GetFont

		internal SKFont GetFont () =>
			font ??= OwnedBy (new SKFont (this), this);

		// ToFont

		public SKFont ToFont () =>
			new SKFont (this);

		public SKFont ToFont (float size, float scaleX = SKFont.DefaultScaleX, float skewX = SKFont.DefaultSkewX) =>
			new SKFont (this, size, scaleX, skewX);

		// OpenStream

		public SKStreamAsset OpenStream () =>
			OpenStream (out _);

		public SKStreamAsset OpenStream (out int ttcIndex)
		{
			fixed (int* ttc = &ttcIndex) {
				return SKStreamAsset.GetObject (SkiaApi.sk_typeface_open_stream (Handle, ttc));
			}
		}

		// GetKerningPairAdjustments

		/// <summary>
		/// If false, then <see cref="GetKerningPairAdjustments"/> will never return nonzero
		/// adjustments for any possible pair of glyphs.
		/// </summary>
		public bool HasGetKerningPairAdjustments =>
			SkiaApi.sk_typeface_get_kerning_pair_adjustments (Handle, null, 0, null);

		/// <summary>
		/// Gets a kerning adjustment for each sequential pair of glyph indices in <paramref name="glyphs"/>.
		/// </summary>
		/// <param name="glyphs">The sequence of glyph indices to get kerning adjustments for.</param>
		/// <returns>
		/// Adjustments are returned in design units, relative to <see cref="UnitsPerEm"/>.
		/// </returns>
		/// <remarks>
		/// For backwards-compatibility reasons, an additional zero entry is present at the end of the array.
		/// </remarks>
		public int[] GetKerningPairAdjustments (ReadOnlySpan<ushort> glyphs)
		{
			var adjustments = new int[glyphs.Length];
			GetKerningPairAdjustments (glyphs, adjustments);
			return adjustments;
		}

		/// <summary>
		/// Gets a kerning adjustment for each sequential pair of glyph indices in <paramref name="glyphs"/>.
		/// </summary>
		/// <param name="glyphs">The sequence of glyph indices to get kerning adjustments for.</param>
		/// <param name="adjustments">
		/// The span that will hold the output adjustments, one per adjacent pari of <paramref name="glyphs"/>.
		/// Adjustments are returned in design units, relative to <see cref="UnitsPerEm"/>.
		/// This must contain a minimum of glyphs.Length - 1 elements.
		/// </param>
		/// <returns>
		/// True if any kerning pair adjustments were written to <paramref name="adjustments"/>.
		/// False if the typeface does not contain adjustments for any of the given pairs of glyphs.
		/// </returns>
		/// <remarks>
		/// If this function returns false, then the first <paramref name="glyphs"/>.Length - 1 elements of <paramref name="adjustments"/> will be zero.
		/// Elements of <paramref name="adjustments"/> beyond <paramref name="glyphs"/>.Length - 1 will not be modified.
		/// </remarks>
		public bool GetKerningPairAdjustments (ReadOnlySpan<ushort> glyphs, Span<int> adjustments)
		{
			if (adjustments.Length < glyphs.Length - 1)
				throw new ArgumentException ("Length of adjustments must be large enough to hold one adjustment per pair of glyphs (or, glyphs.Length - 1).");

			bool res;
			fixed (ushort* gp = glyphs)
			fixed (int* ap = adjustments) {
				res = SkiaApi.sk_typeface_get_kerning_pair_adjustments (Handle, gp, glyphs.Length, ap);
			}

			if (!res && glyphs.Length > 1)
				//Per SkTypeface::GetKerningPairAdjustments documentation, the method may have written
				//nonsense into the array before bailing. Don't return it to the caller, the doc says
				//such values must be ignored.
				adjustments.Slice(0, glyphs.Length - 1).Clear ();

			return res;
		}

		//

		internal static SKTypeface GetObject (IntPtr handle) =>
			GetOrAddObject (handle, (h, o) => new SKTypeface (h, o));

		//

		private sealed class SKTypefaceStatic : SKTypeface
		{
			internal SKTypefaceStatic (IntPtr x)
				: base (x, false)
			{
			}

			protected override void Dispose (bool disposing) { }
		}
	}
}
