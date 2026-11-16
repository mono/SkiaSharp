using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_mesh_make_args_t
	[StructLayout (LayoutKind.Sequential)]
	internal unsafe partial struct SKMeshMakeArgsNative : IEquatable<SKMeshMakeArgsNative> {
		// public sk_mesh_mode_t fMode
		public SKMeshMode fMode;

		// public sk_mesh_vertex_buffer_t* fVB
		public sk_mesh_vertex_buffer_t fVB;

		// public size_t fVCount
		public /* size_t */ IntPtr fVCount;

		// public size_t fVOffset
		public /* size_t */ IntPtr fVOffset;

		// public const void* fUniformData
		public void* fUniformData;

		// public size_t fUniformSize
		public /* size_t */ IntPtr fUniformSize;

		// public sk_flattenable_t** fChildren
		public sk_flattenable_t* fChildren;

		// public size_t fChildCount
		public /* size_t */ IntPtr fChildCount;

		// public sk_rect_t fBounds
		public SKRect fBounds;

		public readonly bool Equals (SKMeshMakeArgsNative obj) =>
#pragma warning disable CS8909
			fMode == obj.fMode && fVB == obj.fVB && fVCount == obj.fVCount && fVOffset == obj.fVOffset && fUniformData == obj.fUniformData && fUniformSize == obj.fUniformSize && fChildren == obj.fChildren && fChildCount == obj.fChildCount && fBounds == obj.fBounds;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKMeshMakeArgsNative f && Equals (f);

		public static bool operator == (SKMeshMakeArgsNative left, SKMeshMakeArgsNative right) =>
			left.Equals (right);

		public static bool operator != (SKMeshMakeArgsNative left, SKMeshMakeArgsNative right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fMode);
			hash.Add (fVB);
			hash.Add (fVCount);
			hash.Add (fVOffset);
			hash.Add (fUniformData);
			hash.Add (fUniformSize);
			hash.Add (fChildren);
			hash.Add (fChildCount);
			hash.Add (fBounds);
			return hash.ToHashCode ();
		}

	}
}
