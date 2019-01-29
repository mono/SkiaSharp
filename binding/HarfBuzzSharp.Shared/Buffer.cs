using System;
using System.Text;

namespace HarfBuzzSharp
{
	using System.Runtime.InteropServices;

	public class Buffer : NativeObject
	{
		internal Buffer (IntPtr handle)
			: base (handle)
		{
		}

		public Buffer ()
			: this (HarfBuzzApi.hb_buffer_create ())
		{
		}

		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero) {
				HarfBuzzApi.hb_buffer_destroy (Handle);
			}

			base.Dispose (disposing);
		}

		public ContentType ContentType {
			get { return HarfBuzzApi.hb_buffer_get_content_type (Handle); }
			set { HarfBuzzApi.hb_buffer_set_content_type (Handle, value); }
		}

		public Direction Direction {
			get { return HarfBuzzApi.hb_buffer_get_direction (Handle); }
			set { HarfBuzzApi.hb_buffer_set_direction (Handle, value); }
		}

		public Language Language {
			get { return new Language (HarfBuzzApi.hb_buffer_get_language (Handle)); }
			set { HarfBuzzApi.hb_buffer_set_language (Handle, value.Handle); }
		}

		public Flags Flags {
			get { return HarfBuzzApi.hb_buffer_get_flags (Handle); }
			set { HarfBuzzApi.hb_buffer_set_flags (Handle, value); }
		}

		public ClusterLevel ClusterLevel {
			get { return HarfBuzzApi.hb_buffer_get_cluster_level (Handle); }
			set { HarfBuzzApi.hb_buffer_set_cluster_level (Handle, value); }
		}

		public uint ReplacementCodepoint {
			get { return HarfBuzzApi.hb_buffer_get_replacement_codepoint (Handle); }
			set { HarfBuzzApi.hb_buffer_set_replacement_codepoint (Handle, value); }
		}

		public Tag Script {
			set { HarfBuzzApi.hb_buffer_set_script (Handle, value); }
			get { return HarfBuzzApi.hb_buffer_get_script (Handle); }
		}

		public uint Length {
			set { HarfBuzzApi.hb_buffer_set_length (Handle, value); }
			get { return HarfBuzzApi.hb_buffer_get_length (Handle); }
		}

		public void Add (uint codepoint, uint cluster)
		{
			HarfBuzzApi.hb_buffer_add (Handle, codepoint, cluster);
		}

		public void AddUtf8 (string utf8text)
		{
			var bytes = Encoding.UTF8.GetBytes (utf8text);
			AddUtf8 (bytes, 0, -1);
		}

		public void AddUtf8 (byte[] bytes)
		{
			AddUtf8 (bytes, 0, -1);
		}
		public unsafe void AddUtf8 (byte[] bytes, uint itemOffset, int itemLength)
		{
			fixed (byte* b = bytes) {
				AddUtf8 ((IntPtr)b, bytes.Length, itemOffset, itemLength);
			}
		}
		//public unsafe void AddUtf8 (ReadOnlySpan<byte> text, uint itemOffset = 0, int itemLength = -1)
		//{
		//	fixed (byte* bytes = text) {
		//		AddUtf8 ((IntPtr)bytes, text.Length, itemOffset, itemLength);
		//	}
		//}

		public void AddUtf8 (IntPtr text, int textLength, uint itemOffset = 0, int itemLength = -1)
		{
			HarfBuzzApi.hb_buffer_add_utf8 (Handle, text, textLength, itemOffset, itemLength);
		}

		public unsafe void AddUtf16 (string text, uint itemOffset = 0, int itemLength = -1)
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

		//public unsafe void AddUtf16 (ReadOnlySpan<char> text, uint itemOffset = 0, int itemLength = -1)
		//{
		//	fixed (char* chars = text) {
		//		AddUtf16 ((IntPtr)chars, text.Length, itemOffset, itemLength);
		//	}
		//}

		public void AddUtf16 (IntPtr text, int textLength, uint itemOffset = 0, int itemLength = -1)
		{
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

		//public unsafe void AddUtf32 (ReadOnlySpan<uint> text, uint itemOffset = 0, int itemLength = -1)
		//{
		//	fixed (uint* integers = text) {
		//		AddUtf32 ((IntPtr)integers, text.Length, itemOffset, itemLength);
		//	}
		//}

		public void AddUtf32 (IntPtr text, int textLength, uint itemOffset = 0, int itemLength = -1)
		{
			HarfBuzzApi.hb_buffer_add_utf32 (Handle, text, textLength, itemOffset, itemLength);
		}

		public void GuessSegmentProperties () => HarfBuzzApi.hb_buffer_guess_segment_properties (Handle);

		public void ClearContents () => HarfBuzzApi.hb_buffer_clear_contents (Handle);

		public void Reset () => HarfBuzzApi.hb_buffer_reset (Handle);

		public GlyphInfo[] GlyphInfos {
			get {
				var infoPtrs = HarfBuzzApi.hb_buffer_get_glyph_infos (Handle, out var length);
				return PtrToStructureArray<GlyphInfo> (infoPtrs, (int)length);
			}
		}

		public GlyphPosition[] GlyphPositions {
			get {
				var infoPtrs = HarfBuzzApi.hb_buffer_get_glyph_positions (Handle, out var length);
				return PtrToStructureArray<GlyphPosition> (infoPtrs, (int)length);
			}
		}

		public string SerializeGlyphs (
			uint start,
			uint end,
			Font font = null,
			SerializeFormat format = SerializeFormat.Text,
			SerializeFlag flags = SerializeFlag.Default)
		{
			const uint bufferSize = 128;

			var builder = new StringBuilder (128);
			var buffer = Marshal.AllocHGlobal ((int)bufferSize);
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

					builder.Append (Marshal.PtrToStringAnsi (buffer, (int)consumed));
				}

			} finally {
				Marshal.FreeHGlobal (buffer);
			}

			return builder.ToString ();
		}

		public void DeserializeGlyphs (string data, Font font = null, SerializeFormat format = SerializeFormat.Text)
		{
			var bytes = Encoding.ASCII.GetBytes (data);

			HarfBuzzApi.hb_buffer_deserialize_glyphs (Handle, bytes, bytes.Length, out _, font?.Handle ?? IntPtr.Zero, format);
		}
	}
}
