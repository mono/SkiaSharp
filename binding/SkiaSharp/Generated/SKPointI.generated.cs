using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_ipoint_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKPointI : IEquatable<SKPointI> {
		// public int32_t x
		private Int32 x;
		public Int32 X {
			readonly get => x;
			set => x = value;
		}

		// public int32_t y
		private Int32 y;
		public Int32 Y {
			readonly get => y;
			set => y = value;
		}

		public readonly bool Equals (SKPointI obj) =>
#pragma warning disable CS8909
			x == obj.x && y == obj.y;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKPointI f && Equals (f);

		public static bool operator == (SKPointI left, SKPointI right) =>
			left.Equals (right);

		public static bool operator != (SKPointI left, SKPointI right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (x);
			hash.Add (y);
			return hash.ToHashCode ();
		}

	}
}
