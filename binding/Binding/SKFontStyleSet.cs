using System;
using System.Collections;
using System.Collections.Generic;

namespace SkiaSharp
{
	public class SKFontStyleSet : SKObject, IEnumerable<SKFontStyle>, IReadOnlyCollection<SKFontStyle>, IReadOnlyList<SKFontStyle>
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

		public SKFontStyle this[int index] => GetStyle (index);

		public string GetStyleName (int index)
		{
			using (var str = new SKString ()) {
				SkiaApi.sk_fontstyleset_get_style (Handle, index, IntPtr.Zero, str.Handle);
				return (string)str;
			}
		}

		public SKTypeface CreateTypeface (int index)
		{
			if (index < 0 || index >= Count)
				throw new ArgumentOutOfRangeException ($"Index was out of range. Must be non-negative and less than the size of the set.", nameof (index));

			return GetObject<SKTypeface> (SkiaApi.sk_fontstyleset_create_typeface (Handle, index));
		}

		public SKTypeface CreateTypeface (SKFontStyle style)
		{
			if (style == null)
				throw new ArgumentNullException (nameof (style));

			return GetObject<SKTypeface> (SkiaApi.sk_fontstyleset_match_style (Handle, style.Handle));
		}

		public IEnumerator<SKFontStyle> GetEnumerator () => GetStyles ().GetEnumerator ();

		IEnumerator IEnumerable.GetEnumerator () => GetStyles ().GetEnumerator ();

		private IEnumerable<SKFontStyle> GetStyles ()
		{
			var count = Count;
			for (var i = 0; i < count; i++) {
				yield return GetStyle (i);
			}
		}

		private SKFontStyle GetStyle (int index)
		{
			var fontStyle = new SKFontStyle ();
			SkiaApi.sk_fontstyleset_get_style (Handle, index, fontStyle.Handle, IntPtr.Zero);
			return fontStyle;
		}
	}
}
