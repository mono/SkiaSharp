using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_runtimeeffect_uniform_t
	[StructLayout (LayoutKind.Sequential)]
	internal unsafe partial struct SKRuntimeEffectUniformNative : IEquatable<SKRuntimeEffectUniformNative> {
		// public const char* fName
		public /* char */ void* fName;

		// public size_t fNameLength
		public /* size_t */ IntPtr fNameLength;

		// public size_t fOffset
		public /* size_t */ IntPtr fOffset;

		// public sk_runtimeeffect_uniform_type_t fType
		public SKRuntimeEffectUniformTypeNative fType;

		// public int fCount
		public Int32 fCount;

		// public sk_runtimeeffect_uniform_flags_t fFlags
		public SKRuntimeEffectUniformFlagsNative fFlags;

		public readonly bool Equals (SKRuntimeEffectUniformNative obj) =>
#pragma warning disable CS8909
			fName == obj.fName && fNameLength == obj.fNameLength && fOffset == obj.fOffset && fType == obj.fType && fCount == obj.fCount && fFlags == obj.fFlags;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKRuntimeEffectUniformNative f && Equals (f);

		public static bool operator == (SKRuntimeEffectUniformNative left, SKRuntimeEffectUniformNative right) =>
			left.Equals (right);

		public static bool operator != (SKRuntimeEffectUniformNative left, SKRuntimeEffectUniformNative right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fName);
			hash.Add (fNameLength);
			hash.Add (fOffset);
			hash.Add (fType);
			hash.Add (fCount);
			hash.Add (fFlags);
			return hash.ToHashCode ();
		}

	}
}
