using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using System.ComponentModel;

namespace SkiaSharp
{
	public class SKVertices : SKObject
	{
		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.sk_vertices_unref (Handle);
			}

			base.Dispose (disposing);
		}

		[Preserve]
		internal SKVertices (IntPtr x, bool owns)
			: base (x, owns)
		{
		}

		public static SKVertices CreateCopy (SKVertexMode vmode, SKPoint[] positions, SKColor[] colors)
		{
			return CreateCopy (vmode, positions, null, colors, null);
		}

		public static SKVertices CreateCopy (SKVertexMode vmode, SKPoint[] positions, SKPoint[] texs, SKColor[] colors)
		{
			return CreateCopy (vmode, positions, texs, colors, null);
		}

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

			return GetObject<SKVertices> (SkiaApi.sk_vertices_make_copy (vmode, vertexCount, positions, texs, colors, indexCount, indices));
		}
	}
}
