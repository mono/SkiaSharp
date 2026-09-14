using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_rect_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKRect : IEquatable<SKRect> {
		// public float left
		private Single left;
		public Single Left {
			readonly get => left;
			set => left = value;
		}

		// public float top
		private Single top;
		public Single Top {
			readonly get => top;
			set => top = value;
		}

		// public float right
		private Single right;
		public Single Right {
			readonly get => right;
			set => right = value;
		}

		// public float bottom
		private Single bottom;
		public Single Bottom {
			readonly get => bottom;
			set => bottom = value;
		}

		public readonly bool Equals (SKRect obj) =>
#pragma warning disable CS8909
			left == obj.left && top == obj.top && right == obj.right && bottom == obj.bottom;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKRect f && Equals (f);

		public static bool operator == (SKRect left, SKRect right) =>
			left.Equals (right);

		public static bool operator != (SKRect left, SKRect right) =>
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
