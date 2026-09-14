using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_feature_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct Feature : IEquatable<Feature> {
		// public hb_tag_t tag
		private UInt32 tag;

		// public uint32_t value
		private UInt32 value;

		// public unsigned int start
		private UInt32 start;

		// public unsigned int end
		private UInt32 end;

		public readonly bool Equals (Feature obj) =>
#pragma warning disable CS8909
			tag == obj.tag && value == obj.value && start == obj.start && end == obj.end;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is Feature f && Equals (f);

		public static bool operator == (Feature left, Feature right) =>
			left.Equals (right);

		public static bool operator != (Feature left, Feature right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (tag);
			hash.Add (value);
			hash.Add (start);
			hash.Add (end);
			return hash.ToHashCode ();
		}

	}
}
