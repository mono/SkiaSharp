﻿using System;
using System.IO;

namespace SkiaSharp
{
	public class SKFontManager : SKObject
	{
		[Preserve]
		internal SKFontManager (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.sk_fontmgr_unref (Handle);
			}

			base.Dispose (disposing);
		}

		public static SKFontManager Default => GetObject<SKFontManager> (SkiaApi.sk_fontmgr_ref_default ());

		public int FontFamilyCount => SkiaApi.sk_fontmgr_count_families (Handle);

		public string GetFamilyName (int index)
		{
			using (var str = new SKString ()) {
				SkiaApi.sk_fontmgr_get_family_name (Handle, index, str.Handle);
				return (string)str;
			}
		}

		public string[] GetFontFamilies ()
		{
			var count = FontFamilyCount;
			var families = new string[count];
			for (int i = 0; i < count; i++) {
				families[i] = GetFamilyName (i);
			}
			return families;
		}

		public SKFontStyleSet CreateStyleSet (int index)
		{
			return GetObject<SKFontStyleSet> (SkiaApi.sk_fontmgr_create_styleset (Handle, index));
		}

		public SKFontStyleSet MatchFamily (string familyName)
		{
			return GetObject<SKFontStyleSet> (SkiaApi.sk_fontmgr_match_family (Handle, familyName));
		}

		public SKTypeface MatchFamily (string familyName, SKFontStyle style)
		{
			if (style == null)
				throw new ArgumentNullException (nameof (style));

			return GetObject<SKTypeface> (SkiaApi.sk_fontmgr_match_family_style (Handle, familyName, style.Handle));
		}

		[Obsolete]
		public SKTypeface MatchTypeface (SKTypeface face, SKFontStyle style)
		{
			if (face == null)
				throw new ArgumentNullException (nameof (face));
			if (style == null)
				throw new ArgumentNullException (nameof (style));

			return GetObject<SKTypeface> (SkiaApi.sk_fontmgr_match_face_style (Handle, face.Handle, style.Handle));
		}

		public SKTypeface FromFile (string path, int index = 0)
		{
			if (path == null)
				throw new ArgumentNullException (nameof (path));

			using (var stream = SKFileStream.OpenStream (path)) {
				if (stream == null) {
					return null;
				} else {
					return FromStream (stream, index);
				}
			}
		}

		public SKTypeface FromStream (Stream stream, int index = 0)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			if (!stream.CanSeek) {
				var fontStream = new MemoryStream ();
				stream.CopyTo (fontStream);
				fontStream.Flush ();
				fontStream.Position = 0;

				stream.Dispose ();
				stream = null;

				stream = fontStream;
				fontStream = null;
			}

			return FromStream (new SKManagedStream (stream, true), index);
		}

		public SKTypeface FromStream (SKStreamAsset stream, int index = 0)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			var typeface = GetObject<SKTypeface> (SkiaApi.sk_fontmgr_create_from_stream (Handle, stream.Handle, index));
			stream.RevokeOwnership ();
			return typeface;
		}

		public SKTypeface FromData (SKData data, int index = 0)
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
	}
}
