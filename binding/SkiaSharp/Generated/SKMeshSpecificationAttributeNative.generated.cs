using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_meshspecification_attribute_t
	[StructLayout (LayoutKind.Sequential)]
	internal unsafe partial struct SKMeshSpecificationAttributeNative : IEquatable<SKMeshSpecificationAttributeNative> {
		// public sk_meshspecification_attribute_type_t fType
		public SKMeshSpecificationAttributeType fType;

		// public size_t fOffset
		public /* size_t */ IntPtr fOffset;

		// public const char* fName
		public /* char */ void* fName;

		public readonly bool Equals (SKMeshSpecificationAttributeNative obj) =>
#pragma warning disable CS8909
			fType == obj.fType && fOffset == obj.fOffset && fName == obj.fName;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKMeshSpecificationAttributeNative f && Equals (f);

		public static bool operator == (SKMeshSpecificationAttributeNative left, SKMeshSpecificationAttributeNative right) =>
			left.Equals (right);

		public static bool operator != (SKMeshSpecificationAttributeNative left, SKMeshSpecificationAttributeNative right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fType);
			hash.Add (fOffset);
			hash.Add (fName);
			return hash.ToHashCode ();
		}

	}
}
