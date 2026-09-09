using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_point3_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKPoint3 : IEquatable<SKPoint3> {
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

		// public float z
		private Single z;
		public Single Z {
			readonly get => z;
			set => z = value;
		}

		public readonly bool Equals (SKPoint3 obj) =>
#pragma warning disable CS8909
			x == obj.x && y == obj.y && z == obj.z;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKPoint3 f && Equals (f);

		public static bool operator == (SKPoint3 left, SKPoint3 right) =>
			left.Equals (right);

		public static bool operator != (SKPoint3 left, SKPoint3 right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (x);
			hash.Add (y);
			hash.Add (z);
			return hash.ToHashCode ();
		}

	}
}
