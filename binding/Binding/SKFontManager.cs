using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SkiaSharp
{
	public class SKFontManager : SKObject, ISKReferenceCounted
	{
		private static readonly Lazy<SKFontManager> defaultManager =
			new Lazy<SKFontManager> (() => new SKFontManagerStatic (SkiaApi.sk_fontmgr_ref_default ()));

		[Preserve]
		internal SKFontManager (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		public static SKFontManager Default => defaultManager.Value;

		public int FontFamilyCount => SkiaApi.sk_fontmgr_count_families (Handle);

		public IEnumerable<string> FontFamilies {
			get {
				var count = FontFamilyCount;
				for (var i = 0; i < count; i++) {
					yield return GetFamilyName (i);
				}
			}
		}

		public string GetFamilyName (int index)
		{
			using (var str = new SKString ()) {
				SkiaApi.sk_fontmgr_get_family_name (Handle, index, str.Handle);
				return (string)str;
			}
		}

		public string[] GetFontFamilies () => FontFamilies.ToArray ();

		public SKFontStyleSet GetFontStyles (int index)
		{
			return GetObject<SKFontStyleSet> (SkiaApi.sk_fontmgr_create_styleset (Handle, index));
		}

		public SKFontStyleSet GetFontStyles (string familyName)
		{
			return GetObject<SKFontStyleSet> (SkiaApi.sk_fontmgr_match_family (Handle, familyName));
		}

		public SKTypeface MatchFamily (string familyName, SKFontStyle style)
		{
			if (style == null)
				throw new ArgumentNullException (nameof (style));

			return GetObject<SKTypeface> (SkiaApi.sk_fontmgr_match_family_style (Handle, familyName, style.Handle));
		}

		public SKTypeface MatchTypeface (SKTypeface face, SKFontStyle style)
		{
			if (face == null)
				throw new ArgumentNullException (nameof (face));
			if (style == null)
				throw new ArgumentNullException (nameof (style));

			return GetObject<SKTypeface> (SkiaApi.sk_fontmgr_match_face_style (Handle, face.Handle, style.Handle));
		}

		public SKTypeface CreateTypeface (string path, int index = 0)
		{
			if (path == null)
				throw new ArgumentNullException (nameof (path));

			var utf8path = StringUtilities.GetEncodedText (path, SKEncoding.Utf8);
			return GetObject<SKTypeface> (SkiaApi.sk_fontmgr_create_from_file(Handle, utf8path, index));
		}

		public SKTypeface CreateTypeface (Stream stream, int index = 0)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			return CreateTypeface (new SKManagedStream (stream, true), index);
		}

		public SKTypeface CreateTypeface (SKStreamAsset stream, int index = 0)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			if (stream is SKManagedStream managed) {
				var native = new SKDynamicMemoryWStream ();
				managed.CopyTo (native);
				stream = native.DetachAsStream ();
				managed.Dispose ();
			}

			var typeface = GetObject<SKTypeface> (SkiaApi.sk_fontmgr_create_from_stream (Handle, stream.Handle, index));
			stream.RevokeOwnership (typeface);
			return typeface;
		}

		public SKTypeface CreateTypeface (SKData data, int index = 0)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));

			return GetObject<SKTypeface> (SkiaApi.sk_fontmgr_create_from_data (Handle, data.Handle, index));
		}

		public SKTypeface MatchCharacter (char character)
		{
			return MatchCharacter (null, SKFontStyle.Normal, null, character);
		}

		public SKTypeface MatchCharacter (int character)
		{
			return MatchCharacter (null, SKFontStyle.Normal, null, character);
		}

		public SKTypeface MatchCharacter (string familyName, char character)
		{
			return MatchCharacter (familyName, SKFontStyle.Normal, null, character);
		}

		public SKTypeface MatchCharacter (string familyName, int character)
		{
			return MatchCharacter (familyName, SKFontStyle.Normal, null, character);
		}

		public SKTypeface MatchCharacter (string familyName, string[] bcp47, char character)
		{
			return MatchCharacter (familyName, SKFontStyle.Normal, bcp47, character);
		}

		public SKTypeface MatchCharacter (string familyName, string[] bcp47, int character)
		{
			return MatchCharacter (familyName, SKFontStyle.Normal, bcp47, character);
		}

		public SKTypeface MatchCharacter (string familyName, SKFontStyleWeight weight, SKFontStyleWidth width, SKFontStyleSlant slant, string[] bcp47, char character)
		{
			return MatchCharacter (familyName, new SKFontStyle (weight, width, slant), bcp47, character);
		}

		public SKTypeface MatchCharacter (string familyName, SKFontStyleWeight weight, SKFontStyleWidth width, SKFontStyleSlant slant, string[] bcp47, int character)
		{
			return MatchCharacter (familyName, new SKFontStyle (weight, width, slant), bcp47, character);
		}

		public SKTypeface MatchCharacter (string familyName, int weight, int width, SKFontStyleSlant slant, string[] bcp47, int character)
		{
			return MatchCharacter (familyName, new SKFontStyle (weight, width, slant), bcp47, character);
		}

		public SKTypeface MatchCharacter (string familyName, SKFontStyle style, string[] bcp47, int character)
		{
			if (style == null)
				throw new ArgumentNullException (nameof (style));

			// TODO: work around for https://bugs.chromium.org/p/skia/issues/detail?id=6196
			if (familyName == null)
				familyName = string.Empty;

			return GetObject<SKTypeface> (SkiaApi.sk_fontmgr_match_family_style_character (Handle, familyName, style.Handle, bcp47, bcp47?.Length ?? 0, character));
		}

		public static SKFontManager CreateDefault ()
		{
			return GetObject<SKFontManager> (SkiaApi.sk_fontmgr_create_default ());
		}

		private sealed class SKFontManagerStatic : SKFontManager
		{
			internal SKFontManagerStatic (IntPtr x)
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
