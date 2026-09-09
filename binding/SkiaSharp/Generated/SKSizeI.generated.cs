using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_isize_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKSizeI : IEquatable<SKSizeI> {
		// public int32_t w
		private Int32 w;
		public Int32 Width {
			readonly get => w;
			set => w = value;
		}

		// public int32_t h
		private Int32 h;
		public Int32 Height {
			readonly get => h;
			set => h = value;
		}

		public readonly bool Equals (SKSizeI obj) =>
#pragma warning disable CS8909
			w == obj.w && h == obj.h;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKSizeI f && Equals (f);

		public static bool operator == (SKSizeI left, SKSizeI right) =>
			left.Equals (right);

		public static bool operator != (SKSizeI left, SKSizeI right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (w);
			hash.Add (h);
			return hash.ToHashCode ();
		}

	}
}
