using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// gr_vk_ycbcr_components_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct GRVkYcbcrComponents : IEquatable<GRVkYcbcrComponents> {
		// public uint32_t r
		private UInt32 r;
		public UInt32 R {
			readonly get => r;
			set => r = value;
		}

		// public uint32_t g
		private UInt32 g;
		public UInt32 G {
			readonly get => g;
			set => g = value;
		}

		// public uint32_t b
		private UInt32 b;
		public UInt32 B {
			readonly get => b;
			set => b = value;
		}

		// public uint32_t a
		private UInt32 a;
		public UInt32 A {
			readonly get => a;
			set => a = value;
		}

		public readonly bool Equals (GRVkYcbcrComponents obj) =>
#pragma warning disable CS8909
			r == obj.r && g == obj.g && b == obj.b && a == obj.a;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is GRVkYcbcrComponents f && Equals (f);

		public static bool operator == (GRVkYcbcrComponents left, GRVkYcbcrComponents right) =>
			left.Equals (right);

		public static bool operator != (GRVkYcbcrComponents left, GRVkYcbcrComponents right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (r);
			hash.Add (g);
			hash.Add (b);
			hash.Add (a);
			return hash.ToHashCode ();
		}

	}
}
