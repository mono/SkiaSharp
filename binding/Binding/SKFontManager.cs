using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SkiaSharp
{
	public unsafe class SKFontManager : SKObject, ISKReferenceCounted
	{
		private static readonly SKFontManager defaultManager;

		static SKFontManager ()
		{
			defaultManager = new SKFontManagerStatic (SkiaApi.sk_fontmgr_ref_default ());
		}

		internal static void EnsureStaticInstanceAreInitialized ()
		{
			// IMPORTANT: do not remove to ensure that the static instances
			//            are initialized before any access is made to them
		}

		private protected SKFontManager (IntPtr handle, bool owns = true, bool registerHandle = true)
			: base (handle, owns, registerHandle)
		{
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		public static SKFontManager Default => defaultManager;

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
			return SKFontStyleSet.GetObject (SkiaApi.sk_fontmgr_create_styleset (Handle, index));
		}

		public SKFontStyleSet GetFontStyles (string familyName)
		{
			return SKFontStyleSet.GetObject (SkiaApi.sk_fontmgr_match_family (Handle, familyName));
		}

		public SKTypeface MatchFamily (string familyName) =>
			MatchFamily (familyName, SKFontStyle.Normal);

		public SKTypeface MatchFamily (string familyName, SKFontStyle style)
		{
			if (style == null)
				throw new ArgumentNullException (nameof (style));

			var tf = SKTypeface.GetObject (SkiaApi.sk_fontmgr_match_family_style (Handle, familyName, style.Handle));
			tf?.PreventPublicDisposal ();
			return tf;
		}

		public SKTypeface MatchTypeface (SKTypeface face, SKFontStyle style)
		{
			if (face == null)
				throw new ArgumentNullException (nameof (face));
			if (style == null)
				throw new ArgumentNullException (nameof (style));

			var tf = SKTypeface.GetObject (SkiaApi.sk_fontmgr_match_face_style (Handle, face.Handle, style.Handle));
			tf?.PreventPublicDisposal ();
			return tf;
		}

		public SKTypeface CreateTypeface (string path, int index = 0)
		{
			if (path == null)
				throw new ArgumentNullException (nameof (path));

			var utf8path = StringUtilities.GetEncodedText (path, SKTextEncoding.Utf8, true);
			fixed (byte* u = utf8path) {
				return SKTypeface.GetObject (SkiaApi.sk_fontmgr_create_from_file (Handle, u, index));
			}
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
				stream = managed.ToMemoryStream ();
				managed.Dispose ();
			}

			var typeface = SKTypeface.GetObject (SkiaApi.sk_fontmgr_create_from_stream (Handle, stream.Handle, index));
			stream.RevokeOwnership (typeface);
			return typeface;
		}

		public SKTypeface CreateTypeface (SKData data, int index = 0)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));

			return SKTypeface.GetObject (SkiaApi.sk_fontmgr_create_from_data (Handle, data.Handle, index));
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

			var tf = SKTypeface.GetObject (SkiaApi.sk_fontmgr_match_family_style_character (Handle, familyName, style.Handle, bcp47, bcp47?.Length ?? 0, character));
			tf?.PreventPublicDisposal ();
			return tf;
		}

		public static SKFontManager CreateDefault ()
		{
			return GetObject (SkiaApi.sk_fontmgr_create_default ());
		}

		//

		internal static SKFontManager GetObject (IntPtr handle) =>
			GetOrAddObject (handle, (h, o) => new SKFontManager (h, o, false));

		//

		private sealed class SKFontManagerStatic : SKFontManager
		{
			internal SKFontManagerStatic (IntPtr handle)
				: base (handle)
			{
				IgnorePublicDispose = true;
			}
		}
	}
}
