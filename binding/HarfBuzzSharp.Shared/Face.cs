using System;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp
{
	public class Face : NativeObject
	{
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate IntPtr GetTableFuncUnmanagedDelegate (IntPtr face, Tag tag, IntPtr user_data);

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate void DestroyFuncUnmanagedDelegate (IntPtr user_data);

		private static readonly IntPtr table_func;
		private static readonly IntPtr destroy_func;

		private static readonly GetTableFuncUnmanagedDelegate GetTableFuncUnmanaged = InternalGetTableFunc;
		private static readonly DestroyFuncUnmanagedDelegate DestroyFuncUnmanaged = InternalDestroyFunc;

		public delegate IntPtr GetTableDelegate (IntPtr face, Tag tag, IntPtr userData);
		public delegate void DestroyDelegate ();

		private readonly GCHandle gcHandle;
		private readonly GetTableDelegate getTableFunc;
		private readonly DestroyDelegate destroyFunc;

		static Face ()
		{
			table_func = Marshal.GetFunctionPointerForDelegate (GetTableFuncUnmanaged);
			destroy_func = Marshal.GetFunctionPointerForDelegate (DestroyFuncUnmanaged);
		}

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

		public Face (GetTableDelegate getTableFunc, DestroyDelegate destroyFunc = null)
				: this (IntPtr.Zero)
		{
			this.getTableFunc = getTableFunc ?? throw new ArgumentNullException (nameof (getTableFunc));
			this.destroyFunc = destroyFunc;
			gcHandle = GCHandle.Alloc (this);
			Handle = HarfBuzzApi.hb_face_create_for_tables (table_func, GCHandle.ToIntPtr (gcHandle), destroy_func);
		}

		internal Face (IntPtr handle)
			: base (handle)
		{
		}

		public static Face Empty => new Face (HarfBuzzApi.hb_face_get_empty ());

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

			if (gcHandle.IsAllocated) {
				gcHandle.Free ();
			}
		}

		[MonoPInvokeCallback (typeof (GetTableFuncUnmanagedDelegate))]
		private static IntPtr InternalGetTableFunc (IntPtr face, Tag tag, IntPtr user_data)
		{
			var obj = (Face)GCHandle.FromIntPtr (user_data).Target;
			return obj.getTableFunc.Invoke (face, tag, user_data);
		}

		[MonoPInvokeCallback (typeof (DestroyFuncUnmanagedDelegate))]
		private static void InternalDestroyFunc (IntPtr user_data)
		{
			var obj = (Face)GCHandle.FromIntPtr (user_data).Target;
			obj.destroyFunc?.Invoke ();
		}
	}
}
