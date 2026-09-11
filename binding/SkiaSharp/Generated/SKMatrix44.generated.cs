using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_matrix44_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKMatrix44 : IEquatable<SKMatrix44> {
		// public float m00
		private Single m00;
		public Single M00 {
			readonly get => m00;
			set => m00 = value;
		}

		// public float m01
		private Single m01;
		public Single M01 {
			readonly get => m01;
			set => m01 = value;
		}

		// public float m02
		private Single m02;
		public Single M02 {
			readonly get => m02;
			set => m02 = value;
		}

		// public float m03
		private Single m03;
		public Single M03 {
			readonly get => m03;
			set => m03 = value;
		}

		// public float m10
		private Single m10;
		public Single M10 {
			readonly get => m10;
			set => m10 = value;
		}

		// public float m11
		private Single m11;
		public Single M11 {
			readonly get => m11;
			set => m11 = value;
		}

		// public float m12
		private Single m12;
		public Single M12 {
			readonly get => m12;
			set => m12 = value;
		}

		// public float m13
		private Single m13;
		public Single M13 {
			readonly get => m13;
			set => m13 = value;
		}

		// public float m20
		private Single m20;
		public Single M20 {
			readonly get => m20;
			set => m20 = value;
		}

		// public float m21
		private Single m21;
		public Single M21 {
			readonly get => m21;
			set => m21 = value;
		}

		// public float m22
		private Single m22;
		public Single M22 {
			readonly get => m22;
			set => m22 = value;
		}

		// public float m23
		private Single m23;
		public Single M23 {
			readonly get => m23;
			set => m23 = value;
		}

		// public float m30
		private Single m30;
		public Single M30 {
			readonly get => m30;
			set => m30 = value;
		}

		// public float m31
		private Single m31;
		public Single M31 {
			readonly get => m31;
			set => m31 = value;
		}

		// public float m32
		private Single m32;
		public Single M32 {
			readonly get => m32;
			set => m32 = value;
		}

		// public float m33
		private Single m33;
		public Single M33 {
			readonly get => m33;
			set => m33 = value;
		}

		public readonly bool Equals (SKMatrix44 obj) =>
#pragma warning disable CS8909
			m00 == obj.m00 && m01 == obj.m01 && m02 == obj.m02 && m03 == obj.m03 && m10 == obj.m10 && m11 == obj.m11 && m12 == obj.m12 && m13 == obj.m13 && m20 == obj.m20 && m21 == obj.m21 && m22 == obj.m22 && m23 == obj.m23 && m30 == obj.m30 && m31 == obj.m31 && m32 == obj.m32 && m33 == obj.m33;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKMatrix44 f && Equals (f);

		public static bool operator == (SKMatrix44 left, SKMatrix44 right) =>
			left.Equals (right);

		public static bool operator != (SKMatrix44 left, SKMatrix44 right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (m00);
			hash.Add (m01);
			hash.Add (m02);
			hash.Add (m03);
			hash.Add (m10);
			hash.Add (m11);
			hash.Add (m12);
			hash.Add (m13);
			hash.Add (m20);
			hash.Add (m21);
			hash.Add (m22);
			hash.Add (m23);
			hash.Add (m30);
			hash.Add (m31);
			hash.Add (m32);
			hash.Add (m33);
			return hash.ToHashCode ();
		}

	}
}
