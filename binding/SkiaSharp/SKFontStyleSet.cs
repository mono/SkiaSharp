#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;

namespace SkiaSharp
{
	/// <summary>
	/// Represets the set of styles for a particular font family.
	/// </summary>
	public class SKFontStyleSet : SKObject, ISKReferenceCounted, IEnumerable<SKFontStyle>, IReadOnlyCollection<SKFontStyle>, IReadOnlyList<SKFontStyle>
	{
		internal SKFontStyleSet (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		/// <summary>
		/// Creates a new, empty <see cref="T:SkiaSharp.SKFontStyleSet" />.
		/// </summary>
		public SKFontStyleSet ()
			: this (SkiaApi.sk_fontstyleset_create_empty (), true)
		{
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		/// <summary>
		/// Gets the number of font styles in the set.
		/// </summary>
		public int Count => SkiaApi.sk_fontstyleset_get_count (Handle);

		/// <summary>
		/// Gets the font style at the specified index.
		/// </summary>
		/// <param name="index">The index of the font style.</param>
		public SKFontStyle this[int index] => GetStyle (index);

		/// <summary>
		/// Returns the name of the font style.
		/// </summary>
		/// <param name="index">The index of the font style.</param>
		/// <returns>Returns the name of the font style.</returns>
		public string GetStyleName (int index)
		{
			using var str = new SKString ();
			SkiaApi.sk_fontstyleset_get_style (Handle, index, IntPtr.Zero, str.Handle);
			GC.KeepAlive (this);
			return (string)str;
		}

		/// <summary>
		/// Creates a new <see cref="T:SkiaSharp.SKTypeface" /> with the style that is the closest match to the style at the specified index.
		/// </summary>
		/// <param name="index">The index of the font style to match.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKTypeface" />.</returns>
		public SKTypeface CreateTypeface (int index)
		{
			if (index < 0 || index >= Count)
				throw new ArgumentOutOfRangeException ($"Index was out of range. Must be non-negative and less than the size of the set.", nameof (index));

			var tf = SKTypeface.GetObject (SkiaApi.sk_fontstyleset_create_typeface (Handle, index));
			tf?.PreventPublicDisposal ();
			GC.KeepAlive (this);
			return tf;
		}

		/// <summary>
		/// Creates a new <see cref="T:SkiaSharp.SKTypeface" /> with a style that is the closest match to the specified font style.
		/// </summary>
		/// <param name="style">The font style to match.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKTypeface" />.</returns>
		public SKTypeface CreateTypeface (SKFontStyle style)
		{
			if (style == null)
				throw new ArgumentNullException (nameof (style));

			var tf = SKTypeface.GetObject (SkiaApi.sk_fontstyleset_match_style (Handle, style.Handle));
			tf?.PreventPublicDisposal ();
			GC.KeepAlive (this);
			return tf;
		}

		/// <summary>
		/// Returns an enumerator that iterates through the font styles.
		/// </summary>
		/// <returns>Returns an enumerator.</returns>
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

		internal static SKFontStyleSet GetObject (IntPtr handle) =>
			GetOrAddObject (handle, (h, o) => new SKFontStyleSet (h, o));
	}
}
