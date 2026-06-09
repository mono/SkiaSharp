#nullable disable

using System;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;

namespace HarfBuzzSharp
{
	public unsafe class Buffer : NativeObject
	{
		public const int DefaultReplacementCodepoint = '\uFFFD';

		internal Buffer (IntPtr handle)
			: base (handle)
		{
		}

		public Buffer ()
			: this (HarfBuzzApi.hb_buffer_create ())
		{
		}

		public ContentType ContentType {
			get {
				var r = HarfBuzzApi.hb_buffer_get_content_type (Handle);
				return r;
			}
			set {
				HarfBuzzApi.hb_buffer_set_content_type (Handle, value);
			}
		}

		public Direction Direction {
			get {
				var r = HarfBuzzApi.hb_buffer_get_direction (Handle);
				return r;
			}
			set {
				HarfBuzzApi.hb_buffer_set_direction (Handle, value);
			}
		}

		public Language Language {
			get {
				var r = new Language (HarfBuzzApi.hb_buffer_get_language (Handle));
				return r;
			}
			set {
				HarfBuzzApi.hb_buffer_set_language (Handle, value.Handle);
				GC.KeepAlive (value);
			}
		}

		public BufferFlags Flags {
			get {
				var r = HarfBuzzApi.hb_buffer_get_flags (Handle);
				return r;
			}
			set {
				HarfBuzzApi.hb_buffer_set_flags (Handle, value);
			}
		}

		public ClusterLevel ClusterLevel {
			get {
				var r = HarfBuzzApi.hb_buffer_get_cluster_level (Handle);
				return r;
			}
			set {
				HarfBuzzApi.hb_buffer_set_cluster_level (Handle, value);
			}
		}

		public uint ReplacementCodepoint {
			get {
				var r = HarfBuzzApi.hb_buffer_get_replacement_codepoint (Handle);
				return r;
			}
			set {
				HarfBuzzApi.hb_buffer_set_replacement_codepoint (Handle, value);
			}
		}

		public uint InvisibleGlyph {
			get {
				var r = HarfBuzzApi.hb_buffer_get_invisible_glyph (Handle);
				return r;
			}
			set {
				HarfBuzzApi.hb_buffer_set_invisible_glyph (Handle, value);
			}
		}

		public Script Script {
			get {
				var r = HarfBuzzApi.hb_buffer_get_script (Handle);
				return r;
			}
			set {
				HarfBuzzApi.hb_buffer_set_script (Handle, value);
			}
		}

		public int Length {
			get {
				var r = (int)HarfBuzzApi.hb_buffer_get_length (Handle);
				return r;
			}
			set {
				HarfBuzzApi.hb_buffer_set_length (Handle, (uint)value);
			}
		}

		public UnicodeFunctions UnicodeFunctions {
			get {
				var r = new UnicodeFunctions (HarfBuzzApi.hb_buffer_get_unicode_funcs (Handle));
				return r;
			}
			set {
				HarfBuzzApi.hb_buffer_set_unicode_funcs (Handle, value.Handle);
				GC.KeepAlive (value);
			}
		}

		public GlyphInfo[] GlyphInfos {
			get {
				var array = GetGlyphInfoSpan ().ToArray ();
				return array;
			}
		}

		public GlyphPosition[] GlyphPositions {
			get {
				var array = GetGlyphPositionSpan ().ToArray ();
				return array;
			}
		}

		public void Add (int codepoint, int cluster) => Add ((uint)codepoint, (uint)cluster);

		public void Add (uint codepoint, uint cluster)
		{
			if (Length != 0 && ContentType != ContentType.Unicode)
				throw new InvalidOperationException ("Non empty buffer's ContentType must be of type Unicode.");
			if (ContentType == ContentType.Glyphs)
				throw new InvalidOperationException ("ContentType must not be of type Glyphs");

			HarfBuzzApi.hb_buffer_add (Handle, codepoint, cluster);
		}

		public void AddUtf8 (string utf8text) => AddUtf8 (Encoding.UTF8.GetBytes (utf8text), 0, -1);

		public void AddUtf8 (byte[] bytes) => AddUtf8 (new ReadOnlySpan<byte> (bytes));

		public void AddUtf8 (ReadOnlySpan<byte> text) => AddUtf8 (text, 0, -1);

		public unsafe void AddUtf8 (ReadOnlySpan<byte> text, int itemOffset, int itemLength)
		{
			fixed (byte* bytes = text) {
				AddUtf8 ((IntPtr)bytes, text.Length, itemOffset, itemLength);
			}
		}

		public void AddUtf8 (IntPtr text, int textLength) => AddUtf8 (text, textLength, 0, -1);

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

		public void AddUtf16 (string text) => AddUtf16 (text, 0, -1);

		public unsafe void AddUtf16 (string text, int itemOffset, int itemLength)
		{
			fixed (char* chars = text) {
				AddUtf16 ((IntPtr)chars, text.Length, itemOffset, itemLength);
			}
		}

		public unsafe void AddUtf16 (ReadOnlySpan<byte> text)
		{
			fixed (byte* bytes = text) {
				AddUtf16 ((IntPtr)bytes, text.Length / 2);
			}
		}

		public void AddUtf16 (ReadOnlySpan<char> text) => AddUtf16 (text, 0, -1);

		public unsafe void AddUtf16 (ReadOnlySpan<char> text, int itemOffset, int itemLength)
		{
			fixed (char* chars = text) {
				AddUtf16 ((IntPtr)chars, text.Length, itemOffset, itemLength);
			}
		}

		public void AddUtf16 (IntPtr text, int textLength) =>
			AddUtf16 (text, textLength, 0, -1);

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

		public void AddUtf32 (string text) => AddUtf32 (Encoding.UTF32.GetBytes (text));

		public unsafe void AddUtf32 (ReadOnlySpan<byte> text)
		{
			fixed (byte* bytes = text) {
				AddUtf32 ((IntPtr)bytes, text.Length / 4);
			}
		}

		public void AddUtf32 (ReadOnlySpan<uint> text) => AddUtf32 (text, 0, -1);

		public unsafe void AddUtf32 (ReadOnlySpan<uint> text, int itemOffset, int itemLength)
		{
			fixed (uint* integers = text) {
				AddUtf32 ((IntPtr)integers, text.Length, itemOffset, itemLength);
			}
		}

		public void AddUtf32 (ReadOnlySpan<int> text) => AddUtf32 (text, 0, -1);

		public unsafe void AddUtf32 (ReadOnlySpan<int> text, int itemOffset, int itemLength)
		{
			fixed (int* integers = text) {
				AddUtf32 ((IntPtr)integers, text.Length, itemOffset, itemLength);
			}
		}

		public void AddUtf32 (IntPtr text, int textLength) =>
			AddUtf32 (text, textLength, 0, -1);

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

		public void AddCodepoints (ReadOnlySpan<uint> text) => AddCodepoints (text, 0, -1);

		public unsafe void AddCodepoints (ReadOnlySpan<uint> text, int itemOffset, int itemLength)
		{
			fixed (uint* codepoints = text) {
				AddCodepoints ((IntPtr)codepoints, text.Length, itemOffset, itemLength);
			}
		}

		public void AddCodepoints (ReadOnlySpan<int> text) => AddCodepoints (text, 0, -1);

		public unsafe void AddCodepoints (ReadOnlySpan<int> text, int itemOffset, int itemLength)
		{
			fixed (int* codepoints = text) {
				AddCodepoints ((IntPtr)codepoints, text.Length, itemOffset, itemLength);
			}
		}

		public void AddCodepoints (IntPtr text, int textLength) => AddCodepoints (text, textLength, 0, -1);

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

		public unsafe ReadOnlySpan<GlyphInfo> GetGlyphInfoSpan ()
		{
			uint length;
			var infoPtrs = HarfBuzzApi.hb_buffer_get_glyph_infos (Handle, &length);
			return new ReadOnlySpan<GlyphInfo> (infoPtrs, (int)length);
		}

		public unsafe ReadOnlySpan<GlyphPosition> GetGlyphPositionSpan ()
		{
			uint length;
			var infoPtrs = HarfBuzzApi.hb_buffer_get_glyph_positions (Handle, &length);
			return new ReadOnlySpan<GlyphPosition> (infoPtrs, (int)length);
		}

		public void GuessSegmentProperties ()
		{
			if (ContentType != ContentType.Unicode)
				throw new InvalidOperationException ("ContentType must be of type Unicode.");

			HarfBuzzApi.hb_buffer_guess_segment_properties (Handle);
		}

		public void ClearContents ()
		{
			HarfBuzzApi.hb_buffer_clear_contents (Handle);
		}

		public void Reset ()
		{
			HarfBuzzApi.hb_buffer_reset (Handle);
		}

		public void Append (Buffer buffer) => Append (buffer, 0, -1);

		public void Append (Buffer buffer, int start, int end)
		{
			if (buffer.Length == 0)
				throw new ArgumentException ("Buffer must be non empty.", nameof (buffer));
			if (buffer.ContentType != ContentType)
				throw new InvalidOperationException ("ContentType must be of same type.");

			HarfBuzzApi.hb_buffer_append (Handle, buffer.Handle, (uint)start, (uint)(end == -1 ? buffer.Length : end));
			GC.KeepAlive (buffer);
		}

		public void NormalizeGlyphs ()
		{
			if (ContentType != ContentType.Glyphs)
				throw new InvalidOperationException ("ContentType must be of type Glyphs.");
			if (GlyphPositions.Length == 0)
				throw new InvalidOperationException ("GlyphPositions can't be empty.");

			HarfBuzzApi.hb_buffer_normalize_glyphs (Handle);
		}

		public void Reverse ()
		{
			HarfBuzzApi.hb_buffer_reverse (Handle);
		}

		public void ReverseRange (int start, int end)
		{
			HarfBuzzApi.hb_buffer_reverse_range (Handle, (uint)start, (uint)(end == -1 ? Length : end));
		}

		public void ReverseClusters ()
		{
			HarfBuzzApi.hb_buffer_reverse_clusters (Handle);
		}

		public string SerializeGlyphs () =>
			SerializeGlyphs (0, -1, null, SerializeFormat.Text, SerializeFlag.Default);

		public string SerializeGlyphs (int start, int end) =>
			SerializeGlyphs (start, end, null, SerializeFormat.Text, SerializeFlag.Default);

		public string SerializeGlyphs (Font font) =>
			SerializeGlyphs (0, -1, font, SerializeFormat.Text, SerializeFlag.Default);

		public string SerializeGlyphs (Font font, SerializeFormat format, SerializeFlag flags) =>
			SerializeGlyphs (0, -1, font, format, flags);

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

			return builder.ToString ();
		}

		public void DeserializeGlyphs (string data) =>
			DeserializeGlyphs (data, null, SerializeFormat.Text);

		public void DeserializeGlyphs (string data, Font font) =>
			DeserializeGlyphs (data, font, SerializeFormat.Text);

		public void DeserializeGlyphs (string data, Font font, SerializeFormat format)
		{
			if (Length != 0)
				throw new InvalidOperationException ("Buffer must be empty.");
			if (ContentType == ContentType.Glyphs)
				throw new InvalidOperationException ("ContentType must not be Glyphs.");

			HarfBuzzApi.hb_buffer_deserialize_glyphs (Handle, data, -1, null, font?.Handle ?? IntPtr.Zero, format);
			GC.KeepAlive (font);
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeHandler ()
		{
			if (Handle != IntPtr.Zero) {
				HarfBuzzApi.hb_buffer_destroy (Handle);
			}
		}
	}
}
