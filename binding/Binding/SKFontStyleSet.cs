using System;

namespace SkiaSharp
{
	public class SKFontStyleSet : SKObject
	{
		[Preserve]
		internal SKFontStyleSet (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		public SKFontStyleSet ()
			: this (SkiaApi.sk_fontstyleset_create_empty (), true)
		{
		}

		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.sk_fontstyleset_unref (Handle);
			}

			base.Dispose (disposing);
		}

		public int Count => SkiaApi.sk_fontstyleset_get_count (Handle);

		public void GetStyle(int index, out SKFontStyle fontStyle, out string styleName)
		{
			fontStyle = new SKFontStyle ();
			using (var str = new SKString ()) {
				SkiaApi.sk_fontstyleset_get_style (Handle, index, fontStyle.Handle, str.Handle);
				styleName = (string)str;
			}
		}

		public SKTypeface CreateTypeface (int index)
		{
			return GetObject<SKTypeface> (SkiaApi.sk_fontstyleset_create_typeface (Handle, index));
		}

		public SKTypeface CreateTypeface (SKFontStyle style)
		{
			if (style == null)
				throw new ArgumentNullException (nameof (style));

			return GetObject<SKTypeface> (SkiaApi.sk_fontstyleset_match_style (Handle, style.Handle));
		}
	}
}
