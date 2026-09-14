using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_graphite_dawn_backend_context_init_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKGraphiteDawnBackendContextInit : IEquatable<SKGraphiteDawnBackendContextInit> {
		// public void* fInstance
		private void* fInstance;
		public void* Instance {
			readonly get => fInstance;
			set => fInstance = value;
		}

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

		// public bool fNonYielding
		private Byte fNonYielding;
		public bool NonYielding {
			readonly get => fNonYielding > 0;
			set => fNonYielding = value ? (byte)1 : (byte)0;
		}

		public readonly bool Equals (SKGraphiteDawnBackendContextInit obj) =>
#pragma warning disable CS8909
			fInstance == obj.fInstance && fDevice == obj.fDevice && fQueue == obj.fQueue && fNonYielding == obj.fNonYielding;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKGraphiteDawnBackendContextInit f && Equals (f);

		public static bool operator == (SKGraphiteDawnBackendContextInit left, SKGraphiteDawnBackendContextInit right) =>
			left.Equals (right);

		public static bool operator != (SKGraphiteDawnBackendContextInit left, SKGraphiteDawnBackendContextInit right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fInstance);
			hash.Add (fDevice);
			hash.Add (fQueue);
			hash.Add (fNonYielding);
			return hash.ToHashCode ();
		}

	}
}
