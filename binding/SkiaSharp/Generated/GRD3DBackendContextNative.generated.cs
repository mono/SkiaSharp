using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// gr_d3d_backendcontext_t
	[StructLayout (LayoutKind.Sequential)]
	internal unsafe partial struct GRD3DBackendContextNative : IEquatable<GRD3DBackendContextNative> {
		// public d3d_dxgi_adapter_t* fAdapter
		public IntPtr fAdapter;

		// public d3d_d12_device_t* fDevice
		public IntPtr fDevice;

		// public d3d_d12_command_queue_t* fQueue
		public IntPtr fQueue;

		// public gr_d3d_memory_allocator_t* fMemoryAllocator
		public IntPtr fMemoryAllocator;

		// public bool fProtectedContext
		public Byte fProtectedContext;

		public readonly bool Equals (GRD3DBackendContextNative obj) =>
#pragma warning disable CS8909
			fAdapter == obj.fAdapter && fDevice == obj.fDevice && fQueue == obj.fQueue && fMemoryAllocator == obj.fMemoryAllocator && fProtectedContext == obj.fProtectedContext;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is GRD3DBackendContextNative f && Equals (f);

		public static bool operator == (GRD3DBackendContextNative left, GRD3DBackendContextNative right) =>
			left.Equals (right);

		public static bool operator != (GRD3DBackendContextNative left, GRD3DBackendContextNative right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fAdapter);
			hash.Add (fDevice);
			hash.Add (fQueue);
			hash.Add (fMemoryAllocator);
			hash.Add (fProtectedContext);
			return hash.ToHashCode ();
		}

	}
}
