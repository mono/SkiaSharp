using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_runtimeeffect_child_t
	[StructLayout (LayoutKind.Sequential)]
	internal unsafe partial struct SKRuntimeEffectChildNative : IEquatable<SKRuntimeEffectChildNative> {
		// public const char* fName
		public /* char */ void* fName;

		// public size_t fNameLength
		public /* size_t */ IntPtr fNameLength;

		// public sk_runtimeeffect_child_type_t fType
		public SKRuntimeEffectChildTypeNative fType;

		// public int fIndex
		public Int32 fIndex;

		public readonly bool Equals (SKRuntimeEffectChildNative obj) =>
#pragma warning disable CS8909
			fName == obj.fName && fNameLength == obj.fNameLength && fType == obj.fType && fIndex == obj.fIndex;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKRuntimeEffectChildNative f && Equals (f);

		public static bool operator == (SKRuntimeEffectChildNative left, SKRuntimeEffectChildNative right) =>
			left.Equals (right);

		public static bool operator != (SKRuntimeEffectChildNative left, SKRuntimeEffectChildNative right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fName);
			hash.Add (fNameLength);
			hash.Add (fType);
			hash.Add (fIndex);
			return hash.ToHashCode ();
		}

	}
}
