#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;

namespace SkiaSharp
{
	/// <summary>Represents the set of styles for a particular font family.</summary>
	/// <remarks />
	public class SKFontStyleSet : SKObject, ISKReferenceCounted, IEnumerable<SKFontStyle>, IReadOnlyCollection<SKFontStyle>, IReadOnlyList<SKFontStyle>
	{
		internal SKFontStyleSet (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		/// <summary>Creates a new, empty <see cref="T:SkiaSharp.SKFontStyleSet" />.</summary>
		/// <remarks />
		public SKFontStyleSet ()
			: this (SkiaApi.sk_fontstyleset_create_empty (), true)
		{
		}

		/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.SKFontStyleSet" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:SkiaSharp.SKFontStyleSet" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		/// <summary>Gets the number of font styles in the set.</summary>
		/// <value>The number of font styles.</value>
		/// <remarks />
		public int Count {
			get {
				var r = SkiaApi.sk_fontstyleset_get_count (Handle);
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Gets the font style at the specified index.</summary>
		/// <param name="index">The index of the font style.</param>
		/// <value>The font style at the specified index.</value>
		/// <remarks />
		public SKFontStyle this[int index] => GetStyle (index);

		/// <summary>Returns the name of the font style.</summary>
		/// <param name="index">The index of the font style.</param>
		/// <returns>Returns the name of the font style.</returns>
		/// <remarks />
		public string GetStyleName (int index)
		{
			using var str = new SKString ();
			SkiaApi.sk_fontstyleset_get_style (Handle, index, IntPtr.Zero, str.Handle);
			GC.KeepAlive (this);
			return (string)str;
		}

		/// <summary>Creates a new <see cref="T:SkiaSharp.SKTypeface" /> with the style that is the closest match to the style at the specified index.</summary>
		/// <param name="index">The index of the font style to match.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKTypeface" />.</returns>
		/// <remarks />
		public SKTypeface CreateTypeface (int index)
		{
			if (index < 0 || index >= Count)
				throw new ArgumentOutOfRangeException ($"Index was out of range. Must be non-negative and less than the size of the set.", nameof (index));

			var tf = SKTypeface.GetDisposeProtectedObject (SkiaApi.sk_fontstyleset_create_typeface (Handle, index));
			GC.KeepAlive (this);
			return tf;
		}

		/// <summary>Creates a new <see cref="T:SkiaSharp.SKTypeface" /> with a style that is the closest match to the specified font style.</summary>
		/// <param name="style">The font style to match.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKTypeface" />.</returns>
		/// <remarks />
		public SKTypeface CreateTypeface (SKFontStyle style)
		{
			if (style == null)
				throw new ArgumentNullException (nameof (style));

			var tf = SKTypeface.GetDisposeProtectedObject (SkiaApi.sk_fontstyleset_match_style (Handle, style.Handle));
			GC.KeepAlive (style);
			GC.KeepAlive (this);
			return tf;
		}

		/// <summary>Returns an enumerator that iterates through the font styles.</summary>
		/// <returns>Returns an enumerator.</returns>
		/// <remarks />
		public IEnumerator<SKFontStyle> GetEnumerator () => GetStyles ().GetEnumerator ();

		/// <summary>Returns an enumerator that iterates through the font styles.</summary>
		/// <returns>Returns an enumerator.</returns>
		/// <remarks />
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
			GC.KeepAlive (this);
			return fontStyle;
		}

		internal static SKFontStyleSet GetObject (IntPtr handle) =>
			GetOrAddObject (handle, (h, o) => new SKFontStyleSet (h, o));
	}
}
