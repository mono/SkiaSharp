using System;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp
{
	using System.Collections.Generic;

	public class NativeObject : IDisposable
	{
		private readonly bool zero;

		internal NativeObject (IntPtr handle)
		{
			Handle = handle;
			zero = true;
		}

		internal NativeObject (IntPtr handle, bool zero)
		{
			Handle = handle;
			this.zero = zero;
		}

		~NativeObject ()
		{
			Dispose (false);
		}

		public virtual IntPtr Handle { get; protected set; }

		protected virtual void Dispose (bool disposing)
		{
			if (zero) {
				Handle = IntPtr.Zero;
			}
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		internal static int SizeOf<T> ()
		{
#if WINDOWS_UWP || NET_STANDARD
			return Marshal.SizeOf<T>();
#else
			return Marshal.SizeOf (typeof (T));
#endif
		}

		internal static T PtrToStructure<T> (IntPtr intPtr)
		{
#if WINDOWS_UWP || NET_STANDARD
			return Marshal.PtrToStructure<T>(intPtr);
#else
			return (T)Marshal.PtrToStructure (intPtr, typeof (T));
#endif
		}

		internal static T[] PtrToStructureArray<T> (IntPtr intPtr, int count)
		{
			var items = new T[count];
			var size = SizeOf<T> ();
			for (var i = 0; i < count; i++) {
				var newPtr = new IntPtr (intPtr.ToInt64 () + (i * size));
				items[i] = PtrToStructure<T> (newPtr);
			}
			return items;
		}

		internal static IntPtr StructureArrayToPtr<T> (IReadOnlyList<T> items)
		{
			var size = SizeOf<T> ();
			var memory = Marshal.AllocCoTaskMem (size * items.Count);
			for (var i = 0; i < items.Count; i++) {
				var ptr = new IntPtr (memory.ToInt64 () + (i * size));
				Marshal.StructureToPtr (items[i], ptr, true);
			}
			return memory;
		}

		internal static T PtrToStructure<T> (IntPtr intPtr, int index)
		{
			var size = SizeOf<T> ();
			var newPtr = new IntPtr (intPtr.ToInt64 () + (index * size));
			return PtrToStructure<T> (newPtr);
		}

		internal static IEnumerable<string> PtrToStringArray (IntPtr intPtr)
		{
			if (intPtr != IntPtr.Zero) {
				IntPtr ptr = Marshal.ReadIntPtr (intPtr);
				while (ptr != IntPtr.Zero) {
					string element = Marshal.PtrToStringAnsi (ptr);
					yield return element;
					intPtr = new IntPtr (intPtr.ToInt64 () + IntPtr.Size);
					ptr = Marshal.ReadIntPtr (intPtr);
				}
			}
		}
	}
}
