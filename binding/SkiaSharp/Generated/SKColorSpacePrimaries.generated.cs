using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_colorspace_primaries_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKColorSpacePrimaries : IEquatable<SKColorSpacePrimaries> {
		// public float fRX
		private Single fRX;
		public Single RX {
			readonly get => fRX;
			set => fRX = value;
		}

		// public float fRY
		private Single fRY;
		public Single RY {
			readonly get => fRY;
			set => fRY = value;
		}

		// public float fGX
		private Single fGX;
		public Single GX {
			readonly get => fGX;
			set => fGX = value;
		}

		// public float fGY
		private Single fGY;
		public Single GY {
			readonly get => fGY;
			set => fGY = value;
		}

		// public float fBX
		private Single fBX;
		public Single BX {
			readonly get => fBX;
			set => fBX = value;
		}

		// public float fBY
		private Single fBY;
		public Single BY {
			readonly get => fBY;
			set => fBY = value;
		}

		// public float fWX
		private Single fWX;
		public Single WX {
			readonly get => fWX;
			set => fWX = value;
		}

		// public float fWY
		private Single fWY;
		public Single WY {
			readonly get => fWY;
			set => fWY = value;
		}

		public readonly bool Equals (SKColorSpacePrimaries obj) =>
#pragma warning disable CS8909
			fRX == obj.fRX && fRY == obj.fRY && fGX == obj.fGX && fGY == obj.fGY && fBX == obj.fBX && fBY == obj.fBY && fWX == obj.fWX && fWY == obj.fWY;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKColorSpacePrimaries f && Equals (f);

		public static bool operator == (SKColorSpacePrimaries left, SKColorSpacePrimaries right) =>
			left.Equals (right);

		public static bool operator != (SKColorSpacePrimaries left, SKColorSpacePrimaries right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fRX);
			hash.Add (fRY);
			hash.Add (fGX);
			hash.Add (fGY);
			hash.Add (fBX);
			hash.Add (fBY);
			hash.Add (fWX);
			hash.Add (fWY);
			return hash.ToHashCode ();
		}

	}
}
