using System;
using System.Collections.Generic;

namespace HarfBuzzSharp
{
	public class Face : NativeObject
	{
		private readonly TableLoader tableLoader;
		private readonly HarfBuzzApi.hb_reference_table_func_t tableLoadFunc;

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

		public Face (TableLoader tableLoader)
			: this (IntPtr.Zero)
		{
			this.tableLoader = tableLoader;
			tableLoadFunc = tableLoader.Load;
			var ctx = new NativeDelegateContext (null, new ReleaseDelegate (x => this.tableLoader.Dispose ()));
			Handle = HarfBuzzApi.hb_face_create_for_tables (tableLoadFunc,
				ctx.NativeContext, destroy_func);
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

		protected override void DisposeHandler ()
		{
			if (Handle != IntPtr.Zero) {
				HarfBuzzApi.hb_face_destroy (Handle);
			}
		}
	}

	public abstract class TableLoader : IDisposable
	{
		private readonly Dictionary<Tag, Blob> tableCache = new Dictionary<Tag, Blob> ();
		private bool isDisposed;

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		private void Dispose (bool disposing)
		{
			if (isDisposed) {
				return;
			}

			isDisposed = true;

			if (!disposing) {
				return;
			}

			DisposeHandler ();
		}

		protected abstract Blob Load (Tag tag);

		protected virtual void DisposeHandler ()
		{
			foreach (var blob in tableCache.Values) {
				blob?.Dispose ();
			}
		}

		internal IntPtr Load (IntPtr face, Tag tag, IntPtr userData)
		{
			Blob blob;

			if (tableCache.ContainsKey (tag)) {
				blob = tableCache[tag];
			} else {
				blob = Load (tag);
				tableCache.Add (tag, blob);
			}

			return blob?.Handle ?? IntPtr.Zero;
		}
	}
}
