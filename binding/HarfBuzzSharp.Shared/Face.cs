using System;

namespace HarfBuzzSharp
{
	public unsafe class Face : NativeObject
	{
		private static readonly Lazy<Face> emptyFace = new Lazy<Face> (() => new StaticFace (HarfBuzzApi.hb_face_get_empty ()));

		public static Face Empty => emptyFace.Value;

		public Face (Blob blob, uint index)
			: this (blob, (int)index)
		{
		}

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

		public Face (GetTableDelegate getTable)
			: this (getTable, null)
		{
		}

		public Face (GetTableDelegate getTable, ReleaseDelegate destroy)
			: this (IntPtr.Zero)
		{
			if (getTable == null)
				throw new ArgumentNullException (nameof (getTable));

			Handle = HarfBuzzApi.hb_face_create_for_tables (
				DelegateProxies.GetTableDelegateProxy,
				(void*)DelegateProxies.CreateMultiUserData (getTable, destroy, this),
				DelegateProxies.ReleaseDelegateProxyForMulti);
		}

		internal Face (IntPtr handle)
			: base (handle)
		{
		}

		public int Index {
			get => HarfBuzzApi.hb_face_get_index (Handle);
			set => HarfBuzzApi.hb_face_set_index (Handle, value);
		}

		public int UnitsPerEm {
			get => HarfBuzzApi.hb_face_get_upem (Handle);
			set => HarfBuzzApi.hb_face_set_upem (Handle, value);
		}

		public int GlyphCount {
			get => HarfBuzzApi.hb_face_get_glyph_count (Handle);
			set => HarfBuzzApi.hb_face_set_glyph_count (Handle, value);
		}

		public unsafe Tag[] Tables {
			get {
				var tableCount = 0;
				var count = HarfBuzzApi.hb_face_get_table_tags (Handle, 0, ref tableCount, IntPtr.Zero);
				var buffer = new Tag[count];
				fixed (Tag* ptr = buffer) {
					HarfBuzzApi.hb_face_get_table_tags (Handle, 0, ref count, (IntPtr)ptr);
				}
				return buffer;
			}
		}

		public Blob ReferenceTable (Tag table) =>
			new Blob (HarfBuzzApi.hb_face_reference_table (Handle, table));

		public bool IsImmutable => HarfBuzzApi.hb_face_is_immutable (Handle);

		public void MakeImmutable () => HarfBuzzApi.hb_face_make_immutable (Handle);

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeHandler ()
		{
			if (Handle != IntPtr.Zero) {
				HarfBuzzApi.hb_face_destroy (Handle);
			}
		}

		private class StaticFace : Face
		{
			public StaticFace (IntPtr handle)
				: base (handle)
			{
			}

			protected override void Dispose (bool disposing)
			{
				// do not dispose
			}
		}
	}
}
