#nullable disable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace SkiaSharp
{
	/// <summary>Manages a collection of fonts.</summary>
	/// <remarks />
	public unsafe class SKFontManager : SKObject, ISKReferenceCounted
	{
		private static SKFontManager defaultManager;
		private static bool defaultManagerInitialized;
		private static object defaultManagerLock = new object ();

		internal SKFontManager (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.SKFontManager" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:SkiaSharp.SKFontManager" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		/// <summary>Gets the default font manager.</summary>
		/// <value>The default font manager.</value>
		/// <remarks />
		public static SKFontManager Default =>
			LazyInitializer.EnsureInitialized (
				ref defaultManager, ref defaultManagerInitialized, ref defaultManagerLock,
				() => GetDisposeProtectedObject (SkiaApi.sk_fontmgr_create_default ()));

		/// <summary>Gets the number of font families available.</summary>
		/// <value>The number of font families.</value>
		/// <remarks />
		public int FontFamilyCount {
			get {
				var r = SkiaApi.sk_fontmgr_count_families (Handle);
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Gets all the font family names loaded by this font manager.</summary>
		/// <value>The collection of font family names.</value>
		/// <remarks />
		public IEnumerable<string> FontFamilies {
			get {
				var count = FontFamilyCount;
				for (var i = 0; i < count; i++) {
					yield return GetFamilyName (i);
				}
			}
		}

		/// <summary>Returns the font family name for the specified index.</summary>
		/// <param name="index">The index of the font family name to retrieve.</param>
		/// <returns>Returns the font family name.</returns>
		/// <remarks />
		public string GetFamilyName (int index)
		{
			using var str = new SKString ();
			SkiaApi.sk_fontmgr_get_family_name (Handle, index, str.Handle);
			GC.KeepAlive (this);
			return (string)str;
		}

		/// <summary>Returns all the font family names loaded by this font manager.</summary>
		/// <returns>Returns an array of all the font family names loaded by this font manager.</returns>
		/// <remarks />
		public string[] GetFontFamilies () => FontFamilies.ToArray ();

		/// <summary>Returns the font style set for the specified index.</summary>
		/// <param name="index">The index of the font style set to retrieve.</param>
		/// <returns>Returns the font style set.</returns>
		/// <remarks>The index must be in the range of [0, <see cref="P:SkiaSharp.SKFontManager.FontFamilyCount" />).</remarks>
		public SKFontStyleSet GetFontStyles (int index)
		{
			var styleSet = SKFontStyleSet.GetObject (SkiaApi.sk_fontmgr_create_styleset (Handle, index));
			GC.KeepAlive (this);
			return styleSet;
		}

		/// <summary>Use the system fallback to find the typeface styles for the given family.</summary>
		/// <param name="familyName">The family name to use when searching.</param>
		/// <returns>Returns a <see cref="T:SkiaSharp.SKFontStyleSet" /> with all the font styles supported by the specified family.</returns>
		/// <remarks>Never returns <see langword="null" /> and will return an empty set if the family is not found.</remarks>
		public SKFontStyleSet GetFontStyles (string familyName)
		{
			var familyNameUtf8ByteList = StringUtilities.GetEncodedText (familyName, SKTextEncoding.Utf8, addNull: true);
			fixed (byte* familyNamePointer = familyNameUtf8ByteList) {
				var styleSet = SKFontStyleSet.GetObject (SkiaApi.sk_fontmgr_match_family (Handle, new IntPtr (familyNamePointer)));
				GC.KeepAlive (this);
				return styleSet;
			}
		}

		/// <summary>Matches a typeface from the specified family using the default font style.</summary>
		/// <param name="familyName">The name of the font family to match, or <see langword="null" /> for the default family.</param>
		/// <returns>The matched typeface, or <see langword="null" /> if no match was found.</returns>
		/// <remarks />
		public SKTypeface MatchFamily (string familyName) =>
			MatchFamily (familyName, SKFontStyle.Normal);

		/// <summary>Find the closest matching typeface to the specified family name and style.</summary>
		/// <param name="familyName">The family name to use when searching.</param>
		/// <param name="style">The font style to use when searching.</param>
		/// <returns>Returns the <see cref="T:SkiaSharp.SKTypeface" /> that contains the given family name and style, or the default font if no matching font was found.</returns>
		/// <remarks>Will never return <see langword="null" />, as it will return the default font if no matching font is found.</remarks>
		public SKTypeface MatchFamily (string familyName, SKFontStyle style)
		{
			if (style == null)
				throw new ArgumentNullException (nameof (style));
			var familyNameUtf8ByteList = StringUtilities.GetEncodedText (familyName, SKTextEncoding.Utf8, addNull: true);
			fixed (byte* familyNamePointer = familyNameUtf8ByteList) {
				var typeface = SKTypeface.GetDisposeProtectedObject (SkiaApi.sk_fontmgr_match_family_style (Handle, new IntPtr (familyNamePointer), style.Handle));
				GC.KeepAlive (style);
				GC.KeepAlive (this);
				return typeface;
			}
		}

		/// <summary>Creates a new <see cref="T:SkiaSharp.SKTypeface" /> from the specified file path.</summary>
		/// <param name="path">The path to the typeface.</param>
		/// <param name="index">The TTC index.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKTypeface" />, or <see langword="null" /> if the file does not exist or the contents are not recognized.</returns>
		/// <remarks />
		public SKTypeface CreateTypeface (string path, int index = 0)
		{
			if (path == null)
				throw new ArgumentNullException (nameof (path));

			var utf8path = StringUtilities.GetEncodedText (path, SKTextEncoding.Utf8, true);
			fixed (byte* u = utf8path) {
				var typeface = SKTypeface.GetObject (SkiaApi.sk_fontmgr_create_from_file (Handle, u, index));
				GC.KeepAlive (this);
				return typeface;
			}
		}

		/// <summary>Creates a new <see cref="T:SkiaSharp.SKTypeface" /> from the specified stream.</summary>
		/// <param name="stream">The stream to read the typeface from.</param>
		/// <param name="index">The TTC index.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKTypeface" />, or <see langword="null" /> if the stream is not recognized.</returns>
		/// <remarks />
		public SKTypeface CreateTypeface (Stream stream, int index = 0)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			return CreateTypeface (new SKManagedStream (stream, true), index);
		}

		/// <summary>Creates a new <see cref="T:SkiaSharp.SKTypeface" /> from the specified stream.</summary>
		/// <param name="stream">The stream to read the typeface from.</param>
		/// <param name="index">The TTC index.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKTypeface" />, or <see langword="null" /> if the stream is not recognized.</returns>
		/// <remarks />
		public SKTypeface CreateTypeface (SKStreamAsset stream, int index = 0)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			if (stream is SKManagedStream managed) {
				stream = managed.ToMemoryStream ();
				managed.Dispose ();
			}

			var typeface = SKTypeface.GetObject (SkiaApi.sk_fontmgr_create_from_stream (Handle, stream.Handle, index));
			GC.KeepAlive (this);
			stream.RevokeOwnership (typeface);
			return typeface;
		}

		/// <summary>Creates a new <see cref="T:SkiaSharp.SKTypeface" /> from the specified <see cref="T:SkiaSharp.SKData" />.</summary>
		/// <param name="data">The data to read the typeface from.</param>
		/// <param name="index">The TTC index.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKTypeface" />, or <see langword="null" /> if the data is not recognized.</returns>
		/// <remarks />
		public SKTypeface CreateTypeface (SKData data, int index = 0)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));

			var typeface = SKTypeface.GetObject (SkiaApi.sk_fontmgr_create_from_data (Handle, data.Handle, index));
			GC.KeepAlive (data);
			GC.KeepAlive (this);
			return typeface;
		}

		/// <summary>Use the system fallback to find a typeface for the given character.</summary>
		/// <param name="character">The character to find a typeface for.</param>
		/// <returns>Returns the <see cref="T:SkiaSharp.SKTypeface" /> that contains the given character, or <see langword="null" /> if none was found.</returns>
		/// <remarks />
		public SKTypeface MatchCharacter (char character)
		{
			return MatchCharacter (null, SKFontStyle.Normal, null, character);
		}

		/// <summary>Use the system fallback to find a typeface for the given character.</summary>
		/// <param name="character">The character to find a typeface for.</param>
		/// <returns>Returns the <see cref="T:SkiaSharp.SKTypeface" /> that contains the given character, or <see langword="null" /> if none was found.</returns>
		/// <remarks />
		public SKTypeface MatchCharacter (int character)
		{
			return MatchCharacter (null, SKFontStyle.Normal, null, character);
		}

		/// <summary>Use the system fallback to find a typeface for the given character.</summary>
		/// <param name="familyName">The family name to use when searching.</param>
		/// <param name="character">The character to find a typeface for.</param>
		/// <returns>Returns the <see cref="T:SkiaSharp.SKTypeface" /> that contains the given character, or <see langword="null" /> if none was found.</returns>
		/// <remarks />
		public SKTypeface MatchCharacter (string familyName, char character)
		{
			return MatchCharacter (familyName, SKFontStyle.Normal, null, character);
		}

		/// <summary>Use the system fallback to find a typeface for the given character.</summary>
		/// <param name="familyName">The family name to use when searching.</param>
		/// <param name="character">The character to find a typeface for.</param>
		/// <returns>Returns the <see cref="T:SkiaSharp.SKTypeface" /> that contains the given character, or <see langword="null" /> if none was found.</returns>
		/// <remarks />
		public SKTypeface MatchCharacter (string familyName, int character)
		{
			return MatchCharacter (familyName, SKFontStyle.Normal, null, character);
		}

		/// <summary>Use the system fallback to find a typeface for the given character.</summary>
		/// <param name="familyName">The family name to use when searching.</param>
		/// <param name="bcp47">The ISO 639, 15924, and 3166-1 code to use when searching, such as "ja" and "zh".</param>
		/// <param name="character">The character to find a typeface for.</param>
		/// <returns>Returns the <see cref="T:SkiaSharp.SKTypeface" /> that contains the given character, or <see langword="null" /> if none was found.</returns>
		/// <remarks><format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// Note that `bcp47` is a combination of ISO 639, 15924, and3166-1 codes, so it
		/// is fine to just pass a ISO 639 here. The first item is the least significant
		/// fallback, and the last is the most significant.
		///
		/// If no specified codes match, any font with the requested character will be
		/// matched.
		///
		/// May return <see langword="null" /> if no family can be found for the character
		/// in the system fallback.]]></format></remarks>
		public SKTypeface MatchCharacter (string familyName, string[] bcp47, char character)
		{
			return MatchCharacter (familyName, SKFontStyle.Normal, bcp47, character);
		}

		/// <summary>Use the system fallback to find a typeface for the given character.</summary>
		/// <param name="familyName">The family name to try and use.</param>
		/// <param name="bcp47">The ISO 639, 15924, and 3166-1 code to use when searching, such as "ja" and "zh".</param>
		/// <param name="character">The character to find a typeface for.</param>
		/// <returns>Returns the <see cref="T:SkiaSharp.SKTypeface" /> that contains the given character, or <see langword="null" /> if none was found.</returns>
		/// <remarks><format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// Note that `bcp47` is a combination of ISO 639, 15924, and3166-1 codes, so it
		/// is fine to just pass a ISO 639 here. The first item is the least significant
		/// fallback, and the last is the most significant.
		///
		/// If no specified codes match, any font with the requested character will be
		/// matched.
		///
		/// May return <see langword="null" /> if no family can be found for the character
		/// in the system fallback.]]></format></remarks>
		public SKTypeface MatchCharacter (string familyName, string[] bcp47, int character)
		{
			return MatchCharacter (familyName, SKFontStyle.Normal, bcp47, character);
		}

		/// <summary>Use the system fallback to find a typeface for the given character.</summary>
		/// <param name="familyName">The family name to use when searching.</param>
		/// <param name="weight">The font weight to use when searching.</param>
		/// <param name="width">The font width to use when searching.</param>
		/// <param name="slant">The font slant to use when searching.</param>
		/// <param name="bcp47">The ISO 639, 15924, and 3166-1 code to use when searching, such as "ja" and "zh".</param>
		/// <param name="character">The character to find a typeface for.</param>
		/// <returns>Returns the <see cref="T:SkiaSharp.SKTypeface" /> that contains the given character, or <see langword="null" /> if none was found.</returns>
		/// <remarks><format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// Note that `bcp47` is a combination of ISO 639, 15924, and3166-1 codes, so it
		/// is fine to just pass a ISO 639 here. The first item is the least significant
		/// fallback, and the last is the most significant.
		///
		/// If no specified codes match, any font with the requested character will be
		/// matched.
		///
		/// May return <see langword="null" /> if no family can be found for the character
		/// in the system fallback.]]></format></remarks>
		public SKTypeface MatchCharacter (string familyName, SKFontStyleWeight weight, SKFontStyleWidth width, SKFontStyleSlant slant, string[] bcp47, char character)
		{
			return MatchCharacter (familyName, new SKFontStyle (weight, width, slant), bcp47, character);
		}

		/// <summary>Use the system fallback to find a typeface for the given character.</summary>
		/// <param name="familyName">The family name to use when searching.</param>
		/// <param name="weight">The font weight to use when searching.</param>
		/// <param name="width">The font width to use when searching.</param>
		/// <param name="slant">The font slant to use when searching.</param>
		/// <param name="bcp47">The ISO 639, 15924, and 3166-1 code to use when searching, such as "ja" and "zh".</param>
		/// <param name="character">The character to find a typeface for.</param>
		/// <returns>Returns the <see cref="T:SkiaSharp.SKTypeface" /> that contains the given character, or <see langword="null" /> if none was found.</returns>
		/// <remarks><format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// Note that `bcp47` is a combination of ISO 639, 15924, and3166-1 codes, so it
		/// is fine to just pass a ISO 639 here. The first item is the least significant
		/// fallback, and the last is the most significant.
		///
		/// If no specified codes match, any font with the requested character will be
		/// matched.
		///
		/// May return <see langword="null" /> if no family can be found for the character
		/// in the system fallback.]]></format></remarks>
		public SKTypeface MatchCharacter (string familyName, SKFontStyleWeight weight, SKFontStyleWidth width, SKFontStyleSlant slant, string[] bcp47, int character)
		{
			return MatchCharacter (familyName, new SKFontStyle (weight, width, slant), bcp47, character);
		}

		/// <summary>Use the system fallback to find a typeface for the given character.</summary>
		/// <param name="familyName">The family name to use when searching.</param>
		/// <param name="weight">The font weight to use when searching.</param>
		/// <param name="width">The font width to use when searching.</param>
		/// <param name="slant">The font slant to use when searching.</param>
		/// <param name="bcp47">The ISO 639, 15924, and 3166-1 code to use when searching, such as "ja" and "zh".</param>
		/// <param name="character">The character to find a typeface for.</param>
		/// <returns>Returns the <see cref="T:SkiaSharp.SKTypeface" /> that contains the given character, or <see langword="null" /> if none was found.</returns>
		/// <remarks><format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// Note that `bcp47` is a combination of ISO 639, 15924, and3166-1 codes, so it
		/// is fine to just pass a ISO 639 here. The first item is the least significant
		/// fallback, and the last is the most significant.
		///
		/// If no specified codes match, any font with the requested character will be
		/// matched.
		///
		/// May return <see langword="null" /> if no family can be found for the character
		/// in the system fallback.]]></format></remarks>
		public SKTypeface MatchCharacter (string familyName, int weight, int width, SKFontStyleSlant slant, string[] bcp47, int character)
		{
			return MatchCharacter (familyName, new SKFontStyle (weight, width, slant), bcp47, character);
		}

		/// <summary>Use the system fallback to find a typeface for the given character.</summary>
		/// <param name="familyName">The family name to use when searching.</param>
		/// <param name="style">The font style to use when searching.</param>
		/// <param name="bcp47">The ISO 639, 15924, and 3166-1 code to use when searching, such as "ja" and "zh".</param>
		/// <param name="character">The character to find a typeface for.</param>
		/// <returns>Returns the <see cref="T:SkiaSharp.SKTypeface" /> that contains the given character, or <see langword="null" /> if none was found.</returns>
		/// <remarks><format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// Note that `bcp47` is a combination of ISO 639, 15924, and3166-1 codes, so it
		/// is fine to just pass a ISO 639 here. The first item is the least significant
		/// fallback, and the last is the most significant.
		///
		/// If no specified codes match, any font with the requested character will be
		/// matched.
		///
		/// May return <see langword="null" /> if no family can be found for the character
		/// in the system fallback.]]></format></remarks>
		public SKTypeface MatchCharacter (string familyName, SKFontStyle style, string[] bcp47, int character)
		{
			if (style == null)
				throw new ArgumentNullException (nameof (style));

			// TODO: work around for https://bugs.chromium.org/p/skia/issues/detail?id=6196
			if (familyName == null)
				familyName = string.Empty;

			var familyNameUtf8ByteList = StringUtilities.GetEncodedText (familyName, SKTextEncoding.Utf8, addNull: true);
			fixed (byte* familyNamePointer = familyNameUtf8ByteList) {
				var typeface = SKTypeface.GetDisposeProtectedObject (SkiaApi.sk_fontmgr_match_family_style_character (Handle, new IntPtr (familyNamePointer), style.Handle, bcp47, bcp47?.Length ?? 0, character));
				GC.KeepAlive (style);
				GC.KeepAlive (this);
				return typeface;
			}
		}

		/// <summary>Creates a new, default font manager.</summary>
		/// <returns>Returns the new font manager.</returns>
		/// <remarks />
		public static SKFontManager CreateDefault ()
		{
			return GetObject (SkiaApi.sk_fontmgr_create_default ());
		}

		//

		internal static SKFontManager GetObject (IntPtr handle) =>
			GetOrAddObject (handle, (h, o) => new SKFontManager (h, o));

		internal static SKFontManager GetDisposeProtectedObject (IntPtr handle) =>
			GetOrAddDisposeProtectedObject (handle, owns: true, unrefExisting: true, (h, o) => new SKFontManager (h, o));

	}
}
