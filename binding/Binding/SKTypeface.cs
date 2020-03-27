using System;
using System.ComponentModel;
using System.IO;

namespace SkiaSharp
{
	[EditorBrowsable (EditorBrowsableState.Never)]
	[Flags]
	[Obsolete ("Use SKFontStyleWeight and SKFontStyleSlant instead.")]
	public enum SKTypefaceStyle
	{
		Normal = 0,
		Bold = 0x01,
		Italic = 0x02,
		BoldItalic = 0x03
	}

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

		public static SKTypeface Default => defaultTypeface.Value;

		public static SKTypeface CreateDefault ()
		{
			return GetObject (SkiaApi.sk_typeface_create_default ());
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use FromFamilyName(string, SKFontStyleWeight, SKFontStyleWidth, SKFontStyleSlant) instead.")]
		public static SKTypeface FromFamilyName (string familyName, SKTypefaceStyle style)
		{
			var weight = style.HasFlag (SKTypefaceStyle.Bold) ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal;
			var slant = style.HasFlag (SKTypefaceStyle.Italic) ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright;

			return FromFamilyName (familyName, weight, SKFontStyleWidth.Normal, slant);
		}

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

			return GetObject (SkiaApi.sk_typeface_create_from_name_with_font_style (familyName, style.Handle));
		}

		public static SKTypeface FromFamilyName (string familyName, SKFontStyleWeight weight, SKFontStyleWidth width, SKFontStyleSlant slant)
		{
			return FromFamilyName (familyName, (int)weight, (int)width, slant);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete]
		public static SKTypeface FromTypeface (SKTypeface typeface, SKTypefaceStyle style)
		{
			if (typeface == null)
				throw new ArgumentNullException (nameof (typeface));

			var weight = style.HasFlag (SKTypefaceStyle.Bold) ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal;
			var width = SKFontStyleWidth.Normal;
			var slant = style.HasFlag (SKTypefaceStyle.Italic) ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright;

			return SKFontManager.Default.MatchTypeface (typeface, new SKFontStyle (weight, width, slant));
		}

		public static SKTypeface FromFile (string path, int index = 0)
		{
			if (path == null)
				throw new ArgumentNullException (nameof (path));

			var utf8path = StringUtilities.GetEncodedText (path, SKEncoding.Utf8);
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
			return SKTypeface.FromStream (new SKMemoryStream (data), index);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use GetGlyphs(string, out ushort[]) instead.")]
		public int CharsToGlyphs (string chars, out ushort[] glyphs)
			=> GetGlyphs (chars, out glyphs);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use GetGlyphs(IntPtr, int, SKEncoding, out ushort[]) instead.")]
		public int CharsToGlyphs (IntPtr str, int strlen, SKEncoding encoding, out ushort[] glyphs)
			=> GetGlyphs (str, strlen, encoding, out glyphs);

		public string FamilyName => (string)SKString.GetObject (SkiaApi.sk_typeface_get_family_name (Handle));

		public SKFontStyle FontStyle => SKFontStyle.GetObject (SkiaApi.sk_typeface_get_fontstyle (Handle));

		public int FontWeight => SkiaApi.sk_typeface_get_font_weight (Handle);

		public int FontWidth => SkiaApi.sk_typeface_get_font_width (Handle);

		public SKFontStyleSlant FontSlant => SkiaApi.sk_typeface_get_font_slant (Handle);

		public bool IsBold => FontStyle.Weight >= (int)SKFontStyleWeight.SemiBold;

		public bool IsItalic => FontStyle.Slant != SKFontStyleSlant.Upright;

		public bool IsFixedPitch => SkiaApi.sk_typeface_is_fixed_pitch (Handle);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use FontWeight and FontSlant instead.")]
		public SKTypefaceStyle Style {
			get {
				var style = SKTypefaceStyle.Normal;
				if (FontWeight >= (int)SKFontStyleWeight.SemiBold)
					style |= SKTypefaceStyle.Bold;
				if (FontSlant != (int)SKFontStyleSlant.Upright)
					style |= SKTypefaceStyle.Italic;
				return style;
			}
		}

		public int UnitsPerEm => SkiaApi.sk_typeface_get_units_per_em (Handle);

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

		public int GetTableSize (UInt32 tag) =>
			(int)SkiaApi.sk_typeface_get_table_size (Handle, tag);

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

		public int CountGlyphs (string str) => CountGlyphs (str, SKEncoding.Utf16);

		public int CountGlyphs (string str, SKEncoding encoding)
		{
			if (str == null)
				throw new ArgumentNullException (nameof (str));

			var bytes = StringUtilities.GetEncodedText (str, encoding);
			return CountGlyphs (bytes, encoding);
		}

		public int CountGlyphs (byte[] str, SKEncoding encoding) =>
			CountGlyphs (new ReadOnlySpan<byte> (str), encoding);

		public int CountGlyphs (ReadOnlySpan<byte> str, SKEncoding encoding)
		{
			if (str == null)
				throw new ArgumentNullException (nameof (str));

			fixed (byte* p = str) {
				return CountGlyphs ((IntPtr)p, str.Length, encoding);
			}
		}

		public int CountGlyphs (IntPtr str, int strLen, SKEncoding encoding)
		{
			if (str == IntPtr.Zero && strLen != 0)
				throw new ArgumentNullException (nameof (str));

			return SkiaApi.sk_typeface_chars_to_glyphs (Handle, (byte*)str, encoding, null, strLen);
		}

		public int GetGlyphs (string text, out ushort[] glyphs) => GetGlyphs (text, SKEncoding.Utf16, out glyphs);

		public int GetGlyphs (string text, SKEncoding encoding, out ushort[] glyphs)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			var bytes = StringUtilities.GetEncodedText (text, encoding);
			return GetGlyphs (bytes, encoding, out glyphs);
		}

		public int GetGlyphs (byte[] text, SKEncoding encoding, out ushort[] glyphs) =>
			GetGlyphs (new ReadOnlySpan<byte> (text), encoding, out glyphs);

		public int GetGlyphs (ReadOnlySpan<byte> text, SKEncoding encoding, out ushort[] glyphs)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			fixed (byte* p = text) {
				return GetGlyphs ((IntPtr)p, text.Length, encoding, out glyphs);
			}
		}

		public int GetGlyphs (IntPtr text, int length, SKEncoding encoding, out ushort[] glyphs)
		{
			if (text == IntPtr.Zero && length != 0)
				throw new ArgumentNullException (nameof (text));

			var n = SkiaApi.sk_typeface_chars_to_glyphs (Handle, (void*)text, encoding, null, length);

			if (n <= 0) {
				glyphs = new ushort[0];
				return 0;
			}

			glyphs = new ushort[n];
			fixed (ushort* gp = glyphs) {
				return SkiaApi.sk_typeface_chars_to_glyphs (Handle, (void*)text, encoding, gp, n);
			}
		}

		public ushort[] GetGlyphs (string text) => GetGlyphs (text, SKEncoding.Utf16);

		public ushort[] GetGlyphs (string text, SKEncoding encoding)
		{
			GetGlyphs (text, encoding, out var glyphs);
			return glyphs;
		}

		public ushort[] GetGlyphs (byte[] text, SKEncoding encoding) =>
			GetGlyphs (new ReadOnlySpan<byte> (text), encoding);

		public ushort[] GetGlyphs (ReadOnlySpan<byte> text, SKEncoding encoding)
		{
			GetGlyphs (text, encoding, out var glyphs);
			return glyphs;
		}

		public ushort[] GetGlyphs (IntPtr text, int length, SKEncoding encoding)
		{
			GetGlyphs (text, length, encoding, out var glyphs);
			return glyphs;
		}

		public SKStreamAsset OpenStream () =>
			OpenStream (out _);

		public SKStreamAsset OpenStream (out int ttcIndex)
		{
			fixed (int* ttc = &ttcIndex) {
				return SKStreamAssetImplementation.GetObject (SkiaApi.sk_typeface_open_stream (Handle, ttc));
			}
		}

		internal static SKTypeface GetObject (IntPtr ptr, bool owns = true, bool unrefExisting = true)
		{
			if (GetInstance<SKTypeface> (ptr, out var instance)) {
				if (unrefExisting && instance is ISKReferenceCounted refcnt) {
#if THROW_OBJECT_EXCEPTIONS
					if (refcnt.GetReferenceCount () == 1)
						throw new InvalidOperationException (
							$"About to unreference an object that has no references. " +
							$"H: {ptr:x} Type: {instance.GetType ()}");
#endif
					refcnt.SafeUnRef ();
				}
				return instance;
			}

			return new SKTypeface (ptr, owns);
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
