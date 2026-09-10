using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_graphite_mtl_backend_context_init_t
	/// <summary>Contains the native Metal handles used to initialize a Metal-backed Graphite context.</summary>
	/// <remarks />
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKGraphiteMtlBackendContextInit : IEquatable<SKGraphiteMtlBackendContextInit> {
		// public void* fDevice
		private void* fDevice;
		/// <summary>Gets or sets the pointer to the Metal device.</summary>
		/// <value>A pointer to the Metal device.</value>
		/// <remarks />
		public void* Device {
			readonly get => fDevice;
			set => fDevice = value;
		}

		// public void* fQueue
		private void* fQueue;
		/// <summary>Gets or sets the pointer to the Metal command queue.</summary>
		/// <value>A pointer to the Metal command queue.</value>
		/// <remarks />
		public void* Queue {
			readonly get => fQueue;
			set => fQueue = value;
		}

		/// <param name="obj">The Metal backend initialization data to compare with the current Metal backend initialization data.</param>
		/// <summary>Determines whether the specified Metal backend initialization data is equal to the current Metal backend initialization data.</summary>
		/// <returns>
		///           <see langword="true" /> if the two values are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (SKGraphiteMtlBackendContextInit obj) =>
#pragma warning disable CS8909
			fDevice == obj.fDevice && fQueue == obj.fQueue;
#pragma warning restore CS8909

		/// <param name="obj">The object to compare with the current Metal backend initialization data.</param>
		/// <summary>Determines whether the specified object is equal to the current Metal backend initialization data.</summary>
		/// <returns>
		///           <see langword="true" /> if the specified object is equal to the current value; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object obj) =>
			obj is SKGraphiteMtlBackendContextInit f && Equals (f);

		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <summary>Indicates whether two Metal backend initialization data values are equal.</summary>
		/// <returns>
		///           <see langword="true" /> if the two values are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (SKGraphiteMtlBackendContextInit left, SKGraphiteMtlBackendContextInit right) =>
			left.Equals (right);

		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <summary>Indicates whether two Metal backend initialization data values are not equal.</summary>
		/// <returns>
		///           <see langword="true" /> if the two values are not equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (SKGraphiteMtlBackendContextInit left, SKGraphiteMtlBackendContextInit right) =>
			!left.Equals (right);

		/// <summary>Returns the hash code for this Metal backend initialization data.</summary>
		/// <returns>A hash code for the current value.</returns>
		/// <remarks />
		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fDevice);
			hash.Add (fQueue);
			return hash.ToHashCode ();
		}

	}
}
