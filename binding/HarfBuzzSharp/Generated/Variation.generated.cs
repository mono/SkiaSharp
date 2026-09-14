using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_variation_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct Variation : IEquatable<Variation> {
		// public hb_tag_t tag
		private UInt32 tag;
		public UInt32 Tag {
			readonly get => tag;
			set => tag = value;
		}

		// public float value
		private Single value;
		public Single Value {
			readonly get => this.value;
			set => this.value = value;
		}

		public readonly bool Equals (Variation obj) =>
#pragma warning disable CS8909
			tag == obj.tag && value == obj.value;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is Variation f && Equals (f);

		public static bool operator == (Variation left, Variation right) =>
			left.Equals (right);

		public static bool operator != (Variation left, Variation right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (tag);
			hash.Add (value);
			return hash.ToHashCode ();
		}

	}
}
