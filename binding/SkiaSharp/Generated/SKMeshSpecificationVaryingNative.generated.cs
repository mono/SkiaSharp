using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_meshspecification_varying_t
	[StructLayout (LayoutKind.Sequential)]
	internal unsafe partial struct SKMeshSpecificationVaryingNative : IEquatable<SKMeshSpecificationVaryingNative> {
		// public sk_meshspecification_varying_type_t fType
		public SKMeshSpecificationVaryingType fType;

		// public const char* fName
		public /* char */ void* fName;

		public readonly bool Equals (SKMeshSpecificationVaryingNative obj) =>
#pragma warning disable CS8909
			fType == obj.fType && fName == obj.fName;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKMeshSpecificationVaryingNative f && Equals (f);

		public static bool operator == (SKMeshSpecificationVaryingNative left, SKMeshSpecificationVaryingNative right) =>
			left.Equals (right);

		public static bool operator != (SKMeshSpecificationVaryingNative left, SKMeshSpecificationVaryingNative right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fType);
			hash.Add (fName);
			return hash.ToHashCode ();
		}

	}
}
