using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_size_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKSize : IEquatable<SKSize> {
		// public float w
		private Single w;
		public Single Width {
			readonly get => w;
			set => w = value;
		}

		// public float h
		private Single h;
		public Single Height {
			readonly get => h;
			set => h = value;
		}

		public readonly bool Equals (SKSize obj) =>
#pragma warning disable CS8909
			w == obj.w && h == obj.h;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKSize f && Equals (f);

		public static bool operator == (SKSize left, SKSize right) =>
			left.Equals (right);

		public static bool operator != (SKSize left, SKSize right) =>
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
