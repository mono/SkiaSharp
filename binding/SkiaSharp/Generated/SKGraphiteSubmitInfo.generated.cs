using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_graphite_submit_info_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKGraphiteSubmitInfo : IEquatable<SKGraphiteSubmitInfo> {
		// public bool fSync
		private Byte fSync;
		public bool Sync {
			readonly get => fSync > 0;
			set => fSync = value ? (byte)1 : (byte)0;
		}

		// public bool fMarkBoundary
		private Byte fMarkBoundary;
		public bool MarkBoundary {
			readonly get => fMarkBoundary > 0;
			set => fMarkBoundary = value ? (byte)1 : (byte)0;
		}

		// public uint64_t fFrameID
		private UInt64 fFrameID;
		public UInt64 FrameID {
			readonly get => fFrameID;
			set => fFrameID = value;
		}

		public readonly bool Equals (SKGraphiteSubmitInfo obj) =>
#pragma warning disable CS8909
			fSync == obj.fSync && fMarkBoundary == obj.fMarkBoundary && fFrameID == obj.fFrameID;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKGraphiteSubmitInfo f && Equals (f);

		public static bool operator == (SKGraphiteSubmitInfo left, SKGraphiteSubmitInfo right) =>
			left.Equals (right);

		public static bool operator != (SKGraphiteSubmitInfo left, SKGraphiteSubmitInfo right) =>
			!left.Equals (right);

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
