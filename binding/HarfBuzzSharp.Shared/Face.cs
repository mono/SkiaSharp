using System;

namespace HarfBuzzSharp
{
	public class Face : NativeObject
	{
		public Face (Blob blob, int index)
			: this (IntPtr.Zero)
		{
			if (blob == null) {
				throw new ArgumentNullException (nameof (blob));
			}

			if (index < 0) {
				throw new ArgumentOutOfRangeException (nameof (index), "Index must be non negative.");
			}

			Handle = HarfBuzzApi.hb_face_create (blob.Handle, index);
		}

		internal Face (IntPtr handle)
			: base (handle)
		{
		}

		public int Index {
			get { return HarfBuzzApi.hb_face_get_index (Handle); }
			set { HarfBuzzApi.hb_face_set_index (Handle, value); }
		}

		public int UnitsPerEm {
			get { return HarfBuzzApi.hb_face_get_upem (Handle); }
			set { HarfBuzzApi.hb_face_set_upem (Handle, value); }
		}

		public int GlyphCount {
			get { return HarfBuzzApi.hb_face_get_glyph_count (Handle); }
			set { HarfBuzzApi.hb_face_set_glyph_count (Handle, value); }
		}

		public bool IsImmutable {
			get {
				return HarfBuzzApi.hb_face_is_immutable (Handle);
			}
		}

		public void MakeImmutable () => HarfBuzzApi.hb_face_make_immutable (Handle);

		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero) {
				HarfBuzzApi.hb_face_destroy (Handle);
			}

			base.Dispose (disposing);
		}
	}
}
