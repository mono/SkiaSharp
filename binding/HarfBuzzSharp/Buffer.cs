#nullable disable

using System;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;

namespace HarfBuzzSharp
{
	/// <summary>Represents a text buffer in memory.</summary>
	/// <remarks></remarks>
	public unsafe class Buffer : NativeObject
	{
		/// <summary>To be added.</summary>
		/// <remarks>To be added.</remarks>
		public const int DefaultReplacementCodepoint = '\uFFFD';

		internal Buffer (IntPtr handle)
			: base (handle)
		{
		}

		/// <summary>Creates a new <see cref="M:HarfBuzzSharp.Buffer.#ctor" /> with default values.</summary>
		/// <remarks></remarks>
		public Buffer ()
			: this (HarfBuzzApi.hb_buffer_create ())
		{
		}

		/// <summary>To be added.</summary>
		/// <value>To be added.</value>
		/// <remarks>To be added.</remarks>
		public ContentType ContentType {
			get => HarfBuzzApi.hb_buffer_get_content_type (Handle);
			set => HarfBuzzApi.hb_buffer_set_content_type (Handle, value);
		}

		/// <summary>To be added.</summary>
		/// <value>To be added.</value>
		/// <remarks>To be added.</remarks>
		public Direction Direction {
			get => HarfBuzzApi.hb_buffer_get_direction (Handle);
			set => HarfBuzzApi.hb_buffer_set_direction (Handle, value);
		}

		/// <summary>To be added.</summary>
		/// <value>To be added.</value>
		/// <remarks>To be added.</remarks>
		public Language Language {
			get => new Language (HarfBuzzApi.hb_buffer_get_language (Handle));
			set => HarfBuzzApi.hb_buffer_set_language (Handle, value.Handle);
		}

		/// <summary>To be added.</summary>
		/// <value>To be added.</value>
		/// <remarks>To be added.</remarks>
		public BufferFlags Flags {
			get => HarfBuzzApi.hb_buffer_get_flags (Handle);
			set => HarfBuzzApi.hb_buffer_set_flags (Handle, value);
		}

		/// <summary>To be added.</summary>
		/// <value>To be added.</value>
		/// <remarks>To be added.</remarks>
		public ClusterLevel ClusterLevel {
			get => HarfBuzzApi.hb_buffer_get_cluster_level (Handle);
			set => HarfBuzzApi.hb_buffer_set_cluster_level (Handle, value);
		}

		/// <summary>To be added.</summary>
		/// <value>To be added.</value>
		/// <remarks>To be added.</remarks>
		public uint ReplacementCodepoint {
			get => HarfBuzzApi.hb_buffer_get_replacement_codepoint (Handle);
			set => HarfBuzzApi.hb_buffer_set_replacement_codepoint (Handle, value);
		}

		/// <summary>To be added.</summary>
		/// <value>To be added.</value>
		/// <remarks>To be added.</remarks>
		public uint InvisibleGlyph {
			get => HarfBuzzApi.hb_buffer_get_invisible_glyph (Handle);
			set => HarfBuzzApi.hb_buffer_set_invisible_glyph (Handle, value);
		}

		/// <summary>To be added.</summary>
		/// <value>To be added.</value>
		/// <remarks>To be added.</remarks>
		public Script Script {
			get => HarfBuzzApi.hb_buffer_get_script (Handle);
			set => HarfBuzzApi.hb_buffer_set_script (Handle, value);
		}

		/// <summary>To be added.</summary>
		/// <value>To be added.</value>
		/// <remarks>To be added.</remarks>
		public int Length {
			get => (int)HarfBuzzApi.hb_buffer_get_length (Handle);
			set => HarfBuzzApi.hb_buffer_set_length (Handle, (uint)value);
		}

		/// <summary>To be added.</summary>
		/// <value>To be added.</value>
		/// <remarks>To be added.</remarks>
		public UnicodeFunctions UnicodeFunctions {
			get => new UnicodeFunctions (HarfBuzzApi.hb_buffer_get_unicode_funcs (Handle));
			set => HarfBuzzApi.hb_buffer_set_unicode_funcs (Handle, value.Handle);
		}

		/// <summary>To be added.</summary>
		/// <value>To be added.</value>
		/// <remarks>To be added.</remarks>
		public GlyphInfo[] GlyphInfos {
			get {
				var array = GetGlyphInfoSpan ().ToArray ();
				GC.KeepAlive (this);
				return array;
			}
		}

		/// <summary>To be added.</summary>
		/// <value>To be added.</value>
		/// <remarks>To be added.</remarks>
		public GlyphPosition[] GlyphPositions {
			get {
				var array = GetGlyphPositionSpan ().ToArray ();
				GC.KeepAlive (this);
				return array;
			}
		}

		/// <summary>To be added.</summary>
		/// <param name="codepoint">To be added.</param>
		/// <param name="cluster">To be added.</param>
		/// <remarks>To be added.</remarks>
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
		}

		/// <summary>Appends the specified text to the buffer.</summary>
		/// <param name="utf8text">The array of UTF-8 characters to append.</param>
		/// <remarks></remarks>
		public void AddUtf8 (string utf8text) => AddUtf8 (Encoding.UTF8.GetBytes (utf8text), 0, -1);

		/// <summary>To be added.</summary>
		/// <param name="bytes">To be added.</param>
		/// <remarks>To be added.</remarks>
		public void AddUtf8 (byte[] bytes) => AddUtf8 (new ReadOnlySpan<byte> (bytes));

		/// <summary>To be added.</summary>
		/// <param name="text">To be added.</param>
		/// <remarks>To be added.</remarks>
		public void AddUtf8 (ReadOnlySpan<byte> text) => AddUtf8 (text, 0, -1);

		/// <summary>To be added.</summary>
		/// <param name="text">To be added.</param>
		/// <param name="itemOffset">To be added.</param>
		/// <param name="itemLength">To be added.</param>
		/// <remarks>To be added.</remarks>
		public unsafe void AddUtf8 (ReadOnlySpan<byte> text, int itemOffset, int itemLength)
		{
			fixed (byte* bytes = text) {
				AddUtf8 ((IntPtr)bytes, text.Length, itemOffset, itemLength);
			}
		}

		/// <summary>To be added.</summary>
		/// <param name="text">To be added.</param>
		/// <param name="textLength">To be added.</param>
		/// <remarks>To be added.</remarks>
		public void AddUtf8 (IntPtr text, int textLength) => AddUtf8 (text, textLength, 0, -1);

		/// <summary>To be added.</summary>
		/// <param name="text">To be added.</param>
		/// <param name="textLength">To be added.</param>
		/// <param name="itemOffset">To be added.</param>
		/// <param name="itemLength">To be added.</param>
		/// <remarks>To be added.</remarks>
		public void AddUtf8 (IntPtr text, int textLength, int itemOffset, int itemLength)
		{
			if (itemOffset < 0)
				throw new ArgumentOutOfRangeException (nameof (itemOffset), "ItemOffset must be non negative.");
			if (Length != 0 && ContentType != ContentType.Unicode)
				throw new InvalidOperationException ("Non empty buffer's ContentType must be of type Unicode.");
			if (ContentType == ContentType.Glyphs)
				throw new InvalidOperationException ("ContentType must not be Glyphs");

			HarfBuzzApi.hb_buffer_add_utf8 (Handle, (void*)text, textLength, (uint)itemOffset, itemLength);
		}

		/// <summary>To be added.</summary>
		/// <param name="text">To be added.</param>
		/// <remarks>To be added.</remarks>
		public void AddUtf16 (string text) => AddUtf16 (text, 0, -1);

		/// <summary>To be added.</summary>
		/// <param name="text">To be added.</param>
		/// <param name="itemOffset">To be added.</param>
		/// <param name="itemLength">To be added.</param>
		/// <remarks>To be added.</remarks>
		public unsafe void AddUtf16 (string text, int itemOffset, int itemLength)
		{
			fixed (char* chars = text) {
				AddUtf16 ((IntPtr)chars, text.Length, itemOffset, itemLength);
			}
		}

		/// <summary>To be added.</summary>
		/// <param name="text">To be added.</param>
		/// <remarks>To be added.</remarks>
		public unsafe void AddUtf16 (ReadOnlySpan<byte> text)
		{
			fixed (byte* bytes = text) {
				AddUtf16 ((IntPtr)bytes, text.Length / 2);
			}
		}

		/// <summary>To be added.</summary>
		/// <param name="text">To be added.</param>
		/// <remarks>To be added.</remarks>
		public void AddUtf16 (ReadOnlySpan<char> text) => AddUtf16 (text, 0, -1);

		/// <summary>To be added.</summary>
		/// <param name="text">To be added.</param>
		/// <param name="itemOffset">To be added.</param>
		/// <param name="itemLength">To be added.</param>
		/// <remarks>To be added.</remarks>
		public unsafe void AddUtf16 (ReadOnlySpan<char> text, int itemOffset, int itemLength)
		{
			fixed (char* chars = text) {
				AddUtf16 ((IntPtr)chars, text.Length, itemOffset, itemLength);
			}
		}

		/// <summary>To be added.</summary>
		/// <param name="text">To be added.</param>
		/// <param name="textLength">To be added.</param>
		/// <remarks>To be added.</remarks>
		public void AddUtf16 (IntPtr text, int textLength) =>
			AddUtf16 (text, textLength, 0, -1);

		/// <summary>To be added.</summary>
		/// <param name="text">To be added.</param>
		/// <param name="textLength">To be added.</param>
		/// <param name="itemOffset">To be added.</param>
		/// <param name="itemLength">To be added.</param>
		/// <remarks>To be added.</remarks>
		public void AddUtf16 (IntPtr text, int textLength, int itemOffset, int itemLength)
		{
			if (itemOffset < 0)
				throw new ArgumentOutOfRangeException (nameof (itemOffset), "ItemOffset must be non negative.");
			if (Length != 0 && ContentType != ContentType.Unicode)
				throw new InvalidOperationException ("Non empty buffer's ContentType must be of type Unicode.");
			if (ContentType == ContentType.Glyphs)
				throw new InvalidOperationException ("ContentType must not be of type Glyphs");

			HarfBuzzApi.hb_buffer_add_utf16 (Handle, (ushort*)text, textLength, (uint)itemOffset, itemLength);
		}

		/// <summary>To be added.</summary>
		/// <param name="text">To be added.</param>
		/// <remarks>To be added.</remarks>
		public void AddUtf32 (string text) => AddUtf32 (Encoding.UTF32.GetBytes (text));

		/// <summary>To be added.</summary>
		/// <param name="text">To be added.</param>
		/// <remarks>To be added.</remarks>
		public unsafe void AddUtf32 (ReadOnlySpan<byte> text)
		{
			fixed (byte* bytes = text) {
				AddUtf32 ((IntPtr)bytes, text.Length / 4);
			}
		}

		/// <summary>To be added.</summary>
		/// <param name="text">The span of Unicode code points to append.</param>
		/// <remarks>This function does not check the validity of the characters.</remarks>
		public void AddUtf32 (ReadOnlySpan<uint> text) => AddUtf32 (text, 0, -1);

		/// <summary>To be added.</summary>
		/// <param name="text">To be added.</param>
		/// <param name="itemOffset">To be added.</param>
		/// <param name="itemLength">To be added.</param>
		/// <remarks>To be added.</remarks>
		public unsafe void AddUtf32 (ReadOnlySpan<uint> text, int itemOffset, int itemLength)
		{
			fixed (uint* integers = text) {
				AddUtf32 ((IntPtr)integers, text.Length, itemOffset, itemLength);
			}
		}

		/// <summary>To be added.</summary>
		/// <param name="text">To be added.</param>
		/// <remarks>To be added.</remarks>
		public void AddUtf32 (ReadOnlySpan<int> text) => AddUtf32 (text, 0, -1);

		/// <summary>To be added.</summary>
		/// <param name="text">To be added.</param>
		/// <param name="itemOffset">To be added.</param>
		/// <param name="itemLength">To be added.</param>
		/// <remarks>To be added.</remarks>
		public unsafe void AddUtf32 (ReadOnlySpan<int> text, int itemOffset, int itemLength)
		{
			fixed (int* integers = text) {
				AddUtf32 ((IntPtr)integers, text.Length, itemOffset, itemLength);
			}
		}

		/// <summary>To be added.</summary>
		/// <param name="text">To be added.</param>
		/// <param name="textLength">To be added.</param>
		/// <remarks>To be added.</remarks>
		public void AddUtf32 (IntPtr text, int textLength) =>
			AddUtf32 (text, textLength, 0, -1);

		/// <summary>To be added.</summary>
		/// <param name="text">To be added.</param>
		/// <param name="textLength">To be added.</param>
		/// <param name="itemOffset">To be added.</param>
		/// <param name="itemLength">To be added.</param>
		/// <remarks>To be added.</remarks>
		public void AddUtf32 (IntPtr text, int textLength, int itemOffset, int itemLength)
		{
			if (itemOffset < 0)
				throw new ArgumentOutOfRangeException (nameof (itemOffset), "ItemOffset must be non negative.");
			if (Length != 0 && ContentType != ContentType.Unicode)
				throw new InvalidOperationException ("Non empty buffer's ContentType must be of type Unicode.");
			if (ContentType == ContentType.Glyphs)
				throw new InvalidOperationException ("ContentType must not be of type Glyphs");

			HarfBuzzApi.hb_buffer_add_utf32 (Handle, (uint*)text, textLength, (uint)itemOffset, itemLength);
		}

		/// <summary>Appends characters from the span to the buffer.</summary>
		/// <param name="text">The span of Unicode code points to append.</param>
		/// <remarks>This function does not check the validity of the characters.</remarks>
		public void AddCodepoints (ReadOnlySpan<uint> text) => AddCodepoints (text, 0, -1);

		/// <summary>To be added.</summary>
		/// <param name="text">To be added.</param>
		/// <param name="itemOffset">To be added.</param>
		/// <param name="itemLength">To be added.</param>
		/// <remarks>To be added.</remarks>
		public unsafe void AddCodepoints (ReadOnlySpan<uint> text, int itemOffset, int itemLength)
		{
			fixed (uint* codepoints = text) {
				AddCodepoints ((IntPtr)codepoints, text.Length, itemOffset, itemLength);
			}
		}

		/// <summary>To be added.</summary>
		/// <param name="text">To be added.</param>
		/// <remarks>To be added.</remarks>
		public void AddCodepoints (ReadOnlySpan<int> text) => AddCodepoints (text, 0, -1);

		/// <summary>To be added.</summary>
		/// <param name="text">To be added.</param>
		/// <param name="itemOffset">To be added.</param>
		/// <param name="itemLength">To be added.</param>
		/// <remarks>To be added.</remarks>
		public unsafe void AddCodepoints (ReadOnlySpan<int> text, int itemOffset, int itemLength)
		{
			fixed (int* codepoints = text) {
				AddCodepoints ((IntPtr)codepoints, text.Length, itemOffset, itemLength);
			}
		}

		/// <summary>To be added.</summary>
		/// <param name="text">To be added.</param>
		/// <param name="textLength">To be added.</param>
		/// <remarks>To be added.</remarks>
		public void AddCodepoints (IntPtr text, int textLength) => AddCodepoints (text, textLength, 0, -1);

		/// <summary>To be added.</summary>
		/// <param name="text">To be added.</param>
		/// <param name="textLength">To be added.</param>
		/// <param name="itemOffset">To be added.</param>
		/// <param name="itemLength">To be added.</param>
		/// <remarks>To be added.</remarks>
		public void AddCodepoints (IntPtr text, int textLength, int itemOffset, int itemLength)
		{
			if (itemOffset < 0)
				throw new ArgumentOutOfRangeException (nameof (itemOffset), "ItemOffset must be non negative.");
			if (Length != 0 && ContentType != ContentType.Unicode)
				throw new InvalidOperationException ("Non empty buffer's ContentType must be of type Unicode.");
			if (ContentType == ContentType.Glyphs)
				throw new InvalidOperationException ("ContentType must not be of type Glyphs");

			HarfBuzzApi.hb_buffer_add_codepoints (Handle, (uint*)text, textLength, (uint)itemOffset, itemLength);
		}

		/// <summary>To be added.</summary>
		/// <returns>To be added.</returns>
		/// <remarks>To be added.</remarks>
		public unsafe ReadOnlySpan<GlyphInfo> GetGlyphInfoSpan ()
		{
			uint length;
			var infoPtrs = HarfBuzzApi.hb_buffer_get_glyph_infos (Handle, &length);
			return new ReadOnlySpan<GlyphInfo> (infoPtrs, (int)length);
		}

		/// <summary>To be added.</summary>
		/// <returns>To be added.</returns>
		/// <remarks>To be added.</remarks>
		public unsafe ReadOnlySpan<GlyphPosition> GetGlyphPositionSpan ()
		{
			uint length;
			var infoPtrs = HarfBuzzApi.hb_buffer_get_glyph_positions (Handle, &length);
			return new ReadOnlySpan<GlyphPosition> (infoPtrs, (int)length);
		}

		/// <summary>To be added.</summary>
		/// <remarks>To be added.</remarks>
		public void GuessSegmentProperties ()
		{
			if (ContentType != ContentType.Unicode)
				throw new InvalidOperationException ("ContentType must be of type Unicode.");

			HarfBuzzApi.hb_buffer_guess_segment_properties (Handle);
		}

		/// <summary>To be added.</summary>
		/// <remarks>To be added.</remarks>
		public void ClearContents () => HarfBuzzApi.hb_buffer_clear_contents (Handle);

		/// <summary>To be added.</summary>
		/// <remarks>To be added.</remarks>
		public void Reset () => HarfBuzzApi.hb_buffer_reset (Handle);

		/// <summary>To be added.</summary>
		/// <param name="buffer">To be added.</param>
		/// <remarks>To be added.</remarks>
		public void Append (Buffer buffer) => Append (buffer, 0, -1);

		/// <summary>To be added.</summary>
		/// <param name="buffer">To be added.</param>
		/// <param name="start">To be added.</param>
		/// <param name="end">To be added.</param>
		/// <remarks>To be added.</remarks>
		public void Append (Buffer buffer, int start, int end)
		{
			if (buffer.Length == 0)
				throw new ArgumentException ("Buffer must be non empty.", nameof (buffer));
			if (buffer.ContentType != ContentType)
				throw new InvalidOperationException ("ContentType must be of same type.");

			HarfBuzzApi.hb_buffer_append (Handle, buffer.Handle, (uint)start, (uint)(end == -1 ? buffer.Length : end));
		}

		/// <summary>To be added.</summary>
		/// <remarks>To be added.</remarks>
		public void NormalizeGlyphs ()
		{
			if (ContentType != ContentType.Glyphs)
				throw new InvalidOperationException ("ContentType must be of type Glyphs.");
			if (GlyphPositions.Length == 0)
				throw new InvalidOperationException ("GlyphPositions can't be empty.");

			HarfBuzzApi.hb_buffer_normalize_glyphs (Handle);
		}

		/// <summary>To be added.</summary>
		/// <remarks>To be added.</remarks>
		public void Reverse () => HarfBuzzApi.hb_buffer_reverse (Handle);

		/// <summary>To be added.</summary>
		/// <param name="start">To be added.</param>
		/// <param name="end">To be added.</param>
		/// <remarks>To be added.</remarks>
		public void ReverseRange (int start, int end) =>
			HarfBuzzApi.hb_buffer_reverse_range (Handle, (uint)start, (uint)(end == -1 ? Length : end));

		/// <summary>To be added.</summary>
		/// <remarks>To be added.</remarks>
		public void ReverseClusters () => HarfBuzzApi.hb_buffer_reverse_clusters (Handle);

		/// <summary>To be added.</summary>
		/// <returns>To be added.</returns>
		/// <remarks>To be added.</remarks>
		public string SerializeGlyphs () =>
			SerializeGlyphs (0, -1, null, SerializeFormat.Text, SerializeFlag.Default);

		/// <summary>To be added.</summary>
		/// <param name="start">To be added.</param>
		/// <param name="end">To be added.</param>
		/// <returns>To be added.</returns>
		/// <remarks>To be added.</remarks>
		public string SerializeGlyphs (int start, int end) =>
			SerializeGlyphs (start, end, null, SerializeFormat.Text, SerializeFlag.Default);

		/// <summary>To be added.</summary>
		/// <param name="font">To be added.</param>
		/// <returns>To be added.</returns>
		/// <remarks>To be added.</remarks>
		public string SerializeGlyphs (Font font) =>
			SerializeGlyphs (0, -1, font, SerializeFormat.Text, SerializeFlag.Default);

		/// <summary>To be added.</summary>
		/// <param name="font">To be added.</param>
		/// <param name="format">To be added.</param>
		/// <param name="flags">To be added.</param>
		/// <returns>To be added.</returns>
		/// <remarks>To be added.</remarks>
		public string SerializeGlyphs (Font font, SerializeFormat format, SerializeFlag flags) =>
			SerializeGlyphs (0, -1, font, format, flags);

		/// <summary>To be added.</summary>
		/// <param name="start">To be added.</param>
		/// <param name="end">To be added.</param>
		/// <param name="font">To be added.</param>
		/// <param name="format">To be added.</param>
		/// <param name="flags">To be added.</param>
		/// <returns>To be added.</returns>
		/// <remarks>To be added.</remarks>
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

			return builder.ToString ();
		}

		/// <summary>To be added.</summary>
		/// <param name="data">To be added.</param>
		/// <remarks>To be added.</remarks>
		public void DeserializeGlyphs (string data) =>
			DeserializeGlyphs (data, null, SerializeFormat.Text);

		/// <summary>To be added.</summary>
		/// <param name="data">To be added.</param>
		/// <param name="font">To be added.</param>
		/// <remarks>To be added.</remarks>
		public void DeserializeGlyphs (string data, Font font) =>
			DeserializeGlyphs (data, font, SerializeFormat.Text);

		/// <summary>To be added.</summary>
		/// <param name="data">To be added.</param>
		/// <param name="font">To be added.</param>
		/// <param name="format">To be added.</param>
		/// <remarks>To be added.</remarks>
		public void DeserializeGlyphs (string data, Font font, SerializeFormat format)
		{
			if (Length != 0)
				throw new InvalidOperationException ("Buffer must be empty.");
			if (ContentType == ContentType.Glyphs)
				throw new InvalidOperationException ("ContentType must not be Glyphs.");

			HarfBuzzApi.hb_buffer_deserialize_glyphs (Handle, data, -1, null, font?.Handle ?? IntPtr.Zero, format);
		}

		/// <summary>To be added.</summary>
		/// <param name="disposing">To be added.</param>
		/// <remarks>To be added.</remarks>
		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		/// <summary>To be added.</summary>
		/// <remarks>To be added.</remarks>
		protected override void DisposeHandler ()
		{
			if (Handle != IntPtr.Zero) {
				HarfBuzzApi.hb_buffer_destroy (Handle);
			}
		}
	}
}
