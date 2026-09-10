#nullable disable

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp
{
	/// <summary>The delegate that will be invoked when a resource is ready to be discarded.</summary>
	/// <remarks />
	public delegate void ReleaseDelegate ();

	/// <summary>Represents a callback method that retrieves a font table blob.</summary>
	/// <param name="face">The font face being queried.</param>
	/// <param name="tag">The table tag to retrieve.</param>
	/// <returns>A <see cref="T:HarfBuzzSharp.Blob" /> containing the table data, or <see langword="null" /> if not found.</returns>
	/// <remarks />
	public delegate Blob GetTableDelegate (Face face, Tag tag);

	internal static unsafe partial class DelegateProxies
	{
		private static partial void DestroyProxyImplementation (void* user_data)
		{
			var del = Get<ReleaseDelegate> ((IntPtr)user_data, out var gch);
			try {
				del.Invoke ();
			} finally {
				gch.Free ();
			}
		}

		private static partial void DestroyProxyImplementationForMulti (void* user_data)
		{
			var del = GetMulti<ReleaseDelegate> ((IntPtr)user_data, out var gch);
			try {
				del?.Invoke ();
			} finally {
				gch.Free ();
			}
		}

		private static partial IntPtr ReferenceTableProxyImplementation (IntPtr face, uint tag, void* user_data)
		{
			GetMultiUserData<GetTableDelegate, Face> ((IntPtr)user_data, out var getTable, out var userData, out _);
			var blob = getTable.Invoke (userData, tag);
			return blob?.Handle ?? IntPtr.Zero;
		}
	}
}
