using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_point_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKPoint : IEquatable<SKPoint> {
		// public float x
		private Single x;
		public Single X {
			readonly get => x;
			set => x = value;
		}

		// public float y
		private Single y;
		public Single Y {
			readonly get => y;
			set => y = value;
		}

		public readonly bool Equals (SKPoint obj) =>
#pragma warning disable CS8909
			x == obj.x && y == obj.y;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKPoint f && Equals (f);

		public static bool operator == (SKPoint left, SKPoint right) =>
			left.Equals (right);

		public static bool operator != (SKPoint left, SKPoint right) =>
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
