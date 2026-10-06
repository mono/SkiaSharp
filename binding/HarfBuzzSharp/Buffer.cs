#nullable disable

using System;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;

namespace HarfBuzzSharp
{
	/// <summary>Represents a text buffer in memory.</summary>
	/// <remarks />
	public unsafe class Buffer : NativeObject
	{
		/// <summary>The default replacement code point (U+FFFD) used for invalid characters.</summary>
		/// <remarks />
		public const int DefaultReplacementCodepoint = '\uFFFD';

		internal Buffer (IntPtr handle)
			: base (handle)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="T:HarfBuzzSharp.Buffer" /> class with default values.</summary>
		/// <remarks />
		public Buffer ()
			: this (HarfBuzzApi.hb_buffer_create ())
		{
		}

		/// <summary>Gets or sets the type of content in the buffer.</summary>
		/// <value>The content type.</value>
		/// <remarks>Buffers can contain either Unicode characters (before shaping) or glyphs (after shaping).</remarks>
		public ContentType ContentType {
			get {
				var r = HarfBuzzApi.hb_buffer_get_content_type (Handle);
				GC.KeepAlive (this);
				return r;
			}
			set {
				HarfBuzzApi.hb_buffer_set_content_type (Handle, value);
				GC.KeepAlive (this);
			}
		}

		/// <summary>Gets or sets the text flow direction of the buffer.</summary>
		/// <value>One of the enumeration values that specifies the text flow direction.</value>
		/// <remarks>No shaping can happen without setting the direction, or invoking <see cref="M:HarfBuzzSharp.Buffer.GuessSegmentProperties" />.</remarks>
		public Direction Direction {
			get {
				var r = HarfBuzzApi.hb_buffer_get_direction (Handle);
				GC.KeepAlive (this);
				return r;
			}
			set {
				HarfBuzzApi.hb_buffer_set_direction (Handle, value);
				GC.KeepAlive (this);
			}
		}

		/// <summary>Gets or sets the language of the text in the buffer.</summary>
		/// <value>The language, or <see langword="null" /> if not set.</value>
		/// <remarks>Language is used by some OpenType features to customize shaping behavior.</remarks>
		public Language Language {
			get {
				var r = new Language (HarfBuzzApi.hb_buffer_get_language (Handle));
				GC.KeepAlive (this);
				return r;
			}
			set {
				HarfBuzzApi.hb_buffer_set_language (Handle, value.Handle);
				GC.KeepAlive (value);
				GC.KeepAlive (this);
			}
		}

		/// <summary>Gets or sets the buffer flags that control shaping behavior.</summary>
		/// <value>The buffer flags.</value>
		/// <remarks />
		public BufferFlags Flags {
			get {
				var r = HarfBuzzApi.hb_buffer_get_flags (Handle);
				GC.KeepAlive (this);
				return r;
			}
			set {
				HarfBuzzApi.hb_buffer_set_flags (Handle, value);
				GC.KeepAlive (this);
			}
		}

		/// <summary>Gets or sets the cluster level of the buffer.</summary>
		/// <value>The cluster level.</value>
		/// <remarks>The cluster level dictates how HarfBuzz will treat non-base characters during shaping.</remarks>
		public ClusterLevel ClusterLevel {
			get {
				var r = HarfBuzzApi.hb_buffer_get_cluster_level (Handle);
				GC.KeepAlive (this);
				return r;
			}
			set {
				HarfBuzzApi.hb_buffer_set_cluster_level (Handle, value);
				GC.KeepAlive (this);
			}
		}

		/// <summary>Gets or sets the code point used to replace invalid characters during text processing.</summary>
		/// <value>The replacement code point. The default is <see cref="F:HarfBuzzSharp.Buffer.DefaultReplacementCodepoint" /> (U+FFFD).</value>
		/// <remarks />
		public uint ReplacementCodepoint {
			get {
				var r = HarfBuzzApi.hb_buffer_get_replacement_codepoint (Handle);
				GC.KeepAlive (this);
				return r;
			}
			set {
				HarfBuzzApi.hb_buffer_set_replacement_codepoint (Handle, value);
				GC.KeepAlive (this);
			}
		}

		/// <summary>Gets or sets the glyph ID used for invisible characters.</summary>
		/// <value>The glyph ID for invisible characters, or 0 to use the default behavior.</value>
		/// <remarks>This glyph is used for characters marked as invisible in the Unicode database.</remarks>
		public uint InvisibleGlyph {
			get {
				var r = HarfBuzzApi.hb_buffer_get_invisible_glyph (Handle);
				GC.KeepAlive (this);
				return r;
			}
			set {
				HarfBuzzApi.hb_buffer_set_invisible_glyph (Handle, value);
				GC.KeepAlive (this);
			}
		}

		/// <summary>Gets or sets the script of the text in the buffer.</summary>
		/// <value>The script.</value>
		/// <remarks>Script determines which OpenType features are applied during shaping.</remarks>
		public Script Script {
			get {
				var r = HarfBuzzApi.hb_buffer_get_script (Handle);
				GC.KeepAlive (this);
				return r;
			}
			set {
				HarfBuzzApi.hb_buffer_set_script (Handle, value);
				GC.KeepAlive (this);
			}
		}

		/// <summary>Gets or sets the size of the buffer.</summary>
		/// <value>The size of the buffer.</value>
		/// <remarks>If the new length is greater that the current length, more memory will be allocated. If the new length is less than the current length, the extra items will be cleared.</remarks>
		public int Length {
			get {
				var r = (int)HarfBuzzApi.hb_buffer_get_length (Handle);
				GC.KeepAlive (this);
				return r;
			}
			set {
				HarfBuzzApi.hb_buffer_set_length (Handle, (uint)value);
				GC.KeepAlive (this);
			}
		}

		/// <summary>Gets or sets the Unicode functions used by the buffer.</summary>
		/// <value>The Unicode functions.</value>
		/// <remarks>Unicode functions provide character property lookups needed during shaping.</remarks>
		public UnicodeFunctions UnicodeFunctions {
			get {
				var r = new UnicodeFunctions (HarfBuzzApi.hb_buffer_get_unicode_funcs (Handle));
				GC.KeepAlive (this);
				return r;
			}
			set {
				HarfBuzzApi.hb_buffer_set_unicode_funcs (Handle, value.Handle);
				GC.KeepAlive (value);
				GC.KeepAlive (this);
			}
		}

		/// <summary>Gets the buffer glyph information array.</summary>
		/// <value>An array of <see cref="T:HarfBuzzSharp.GlyphInfo" /> values.</value>
		/// <remarks>The information is valid as long as buffer contents are not modified.</remarks>
		public GlyphInfo[] GlyphInfos {
			get {
				var array = GetGlyphInfoSpan ().ToArray ();
				GC.KeepAlive (this);
				return array;
			}
		}

		/// <summary>Gets the buffer glyph position array.</summary>
		/// <value>An array of <see cref="T:HarfBuzzSharp.GlyphPosition" /> values.</value>
		/// <remarks>The positions are valid as long as buffer contents are not modified.</remarks>
		public GlyphPosition[] GlyphPositions {
			get {
				var array = GetGlyphPositionSpan ().ToArray ();
				GC.KeepAlive (this);
				return array;
			}
		}

		/// <summary>Appends a character with the Unicode value and gives it the initial cluster value.</summary>
		/// <param name="codepoint">The Unicode code point.</param>
		/// <param name="cluster">The cluster value of the code point.</param>
		/// <remarks>This function does not check the validity of the codepoint.</remarks>
		public void Add (int codepoint, int cluster) => Add ((uint)codepoint, (uint)cluster);

		/// <summary>Appends a character with the Unicode value and gives it the initial cluster value.</summary>
		/// <param name="codepoint">The Unicode code point.</param>
		/// <param name="cluster">The cluster value of the code point.</param>
		/// <remarks>This function does not check the validity of the codepoint.</remarks>
		public void Add (uint codepoint, uint cluster)
		{
			if (Length != 0 && ContentType != ContentType.Unicode)
				throw new InvalidOperationException ("Non empty buffer's ContentType must be of type Unicode.");
			if (ContentType == ContentType.Glyphs)
				throw new InvalidOperationException ("ContentType must not be of type Glyphs");

			HarfBuzzApi.hb_buffer_add (Handle, codepoint, cluster);
			GC.KeepAlive (this);
		}

		/// <summary>Appends the specified text to the buffer.</summary>
		/// <param name="utf8text">The array of UTF-8 characters to append.</param>
		/// <remarks />
		public void AddUtf8 (string utf8text)
		{
			if (utf8text == null)
				throw new ArgumentNullException (nameof (utf8text));

			// An empty string flows through the pooled path as a no-op:
			// GetMaxByteCount(0) rents a small buffer, GetBytes writes 0 bytes,
			// and AddUtf8(ptr, 0, ...) adds nothing (mirrors AddUtf16(string)).
			var maxByteCount = Encoding.UTF8.GetMaxByteCount (utf8text.Length);
			var utf8bytes = ArrayPool<byte>.Shared.Rent (maxByteCount);
			try {
				fixed (char* chars = utf8text)
				fixed (byte* bytes = utf8bytes) {
					var byteCount = Encoding.UTF8.GetBytes (chars, utf8text.Length, bytes, utf8bytes.Length);
					AddUtf8 ((IntPtr)bytes, byteCount, 0, -1);
				}
			} finally {
				ArrayPool<byte>.Shared.Return (utf8bytes);
			}
		}

		/// <summary>Appends the specified text bytes to the buffer.</summary>
		/// <param name="bytes">The array of UTF-8 character bytes to append.</param>
		/// <remarks />
		public void AddUtf8 (byte[] bytes) => AddUtf8 (new ReadOnlySpan<byte> (bytes));

		/// <summary>Appends UTF-8 encoded text to the buffer.</summary>
		/// <param name="text">The span of UTF-8 encoded bytes to append.</param>
		/// <remarks />
		public void AddUtf8 (ReadOnlySpan<byte> text) => AddUtf8 (text, 0, -1);

		/// <summary>Appends a range of UTF-8 encoded text to the buffer.</summary>
		/// <param name="text">The span of UTF-8 encoded bytes to append.</param>
		/// <param name="itemOffset">The offset of the first byte to add to the buffer.</param>
		/// <param name="itemLength">The number of bytes to add to the buffer, or -1 for the end of the text.</param>
		/// <remarks />
		public unsafe void AddUtf8 (ReadOnlySpan<byte> text, int itemOffset, int itemLength)
		{
			fixed (byte* bytes = text) {
				AddUtf8 ((IntPtr)bytes, text.Length, itemOffset, itemLength);
			}
		}

		/// <summary>Appends UTF-8 encoded text to the buffer.</summary>
		/// <param name="text">A pointer to UTF-8 encoded text.</param>
		/// <param name="textLength">The length of the text in bytes.</param>
		/// <remarks />
		public void AddUtf8 (IntPtr text, int textLength) => AddUtf8 (text, textLength, 0, -1);

		/// <summary>Appends a range of UTF-8 encoded text to the buffer.</summary>
		/// <param name="text">A pointer to UTF-8 encoded text.</param>
		/// <param name="textLength">The length of the text in bytes.</param>
		/// <param name="itemOffset">The offset of the first byte to add to the buffer.</param>
		/// <param name="itemLength">The number of bytes to add to the buffer, or -1 for the end of the text.</param>
		/// <remarks />
		public void AddUtf8 (IntPtr text, int textLength, int itemOffset, int itemLength)
		{
			if (itemOffset < 0)
				throw new ArgumentOutOfRangeException (nameof (itemOffset), "ItemOffset must be non negative.");
			if (Length != 0 && ContentType != ContentType.Unicode)
				throw new InvalidOperationException ("Non empty buffer's ContentType must be of type Unicode.");
			if (ContentType == ContentType.Glyphs)
				throw new InvalidOperationException ("ContentType must not be Glyphs");

			HarfBuzzApi.hb_buffer_add_utf8 (Handle, (void*)text, textLength, (uint)itemOffset, itemLength);
			GC.KeepAlive (this);
		}

		/// <summary>Appends UTF-16 encoded text to the buffer.</summary>
		/// <param name="text">The UTF-16 string to append.</param>
		/// <remarks />
		public void AddUtf16 (string text) => AddUtf16 (text, 0, -1);

		/// <summary>Appends a range of UTF-16 encoded text to the buffer.</summary>
		/// <param name="text">The UTF-16 string to append.</param>
		/// <param name="itemOffset">The offset of the first character to add to the buffer.</param>
		/// <param name="itemLength">The number of characters to add to the buffer, or -1 for the end of the text.</param>
		/// <remarks />
		public unsafe void AddUtf16 (string text, int itemOffset, int itemLength)
		{
			fixed (char* chars = text) {
				AddUtf16 ((IntPtr)chars, text.Length, itemOffset, itemLength);
			}
		}

		/// <summary>Appends UTF-16 encoded text to the buffer.</summary>
		/// <param name="text">The span of UTF-16 encoded bytes to append.</param>
		/// <remarks />
		public unsafe void AddUtf16 (ReadOnlySpan<byte> text)
		{
			fixed (byte* bytes = text) {
				AddUtf16 ((IntPtr)bytes, text.Length / 2);
			}
		}

		/// <summary>Appends UTF-16 encoded text to the buffer.</summary>
		/// <param name="text">The span of UTF-16 characters to append.</param>
		/// <remarks />
		public void AddUtf16 (ReadOnlySpan<char> text) => AddUtf16 (text, 0, -1);

		/// <summary>Appends a range of UTF-16 encoded text to the buffer.</summary>
		/// <param name="text">The span of UTF-16 characters to append.</param>
		/// <param name="itemOffset">The offset of the first character to add to the buffer.</param>
		/// <param name="itemLength">The number of characters to add to the buffer, or -1 for the end of the text.</param>
		/// <remarks />
		public unsafe void AddUtf16 (ReadOnlySpan<char> text, int itemOffset, int itemLength)
		{
			fixed (char* chars = text) {
				AddUtf16 ((IntPtr)chars, text.Length, itemOffset, itemLength);
			}
		}

		/// <summary>Appends UTF-16 encoded text to the buffer.</summary>
		/// <param name="text">A pointer to UTF-16 encoded text.</param>
		/// <param name="textLength">The length of the text in UTF-16 code units.</param>
		/// <remarks />
		public void AddUtf16 (IntPtr text, int textLength) =>
			AddUtf16 (text, textLength, 0, -1);

		/// <summary>Appends a range of UTF-16 encoded text to the buffer.</summary>
		/// <param name="text">A pointer to UTF-16 encoded text.</param>
		/// <param name="textLength">The length of the text in UTF-16 code units.</param>
		/// <param name="itemOffset">The offset of the first character to add to the buffer.</param>
		/// <param name="itemLength">The number of characters to add to the buffer, or -1 for the end of the text.</param>
		/// <remarks />
		public void AddUtf16 (IntPtr text, int textLength, int itemOffset, int itemLength)
		{
			if (itemOffset < 0)
				throw new ArgumentOutOfRangeException (nameof (itemOffset), "ItemOffset must be non negative.");
			if (Length != 0 && ContentType != ContentType.Unicode)
				throw new InvalidOperationException ("Non empty buffer's ContentType must be of type Unicode.");
			if (ContentType == ContentType.Glyphs)
				throw new InvalidOperationException ("ContentType must not be of type Glyphs");

			HarfBuzzApi.hb_buffer_add_utf16 (Handle, (ushort*)text, textLength, (uint)itemOffset, itemLength);
			GC.KeepAlive (this);
		}

		/// <summary>Appends the string as UTF-32 encoded text to the buffer.</summary>
		/// <param name="text">The string to append as UTF-32 encoded text.</param>
		/// <remarks />
		public void AddUtf32 (string text) => AddUtf32 (Encoding.UTF32.GetBytes (text));

		/// <summary>Appends UTF-32 encoded text to the buffer.</summary>
		/// <param name="text">The span of UTF-32 encoded bytes to append.</param>
		/// <remarks />
		public unsafe void AddUtf32 (ReadOnlySpan<byte> text)
		{
			fixed (byte* bytes = text) {
				AddUtf32 ((IntPtr)bytes, text.Length / 4);
			}
		}

		/// <summary>Appends UTF-32 encoded text to the buffer.</summary>
		/// <param name="text">The span of UTF-32 code points to append.</param>
		/// <remarks />
		public void AddUtf32 (ReadOnlySpan<uint> text) => AddUtf32 (text, 0, -1);

		/// <summary>Appends a range of UTF-32 encoded text to the buffer.</summary>
		/// <param name="text">The span of UTF-32 code points to append.</param>
		/// <param name="itemOffset">The offset of the first code point to add to the buffer.</param>
		/// <param name="itemLength">The number of code points to add to the buffer, or -1 for the end of the text.</param>
		/// <remarks />
		public unsafe void AddUtf32 (ReadOnlySpan<uint> text, int itemOffset, int itemLength)
		{
			fixed (uint* integers = text) {
				AddUtf32 ((IntPtr)integers, text.Length, itemOffset, itemLength);
			}
		}

		/// <summary>Appends UTF-32 encoded text to the buffer.</summary>
		/// <param name="text">The span of UTF-32 code points to append.</param>
		/// <remarks />
		public void AddUtf32 (ReadOnlySpan<int> text) => AddUtf32 (text, 0, -1);

		/// <summary>Appends a range of UTF-32 encoded text to the buffer.</summary>
		/// <param name="text">The span of UTF-32 code points to append.</param>
		/// <param name="itemOffset">The offset of the first code point to add to the buffer.</param>
		/// <param name="itemLength">The number of code points to add to the buffer, or -1 for the end of the text.</param>
		/// <remarks />
		public unsafe void AddUtf32 (ReadOnlySpan<int> text, int itemOffset, int itemLength)
		{
			fixed (int* integers = text) {
				AddUtf32 ((IntPtr)integers, text.Length, itemOffset, itemLength);
			}
		}

		/// <summary>Appends UTF-32 encoded text to the buffer.</summary>
		/// <param name="text">A pointer to UTF-32 encoded text.</param>
		/// <param name="textLength">The length of the text in UTF-32 code units.</param>
		/// <remarks />
		public void AddUtf32 (IntPtr text, int textLength) =>
			AddUtf32 (text, textLength, 0, -1);

		/// <summary>Appends a range of UTF-32 encoded text to the buffer.</summary>
		/// <param name="text">A pointer to UTF-32 encoded text.</param>
		/// <param name="textLength">The length of the text in UTF-32 code units.</param>
		/// <param name="itemOffset">The offset of the first code point to add to the buffer.</param>
		/// <param name="itemLength">The number of code points to add to the buffer, or -1 for the end of the text.</param>
		/// <remarks />
		public void AddUtf32 (IntPtr text, int textLength, int itemOffset, int itemLength)
		{
			if (itemOffset < 0)
				throw new ArgumentOutOfRangeException (nameof (itemOffset), "ItemOffset must be non negative.");
			if (Length != 0 && ContentType != ContentType.Unicode)
				throw new InvalidOperationException ("Non empty buffer's ContentType must be of type Unicode.");
			if (ContentType == ContentType.Glyphs)
				throw new InvalidOperationException ("ContentType must not be of type Glyphs");

			HarfBuzzApi.hb_buffer_add_utf32 (Handle, (uint*)text, textLength, (uint)itemOffset, itemLength);
			GC.KeepAlive (this);
		}

		/// <summary>Appends characters from the span to the buffer.</summary>
		/// <param name="text">The span of Unicode code points to append.</param>
		/// <remarks>This function does not check the validity of the characters.</remarks>
		public void AddCodepoints (ReadOnlySpan<uint> text) => AddCodepoints (text, 0, -1);

		/// <summary>Appends characters from the span to the buffer.</summary>
		/// <param name="text">The span of Unicode code points to append.</param>
		/// <param name="itemOffset">The offset of the first code point to add to the buffer.</param>
		/// <param name="itemLength">The number of code points to add to the buffer, or -1 for the end of the text.</param>
		/// <remarks>This function does not check the validity of the characters.</remarks>
		public unsafe void AddCodepoints (ReadOnlySpan<uint> text, int itemOffset, int itemLength)
		{
			fixed (uint* codepoints = text) {
				AddCodepoints ((IntPtr)codepoints, text.Length, itemOffset, itemLength);
			}
		}

		/// <summary>Appends characters from the span to the buffer.</summary>
		/// <param name="text">The span of Unicode code points to append.</param>
		/// <remarks>This function does not check the validity of the characters.</remarks>
		public void AddCodepoints (ReadOnlySpan<int> text) => AddCodepoints (text, 0, -1);

		/// <summary>Appends characters from the span to the buffer.</summary>
		/// <param name="text">The span of Unicode code points to append.</param>
		/// <param name="itemOffset">The offset of the first code point to add to the buffer.</param>
		/// <param name="itemLength">The number of code points to add to the buffer, or -1 for the end of the text.</param>
		/// <remarks>This function does not check the validity of the characters.</remarks>
		public unsafe void AddCodepoints (ReadOnlySpan<int> text, int itemOffset, int itemLength)
		{
			fixed (int* codepoints = text) {
				AddCodepoints ((IntPtr)codepoints, text.Length, itemOffset, itemLength);
			}
		}

		/// <summary>Appends characters from the pointer to the buffer.</summary>
		/// <param name="text">A pointer to an array of Unicode code points.</param>
		/// <param name="textLength">The number of code points in the array.</param>
		/// <remarks>This function does not check the validity of the characters.</remarks>
		public void AddCodepoints (IntPtr text, int textLength) => AddCodepoints (text, textLength, 0, -1);

		/// <summary>Appends characters from the pointer to the buffer.</summary>
		/// <param name="text">A pointer to an array of Unicode code points.</param>
		/// <param name="textLength">The number of code points in the array.</param>
		/// <param name="itemOffset">The offset of the first code point to add to the buffer.</param>
		/// <param name="itemLength">The number of code points to add to the buffer, or -1 for the end of the text.</param>
		/// <remarks>This function does not check the validity of the characters.</remarks>
		public void AddCodepoints (IntPtr text, int textLength, int itemOffset, int itemLength)
		{
			if (itemOffset < 0)
				throw new ArgumentOutOfRangeException (nameof (itemOffset), "ItemOffset must be non negative.");
			if (Length != 0 && ContentType != ContentType.Unicode)
				throw new InvalidOperationException ("Non empty buffer's ContentType must be of type Unicode.");
			if (ContentType == ContentType.Glyphs)
				throw new InvalidOperationException ("ContentType must not be of type Glyphs");

			HarfBuzzApi.hb_buffer_add_codepoints (Handle, (uint*)text, textLength, (uint)itemOffset, itemLength);
			GC.KeepAlive (this);
		}

		/// <summary>Gets a span over the glyph information array.</summary>
		/// <returns>A read-only span of glyph information.</returns>
		/// <remarks>The span is valid as long as buffer contents are not modified.</remarks>
		public unsafe ReadOnlySpan<GlyphInfo> GetGlyphInfoSpan ()
		{
			uint length;
			var infoPtrs = HarfBuzzApi.hb_buffer_get_glyph_infos (Handle, &length);
			GC.KeepAlive (this);
			return new ReadOnlySpan<GlyphInfo> (infoPtrs, (int)length);
		}

		/// <summary>Gets a span over the glyph position array.</summary>
		/// <returns>A read-only span of glyph positions.</returns>
		/// <remarks>The span is valid as long as buffer contents are not modified.</remarks>
		public unsafe ReadOnlySpan<GlyphPosition> GetGlyphPositionSpan ()
		{
			uint length;
			var infoPtrs = HarfBuzzApi.hb_buffer_get_glyph_positions (Handle, &length);
			GC.KeepAlive (this);
			return new ReadOnlySpan<GlyphPosition> (infoPtrs, (int)length);
		}

		/// <summary>Sets the unset buffer segment properties based on the buffer's Unicode contents.</summary>
		/// <remarks />
		public void GuessSegmentProperties ()
		{
			if (ContentType != ContentType.Unicode)
				throw new InvalidOperationException ("ContentType must be of type Unicode.");

			HarfBuzzApi.hb_buffer_guess_segment_properties (Handle);
			GC.KeepAlive (this);
		}

		/// <summary>Clears the buffer's contents.</summary>
		/// <remarks>This operation preserves the Unicode functions and replacement code point.</remarks>
		public void ClearContents ()
		{
			HarfBuzzApi.hb_buffer_clear_contents (Handle);
			GC.KeepAlive (this);
		}

		/// <summary>Resets the buffer to its initial state as if it were newly created.</summary>
		/// <remarks>This clears contents and resets all properties to their default values.</remarks>
		public void Reset ()
		{
			HarfBuzzApi.hb_buffer_reset (Handle);
			GC.KeepAlive (this);
		}

		/// <summary>Appends the contents of another buffer to this buffer.</summary>
		/// <param name="buffer">The buffer to append.</param>
		/// <remarks />
		public void Append (Buffer buffer) => Append (buffer, 0, -1);

		/// <summary>Appends a range from another buffer to this buffer.</summary>
		/// <param name="buffer">The buffer to append from.</param>
		/// <param name="start">The start index of the range to append.</param>
		/// <param name="end">The end index of the range to append.</param>
		/// <remarks />
		public void Append (Buffer buffer, int start, int end)
		{
			if (buffer.Length == 0)
				throw new ArgumentException ("Buffer must be non empty.", nameof (buffer));
			if (buffer.ContentType != ContentType)
				throw new InvalidOperationException ("ContentType must be of same type.");

			HarfBuzzApi.hb_buffer_append (Handle, buffer.Handle, (uint)start, (uint)(end == -1 ? buffer.Length : end));
			GC.KeepAlive (buffer);
			GC.KeepAlive (this);
		}

		/// <summary>Reorders glyphs so that they can be run through subsequent phases consistently.</summary>
		/// <remarks>This ensures that clusters of glyphs are arranged in a canonical order after shaping.</remarks>
		public void NormalizeGlyphs ()
		{
			if (ContentType != ContentType.Glyphs)
				throw new InvalidOperationException ("ContentType must be of type Glyphs.");
			if (GlyphPositions.Length == 0)
				throw new InvalidOperationException ("GlyphPositions can't be empty.");

			HarfBuzzApi.hb_buffer_normalize_glyphs (Handle);
			GC.KeepAlive (this);
		}

		/// <summary>Reverses the order of buffer contents.</summary>
		/// <remarks />
		public void Reverse ()
		{
			HarfBuzzApi.hb_buffer_reverse (Handle);
			GC.KeepAlive (this);
		}

		/// <summary>Reverses the order of buffer contents within a specified range.</summary>
		/// <param name="start">The start index of the range to reverse.</param>
		/// <param name="end">The end index of the range to reverse.</param>
		/// <remarks />
		public void ReverseRange (int start, int end)
		{
			HarfBuzzApi.hb_buffer_reverse_range (Handle, (uint)start, (uint)(end == -1 ? Length : end));
			GC.KeepAlive (this);
		}

		/// <summary>Reverses the order of clusters in the buffer.</summary>
		/// <remarks>Within each cluster, glyphs maintain their original order.</remarks>
		public void ReverseClusters ()
		{
			HarfBuzzApi.hb_buffer_reverse_clusters (Handle);
			GC.KeepAlive (this);
		}

		/// <summary>Serializes the buffer glyphs to a string.</summary>
		/// <returns>A string containing the serialized glyph data.</returns>
		/// <remarks />
		public string SerializeGlyphs () =>
			SerializeGlyphs (0, -1, null, SerializeFormat.Text, SerializeFlag.Default);

		/// <summary>Serializes a range of buffer glyphs to a string.</summary>
		/// <param name="start">The start index of the range to serialize.</param>
		/// <param name="end">The end index of the range to serialize.</param>
		/// <returns>A string containing the serialized glyph data.</returns>
		/// <remarks />
		public string SerializeGlyphs (int start, int end) =>
			SerializeGlyphs (start, end, null, SerializeFormat.Text, SerializeFlag.Default);

		/// <summary>Serializes the buffer glyphs to a string using the specified font.</summary>
		/// <param name="font">The font to use for glyph names, or <see langword="null" />.</param>
		/// <returns>A string containing the serialized glyph data.</returns>
		/// <remarks />
		public string SerializeGlyphs (Font font) =>
			SerializeGlyphs (0, -1, font, SerializeFormat.Text, SerializeFlag.Default);

		/// <summary>Serializes the buffer glyphs to a string using the specified font, format, and flags.</summary>
		/// <param name="font">The font to use for glyph names, or <see langword="null" />.</param>
		/// <param name="format">The serialization format.</param>
		/// <param name="flags">Flags that control serialization behavior.</param>
		/// <returns>A string containing the serialized glyph data.</returns>
		/// <remarks />
		public string SerializeGlyphs (Font font, SerializeFormat format, SerializeFlag flags) =>
			SerializeGlyphs (0, -1, font, format, flags);

		/// <summary>Serializes a range of buffer glyphs to a string using the specified font, format, and flags.</summary>
		/// <param name="start">The start index of the range to serialize.</param>
		/// <param name="end">The end index of the range to serialize.</param>
		/// <param name="font">The font to use for glyph names, or <see langword="null" />.</param>
		/// <param name="format">The serialization format.</param>
		/// <param name="flags">Flags that control serialization behavior.</param>
		/// <returns>A string containing the serialized glyph data.</returns>
		/// <remarks />
		public unsafe string SerializeGlyphs (int start, int end, Font font, SerializeFormat format, SerializeFlag flags)
		{
			if (Length == 0)
				throw new InvalidOperationException ("Buffer should not be empty.");
			if (ContentType != ContentType.Glyphs)
				throw new InvalidOperationException ("ContentType should be of type Glyphs.");

			if (end == -1)
				end = Length;

			using var buffer = MemoryPool<byte>.Shared.Rent ();
			using var pinned = buffer.Memory.Pin ();

			var bufferSize = buffer.Memory.Length;
			var currentPosition = (uint)start;
			var builder = new StringBuilder (bufferSize);

			while (currentPosition < end) {
				uint consumed;
				currentPosition += HarfBuzzApi.hb_buffer_serialize_glyphs (
					Handle,
					(uint)currentPosition,
					(uint)end,
					pinned.Pointer,
					(uint)bufferSize,
					&consumed,
					font?.Handle ?? IntPtr.Zero,
					format,
					flags);

				builder.Append (Marshal.PtrToStringAnsi ((IntPtr)pinned.Pointer, (int)consumed));
			}

			GC.KeepAlive (font);
			GC.KeepAlive (this);

			return builder.ToString ();
		}

		/// <summary>Deserializes glyphs from a string and populates the buffer.</summary>
		/// <param name="data">The serialized glyph data.</param>
		/// <remarks />
		public void DeserializeGlyphs (string data) =>
			DeserializeGlyphs (data, null, SerializeFormat.Text);

		/// <summary>Deserializes glyphs from a string and populates the buffer using the specified font.</summary>
		/// <param name="data">The serialized glyph data.</param>
		/// <param name="font">The font to use for resolving glyph names, or <see langword="null" />.</param>
		/// <remarks />
		public void DeserializeGlyphs (string data, Font font) =>
			DeserializeGlyphs (data, font, SerializeFormat.Text);

		/// <summary>Deserializes glyphs from a string and populates the buffer using the specified font and format.</summary>
		/// <param name="data">The serialized glyph data.</param>
		/// <param name="font">The font to use for resolving glyph names, or <see langword="null" />.</param>
		/// <param name="format">The serialization format of the data.</param>
		/// <remarks />
		public void DeserializeGlyphs (string data, Font font, SerializeFormat format)
		{
			if (Length != 0)
				throw new InvalidOperationException ("Buffer must be empty.");
			if (ContentType == ContentType.Glyphs)
				throw new InvalidOperationException ("ContentType must not be Glyphs.");

			HarfBuzzApi.hb_buffer_deserialize_glyphs (Handle, data, -1, null, font?.Handle ?? IntPtr.Zero, format);
			GC.KeepAlive (font);
			GC.KeepAlive (this);
		}

		/// <summary>Releases the unmanaged resources used by the <see cref="T:HarfBuzzSharp.Buffer" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:HarfBuzzSharp.Buffer" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		/// <summary>Releases the unmanaged resources used.</summary>
		/// <remarks />
		protected override void DisposeHandler ()
		{
			if (Handle != IntPtr.Zero) {
				HarfBuzzApi.hb_buffer_destroy (Handle);
			}
		}
	}
}
