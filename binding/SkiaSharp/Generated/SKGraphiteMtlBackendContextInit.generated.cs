using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_graphite_mtl_backend_context_init_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKGraphiteMtlBackendContextInit : IEquatable<SKGraphiteMtlBackendContextInit> {
		// public void* fDevice
		private void* fDevice;
		public void* Device {
			readonly get => fDevice;
			set => fDevice = value;
		}

		// public void* fQueue
		private void* fQueue;
		public void* Queue {
			readonly get => fQueue;
			set => fQueue = value;
		}

		public readonly bool Equals (SKGraphiteMtlBackendContextInit obj) =>
#pragma warning disable CS8909
			fDevice == obj.fDevice && fQueue == obj.fQueue;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKGraphiteMtlBackendContextInit f && Equals (f);

		public static bool operator == (SKGraphiteMtlBackendContextInit left, SKGraphiteMtlBackendContextInit right) =>
			left.Equals (right);

		public static bool operator != (SKGraphiteMtlBackendContextInit left, SKGraphiteMtlBackendContextInit right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fDevice);
			hash.Add (fQueue);
			return hash.ToHashCode ();
		}

	}
}
