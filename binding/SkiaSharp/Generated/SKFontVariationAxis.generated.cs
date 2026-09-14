using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_fontarguments_variation_axis_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKFontVariationAxis : IEquatable<SKFontVariationAxis> {
		// public sk_fourbytetag_t tag
		private SKFourByteTag tag;
		public SKFourByteTag Tag {
			readonly get => tag;
			set => tag = value;
		}

		// public float min
		private Single min;
		public Single Min {
			readonly get => min;
			set => min = value;
		}

		// public float def
		private Single def;
		public Single Default {
			readonly get => def;
			set => def = value;
		}

		// public float max
		private Single max;
		public Single Max {
			readonly get => max;
			set => max = value;
		}

		// public bool isHidden
		private Byte isHidden;
		public bool IsHidden {
			readonly get => isHidden > 0;
			set => isHidden = value ? (byte)1 : (byte)0;
		}

		public readonly bool Equals (SKFontVariationAxis obj) =>
#pragma warning disable CS8909
			tag == obj.tag && min == obj.min && def == obj.def && max == obj.max && isHidden == obj.isHidden;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKFontVariationAxis f && Equals (f);

		public static bool operator == (SKFontVariationAxis left, SKFontVariationAxis right) =>
			left.Equals (right);

		public static bool operator != (SKFontVariationAxis left, SKFontVariationAxis right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (tag);
			hash.Add (min);
			hash.Add (def);
			hash.Add (max);
			hash.Add (isHidden);
			return hash.ToHashCode ();
		}

	}
}
