#nullable disable

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp
{
	/// <summary>
	/// Represents a native object.
	/// </summary>
	/// <remarks></remarks>
	public class NativeObject : IDisposable
	{
		private bool isDisposed;
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

		/// <summary>
		/// Allows an object to try to free resources and perform other cleanup operations before it is reclaimed by garbage collection.
		/// </summary>
		/// <remarks></remarks>
		~NativeObject ()
		{
			Dispose (false);
		}

		/// <summary>
		/// Gets or sets the handle to the underlying native object.
		/// </summary>
		/// <value></value>
		/// <remarks></remarks>
		public virtual IntPtr Handle { get; protected set; }

		// Dispose method - always called
		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="T:HarfBuzzSharp.NativeObject" /> and optionally releases the managed resources.
		/// </summary>
		/// <param name="disposing">
		/// <see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:HarfBuzzSharp.NativeObject" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
		protected virtual void Dispose (bool disposing)
		{
			if (isDisposed) {
				return;
			}

			isDisposed = true;

			if (!disposing) {
				return;
			}

			DisposeHandler ();

			if (zero) {
				Handle = IntPtr.Zero;
			}
		}

		// Intended to be overridden - always safe to use
		// since it will never be called unless applicable
		/// <summary>
		/// Releases the unmanaged resources used.
		/// </summary>
		/// <remarks></remarks>
		protected virtual void DisposeHandler ()
		{
		}

		/// <summary>
		/// Releases all resources used by this <see cref="T:HarfBuzzSharp.NativeObject" />.
		/// </summary>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:HarfBuzzSharp.NativeObject" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		internal static IEnumerable<string> PtrToStringArray (IntPtr intPtr)
		{
			if (intPtr != IntPtr.Zero) {
				var ptr = Marshal.ReadIntPtr (intPtr);
				while (ptr != IntPtr.Zero) {
					var element = Marshal.PtrToStringAnsi (ptr);
					yield return element;
					intPtr = new IntPtr (intPtr.ToInt64 () + IntPtr.Size);
					ptr = Marshal.ReadIntPtr (intPtr);
				}
			}
		}
	}
}
