#nullable disable

using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using System.ComponentModel;

namespace SkiaSharp
{
	/// <summary>
	/// Represents an immutable set of vertex data that can be used with <see cref="M:SkiaSharp.SKCanvas.DrawVertices(SkiaSharp.SKVertices,SkiaSharp.SKBlendMode,SkiaSharp.SKPaint)" />.
	/// </summary>
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

		/// <summary>
		/// Creates a new <see cref="T:SkiaSharp.SKVertices" /> instance, making a copy of the vertices and related data.
		/// </summary>
		/// <param name="vmode">How to interpret the array of vertices.</param>
		/// <param name="positions">The array of vertices for the mesh.</param>
		/// <param name="colors">The color for each vertex, to be interpolated across the triangle. May be <see langword="null" />.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKVertices" /> instance.</returns>
		public static SKVertices CreateCopy (SKVertexMode vmode, SKPoint[] positions, SKColor[] colors)
		{
			return CreateCopy (vmode, positions, null, colors, null);
		}

		/// <summary>
		/// Creates a new <see cref="T:SkiaSharp.SKVertices" /> instance, making a copy of the vertices and related data.
		/// </summary>
		/// <param name="vmode">How to interpret the array of vertices.</param>
		/// <param name="positions">The array of vertices for the mesh.</param>
		/// <param name="texs">The coordinates in texture space (not UV space) for each vertex. May be <see langword="null" />.</param>
		/// <param name="colors">The color for each vertex, to be interpolated across the triangle. May be <see langword="null" />.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKVertices" /> instance.</returns>
		public static SKVertices CreateCopy (SKVertexMode vmode, SKPoint[] positions, SKPoint[] texs, SKColor[] colors)
		{
			return CreateCopy (vmode, positions, texs, colors, null);
		}

		/// <summary>
		/// Creates a new <see cref="T:SkiaSharp.SKVertices" /> instance, making a copy of the vertices and related data.
		/// </summary>
		/// <param name="vmode">How to interpret the array of vertices.</param>
		/// <param name="positions">The array of vertices for the mesh.</param>
		/// <param name="texs">The coordinates in texture space (not UV space) for each vertex. May be <see langword="null" />.</param>
		/// <param name="colors">The color for each vertex, to be interpolated across the triangle. May be <see langword="null" />.</param>
		/// <param name="indices">The array of indices to reference into the vertex (texture coordinates, colors) array.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKVertices" /> instance.</returns>
		public static SKVertices CreateCopy (SKVertexMode vmode, SKPoint[] positions, SKPoint[] texs, SKColor[] colors, UInt16[] indices)
		{
			if (positions == null)
				throw new ArgumentNullException (nameof (positions));

			if (texs != null && positions.Length != texs.Length)
				throw new ArgumentException ("The number of texture coordinates must match the number of vertices.", nameof (texs));
			if (colors != null && positions.Length != colors.Length)
				throw new ArgumentException ("The number of colors must match the number of vertices.", nameof (colors));

			var vertexCount = positions.Length;
			var indexCount = indices?.Length ?? 0;

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
