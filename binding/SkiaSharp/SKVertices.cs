﻿#nullable disable

using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using System.ComponentModel;

namespace SkiaSharp
{
	public unsafe class SKVertices : SKObject, ISKNonVirtualReferenceCounted, ISKSkipObjectRegistration
	{
		internal SKVertices (IntPtr x, bool owns)
			: base (x, owns)
		{
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		void ISKNonVirtualReferenceCounted.ReferenceNative () => SkiaApi.sk_vertices_ref (Handle);

		void ISKNonVirtualReferenceCounted.UnreferenceNative () => SkiaApi.sk_vertices_unref (Handle);

		public static SKVertices CreateCopy (SKVertexMode vmode, SKPoint[] positions, SKColor[] colors) =>
			CreateCopy (vmode, positions.AsSpan (), colors.AsSpan ());

		public static SKVertices CreateCopy (SKVertexMode vmode, ReadOnlySpan<SKPoint> positions, ReadOnlySpan<SKColor> colors) =>
			CreateCopy (vmode, positions, null, colors, null);

		public static SKVertices CreateCopy (SKVertexMode vmode, SKPoint[] positions, SKPoint[] texs, SKColor[] colors) =>
			CreateCopy (vmode, positions.AsSpan (), texs.AsSpan (), colors.AsSpan ());

		public static SKVertices CreateCopy (SKVertexMode vmode, ReadOnlySpan<SKPoint> positions, ReadOnlySpan<SKPoint> texs, ReadOnlySpan<SKColor> colors) =>
			CreateCopy (vmode, positions, texs, colors, null);

		public static SKVertices CreateCopy (SKVertexMode vmode, SKPoint[] positions, SKPoint[] texs, SKColor[] colors, UInt16[] indices) =>
			CreateCopy (vmode, positions.AsSpan (), texs.AsSpan (), colors.AsSpan (), indices.AsSpan ());

		public static SKVertices CreateCopy (SKVertexMode vmode, ReadOnlySpan<SKPoint> positions, ReadOnlySpan<SKPoint> texs, ReadOnlySpan<SKColor> colors, ReadOnlySpan<UInt16> indices)
		{
			if (texs != null && positions.Length != texs.Length)
				throw new ArgumentException ("The number of texture coordinates must match the number of vertices.", nameof (texs));
			if (colors != null && positions.Length != colors.Length)
				throw new ArgumentException ("The number of colors must match the number of vertices.", nameof (colors));

			var vertexCount = positions.Length;
			var indexCount = indices.Length;

			fixed (SKPoint* p = positions)
			fixed (SKPoint* t = texs)
			fixed (SKColor* c = colors)
			fixed (UInt16* i = indices) {
				return GetObject (SkiaApi.sk_vertices_make_copy (vmode, vertexCount, p, t, (uint*)c, indexCount, i));
			}
		}

		internal static SKVertices GetObject (IntPtr handle) =>
			handle == IntPtr.Zero ? null : new SKVertices (handle, true);
	}
}
