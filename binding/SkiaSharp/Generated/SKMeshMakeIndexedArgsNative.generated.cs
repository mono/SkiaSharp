using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_mesh_make_indexed_args_t
	[StructLayout (LayoutKind.Sequential)]
	internal unsafe partial struct SKMeshMakeIndexedArgsNative : IEquatable<SKMeshMakeIndexedArgsNative> {
		// public sk_mesh_mode_t fMode
		public SKMeshMode fMode;

		// public sk_mesh_vertex_buffer_t* fVB
		public sk_mesh_vertex_buffer_t fVB;

		// public size_t fVCount
		public /* size_t */ IntPtr fVCount;

		// public size_t fVOffset
		public /* size_t */ IntPtr fVOffset;

		// public sk_mesh_index_buffer_t* fIB
		public sk_mesh_index_buffer_t fIB;

		// public size_t fICount
		public /* size_t */ IntPtr fICount;

		// public size_t fIOffset
		public /* size_t */ IntPtr fIOffset;

		// public const sk_data_t* fUniforms
		public sk_data_t fUniforms;

		// public sk_flattenable_t** fChildren
		public sk_flattenable_t* fChildren;

		// public size_t fChildCount
		public /* size_t */ IntPtr fChildCount;

		// public sk_rect_t fBounds
		public SKRect fBounds;

		public readonly bool Equals (SKMeshMakeIndexedArgsNative obj) =>
#pragma warning disable CS8909
			fMode == obj.fMode && fVB == obj.fVB && fVCount == obj.fVCount && fVOffset == obj.fVOffset && fIB == obj.fIB && fICount == obj.fICount && fIOffset == obj.fIOffset && fUniforms == obj.fUniforms && fChildren == obj.fChildren && fChildCount == obj.fChildCount && fBounds == obj.fBounds;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKMeshMakeIndexedArgsNative f && Equals (f);

		public static bool operator == (SKMeshMakeIndexedArgsNative left, SKMeshMakeIndexedArgsNative right) =>
			left.Equals (right);

		public static bool operator != (SKMeshMakeIndexedArgsNative left, SKMeshMakeIndexedArgsNative right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fMode);
			hash.Add (fVB);
			hash.Add (fVCount);
			hash.Add (fVOffset);
			hash.Add (fIB);
			hash.Add (fICount);
			hash.Add (fIOffset);
			hash.Add (fUniforms);
			hash.Add (fChildren);
			hash.Add (fChildCount);
			hash.Add (fBounds);
			return hash.ToHashCode ();
		}

	}
}
