using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_irect_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKRectI : IEquatable<SKRectI> {
		// public int32_t left
		private Int32 left;
		public Int32 Left {
			readonly get => left;
			set => left = value;
		}

		// public int32_t top
		private Int32 top;
		public Int32 Top {
			readonly get => top;
			set => top = value;
		}

		// public int32_t right
		private Int32 right;
		public Int32 Right {
			readonly get => right;
			set => right = value;
		}

		// public int32_t bottom
		private Int32 bottom;
		public Int32 Bottom {
			readonly get => bottom;
			set => bottom = value;
		}

		public readonly bool Equals (SKRectI obj) =>
#pragma warning disable CS8909
			left == obj.left && top == obj.top && right == obj.right && bottom == obj.bottom;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKRectI f && Equals (f);

		public static bool operator == (SKRectI left, SKRectI right) =>
			left.Equals (right);

		public static bool operator != (SKRectI left, SKRectI right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (left);
			hash.Add (top);
			hash.Add (right);
			hash.Add (bottom);
			return hash.ToHashCode ();
		}

	}
}
