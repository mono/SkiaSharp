using System;
using System.Runtime.InteropServices;
using System.Text;

namespace HarfBuzzSharp
{
	public class Buffer : NativeObject
	{
		public const int DefaultReplacementCodepoint = '\uFFFD';

		public Buffer ()
			: this (HarfBuzzApi.hb_buffer_create ())
		{
		}

		internal Buffer (IntPtr handle)
			: base (handle)
		{
			Language = Language.Default;
		}

		public ContentType ContentType {
			get => HarfBuzzApi.hb_buffer_get_content_type (Handle);
			set => HarfBuzzApi.hb_buffer_set_content_type (Handle, value);
		}

		public Direction Direction {
			get => HarfBuzzApi.hb_buffer_get_direction (Handle);
			set => HarfBuzzApi.hb_buffer_set_direction (Handle, value);
		}

		public Language Language {
			get => new Language (HarfBuzzApi.hb_buffer_get_language (Handle));
			set => HarfBuzzApi.hb_buffer_set_language (Handle, value.Handle);
		}

		public Flags Flags {
			get => HarfBuzzApi.hb_buffer_get_flags (Handle);
			set => HarfBuzzApi.hb_buffer_set_flags (Handle, value);
		}

		public ClusterLevel ClusterLevel {
			get => HarfBuzzApi.hb_buffer_get_cluster_level (Handle);
			set => HarfBuzzApi.hb_buffer_set_cluster_level (Handle, value);
		}

		public uint ReplacementCodepoint {
			get => HarfBuzzApi.hb_buffer_get_replacement_codepoint (Handle);
			set => HarfBuzzApi.hb_buffer_set_replacement_codepoint (Handle, value);
		}

		public uint InvisibleGlyph {
			get => HarfBuzzApi.hb_buffer_get_invisible_glyph (Handle);
			set => HarfBuzzApi.hb_buffer_set_invisible_glyph (Handle, value);
		}

		public Script Script {
			get => HarfBuzzApi.hb_buffer_get_script (Handle);
			set => HarfBuzzApi.hb_buffer_set_script (Handle, value);
		}

		public int Length {
			get => HarfBuzzApi.hb_buffer_get_length (Handle);
			set => HarfBuzzApi.hb_buffer_set_length (Handle, value);
		}

		public UnicodeFunctions UnicodeFunctions {
			get => new UnicodeFunctions (HarfBuzzApi.hb_buffer_get_unicode_funcs (Handle));
			set => HarfBuzzApi.hb_buffer_set_unicode_funcs (Handle, value.Handle);
		}

		public unsafe ReadOnlySpan<GlyphInfo> GlyphInfos {
			get {
				var infoPtrs = HarfBuzzApi.hb_buffer_get_glyph_infos (Handle, out var length);
				return new ReadOnlySpan<GlyphInfo> (infoPtrs, length);
			}
		}

		public unsafe ReadOnlySpan<GlyphPosition> GlyphPositions {
			get {
				var infoPtrs = HarfBuzzApi.hb_buffer_get_glyph_positions (Handle, out var length);
				return new ReadOnlySpan<GlyphPosition> (infoPtrs, length);
			}
		}

		public unsafe void Add (uint codepoint, uint cluster)
		{
			if (Length != 0 && ContentType != ContentType.Unicode) {
				throw new InvalidOperationException ("Non empty buffer's ContentType must be of type Unicode.");
			}

			if (ContentType == ContentType.Glyphs) {
				throw new InvalidOperationException ("ContentType must not be of type Glyphs");
			}

			HarfBuzzApi.hb_buffer_add (Handle, codepoint, cluster);
		}

		public unsafe void Add (int codepoint, int cluster)
		{
			if (codepoint < 0) {
				throw new ArgumentOutOfRangeException (nameof (codepoint), "Codepoint must be non negative.");
			}

			if (cluster < 0) {
				throw new ArgumentOutOfRangeException (nameof (codepoint), "Cluster must be non negative.");
			}

			if (Length != 0 && ContentType != ContentType.Unicode) {
				throw new InvalidOperationException ("Non empty buffer's ContentType must be of type Unicode.");
			}

			if (ContentType == ContentType.Glyphs) {
				throw new InvalidOperationException ("ContentType must not be of type Glyphs");
			}

			HarfBuzzApi.hb_buffer_add (Handle, (uint)codepoint, (uint)cluster);
		}

		public void AddUtf8 (string utf8text)
		{
			var bytes = Encoding.UTF8.GetBytes (utf8text);
			AddUtf8 (bytes, 0, -1);
		}

		public unsafe void AddUtf8 (byte[] bytes, int itemOffset = 0, int itemLength = -1)
		{
			fixed (byte* b = bytes) {
				AddUtf8 ((IntPtr)b, bytes.Length, itemOffset, itemLength);
			}
		}

		public unsafe void AddUtf8 (ReadOnlySpan<byte> text, int itemOffset = 0, int itemLength = -1)
		{
			fixed (byte* bytes = text) {
				AddUtf8 ((IntPtr)bytes, text.Length, itemOffset, itemLength);
			}
		}

		public void AddUtf8 (IntPtr text, int textLength, int itemOffset = 0, int itemLength = -1)
		{
			if (itemOffset < 0) {
				throw new ArgumentOutOfRangeException (nameof (itemOffset), "ItemOffset must be non negative.");
			}

			if (Length != 0 && ContentType != ContentType.Unicode) {
				throw new InvalidOperationException ("Non empty buffer's ContentType must be of type Unicode.");
			}

			if (ContentType == ContentType.Glyphs) {
				throw new InvalidOperationException ("ContentType must not be Glyphs");
			}

			HarfBuzzApi.hb_buffer_add_utf8 (Handle, text, textLength, itemOffset, itemLength);
		}

		public unsafe void AddUtf16 (string text, int itemOffset = 0, int itemLength = -1)
		{
			fixed (char* chars = text) {
				AddUtf16 ((IntPtr)chars, text.Length, itemOffset, itemLength);
			}
		}

		public unsafe void AddUtf16 (byte[] text)
		{
			fixed (byte* bytes = text) {
				AddUtf16 ((IntPtr)bytes, text.Length / 2);
			}
		}

		public unsafe void AddUtf16 (ReadOnlySpan<char> text, int itemOffset = 0, int itemLength = -1)
		{
			fixed (char* chars = text) {
				AddUtf16 ((IntPtr)chars, text.Length, itemOffset, itemLength);
			}
		}

		public void AddUtf16 (IntPtr text, int textLength, int itemOffset = 0, int itemLength = -1)
		{
			if (itemOffset < 0) {
				throw new ArgumentOutOfRangeException (nameof (itemOffset), "ItemOffset must be non negative.");
			}

			if (Length != 0 && ContentType != ContentType.Unicode) {
				throw new InvalidOperationException ("Non empty buffer's ContentType must be of type Unicode.");
			}

			if (ContentType == ContentType.Glyphs) {
				throw new InvalidOperationException ("ContentType must not be of type Glyphs");
			}

			HarfBuzzApi.hb_buffer_add_utf16 (Handle, text, textLength, itemOffset, itemLength);
		}

		public void AddUtf32 (string text)
		{
			var bytes = Encoding.UTF32.GetBytes (text);
			AddUtf32 (bytes);
		}

		public unsafe void AddUtf32 (byte[] text)
		{
			fixed (byte* bytes = text) {
				AddUtf32 ((IntPtr)bytes, text.Length / 4);
			}
		}

		public unsafe void AddUtf32 (ReadOnlySpan<int> text, int itemOffset = 0, int itemLength = -1)
		{
			fixed (int* integers = text) {
				AddUtf32 ((IntPtr)integers, text.Length, itemOffset, itemLength);
			}
		}

		public unsafe void AddUtf32 (ReadOnlySpan<uint> text, int itemOffset = 0, int itemLength = -1)
		{
			fixed (uint* integers = text) {
				AddUtf32 ((IntPtr)integers, text.Length, itemOffset, itemLength);
			}
		}

		public void AddUtf32 (IntPtr text, int textLength, int itemOffset = 0, int itemLength = -1)
		{
			if (itemOffset < 0) {
				throw new ArgumentOutOfRangeException (nameof (itemOffset), "ItemOffset must be non negative.");
			}

			if (Length != 0 && ContentType != ContentType.Unicode) {
				throw new InvalidOperationException ("Non empty buffer's ContentType must be of type Unicode.");
			}

			if (ContentType == ContentType.Glyphs) {
				throw new InvalidOperationException ("ContentType must not be of type Glyphs");
			}

			HarfBuzzApi.hb_buffer_add_utf32 (Handle, text, textLength, itemOffset, itemLength);
		}

		public unsafe void AddCodepoints (int[] text, int itemOffset = 0, int itemLength = -1)
		{
			fixed (int* codepoints = text) {
				AddCodepoints ((IntPtr)codepoints, text.Length, itemOffset, itemLength);
			}
		}

		public unsafe void AddCodepoints (uint[] text, int itemOffset = 0, int itemLength = -1)
		{
			fixed (uint* codepoints = text) {
				AddCodepoints ((IntPtr)codepoints, text.Length, itemOffset, itemLength);
			}
		}

		public unsafe void AddCodepoints (ReadOnlySpan<int> text, int itemOffset = 0, int itemLength = -1)
		{
			fixed (int* codepoints = text) {
				AddCodepoints ((IntPtr)codepoints, text.Length, itemOffset, itemLength);
			}
		}

		public unsafe void AddCodepoints (ReadOnlySpan<uint> text, int itemOffset = 0, int itemLength = -1)
		{
			fixed (uint* codepoints = text) {
				AddCodepoints ((IntPtr)codepoints, text.Length, itemOffset, itemLength);
			}
		}

		public void AddCodepoints (IntPtr text, int textLength, int itemOffset = 0, int itemLength = -1)
		{
			if (itemOffset < 0) {
				throw new ArgumentOutOfRangeException (nameof (itemOffset), "ItemOffset must be non negative.");
			}

			if (Length != 0 && ContentType != ContentType.Unicode) {
				throw new InvalidOperationException ("Non empty buffer's ContentType must be of type Unicode.");
			}

			if (ContentType == ContentType.Glyphs) {
				throw new InvalidOperationException ("ContentType must not be of type Glyphs");
			}

			HarfBuzzApi.hb_buffer_add_codepoints (Handle, text, textLength, itemOffset, itemLength);
		}

		public void GuessSegmentProperties () => HarfBuzzApi.hb_buffer_guess_segment_properties (Handle);

		public void ClearContents () => HarfBuzzApi.hb_buffer_clear_contents (Handle);

		public void Reset () => HarfBuzzApi.hb_buffer_reset (Handle);

		public void Append (Buffer buffer, int start = 0, int end = -1)
		{
			HarfBuzzApi.hb_buffer_append (Handle, buffer.Handle, start, end == -1 ? buffer.Length : end);
		}

		public void NormalizeGlyphs ()
		{
			if (ContentType != ContentType.Glyphs) {
				throw new InvalidOperationException ("ContentType must be of type Glyphs.");
			}

			if (GlyphPositions.Length == 0) {
				throw new InvalidOperationException ("GlyphPositions can't be empty.");
			}

			HarfBuzzApi.hb_buffer_normalize_glyphs (Handle);
		}

		public void Reverse () => HarfBuzzApi.hb_buffer_reverse (Handle);

		public void ReverseRange (int start, int end = -1)
		{
			HarfBuzzApi.hb_buffer_reverse_range (Handle, start, end == -1 ? Length : end);
		}

		public void ReverseClusters () => HarfBuzzApi.hb_buffer_reverse_clusters (Handle);

		public string SerializeGlyphs (
			int start = 0,
			int end = -1,
			Font font = null,
			SerializeFormat format = SerializeFormat.Text,
			SerializeFlag flags = SerializeFlag.Default)
		{
			const int bufferSize = 128;

			if (Length == 0) {
				throw new InvalidOperationException ("Buffer should not be empty.");
			}

			if (ContentType != ContentType.Glyphs) {
				throw new InvalidOperationException ("ContentType should be of type Glyphs.");
			}

			if (end == -1) {
				end = Length;
			}

			var builder = new StringBuilder (128);
			var buffer = Marshal.AllocHGlobal (bufferSize);
			var currentPosition = start;

			try {
				while (currentPosition < end) {
					currentPosition += HarfBuzzApi.hb_buffer_serialize_glyphs (
						Handle,
						currentPosition,
						end,
						buffer,
						bufferSize,
						out var consumed,
						font?.Handle ?? IntPtr.Zero,
						format,
						flags);

					builder.Append (Marshal.PtrToStringAnsi (buffer, consumed));
				}

			} finally {
				Marshal.FreeHGlobal (buffer);
			}

			return builder.ToString ();
		}

		public void DeserializeGlyphs (string data, Font font = null, SerializeFormat format = SerializeFormat.Text)
		{
			if (Length != 0) {
				throw new InvalidOperationException ("Buffer must be empty.");
			}

			if (ContentType == ContentType.Glyphs) {
				throw new InvalidOperationException ("ContentType must not be Glyphs.");
			}

			HarfBuzzApi.hb_buffer_deserialize_glyphs (Handle, data, -1, out _, font?.Handle ?? IntPtr.Zero, format);
		}
		protected override void DisposeHandler()
		{
			if (Handle != IntPtr.Zero) {
				HarfBuzzApi.hb_buffer_destroy (Handle);
			}
		}
	}
}
