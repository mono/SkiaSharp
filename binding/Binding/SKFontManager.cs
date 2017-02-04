//
// Bindings for SKTypeface
//
// Author:
//   Miguel de Icaza
//
// Copyright 2016 Xamarin Inc
//
using System;
using System.Runtime.InteropServices;

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
			var families = new string [count];
			for	(int i = 0; i < count; i++) {
				families [i] = GetFamilyName (i);
			}
			return families;
		}

		public SKTypeface MatchCharacter (char character)
		{
			return MatchCharacter ((int)character);
		}
		
		public SKTypeface MatchCharacter (int character)
		{
			return MatchCharacter (null, character);
		}
		
		public SKTypeface MatchCharacter (string familyName, char character)
		{
			return MatchCharacter (familyName, (int)character);
		}
		
		public SKTypeface MatchCharacter (string familyName, int character)
		{
			return MatchCharacter (familyName, null, character);
		}
		
		public SKTypeface MatchCharacter (string familyName, string[] bcp47, char character)
		{
			return MatchCharacter (familyName, bcp47, (int)character);
		}
		
		public SKTypeface MatchCharacter (string familyName, string[] bcp47, int character)
		{
			return MatchCharacter (familyName, SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright, bcp47, character);
		}
		
		public SKTypeface MatchCharacter (string familyName, SKFontStyleWeight weight, SKFontStyleWidth width, SKFontStyleSlant slant, string[] bcp47, char character)
		{
			return MatchCharacter (familyName, weight, width, slant, bcp47, (int)character);
		}
		
		public SKTypeface MatchCharacter (string familyName, SKFontStyleWeight weight, SKFontStyleWidth width, SKFontStyleSlant slant, string[] bcp47, int character)
		{
			return MatchCharacter (familyName, (int)weight, (int)width, slant, bcp47, character);
		}
		
		public SKTypeface MatchCharacter (string familyName, int weight, int width, SKFontStyleSlant slant, string[] bcp47, int character)
		{
			// TODO: work around for https://bugs.chromium.org/p/skia/issues/detail?id=6196
			if (familyName == null)
			{
				familyName = string.Empty;
			}

			return GetObject<SKTypeface> (SkiaApi.sk_fontmgr_match_family_style_character (Handle, familyName, weight, width, slant, bcp47, bcp47?.Length ?? 0, character));
		}
	}
}

