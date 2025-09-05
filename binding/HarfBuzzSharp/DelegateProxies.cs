#nullable disable

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
// ReSharper disable PartialMethodParameterNameMismatch

namespace HarfBuzzSharp
{
	/// <summary>
	/// The delegate that will be invoked when a resource is ready to be discarded.
	/// </summary>
	public delegate void ReleaseDelegate ();

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
