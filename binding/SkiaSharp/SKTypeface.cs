#nullable disable

using System;
using System.ComponentModel;
using System.IO;
using System.Threading;

namespace SkiaSharp
{
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

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

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

		public static SKTypeface Empty =>
			LazyInitializer.EnsureInitialized (
				ref empty, ref emptyInitialized, ref emptyLock,
				// Immortal Skia singleton (SkNoDestructor<SkEmptyTypeface>) — never unref it.
				// See SKColorFilter.GetDisposeProtectedObject for the full teardown-crash rationale.
				() => GetDisposeProtectedObject (SkiaApi.sk_typeface_create_empty (), owns: false, unrefExisting: false));

		public bool IsEmpty => GlyphCount == 0;

		public static SKTypeface CreateDefault ()
		{
			var matched = SkiaApi.sk_fontmgr_legacy_create_typeface (
				SKFontManager.Default.Handle, IntPtr.Zero, SKFontStyle.Normal.Handle);
			return matched == IntPtr.Zero
				? Empty
				: GetObject (matched);
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

		public static SKTypeface FromFamilyName (string familyName, SKFontStyle style) =>
			SKFontManager.Default.MatchFamily (familyName, style) ?? Default;

		public static SKTypeface FromFamilyName (string familyName, SKFontStyleWeight weight, SKFontStyleWidth width, SKFontStyleSlant slant)
		{
			return FromFamilyName (familyName, (int)weight, (int)width, slant);
		}

		// From*

		public static SKTypeface FromFile (string path, int index = 0) =>
			SKFontManager.Default.CreateTypeface (path, index);

		public static SKTypeface FromStream (Stream stream, int index = 0) =>
			SKFontManager.Default.CreateTypeface (stream, index);

		public static SKTypeface FromStream (SKStreamAsset stream, int index = 0) =>
			SKFontManager.Default.CreateTypeface (stream, index);

		public static SKTypeface FromData (SKData data, int index = 0) =>
			SKFontManager.Default.CreateTypeface (data, index);

		// Properties

		public string FamilyName {
			get {
				var r = (string)SKString.GetObject (SkiaApi.sk_typeface_get_family_name (Handle));
				GC.KeepAlive (this);
				return r;
			}
		}

		public SKFontStyle FontStyle {
			get {
				var r = SKFontStyle.GetObject (SkiaApi.sk_typeface_get_fontstyle (Handle));
				GC.KeepAlive (this);
				return r;
			}
		}

		public int FontWeight {
			get {
				var r = SkiaApi.sk_typeface_get_font_weight (Handle);
				GC.KeepAlive (this);
				return r;
			}
		}

		public int FontWidth {
			get {
				var r = SkiaApi.sk_typeface_get_font_width (Handle);
				GC.KeepAlive (this);
				return r;
			}
		}

		public SKFontStyleSlant FontSlant {
			get {
				var r = SkiaApi.sk_typeface_get_font_slant (Handle);
				GC.KeepAlive (this);
				return r;
			}
		}

		public bool IsBold => FontStyle.Weight >= (int)SKFontStyleWeight.SemiBold;

		public bool IsItalic => FontStyle.Slant != SKFontStyleSlant.Upright;

		public bool IsFixedPitch {
			get {
				var r = SkiaApi.sk_typeface_is_fixed_pitch (Handle);
				GC.KeepAlive (this);
				return r;
			}
		}

		public int UnitsPerEm {
			get {
				var r = SkiaApi.sk_typeface_get_units_per_em (Handle);
				GC.KeepAlive (this);
				return r;
			}
		}

		public int GlyphCount {
			get {
				var r = SkiaApi.sk_typeface_count_glyphs (Handle);
				GC.KeepAlive (this);
				return r;
			}
		}

		public string PostScriptName {
			get {
				var r = (string)SKString.GetObject (SkiaApi.sk_typeface_get_post_script_name (Handle));
				GC.KeepAlive (this);
				return r;
			}
		}

		// GetTableTags

		public int TableCount {
			get {
				var r = SkiaApi.sk_typeface_count_tables (Handle);
				GC.KeepAlive (this);
				return r;
			}
		}

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

		public int GetTableSize (UInt32 tag)
		{
			var r = (int)SkiaApi.sk_typeface_get_table_size (Handle, tag);
			GC.KeepAlive (this);
			return r;
		}

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
			GC.KeepAlive (this);
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

		public ushort[] GetGlyphs (ReadOnlySpan<char> text)
		{
			using var font = ToFont ();
			return font.GetGlyphs (text);
		}

		public ushort[] GetGlyphs (ReadOnlySpan<byte> text, SKTextEncoding encoding)
		{
			using var font = ToFont ();
			return font.GetGlyphs (text, encoding);
		}

		public ushort[] GetGlyphs (IntPtr text, int length, SKTextEncoding encoding)
		{
			using var font = ToFont ();
			return font.GetGlyphs (text, length * encoding.GetCharacterByteSize (), encoding);
		}

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
				var r = SKStreamAsset.GetObject (SkiaApi.sk_typeface_open_stream (Handle, ttc));
				GC.KeepAlive (this);
				return r;
			}
		}

		// GetKerningPairAdjustments

		/// <summary>
		/// If false, then <see cref="GetKerningPairAdjustments(ReadOnlySpan{ushort})"/> will never return nonzero
		/// adjustments for any possible pair of glyphs.
		/// </summary>
		public bool HasGetKerningPairAdjustments {
			get {
				var r = SkiaApi.sk_typeface_get_kerning_pair_adjustments (Handle, null, 0, null);
				GC.KeepAlive (this);
				return r;
			}
		}

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

		public int VariationDesignParameterCount {
			get {
				var r = SkiaApi.sk_typeface_get_variation_design_parameters (Handle, null, 0);
				GC.KeepAlive (this);
				return r;
			}
		}

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

		public int VariationDesignPositionCount {
			get {
				var r = SkiaApi.sk_typeface_get_variation_design_position (Handle, null, 0);
				GC.KeepAlive (this);
				return r;
			}
		}

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

		public SKTypeface Clone (ReadOnlySpan<SKFontVariationPositionCoordinate> position)
		{
			fixed (SKFontVariationPositionCoordinate* ptr = position) {
				var r = GetObject (SkiaApi.sk_typeface_clone_with_arguments (Handle, ptr, position.Length, 0, 0, null, 0));
				GC.KeepAlive (this);
				return r;
			}
		}

		public SKTypeface Clone (int paletteIndex)
		{
			if (paletteIndex < 0)
				throw new ArgumentOutOfRangeException (nameof (paletteIndex));
			var r = GetObject (SkiaApi.sk_typeface_clone_with_arguments (Handle, null, 0, 0, paletteIndex, null, 0));
			GC.KeepAlive (this);
			return r;
		}

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
