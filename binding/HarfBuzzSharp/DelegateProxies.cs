#nullable disable

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
// ReSharper disable PartialMethodParameterNameMismatch

namespace HarfBuzzSharp
{
	public delegate void ReleaseDelegate ();

	/// <param name="context">The user data passed to <see cref="M:HarfBuzzSharp.Blob.#ctor(System.IntPtr,System.UInt32,HarfBuzzSharp.MemoryMode,System.Object,HarfBuzzSharp.BlobReleaseDelegate)" />.</param>
	/// <summary>The delegate that will be invoked when a blob is ready to be discarded.</summary>
	/// <remarks></remarks>
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	[System.Obsolete("Use ReleaseDelegate instead.")]
	public delegate void BlobReleaseDelegate (object context);

	public delegate Blob GetTableDelegate (Face face, Tag tag);

	internal static unsafe partial class DelegateProxies
	{
		private static partial void DestroyProxyImplementation (void* context)
		{
			var del = Get<ReleaseDelegate> ((IntPtr)context, out var gch);
			try {
				del.Invoke ();
			} finally {
				gch.Free ();
			}
		}

		private static partial void DestroyProxyImplementationForMulti (void* context)
		{
			var del = GetMulti<ReleaseDelegate> ((IntPtr)context, out var gch);
			try {
				del?.Invoke ();
			} finally {
				gch.Free ();
			}
		}

		private static partial IntPtr ReferenceTableProxyImplementation (IntPtr face, uint tag, void* context)
		{
			GetMultiUserData<GetTableDelegate, Face> ((IntPtr)context, out var getTable, out var userData, out _);
			var blob = getTable.Invoke (userData, tag);
			return blob?.Handle ?? IntPtr.Zero;
		}
	}
}
