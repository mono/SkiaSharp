using System;

namespace HarfBuzzSharp
{
	public class Face : NativeObject
	{
		internal Face(IntPtr handle)
			: base(handle)
		{
		}

		public Face(Blob blob, uint index)
			: this(IntPtr.Zero)
		{
			if (blob == null)
			{
				throw new ArgumentNullException(nameof(blob));
			}

			Handle = HarfBuzzApi.hb_face_create(blob.Handle, index);
		}

		protected override void Dispose(bool disposing)
		{
			if (Handle != IntPtr.Zero)
			{
				HarfBuzzApi.hb_face_destroy(Handle);
			}

			base.Dispose(disposing);
		}

		public uint Index
		{
			get { return HarfBuzzApi.hb_face_get_index(Handle); }
			set { HarfBuzzApi.hb_face_set_index(Handle, value); }
		}

		public uint UnitsPerEm
		{
			get { return HarfBuzzApi.hb_face_get_upem(Handle); }
			set { HarfBuzzApi.hb_face_set_upem(Handle, value); }
		}
	}
}
