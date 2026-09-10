#nullable disable

using System;
using System.ComponentModel;
using System.IO;
using System.Threading;

namespace SkiaSharp
{
	/// <summary>Represents a specific typeface and intrinsic style of a font.</summary>
	/// <remarks><format type="text/markdown"><![CDATA[
	/// ## Remarks
	///
	/// This is used in the font, along with optionally algorithmic settings like
	/// <xref:SkiaSharp.SKFont.Size?displayProperty=nameWithType>,
	/// <xref:SkiaSharp.SKFont.SkewX?displayProperty=nameWithType>,
	/// <xref:SkiaSharp.SKFont.ScaleX?displayProperty=nameWithType>, and
	/// <xref:SkiaSharp.SKFont.Embolden?displayProperty=nameWithType>
	/// to specify how text appears when drawn (and measured).
	///
	/// Typeface objects are immutable, and so they can be shared between threads.
	/// ]]></format></remarks>
	public unsafe class SKTypeface : SKObject, ISKReferenceCounted
	{
		private static SKTypeface empty;
		private static bool emptyInitialized;
		private static object emptyLock = new object ();

		private static SKTypeface defaultTypeface;
		private static bool defaultTypefaceInitialized;
		private static object defaultTypefaceLock = new object ();

		private SKFont font;

		internal SKTypeface (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		// Default

		/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.SKTypeface" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:SkiaSharp.SKTypeface" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		/// <summary>Gets the default, Normal typeface.</summary>
		/// <value>The default typeface.</value>
		/// <remarks>This will never be <see langword="null" />.</remarks>
		public static SKTypeface Default =>
			LazyInitializer.EnsureInitialized (
				ref defaultTypeface, ref defaultTypefaceInitialized, ref defaultTypefaceLock,
				() => {
					// Use legacyMakeTypeface(null) to get the platform default — this uses
					// fDefaultStyleSet on Android (which searches "sans-serif", "Roboto",
					// then falls back to style set 0). matchFamilyStyle(null) doesn't work
					// on Android/NDK/Custom because onMatchFamily(null) returns null.
					var matched = SkiaApi.sk_fontmgr_legacy_create_typeface (
						SKFontManager.Default.Handle, IntPtr.Zero, SKFontStyle.Normal.Handle);
					return matched == IntPtr.Zero ? Empty : GetDisposeProtectedObject (matched);
				});

		/// <summary>Gets a shared empty <see cref="T:SkiaSharp.SKTypeface" /> instance.</summary>
		/// <value>A shared <see cref="T:SkiaSharp.SKTypeface" /> instance that represents an empty typeface.</value>
		/// <remarks />
		public static SKTypeface Empty =>
			LazyInitializer.EnsureInitialized (
				ref empty, ref emptyInitialized, ref emptyLock,
				// Immortal Skia singleton (SkNoDestructor<SkEmptyTypeface>) — never unref it.
				// See SKColorFilter.GetDisposeProtectedObject for the full teardown-crash rationale.
				() => GetDisposeProtectedObject (SkiaApi.sk_typeface_create_empty (), owns: false, unrefExisting: false));

		/// <summary>Gets a value indicating whether this typeface is the empty typeface.</summary>
		/// <value><see langword="true" /> if this typeface is the empty typeface; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool IsEmpty => GlyphCount == 0;

		/// <summary>Creates a new <see cref="T:SkiaSharp.SKTypeface" /> which is the default, Normal typeface.</summary>
		/// <returns>The default typeface.</returns>
		/// <remarks>This will never be <see langword="null" />.</remarks>
		public static SKTypeface CreateDefault ()
		{
			var matched = SkiaApi.sk_fontmgr_legacy_create_typeface (
				SKFontManager.Default.Handle, IntPtr.Zero, SKFontStyle.Normal.Handle);
			return matched == IntPtr.Zero
				? Empty
				: GetObject (matched);
		}

		// FromFamilyName

		/// <summary>Returns a new instance to a typeface that most closely matches the requested family name and style.</summary>
		/// <param name="familyName">The name of the font family. May be <see langword="null" />.</param>
		/// <param name="weight">The weight of the typeface.</param>
		/// <param name="width">The width of the typeface.</param>
		/// <param name="slant">The slant of the typeface.</param>
		/// <returns>Returns to the closest-matching typeface.</returns>
		/// <remarks />
		public static SKTypeface FromFamilyName (string familyName, int weight, int width, SKFontStyleSlant slant)
		{
			return FromFamilyName (familyName, new SKFontStyle (weight, width, slant));
		}

		/// <summary>Returns a new instance to a typeface that most closely matches the requested family name and style.</summary>
		/// <param name="familyName">The name of the font family. May be <see langword="null" />.</param>
		/// <returns>Returns to the closest-matching typeface.</returns>
		/// <remarks />
		public static SKTypeface FromFamilyName (string familyName)
		{
			return FromFamilyName (familyName, SKFontStyle.Normal);
		}

		/// <summary>Returns a new instance to a typeface that most closely matches the requested family name and style.</summary>
		/// <param name="familyName">The name of the font family. May be <see langword="null" />.</param>
		/// <param name="style">The style (normal, bold, italic) of the typeface.</param>
		/// <returns>Returns to the closest-matching typeface.</returns>
		/// <remarks />
		public static SKTypeface FromFamilyName (string familyName, SKFontStyle style) =>
			SKFontManager.Default.MatchFamily (familyName, style) ?? Default;

		/// <summary>Returns a new instance to a typeface that most closely matches the requested family name and style.</summary>
		/// <param name="familyName">The name of the font family. May be <see langword="null" />.</param>
		/// <param name="weight">The weight of the typeface.</param>
		/// <param name="width">The width of the typeface.</param>
		/// <param name="slant">The slant of the typeface.</param>
		/// <returns>Returns to the closest-matching typeface.</returns>
		/// <remarks />
		public static SKTypeface FromFamilyName (string familyName, SKFontStyleWeight weight, SKFontStyleWidth width, SKFontStyleSlant slant)
		{
			return FromFamilyName (familyName, (int)weight, (int)width, slant);
		}

		// From*

		/// <summary>Returns a new typeface given a file.</summary>
		/// <param name="path">The path of the file.</param>
		/// <param name="index">The font face index.</param>
		/// <returns>Returns a new typeface, or <see langword="null" /> if the file does not exist, or is not a valid font file.</returns>
		/// <remarks />
		public static SKTypeface FromFile (string path, int index = 0) =>
			SKFontManager.Default.CreateTypeface (path, index);

		/// <summary>Returns a new typeface given a stream. Ownership of the stream is transferred, so the caller must not reference it again.</summary>
		/// <param name="stream">The input stream.</param>
		/// <param name="index">The font face index.</param>
		/// <returns>Returns a new typeface, or <see langword="null" /> if the file does not exist, or is not a valid font file.</returns>
		/// <remarks />
		public static SKTypeface FromStream (Stream stream, int index = 0) =>
			SKFontManager.Default.CreateTypeface (stream, index);

		/// <summary>Returns a new typeface given a stream. Ownership of the stream is transferred, so the caller must not reference it again.</summary>
		/// <param name="stream">The input stream.</param>
		/// <param name="index">The font face index.</param>
		/// <returns>Returns a new typeface, or <see langword="null" /> if the file does not exist, or is not a valid font file.</returns>
		/// <remarks />
		public static SKTypeface FromStream (SKStreamAsset stream, int index = 0) =>
			SKFontManager.Default.CreateTypeface (stream, index);

		/// <summary>Returns a new typeface given data.</summary>
		/// <param name="data">The input data.</param>
		/// <param name="index">The font face index.</param>
		/// <returns>Returns a new typeface, or <see langword="null" /> if the file does not exist, or is not a valid font file.</returns>
		/// <remarks />
		public static SKTypeface FromData (SKData data, int index = 0) =>
			SKFontManager.Default.CreateTypeface (data, index);

		// Properties

		/// <summary>Gets the family name for the typeface.</summary>
		/// <value>The family name for the typeface.</value>
		/// <remarks>The family name will always be returned encoded as UTF8, but the language of the name is whatever the host platform chooses.</remarks>
		public string FamilyName {
			get {
				var r = (string)SKString.GetObject (SkiaApi.sk_typeface_get_family_name (Handle));
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Gets the font style for the typeface.</summary>
		/// <value>The font style for the typeface.</value>
		/// <remarks />
		public SKFontStyle FontStyle {
			get {
				var r = SKFontStyle.GetObject (SkiaApi.sk_typeface_get_fontstyle (Handle));
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Gets the font weight for the typeface.</summary>
		/// <value>The font weight for the typeface.</value>
		/// <remarks>This may be one of the values in <see cref="T:SkiaSharp.SKFontStyleWeight" />.</remarks>
		public int FontWeight {
			get {
				var r = SkiaApi.sk_typeface_get_font_weight (Handle);
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Gets the font width for the typeface.</summary>
		/// <value>The font width for the typeface.</value>
		/// <remarks>This may be one of the values in <see cref="T:SkiaSharp.SKFontStyleWidth" />.</remarks>
		public int FontWidth {
			get {
				var r = SkiaApi.sk_typeface_get_font_width (Handle);
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Gets the font slant for the typeface.</summary>
		/// <value>One of the enumeration values that specifies the font slant.</value>
		/// <remarks>This may be one of the values in <see cref="T:SkiaSharp.SKFontStyleSlant" />.</remarks>
		public SKFontStyleSlant FontSlant {
			get {
				var r = SkiaApi.sk_typeface_get_font_slant (Handle);
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Gets a value indicating whether the typeface claims to be a bold typeface.</summary>
		/// <value><see langword="true" /> if the typeface is bold; otherwise, <see langword="false" />.</value>
		/// <remarks><format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// A typeface is understood to be bold when the weight is greater than or equal to
		/// 600 or <xref:SkiaSharp.SKFontStyleWeight.SemiBold>.
		/// ]]></format></remarks>
		public bool IsBold => FontStyle.Weight >= (int)SKFontStyleWeight.SemiBold;

		/// <summary>Gets a value indicating whether the typeface claims to be slanted.</summary>
		/// <value><see langword="true" /> if the typeface is italic or oblique; otherwise, <see langword="false" />.</value>
		/// <remarks><format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// A typeface is understood to be italic when it has a slant of either
		/// <xref:SkiaSharp.SKFontStyleSlant.Italic> or
		/// <xref:SkiaSharp.SKFontStyleSlant.Oblique>.
		/// ]]></format></remarks>
		public bool IsItalic => FontStyle.Slant != SKFontStyleSlant.Upright;

		/// <summary>Gets a value indicating whether the typeface claims to be fixed-pitch.</summary>
		/// <value><see langword="true" /> if the typeface is fixed-pitch; otherwise, <see langword="false" />.</value>
		/// <remarks><format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// This does not guarentee that the advance widths will not vary as this is a
		/// style bit on the typeface.
		/// ]]></format></remarks>
		public bool IsFixedPitch {
			get {
				var r = SkiaApi.sk_typeface_is_fixed_pitch (Handle);
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Gets the units-per-em value for this typeface, or zero if there is an error.</summary>
		/// <value>The units-per-em value, or zero if there is an error.</value>
		/// <remarks />
		public int UnitsPerEm {
			get {
				var r = SkiaApi.sk_typeface_get_units_per_em (Handle);
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Gets the number of glyphs in this typeface.</summary>
		/// <value>The total number of glyphs in the typeface.</value>
		/// <remarks />
		public int GlyphCount {
			get {
				var r = SkiaApi.sk_typeface_count_glyphs (Handle);
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Gets the PostScript name of this typeface.</summary>
		/// <value>The PostScript name, or <see langword="null" /> if the typeface does not have one.</value>
		/// <remarks />
		public string PostScriptName {
			get {
				var r = (string)SKString.GetObject (SkiaApi.sk_typeface_get_post_script_name (Handle));
				GC.KeepAlive (this);
				return r;
			}
		}

		// GetTableTags

		/// <summary>Gets the number of data tables in the typeface.</summary>
		/// <value>The number of data tables.</value>
		/// <remarks />
		public int TableCount {
			get {
				var r = SkiaApi.sk_typeface_count_tables (Handle);
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Returns the list of table tags in the font.</summary>
		/// <returns>An array of table tags.</returns>
		/// <remarks />
		public UInt32[] GetTableTags ()
		{
			if (!TryGetTableTags (out var result)) {
				throw new Exception ("Unable to read the tables for the file.");
			}
			return result;
		}

		/// <summary>Returns the list of table tags in the font.</summary>
		/// <param name="tags">The table tags.</param>
		/// <returns><see langword="true" /> if the tags could be fetched; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool TryGetTableTags (out UInt32[] tags)
		{
			var buffer = new UInt32[TableCount];
			fixed (UInt32* b = buffer) {
				if (SkiaApi.sk_typeface_get_table_tags (Handle, b) == 0) {
					GC.KeepAlive (this);
					tags = null;
					return false;
				}
				GC.KeepAlive (this);
			}
			tags = buffer;
			return true;
		}

		// GetTableSize

		/// <summary>Returns the size of the data for the specified tag.</summary>
		/// <param name="tag">The tag to retrieve.</param>
		/// <returns>Returns the size of the data.</returns>
		/// <remarks />
		public int GetTableSize (UInt32 tag)
		{
			var r = (int)SkiaApi.sk_typeface_get_table_size (Handle, tag);
			GC.KeepAlive (this);
			return r;
		}

		// GetTableData

		/// <summary>Returns the contents of the table data for the specified tag.</summary>
		/// <param name="tag">The table tag to get the data for.</param>
		/// <returns>Returns the contents, if it exists, otherwise throws.</returns>
		/// <remarks />
		public byte[] GetTableData (UInt32 tag)
		{
			if (!TryGetTableData (tag, out var result)) {
				throw new Exception ("Unable to read the data table.");
			}
			return result;
		}

		/// <summary>Returns the contents of the table data for the specified tag.</summary>
		/// <param name="tag">The table tag to get the data for.</param>
		/// <param name="tableData">The contents of the table data for the specified tag.</param>
		/// <returns><see langword="true" /> if the content exists; otherwise, <see langword="false" />.</returns>
		/// <remarks />
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

		/// <summary>Returns the contents of the table data for the specified tag.</summary>
		/// <param name="tag">The table tag to get the data for.</param>
		/// <param name="offset">The offset of the data to fetch.</param>
		/// <param name="length">The length of data to fetch.</param>
		/// <param name="tableData">The contents of the table data for the specified tag.</param>
		/// <returns><see langword="true" /> if the content exists; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool TryGetTableData (UInt32 tag, int offset, int length, IntPtr tableData)
		{
			var actual = SkiaApi.sk_typeface_get_table_data (Handle, tag, (IntPtr)offset, (IntPtr)length, (byte*)tableData);
			GC.KeepAlive (this);
			return actual != IntPtr.Zero;
		}

		// CountGlyphs

		/// <summary>Returns the number of glyphs in the string.</summary>
		/// <param name="str">The string containing characters.</param>
		/// <returns>The number of number of continuous non-zero glyph IDs computed from the beginning of string.</returns>
		/// <remarks />
		[Obsolete ("Use SKFont directly instead.")]
		public int CountGlyphs (string str) =>
			GetFont ().CountGlyphs (str);

		/// <summary>Returns the number of glyphs in the text.</summary>
		/// <param name="str">The text containing characters.</param>
		/// <returns>The number of continuous non-zero glyph IDs computed from the beginning of the text.</returns>
		/// <remarks />
		[Obsolete ("Use SKFont directly instead.")]
		public int CountGlyphs (ReadOnlySpan<char> str) =>
			GetFont ().CountGlyphs (str);

		/// <summary>Returns the number of glyphs in the buffer.</summary>
		/// <param name="str">The buffer containing character codes.</param>
		/// <param name="encoding">The encoding of the character codes.</param>
		/// <returns>The number of continuous non-zero glyph IDs computed from the beginning of the buffer.</returns>
		/// <remarks />
		[Obsolete ("Use SKFont directly instead.")]
		public int CountGlyphs (byte[] str, SKTextEncoding encoding) =>
			GetFont ().CountGlyphs (str, encoding);

		/// <summary>Returns the number of glyphs in the buffer.</summary>
		/// <param name="str">The buffer containing character codes.</param>
		/// <param name="encoding">The encoding of the character codes.</param>
		/// <returns>The number of continuous non-zero glyph IDs computed from the beginning of the buffer.</returns>
		/// <remarks />
		[Obsolete ("Use SKFont directly instead.")]
		public int CountGlyphs (ReadOnlySpan<byte> str, SKTextEncoding encoding) =>
			GetFont ().CountGlyphs (str, encoding);

		/// <summary>Returns the number of glyphs in the specified buffer.</summary>
		/// <param name="str">The pointer to the buffer containing character codes.</param>
		/// <param name="strLen">The length of the buffer in bytes.</param>
		/// <param name="encoding">The encoding of the character codes.</param>
		/// <returns>The number of continuous non-zero glyph IDs computed from the beginning of the buffer.</returns>
		/// <remarks />
		[Obsolete ("Use SKFont directly instead.")]
		public int CountGlyphs (IntPtr str, int strLen, SKTextEncoding encoding) =>
			GetFont ().CountGlyphs (str, strLen * encoding.GetCharacterByteSize (), encoding);

		// GetGlyph

		/// <summary>Returns the glyph ID for the specified Unicode codepoint.</summary>
		/// <param name="codepoint">The Unicode codepoint.</param>
		/// <returns>The glyph ID, or 0 if the typeface does not contain a glyph for this codepoint.</returns>
		/// <remarks />
		[Obsolete ("Use SKFont directly instead.")]
		public ushort GetGlyph (int codepoint) =>
			GetFont ().GetGlyph (codepoint);

		// GetGlyphs

		/// <summary>Returns the glyph IDs for the specified Unicode codepoints.</summary>
		/// <param name="codepoints">The Unicode codepoints.</param>
		/// <returns>The corresponding glyph IDs for each codepoint.</returns>
		/// <remarks />
		[Obsolete ("Use SKFont directly instead.")]
		public ushort[] GetGlyphs (ReadOnlySpan<int> codepoints) =>
			GetFont ().GetGlyphs (codepoints);

		/// <summary>Retrieve the corresponding glyph IDs of a string of characters.</summary>
		/// <param name="text">The string of characters.</param>
		/// <returns>Returns the corresponding glyph IDs for each character.</returns>
		/// <remarks />
		public ushort[] GetGlyphs (string text) =>
			GetGlyphs (text.AsSpan ());

		/// <summary>Returns the glyph IDs for the specified text.</summary>
		/// <param name="text">The text containing characters.</param>
		/// <returns>The corresponding glyph IDs for each character.</returns>
		/// <remarks />
		public ushort[] GetGlyphs (ReadOnlySpan<char> text)
		{
			using var font = ToFont ();
			return font.GetGlyphs (text);
		}

		/// <summary>Returns the glyph IDs for the specified buffer of character codes.</summary>
		/// <param name="text">The buffer containing character codes.</param>
		/// <param name="encoding">The encoding of the character codes.</param>
		/// <returns>The corresponding glyph IDs for each character.</returns>
		/// <remarks />
		public ushort[] GetGlyphs (ReadOnlySpan<byte> text, SKTextEncoding encoding)
		{
			using var font = ToFont ();
			return font.GetGlyphs (text, encoding);
		}

		/// <summary>Returns the glyph IDs for the specified buffer of character codes.</summary>
		/// <param name="text">The pointer to the buffer containing character codes.</param>
		/// <param name="length">The length of the buffer in bytes.</param>
		/// <param name="encoding">The encoding of the character codes.</param>
		/// <returns>The corresponding glyph IDs for each character.</returns>
		/// <remarks />
		public ushort[] GetGlyphs (IntPtr text, int length, SKTextEncoding encoding)
		{
			using var font = ToFont ();
			return font.GetGlyphs (text, length * encoding.GetCharacterByteSize (), encoding);
		}

		// ContainsGlyph

		/// <summary>Determines whether this typeface contains a glyph for the specified Unicode codepoint.</summary>
		/// <param name="codepoint">The Unicode codepoint to check.</param>
		/// <returns>Returns <see langword="true" /> if this typeface contains a glyph for the codepoint; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		[Obsolete ("Use SKFont directly instead.")]
		public bool ContainsGlyph (int codepoint) =>
			GetFont ().ContainsGlyph (codepoint);

		// ContainsGlyphs

		/// <summary>Determines whether this typeface contains glyphs for all the specified Unicode codepoints.</summary>
		/// <param name="codepoints">The Unicode codepoints to check.</param>
		/// <returns>Returns <see langword="true" /> if this typeface contains glyphs for all codepoints; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		[Obsolete ("Use SKFont directly instead.")]
		public bool ContainsGlyphs (ReadOnlySpan<int> codepoints) =>
			GetFont ().ContainsGlyphs (codepoints);

		/// <summary>Determines whether this typeface contains glyphs for all characters in the specified text.</summary>
		/// <param name="text">The text to check.</param>
		/// <returns>Returns <see langword="true" /> if this typeface contains glyphs for all characters; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		[Obsolete ("Use SKFont directly instead.")]
		public bool ContainsGlyphs (string text) =>
			GetFont ().ContainsGlyphs (text);

		/// <summary>Determines whether this typeface contains glyphs for all characters in the specified text.</summary>
		/// <param name="text">The text to check.</param>
		/// <returns>Returns <see langword="true" /> if this typeface contains glyphs for all characters; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		[Obsolete ("Use SKFont directly instead.")]
		public bool ContainsGlyphs (ReadOnlySpan<char> text) =>
			GetFont ().ContainsGlyphs (text);

		/// <summary>Determines whether this typeface contains glyphs for all characters in the specified buffer.</summary>
		/// <param name="text">The buffer containing character codes.</param>
		/// <param name="encoding">The encoding of the character codes.</param>
		/// <returns>Returns <see langword="true" /> if this typeface contains glyphs for all characters; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		[Obsolete ("Use SKFont directly instead.")]
		public bool ContainsGlyphs (ReadOnlySpan<byte> text, SKTextEncoding encoding) =>
			GetFont ().ContainsGlyphs (text, encoding);

		/// <summary>Determines whether this typeface contains glyphs for all characters in the specified buffer.</summary>
		/// <param name="text">The pointer to the buffer containing character codes.</param>
		/// <param name="length">The length of the buffer in bytes.</param>
		/// <param name="encoding">The encoding of the character codes.</param>
		/// <returns>Returns <see langword="true" /> if this typeface contains glyphs for all characters; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		[Obsolete ("Use SKFont directly instead.")]
		public bool ContainsGlyphs (IntPtr text, int length, SKTextEncoding encoding) =>
			GetFont ().ContainsGlyphs (text, length * encoding.GetCharacterByteSize (), encoding);

		// GetFont

		[Obsolete ("Use SKFont directly instead.")]
		internal SKFont GetFont () =>
			font ??= OwnedBy (new SKFont (this), this);

		// ToFont

		/// <summary>Creates a new <see cref="T:SkiaSharp.SKFont" /> from this typeface with default settings.</summary>
		/// <returns>A new <see cref="T:SkiaSharp.SKFont" /> instance using this typeface.</returns>
		/// <remarks />
		public SKFont ToFont () =>
			new SKFont (this);

		/// <summary>Creates a new <see cref="T:SkiaSharp.SKFont" /> from this typeface with the specified size and style parameters.</summary>
		/// <param name="size">The font size in points.</param>
		/// <param name="scaleX">The horizontal scale factor.</param>
		/// <param name="skewX">The horizontal skew factor for synthetic italic.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKFont" /> instance using this typeface.</returns>
		/// <remarks />
		public SKFont ToFont (float size, float scaleX = SKFont.DefaultScaleX, float skewX = SKFont.DefaultSkewX) =>
			new SKFont (this, size, scaleX, skewX);

		// OpenStream

		/// <summary>Returns a stream for the contents of the font data.</summary>
		/// <returns>Returns a stream for the contents of the font data, or <see langword="null" /> on failure.</returns>
		/// <remarks>The caller is responsible for deleting the stream.</remarks>
		public SKStreamAsset OpenStream () =>
			OpenStream (out _);

		/// <summary>Returns a stream for the contents of the font data.</summary>
		/// <param name="ttcIndex">The TrueTypeCollection index of this typeface within the stream, or 0 if the stream is not a collection.</param>
		/// <returns>Returns a stream for the contents of the font data, or <see langword="null" /> on failure.</returns>
		/// <remarks>The caller is responsible for deleting the stream.</remarks>
		public SKStreamAsset OpenStream (out int ttcIndex)
		{
			fixed (int* ttc = &ttcIndex) {
				var r = SKStreamAsset.GetObject (SkiaApi.sk_typeface_open_stream (Handle, ttc));
				GC.KeepAlive (this);
				return r;
			}
		}

		// GetKerningPairAdjustments

		/// <summary>Gets a value indicating whether this typeface supports retrieving kerning pair adjustments.</summary>
		/// <value>Returns <see langword="true" /> if kerning pair adjustments can be retrieved; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool HasGetKerningPairAdjustments {
			get {
				var r = SkiaApi.sk_typeface_get_kerning_pair_adjustments (Handle, null, 0, null);
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Returns kerning pair adjustments for the specified glyphs.</summary>
		/// <param name="glyphs">The glyph IDs to get kerning adjustments for.</param>
		/// <returns>An array of kerning adjustments, one less than the number of glyphs.</returns>
		/// <remarks />
		public int[] GetKerningPairAdjustments (ReadOnlySpan<ushort> glyphs)
		{
			var adjustments = new int[glyphs.Length];
			GetKerningPairAdjustments (glyphs, adjustments);
			return adjustments;
		}

		/// <summary>Gets the kerning pair adjustments for the specified glyphs.</summary>
		/// <param name="glyphs">The glyph IDs to get kerning adjustments for.</param>
		/// <param name="adjustments">The destination span for the kerning adjustments.</param>
		/// <returns>Returns <see langword="true" /> if kerning data was found; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool GetKerningPairAdjustments (ReadOnlySpan<ushort> glyphs, Span<int> adjustments)
		{
			if (adjustments.Length < glyphs.Length - 1)
				throw new ArgumentException ("Length of adjustments must be large enough to hold one adjustment per pair of glyphs (or, glyphs.Length - 1).");

			bool res;
			fixed (ushort* gp = glyphs)
			fixed (int* ap = adjustments) {
				res = SkiaApi.sk_typeface_get_kerning_pair_adjustments (Handle, gp, glyphs.Length, ap);
				GC.KeepAlive (this);
			}

			if (!res && glyphs.Length > 1)
				//Per SkTypeface::GetKerningPairAdjustments documentation, the method may have written
				//nonsense into the array before bailing. Don't return it to the caller, the doc says
				//such values must be ignored.
				adjustments.Slice(0, glyphs.Length - 1).Clear ();

			return res;
		}

		// Variable fonts

		/// <summary>Gets the number of variation design parameters (axes) in this typeface.</summary>
		/// <value>The number of variation axes defined in this typeface's fvar table.</value>
		/// <remarks />
		public int VariationDesignParameterCount {
			get {
				var r = SkiaApi.sk_typeface_get_variation_design_parameters (Handle, null, 0);
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Gets all variation design parameters (axes) defined in this typeface.</summary>
		/// <value>An array of <see cref="T:SkiaSharp.SKFontVariationAxis" /> describing each variation axis, or an empty array if this typeface has no variation axes.</value>
		/// <remarks />
		public SKFontVariationAxis[] VariationDesignParameters
		{
			get {
				var count = VariationDesignParameterCount;
				if (count <= 0)
					return Array.Empty<SKFontVariationAxis> ();

				var axes = new SKFontVariationAxis[count];
				fixed (SKFontVariationAxis* ptr = axes) {
					SkiaApi.sk_typeface_get_variation_design_parameters (Handle, ptr, count);
					GC.KeepAlive (this);
				}
				return axes;
			}
		}

		/// <summary>Fills a span with the variation design parameters (axes) defined in this typeface.</summary>
		/// <param name="axes">A span to receive the <see cref="T:SkiaSharp.SKFontVariationAxis" /> values.</param>
		/// <returns>The number of variation axis parameters written to <paramref name="axes" />.</returns>
		/// <remarks />
		public int GetVariationDesignParameters (Span<SKFontVariationAxis> axes)
		{
			if (axes.Length == 0)
				return 0;

			fixed (SKFontVariationAxis* ptr = axes) {
				var total = SkiaApi.sk_typeface_get_variation_design_parameters (Handle, ptr, axes.Length);
				if (total <= axes.Length) {
					GC.KeepAlive (this);
					return total;
				}

				// Skia is all-or-nothing: if buffer is undersized it writes nothing.
				// Retry with a pooled buffer and copy what fits.
				using var temp = Utils.RentArray<SKFontVariationAxis> (total);
				fixed (SKFontVariationAxis* tempPtr = temp.Span) {
					SkiaApi.sk_typeface_get_variation_design_parameters (Handle, tempPtr, total);
					GC.KeepAlive (this);
				}
				temp.Span.Slice (0, axes.Length).CopyTo (axes);
				return axes.Length;
			}
		}

		/// <summary>Gets the number of axes in the current variation design position of this typeface.</summary>
		/// <value>The number of variation axes for which the typeface has a current design-space position.</value>
		/// <remarks />
		public int VariationDesignPositionCount {
			get {
				var r = SkiaApi.sk_typeface_get_variation_design_position (Handle, null, 0);
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Gets the current variation design position of this typeface.</summary>
		/// <value>An array of <see cref="T:SkiaSharp.SKFontVariationPositionCoordinate" /> representing the current design-space position for each variation axis.</value>
		/// <remarks />
		public SKFontVariationPositionCoordinate[] VariationDesignPosition
		{
			get {
				var count = VariationDesignPositionCount;
				if (count <= 0)
					return Array.Empty<SKFontVariationPositionCoordinate> ();

				var coords = new SKFontVariationPositionCoordinate[count];
				fixed (SKFontVariationPositionCoordinate* ptr = coords) {
					SkiaApi.sk_typeface_get_variation_design_position (Handle, ptr, count);
					GC.KeepAlive (this);
				}
				return coords;
			}
		}

		/// <summary>Fills a span with the current variation design position of this typeface.</summary>
		/// <param name="coordinates">A span to receive the <see cref="T:SkiaSharp.SKFontVariationPositionCoordinate" /> values.</param>
		/// <returns>The number of axis coordinates written to <paramref name="coordinates" />.</returns>
		/// <remarks />
		public int GetVariationDesignPosition (Span<SKFontVariationPositionCoordinate> coordinates)
		{
			if (coordinates.Length == 0)
				return 0;

			fixed (SKFontVariationPositionCoordinate* ptr = coordinates) {
				var total = SkiaApi.sk_typeface_get_variation_design_position (Handle, ptr, coordinates.Length);
				if (total <= coordinates.Length) {
					GC.KeepAlive (this);
					return total;
				}

				// Skia is all-or-nothing: if buffer is undersized it writes nothing.
				// Retry with a pooled buffer and copy what fits.
				using var temp = Utils.RentArray<SKFontVariationPositionCoordinate> (total);
				fixed (SKFontVariationPositionCoordinate* tempPtr = temp.Span) {
					SkiaApi.sk_typeface_get_variation_design_position (Handle, tempPtr, total);
					GC.KeepAlive (this);
				}
				temp.Span.Slice (0, coordinates.Length).CopyTo (coordinates);
				return coordinates.Length;
			}
		}

		/// <summary>Creates a new typeface derived from this typeface with the specified variation design position.</summary>
		/// <param name="position">A read-only span of axis-tag/value pairs defining the variation design position.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKTypeface" /> based on this typeface with the specified variation design position applied.</returns>
		/// <remarks />
		public SKTypeface Clone (ReadOnlySpan<SKFontVariationPositionCoordinate> position)
		{
			fixed (SKFontVariationPositionCoordinate* ptr = position) {
				var r = GetObject (SkiaApi.sk_typeface_clone_with_arguments (Handle, ptr, position.Length, 0, 0, null, 0));
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Creates a new typeface derived from this typeface using the specified color palette index.</summary>
		/// <param name="paletteIndex">The zero-based index of the color palette to use in the cloned typeface.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKTypeface" /> based on this typeface with the specified color palette.</returns>
		/// <remarks />
		public SKTypeface Clone (int paletteIndex)
		{
			if (paletteIndex < 0)
				throw new ArgumentOutOfRangeException (nameof (paletteIndex));
			var r = GetObject (SkiaApi.sk_typeface_clone_with_arguments (Handle, null, 0, 0, paletteIndex, null, 0));
			GC.KeepAlive (this);
			return r;
		}

		/// <summary>Creates a new typeface derived from this typeface with the specified font arguments.</summary>
		/// <param name="args">The font arguments specifying palette, variation settings, and other parameters to apply to the clone.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKTypeface" /> based on this typeface with the specified font arguments applied.</returns>
		/// <remarks />
		public SKTypeface Clone (SKFontArguments args)
		{
			fixed (SKFontVariationPositionCoordinate* posPtr = args.VariationDesignPosition)
			fixed (SKFontPaletteOverride* palPtr = args.PaletteOverrides) {
				var r = GetObject (SkiaApi.sk_typeface_clone_with_arguments (Handle, posPtr, args.VariationDesignPosition.Length, args.CollectionIndex, args.PaletteIndex, palPtr, args.PaletteOverrides.Length));
				GC.KeepAlive (this);
				return r;
			}
		}

		//

		internal static SKTypeface GetObject (IntPtr handle) =>
			GetOrAddObject (handle, (h, o) => new SKTypeface (h, o));

		internal static SKTypeface GetDisposeProtectedObject (IntPtr handle, bool owns = true, bool unrefExisting = true) =>
			GetOrAddDisposeProtectedObject (handle, owns, unrefExisting, (h, o) => new SKTypeface (h, o));

	}
}
