using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_graphite_dawn_backend_context_init_t
	/// <summary>Contains the native Dawn (WebGPU) handles used to initialize a Dawn-backed Graphite context.</summary>
	/// <remarks />
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKGraphiteDawnBackendContextInit : IEquatable<SKGraphiteDawnBackendContextInit> {
		// public void* fInstance
		private void* fInstance;
		/// <summary>Gets or sets the pointer to the WebGPU instance.</summary>
		/// <value>A pointer to the WebGPU instance.</value>
		/// <remarks />
		public void* Instance {
			readonly get => fInstance;
			set => fInstance = value;
		}

		// public void* fDevice
		private void* fDevice;
		/// <summary>Gets or sets the pointer to the WebGPU device.</summary>
		/// <value>A pointer to the WebGPU device.</value>
		/// <remarks />
		public void* Device {
			readonly get => fDevice;
			set => fDevice = value;
		}

		// public void* fQueue
		private void* fQueue;
		/// <summary>Gets or sets the pointer to the WebGPU queue.</summary>
		/// <value>A pointer to the WebGPU queue.</value>
		/// <remarks />
		public void* Queue {
			readonly get => fQueue;
			set => fQueue = value;
		}

		// public bool fNonYielding
		private Byte fNonYielding;
		/// <summary>Gets or sets a value indicating whether the environment cannot yield to pump the Dawn event loop.</summary>
		/// <value>
		///           <see langword="true" /> if the environment is non-yielding; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool NonYielding {
			readonly get => fNonYielding > 0;
			set => fNonYielding = value ? (byte)1 : (byte)0;
		}

		/// <param name="obj">The Dawn backend initialization data to compare with the current Dawn backend initialization data.</param>
		/// <summary>Determines whether the specified Dawn backend initialization data is equal to the current Dawn backend initialization data.</summary>
		/// <returns>
		///           <see langword="true" /> if the two values are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (SKGraphiteDawnBackendContextInit obj) =>
#pragma warning disable CS8909
			fInstance == obj.fInstance && fDevice == obj.fDevice && fQueue == obj.fQueue && fNonYielding == obj.fNonYielding;
#pragma warning restore CS8909

		/// <param name="obj">The object to compare with the current Dawn backend initialization data.</param>
		/// <summary>Determines whether the specified object is equal to the current Dawn backend initialization data.</summary>
		/// <returns>
		///           <see langword="true" /> if the specified object is equal to the current value; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object obj) =>
			obj is SKGraphiteDawnBackendContextInit f && Equals (f);

		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <summary>Indicates whether two Dawn backend initialization data values are equal.</summary>
		/// <returns>
		///           <see langword="true" /> if the two values are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (SKGraphiteDawnBackendContextInit left, SKGraphiteDawnBackendContextInit right) =>
			left.Equals (right);

		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <summary>Indicates whether two Dawn backend initialization data values are not equal.</summary>
		/// <returns>
		///           <see langword="true" /> if the two values are not equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (SKGraphiteDawnBackendContextInit left, SKGraphiteDawnBackendContextInit right) =>
			!left.Equals (right);

		/// <summary>Returns the hash code for this Dawn backend initialization data.</summary>
		/// <returns>A hash code for the current value.</returns>
		/// <remarks />
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
