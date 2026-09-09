using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_managedtracememorydump_procs_t
	[StructLayout (LayoutKind.Sequential)]
	internal unsafe partial struct SKManagedTraceMemoryDumpDelegates : IEquatable<SKManagedTraceMemoryDumpDelegates> {
		// public sk_managedtraceMemoryDump_dumpNumericValue_proc fDumpNumericValue
#if USE_LIBRARY_IMPORT
		public delegate* unmanaged[Cdecl] <IntPtr, void*, /* char */ void*, /* char */ void*, /* char */ void*, UInt64, void> fDumpNumericValue;
#else
		public SKManagedTraceMemoryDumpDumpNumericValueProxyDelegate fDumpNumericValue;
#endif

		// public sk_managedtraceMemoryDump_dumpStringValue_proc fDumpStringValue
#if USE_LIBRARY_IMPORT
		public delegate* unmanaged[Cdecl] <IntPtr, void*, /* char */ void*, /* char */ void*, /* char */ void*, void> fDumpStringValue;
#else
		public SKManagedTraceMemoryDumpDumpStringValueProxyDelegate fDumpStringValue;
#endif

		public readonly bool Equals (SKManagedTraceMemoryDumpDelegates obj) =>
#pragma warning disable CS8909
			fDumpNumericValue == obj.fDumpNumericValue && fDumpStringValue == obj.fDumpStringValue;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKManagedTraceMemoryDumpDelegates f && Equals (f);

		public static bool operator == (SKManagedTraceMemoryDumpDelegates left, SKManagedTraceMemoryDumpDelegates right) =>
			left.Equals (right);

		public static bool operator != (SKManagedTraceMemoryDumpDelegates left, SKManagedTraceMemoryDumpDelegates right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fDumpNumericValue);
			hash.Add (fDumpStringValue);
			return hash.ToHashCode ();
		}

	}
}
