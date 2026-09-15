using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_graphite_submit_info_t
	/// <summary>Specifies options that control how queued work is submitted to the GPU by a <see cref="T:SkiaSharp.SKGraphiteContext" />.</summary>
	/// <remarks />
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKGraphiteSubmitInfo : IEquatable<SKGraphiteSubmitInfo> {
		// public bool fSync
		private Byte fSync;
		/// <summary>Gets or sets a value indicating whether the submit call blocks until the GPU has finished the submitted work.</summary>
		/// <value><see langword="true" /> to wait for the GPU to finish; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool Sync {
			readonly get => fSync > 0;
			set => fSync = value ? (byte)1 : (byte)0;
		}

		// public bool fMarkBoundary
		private Byte fMarkBoundary;
		/// <summary>Gets or sets a value indicating whether the submission marks a frame boundary.</summary>
		/// <value><see langword="true" /> to mark a frame boundary; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool MarkBoundary {
			readonly get => fMarkBoundary > 0;
			set => fMarkBoundary = value ? (byte)1 : (byte)0;
		}

		// public uint64_t fFrameID
		private UInt64 fFrameID;
		/// <summary>Gets or sets the identifier of the frame associated with this submission.</summary>
		/// <value>The frame identifier.</value>
		/// <remarks />
		public UInt64 FrameID {
			readonly get => fFrameID;
			set => fFrameID = value;
		}

		/// <summary>Determines whether the specified submission info is equal to the current submission info.</summary>
		/// <param name="obj">The submission info to compare with the current submission info.</param>
		/// <returns><see langword="true" /> if the two values are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (SKGraphiteSubmitInfo obj) =>
#pragma warning disable CS8909
			fSync == obj.fSync && fMarkBoundary == obj.fMarkBoundary && fFrameID == obj.fFrameID;
#pragma warning restore CS8909

		/// <summary>Determines whether the specified object is equal to the current submission info.</summary>
		/// <param name="obj">The object to compare with the current submission info.</param>
		/// <returns><see langword="true" /> if the specified object is equal to the current value; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object obj) =>
			obj is SKGraphiteSubmitInfo f && Equals (f);

		/// <summary>Indicates whether two submission info values are equal.</summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns><see langword="true" /> if the two values are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (SKGraphiteSubmitInfo left, SKGraphiteSubmitInfo right) =>
			left.Equals (right);

		/// <summary>Indicates whether two submission info values are not equal.</summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns><see langword="true" /> if the two values are not equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (SKGraphiteSubmitInfo left, SKGraphiteSubmitInfo right) =>
			!left.Equals (right);

		/// <summary>Returns the hash code for this submission info.</summary>
		/// <returns>A hash code for the current value.</returns>
		/// <remarks />
		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fSync);
			hash.Add (fMarkBoundary);
			hash.Add (fFrameID);
			return hash.ToHashCode ();
		}

	}
}
