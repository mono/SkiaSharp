#nullable disable

using System;
using System.ComponentModel;
using System.IO;

namespace SkiaSharp
{
	/// <summary>
	/// Represents a specific typeface and intrinsic style of a font.
	/// </summary>
	/// <remarks>
	/// This is used in the paint, along with optionally algorithmic settings like
	/// <see cref="SkiaSharp.SKPaint.TextSize" />,
	/// <see cref="SkiaSharp.SKPaint.TextSkewX" />,
	/// <see cref="SkiaSharp.SKPaint.TextScaleX" />, and
	/// <see cref="SkiaSharp.SKPaint.FakeBoldText" />
	/// to specify how text appears when drawn (and measured).
	/// Typeface objects are immutable, and so they can be shared between threads.
	/// </remarks>
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

		/// <summary>
		/// Gets the default, Normal typeface.
		/// </summary>
		/// <remarks>
		/// This will never be <see langword="null" />.
		/// </remarks>
		public static SKTypeface Default => defaultTypeface;

		/// <summary>
		/// Creates a new <see cref="SKTypeface" /> which is the default, Normal typeface.
		/// </summary>
		/// <remarks>
		/// This will never be null.
		/// </remarks>
		public static SKTypeface CreateDefault ()
		{
			return GetObject (SkiaApi.sk_typeface_create_default ());
		}

		// FromFamilyName

		/// <summary>
		/// Return a new instance to a typeface that most closely matches the requested family name and style.
		/// </summary>
		/// <param name="familyName">The name of the font family. May be <paramref name="null" />.</param>
		/// <param name="weight">The weight of the typeface.</param>
		/// <param name="width">The width of the typeface.</param>
		/// <param name="slant">The slant of the typeface.</param>
		/// <returns>Returns to the closest-matching typeface.</returns>
		public static SKTypeface FromFamilyName (string familyName, int weight, int width, SKFontStyleSlant slant)
		{
			return FromFamilyName (familyName, new SKFontStyle (weight, width, slant));
		}

		/// <summary>
		/// Returns a new instance to a typeface that most closely matches the requested family name and style.
		/// </summary>
		/// <param name="familyName">The name of the font family. May be <paramref name="null" />.</param>
		/// <returns>Returns to the closest-matching typeface.</returns>
		public static SKTypeface FromFamilyName (string familyName)
		{
			return FromFamilyName (familyName, SKFontStyle.Normal);
		}

		/// <summary>
		/// Returns a new instance to a typeface that most closely matches the requested family name and style.
		/// </summary>
		/// <param name="familyName">The name of the font family. May be <paramref name="null" />.</param>
		/// <param name="style">The style (normal, bold, italic) of the typeface.</param>
		/// <returns>Returns to the closest-matching typeface.</returns>
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

		/// <summary>
		/// Return a new instance to a typeface that most closely matches the requested family name and style.
		/// </summary>
		/// <param name="familyName">The name of the font family. May be <paramref name="null" />.</param>
		/// <param name="weight">The weight of the typeface.</param>
		/// <param name="width">The width of the typeface.</param>
		/// <param name="slant">The slant of the typeface.</param>
		/// <returns>Returns to the closest-matching typeface.</returns>
		public static SKTypeface FromFamilyName (string familyName, SKFontStyleWeight weight, SKFontStyleWidth width, SKFontStyleSlant slant)
		{
			return FromFamilyName (familyName, (int)weight, (int)width, slant);
		}

		// From*

		/// <summary>
		/// Returns a new typeface given a file.
		/// </summary>
		/// <param name="path">The path of the file.</param>
		/// <param name="index">The font face index.</param>
		/// <returns>Returns a new typeface, or <paramref name="null" /> if the file does not exist, or is not a valid font file.</returns>
		public static SKTypeface FromFile (string path, int index = 0)
		{
			if (path == null)
				throw new ArgumentNullException (nameof (path));

			var utf8path = StringUtilities.GetEncodedText (path, SKTextEncoding.Utf8, true);
			fixed (byte* u = utf8path) {
				return GetObject (SkiaApi.sk_typeface_create_from_file (u, index));
			}
		}

		/// <summary>
		/// Returns a new typeface given a stream. Ownership of the stream is transferred, so the caller must not reference it again.
		/// </summary>
		/// <param name="stream">The input stream.</param>
		/// <param name="index">The font face index.</param>
		/// <returns>Returns a new typeface, or <paramref name="null" /> if the file does not exist, or is not a valid font file.</returns>
		public static SKTypeface FromStream (Stream stream, int index = 0)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			return FromStream (new SKManagedStream (stream, true), index);
		}

		/// <summary>
		/// Returns a new typeface given a stream. Ownership of the stream is transferred, so the caller must not reference it again.
		/// </summary>
		/// <param name="stream">The input stream.</param>
		/// <param name="index">The font face index.</param>
		/// <returns>Returns a new typeface, or <paramref name="null" /> if the file does not exist, or is not a valid font file.</returns>
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

		/// <summary>
		/// Returns a new typeface given data.
		/// </summary>
		/// <param name="data">The input data.</param>
		/// <param name="index">The font face index.</param>
		/// <returns>Returns a new typeface, or <paramref name="null" /> if the file does not exist, or is not a valid font file.</returns>
		public static SKTypeface FromData (SKData data, int index = 0)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));

			return GetObject (SkiaApi.sk_typeface_create_from_data (data.Handle, index));
		}

		// Properties

		/// <summary>
		/// Gets the family name for the typeface.
		/// </summary>
		/// <remarks>
		/// The family name will always be returned encoded as UTF8, but the language of the name is whatever the host platform chooses.
		/// </remarks>
		public string FamilyName => (string)SKString.GetObject (SkiaApi.sk_typeface_get_family_name (Handle));

		/// <summary>
		/// Gets the font style for the typeface.
		/// </summary>
		public SKFontStyle FontStyle => SKFontStyle.GetObject (SkiaApi.sk_typeface_get_fontstyle (Handle));

		/// <summary>
		/// Gets the font weight for the typeface.
		/// </summary>
		/// <remarks>
		/// This may be one of the values in <see cref="SKFontStyleWeight" />.
		/// </remarks>
		public int FontWeight => SkiaApi.sk_typeface_get_font_weight (Handle);

		/// <summary>
		/// Gets the font width for the typeface.
		/// </summary>
		/// <remarks>
		/// This may be one of the values in <see cref="SKFontStyleWidth" />.
		/// </remarks>
		public int FontWidth => SkiaApi.sk_typeface_get_font_width (Handle);

		/// <summary>
		/// Gets the font slant for the typeface.
		/// </summary>
		/// <remarks>
		/// This may be one of the values in <see cref="SKFontStyleSlant" />.
		/// </remarks>
		public SKFontStyleSlant FontSlant => SkiaApi.sk_typeface_get_font_slant (Handle);

		/// <summary>
		/// Gets a value indicating whether the typeface claims to be a bold typeface.
		/// </summary>
		/// <remarks>
		/// A typeface is understood to be bold when the weight is greater than or equal to
		/// 600 or <see cref="SkiaSharp.SKFontStyleWeight.SemiBold" />.
		/// </remarks>
		public bool IsBold => FontStyle.Weight >= (int)SKFontStyleWeight.SemiBold;

		/// <summary>
		/// Gets a value indicating whether the typeface claims to be slanted.
		/// </summary>
		/// <remarks>
		/// A typeface is understood to be italic when it has a slant of either
		/// <see cref="SkiaSharp.SKFontStyleSlant.Italic" /> or
		/// <see cref="SkiaSharp.SKFontStyleSlant.Oblique" />.
		/// </remarks>
		public bool IsItalic => FontStyle.Slant != SKFontStyleSlant.Upright;

		/// <summary>
		/// Gets a value indicating whether the typeface claims to be fixed-pitch.
		/// </summary>
		/// <remarks>
		/// This does not guarentee that the advance widths will not vary as this is a
		/// style bit on the typeface.
		/// </remarks>
		public bool IsFixedPitch => SkiaApi.sk_typeface_is_fixed_pitch (Handle);

		/// <summary>
		/// Gets the units-per-em value for this typeface, or zero if there is an error.
		/// </summary>
		public int UnitsPerEm => SkiaApi.sk_typeface_get_units_per_em (Handle);

		public int GlyphCount => SkiaApi.sk_typeface_count_glyphs (Handle);

		public string PostScriptName => (string)SKString.GetObject (SkiaApi.sk_typeface_get_post_script_name (Handle));

		// GetTableTags

		/// <summary>
		/// Gets the number of data tables in the typeface.
		/// </summary>
		public int TableCount => SkiaApi.sk_typeface_count_tables (Handle);

		/// <summary>
		/// Returns the list of table tags in the font.
		/// </summary>
		public UInt32[] GetTableTags ()
		{
			if (!TryGetTableTags (out var result)) {
				throw new Exception ("Unable to read the tables for the file.");
			}
			return result;
		}

		/// <summary>
		/// Returns the list of table tags in the font.
		/// </summary>
		/// <param name="tags">The table tags.</param>
		/// <returns>Returns true if the tags could be fetched, otherwise false.</returns>
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

		/// <summary>
		/// Returns the size of the data for the specified tag.
		/// </summary>
		/// <param name="tag">The tag to retrieve.</param>
		/// <returns>Returns the size of the data.</returns>
		public int GetTableSize (UInt32 tag) =>
			(int)SkiaApi.sk_typeface_get_table_size (Handle, tag);

		// GetTableData

		/// <summary>
		/// Returns the contents of the table data for the specified tag.
		/// </summary>
		/// <param name="tag">The table tag to get the data for.</param>
		/// <returns>Returns the contents, if it exists, otherwise throws.</returns>
		public byte[] GetTableData (UInt32 tag)
		{
			if (!TryGetTableData (tag, out var result)) {
				throw new Exception ("Unable to read the data table.");
			}
			return result;
		}

		/// <summary>
		/// Returns the contents of the table data for the specified tag.
		/// </summary>
		/// <param name="tag">The table tag to get the data for.</param>
		/// <param name="tableData">The contents of the table data for the specified tag.</param>
		/// <returns>Returns true if the content exists, otherwise false.</returns>
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

		/// <summary>
		/// Returns the contents of the table data for the specified tag.
		/// </summary>
		/// <param name="tag">The table tag to get the data for.</param>
		/// <param name="offset">The offset of the data to fetch.</param>
		/// <param name="length">The length of data to fetch.</param>
		/// <param name="tableData">The contents of the table data for the specified tag.</param>
		/// <returns>Returns true if the content exists, otherwise false.</returns>
		public bool TryGetTableData (UInt32 tag, int offset, int length, IntPtr tableData)
		{
			var actual = SkiaApi.sk_typeface_get_table_data (Handle, tag, (IntPtr)offset, (IntPtr)length, (byte*)tableData);
			return actual != IntPtr.Zero;
		}

		// CountGlyphs

		/// <summary>
		/// Returns the number of glyphs in the string.
		/// </summary>
		/// <param name="str">The string containing characters.</param>
		/// <returns>The number of number of continuous non-zero glyph IDs computed from the beginning of string.</returns>
		public int CountGlyphs (string str) =>
			GetFont ().CountGlyphs (str);

		/// <param name="str"></param>
		public int CountGlyphs (ReadOnlySpan<char> str) =>
			GetFont ().CountGlyphs (str);

		/// <param name="str"></param>
		/// <param name="encoding"></param>
		public int CountGlyphs (byte[] str, SKTextEncoding encoding) =>
			GetFont ().CountGlyphs (str, encoding);

		/// <param name="str"></param>
		/// <param name="encoding"></param>
		public int CountGlyphs (ReadOnlySpan<byte> str, SKTextEncoding encoding) =>
			GetFont ().CountGlyphs (str, encoding);

		/// <param name="str"></param>
		/// <param name="strLen"></param>
		/// <param name="encoding"></param>
		public int CountGlyphs (IntPtr str, int strLen, SKTextEncoding encoding) =>
			GetFont ().CountGlyphs (str, strLen * encoding.GetCharacterByteSize (), encoding);

		// GetGlyph

		/// <param name="codepoint"></param>
		public ushort GetGlyph (int codepoint) =>
			GetFont ().GetGlyph (codepoint);

		// GetGlyphs

		/// <param name="codepoints"></param>
		public ushort[] GetGlyphs (ReadOnlySpan<int> codepoints) =>
			GetFont ().GetGlyphs (codepoints);

		/// <summary>
		/// Retrieve the corresponding glyph IDs of a string of characters.
		/// </summary>
		/// <param name="text">The string of characters.</param>
		/// <returns>Returns the corresponding glyph IDs for each character.</returns>
		public ushort[] GetGlyphs (string text) =>
			GetGlyphs (text.AsSpan ());

		/// <param name="text"></param>
		public ushort[] GetGlyphs (ReadOnlySpan<char> text)
		{
			using var font = ToFont ();
			return font.GetGlyphs (text);
		}

		/// <param name="text"></param>
		/// <param name="encoding"></param>
		public ushort[] GetGlyphs (ReadOnlySpan<byte> text, SKTextEncoding encoding)
		{
			using var font = ToFont ();
			return font.GetGlyphs (text, encoding);
		}

		/// <param name="text"></param>
		/// <param name="length"></param>
		/// <param name="encoding"></param>
		public ushort[] GetGlyphs (IntPtr text, int length, SKTextEncoding encoding)
		{
			using var font = ToFont ();
			return font.GetGlyphs (text, length * encoding.GetCharacterByteSize (), encoding);
		}

		// ContainsGlyph

		/// <param name="codepoint"></param>
		public bool ContainsGlyph (int codepoint) =>
			GetFont ().ContainsGlyph (codepoint);

		// ContainsGlyphs

		/// <param name="codepoints"></param>
		public bool ContainsGlyphs (ReadOnlySpan<int> codepoints) =>
			GetFont ().ContainsGlyphs (codepoints);

		/// <param name="text"></param>
		public bool ContainsGlyphs (string text) =>
			GetFont ().ContainsGlyphs (text);

		/// <param name="text"></param>
		public bool ContainsGlyphs (ReadOnlySpan<char> text) =>
			GetFont ().ContainsGlyphs (text);

		/// <param name="text"></param>
		/// <param name="encoding"></param>
		public bool ContainsGlyphs (ReadOnlySpan<byte> text, SKTextEncoding encoding) =>
			ContainsGlyphs (text, encoding);

		/// <param name="text"></param>
		/// <param name="length"></param>
		/// <param name="encoding"></param>
		public bool ContainsGlyphs (IntPtr text, int length, SKTextEncoding encoding) =>
			GetFont ().ContainsGlyphs (text, length * encoding.GetCharacterByteSize (), encoding);

		// GetFont

		internal SKFont GetFont () =>
			font ??= OwnedBy (new SKFont (this), this);

		// ToFont

		public SKFont ToFont () =>
			new SKFont (this);

		/// <param name="size"></param>
		/// <param name="scaleX"></param>
		/// <param name="skewX"></param>
		public SKFont ToFont (float size, float scaleX = SKFont.DefaultScaleX, float skewX = SKFont.DefaultSkewX) =>
			new SKFont (this, size, scaleX, skewX);

		// OpenStream

		/// <summary>
		/// Returns a stream for the contents of the font data.
		/// </summary>
		/// <returns>Returns a stream for the contents of the font data, or <see langword="null" /> on failure.</returns>
		/// <remarks>
		/// The caller is responsible for deleting the stream.
		/// </remarks>
		public SKStreamAsset OpenStream () =>
			OpenStream (out _);

		/// <summary>
		/// Returns a stream for the contents of the font data.
		/// </summary>
		/// <param name="ttcIndex">The TrueTypeCollection index of this typeface within the stream, or 0 if the stream is not a collection.</param>
		/// <returns>Returns a stream for the contents of the font data, or <see langword="null" /> on failure.</returns>
		/// <remarks>
		/// The caller is responsible for deleting the stream.
		/// </remarks>
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
