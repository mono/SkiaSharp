#nullable disable

using System;

namespace HarfBuzzSharp
{
	/// <summary>
	/// Represents a typeface.
	/// </summary>
	public unsafe class Face : NativeObject
	{
		private static readonly Lazy<Face> emptyFace = new Lazy<Face> (() => new StaticFace (HarfBuzzApi.hb_face_get_empty ()));

		/// <summary>
		/// Gets a reference to the empty <see cref="Face" /> instance.
		/// </summary>
		public static Face Empty => emptyFace.Value;

		/// <summary>
		/// Creates a new <see cref="Face" /> instance, using the specified typeface blob.
		/// </summary>
		/// <param name="blob">The typeface data.</param>
		/// <param name="index">The zero-based face index in a collection.</param>
		public Face (Blob blob, uint index)
			: this (blob, (int)index)
		{
		}

		/// <summary>
		/// Creates a new <see cref="Face" /> instance, using the specified typeface blob.
		/// </summary>
		/// <param name="blob">The typeface data.</param>
		/// <param name="index">The zero-based face index in a collection.</param>
		public Face (Blob blob, int index)
			: this (IntPtr.Zero)
		{
			if (blob == null) {
				throw new ArgumentNullException (nameof (blob));
			}

			if (index < 0) {
				throw new ArgumentOutOfRangeException (nameof (index), "Index must be non negative.");
			}

			Handle = HarfBuzzApi.hb_face_create (blob.Handle, (uint)index);
		}

		/// <summary>
		/// Creates a new <see cref="Face" /> instance, using the delegate to assemble the data.
		/// </summary>
		/// <param name="getTable">The delegate to retrieve the table data.</param>
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
				DelegateProxies.ReferenceTableProxy,
				(void*)DelegateProxies.CreateMultiUserData (getTable, destroy, this),
				DelegateProxies.DestroyProxyForMulti);
		}

		internal Face (IntPtr handle)
			: base (handle)
		{
		}

		/// <summary>
		/// Gets or sets the zero-based face index in a collection.
		/// </summary>
		public int Index {
			get => (int)HarfBuzzApi.hb_face_get_index (Handle);
			set => HarfBuzzApi.hb_face_set_index (Handle, (uint)value);
		}

		/// <summary>
		/// Gets or sets the units per EM.
		/// </summary>
		public int UnitsPerEm {
			get => (int)HarfBuzzApi.hb_face_get_upem (Handle);
			set => HarfBuzzApi.hb_face_set_upem (Handle, (uint)value);
		}

		public int GlyphCount {
			get => (int)HarfBuzzApi.hb_face_get_glyph_count (Handle);
			set => HarfBuzzApi.hb_face_set_glyph_count (Handle, (uint)value);
		}

		public unsafe Tag[] Tables {
			get {
				uint tableCount;
				var count = HarfBuzzApi.hb_face_get_table_tags (Handle, 0, &tableCount, null);
				var buffer = new Tag[count];
				fixed (void* ptr = buffer) {
					HarfBuzzApi.hb_face_get_table_tags (Handle, 0, &count, (uint*)ptr);
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
