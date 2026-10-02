#nullable disable

using System;

namespace SkiaSharp
{
	/// <summary>Represents a type that can be used to calculate the length of, and segments of, a path.</summary>
	/// <remarks />
	public unsafe class SKPathMeasure : SKObject, ISKSkipObjectRegistration
	{
		internal SKPathMeasure (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		/// <summary>Create a new <see cref="T:SkiaSharp.SKPathMeasure" /> instance with a <see langword="null" /> path.</summary>
		/// <remarks />
		public SKPathMeasure ()
			: this (SkiaApi.sk_pathmeasure_new (), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKPathMeasure instance.");
			}
		}

		/// <summary>Create a new <see cref="T:SkiaSharp.SKPathMeasure" /> instance with the specified path.</summary>
		/// <param name="path">The path to use, or <see langword="null" />.</param>
		/// <param name="forceClosed"><see langword="true" /> to treat the path as closed; otherwise, <see langword="false" />.</param>
		/// <param name="resScale">Controls the precision of the measure. Values greater 1 increase the precision (and possibly slow down the computation).</param>
		/// <remarks>The path must remain valid for the lifetime of the measure object, or until <see cref="M:SkiaSharp.SKPathMeasure.SetPath(SkiaSharp.SKPath,System.Boolean)" /> is called with a different path (or <see langword="null" />), since the measure object keeps a reference to the path object (does not copy its data).</remarks>
		public SKPathMeasure (SKPath path, bool forceClosed = false, float resScale = 1)
			: this (IntPtr.Zero, true)
		{
			if (path == null)
				throw new ArgumentNullException (nameof (path));

			Handle = SkiaApi.sk_pathmeasure_new_with_path (path.Handle, forceClosed, resScale);
			GC.KeepAlive (path);

			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKPathMeasure instance.");
			}
		}

		/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.SKPathMeasure" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:SkiaSharp.SKPathMeasure" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKObject" /> types to destroy any native objects.</summary>
		/// <remarks />
		protected override void DisposeNative () =>
			SkiaApi.sk_pathmeasure_destroy (Handle);

		// properties

		/// <summary>Gets the total length of the current contour, or 0 if no path is associated.</summary>
		/// <value>The total length of the current contour.</value>
		/// <remarks />
		public float Length {
			get {
				var r = SkiaApi.sk_pathmeasure_get_length (Handle);
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Gets a value indicating if the current contour is closed.</summary>
		/// <value><see langword="true" /> if the current contour is closed.</value>
		/// <remarks />
		public bool IsClosed {
			get {
				var r = SkiaApi.sk_pathmeasure_is_closed (Handle);
				GC.KeepAlive (this);
				return r;
			}
		}

		// SetPath

		/// <summary>Reset the path measure with the specified path.</summary>
		/// <param name="path">The path to use, or <see langword="null" />.</param>
		/// <remarks>The path must remain valid for the lifetime of the measure object, or until <see cref="M:SkiaSharp.SKPathMeasure.SetPath(SkiaSharp.SKPath,System.Boolean)" /> is called with a different path (or <see langword="null" />), since the measure object keeps a reference to the path object (does not copy its data).</remarks>
		public void SetPath (SKPath path) =>
			SetPath (path, false);

		/// <summary>Reset the path measure with the specified path.</summary>
		/// <param name="path">The path to use, or <see langword="null" />.</param>
		/// <param name="forceClosed"><see langword="true" /> to treat the path as closed; otherwise, <see langword="false" />.</param>
		/// <remarks>The path must remain valid for the lifetime of the measure object, or until <see cref="M:SkiaSharp.SKPathMeasure.SetPath(SkiaSharp.SKPath,System.Boolean)" /> is called with a different path (or <see langword="null" />), since the measure object keeps a reference to the path object (does not copy its data).</remarks>
		public void SetPath (SKPath path, bool forceClosed)
		{
			SkiaApi.sk_pathmeasure_set_path (Handle, path == null ? IntPtr.Zero : path.Handle, forceClosed);
			GC.KeepAlive (path);
			GC.KeepAlive (this);
		}

		// GetPositionAndTangent

		/// <summary>Computes the corresponding position and tangent from the specified distance along the path.</summary>
		/// <param name="distance">The distance to use.</param>
		/// <param name="position">The position of a point along the current contour.</param>
		/// <param name="tangent">The tangent along the current contour.</param>
		/// <returns><see langword="false" /> if there is no path, or a zero-length path was specified, in which case position and tangent are unchanged.</returns>
		/// <remarks>Distance is pinned to 0 &lt;= distance &lt;= <see cref="P:SkiaSharp.SKPathMeasure.Length" />.</remarks>
		public bool GetPositionAndTangent (float distance, out SKPoint position, out SKPoint tangent)
		{
			fixed (SKPoint* p = &position)
			fixed (SKPoint* t = &tangent) {
				var r = SkiaApi.sk_pathmeasure_get_pos_tan (Handle, distance, p, t);
				GC.KeepAlive (this);
				return r;
			}
		}

		// GetPosition

		/// <summary>Computes the corresponding position from the specified distance along the path.</summary>
		/// <param name="distance">The distance along the path.</param>
		/// <returns>The position of a point along the current contour, or <see cref="F:SkiaSharp.SKPoint.Empty" /> if there is no path or a zero-length path was specified.</returns>
		/// <remarks>Distance is pinned to 0 &lt;= distance &lt;= <see cref="P:SkiaSharp.SKPathMeasure.Length" />.</remarks>
		public SKPoint GetPosition (float distance)
		{
			if (!GetPosition (distance, out var position))
				position = SKPoint.Empty;
			return position;
		}

		/// <summary>Computes the corresponding position from the specified distance along the path.</summary>
		/// <param name="distance">The distance to use.</param>
		/// <param name="position">The position of a point along the current contour.</param>
		/// <returns><see langword="false" /> if there is no path, or a zero-length path was specified, in which case position is unchanged.</returns>
		/// <remarks>Distance is pinned to 0 &lt;= distance &lt;= <see cref="P:SkiaSharp.SKPathMeasure.Length" />.</remarks>
		public bool GetPosition (float distance, out SKPoint position)
		{
			fixed (SKPoint* p = &position) {
				var r = SkiaApi.sk_pathmeasure_get_pos_tan (Handle, distance, p, null);
				GC.KeepAlive (this);
				return r;
			}
		}

		// GetTangent

		/// <summary>Computes the corresponding tangent from the specified distance along the path.</summary>
		/// <param name="distance">The distance along the path.</param>
		/// <returns>The tangent along the current contour, or <see cref="F:SkiaSharp.SKPoint.Empty" /> if there is no path or a zero-length path was specified.</returns>
		/// <remarks>Distance is pinned to 0 &lt;= distance &lt;= <see cref="P:SkiaSharp.SKPathMeasure.Length" />.</remarks>
		public SKPoint GetTangent (float distance)
		{
			if (!GetTangent (distance, out var tangent))
				tangent = SKPoint.Empty;
			return tangent;
		}

		/// <summary>Computes the corresponding tangent from the specified distance along the path.</summary>
		/// <param name="distance">The distance to use.</param>
		/// <param name="tangent">The tangent along the current contour.</param>
		/// <returns><see langword="false" /> if there is no path, or a zero-length path was specified, in which case position and tangent are unchanged.</returns>
		/// <remarks>Distance is pinned to 0 &lt;= distance &lt;= <see cref="P:SkiaSharp.SKPathMeasure.Length" />.</remarks>
		public bool GetTangent (float distance, out SKPoint tangent)
		{
			fixed (SKPoint* t = &tangent) {
				var r = SkiaApi.sk_pathmeasure_get_pos_tan (Handle, distance, null, t);
				GC.KeepAlive (this);
				return r;
			}
		}

		// GetMatrix

		/// <summary>Computes a <see cref="T:SkiaSharp.SKMatrix" /> from the specified distance along the path.</summary>
		/// <param name="distance">The distance along the path.</param>
		/// <param name="flags">Flags to indicate how to compute the matrix.</param>
		/// <returns>The computed matrix, or <see cref="F:SkiaSharp.SKMatrix.Empty" /> if there is no path or a zero-length path was specified.</returns>
		/// <remarks>Distance is pinned to 0 &lt;= distance &lt;= <see cref="P:SkiaSharp.SKPathMeasure.Length" />.</remarks>
		public SKMatrix GetMatrix (float distance, SKPathMeasureMatrixFlags flags)
		{
			if (!GetMatrix (distance, out var matrix, flags))
				matrix = SKMatrix.Empty;
			return matrix;
		}

		/// <summary>Computes a <see cref="T:SkiaSharp.SKMatrix" /> from the specified distance along the path.</summary>
		/// <param name="distance">The distance to use.</param>
		/// <param name="matrix">The computed matrix.</param>
		/// <param name="flags">Flags to indicate how to compute the matrix.</param>
		/// <returns><see langword="false" /> if there is no path, or a zero-length path was specified, in which case matrix is unchanged.</returns>
		/// <remarks>Distance is pinned to 0 &lt;= distance &lt;= <see cref="P:SkiaSharp.SKPathMeasure.Length" />.</remarks>
		public bool GetMatrix (float distance, out SKMatrix matrix, SKPathMeasureMatrixFlags flags)
		{
			fixed (SKMatrix* m = &matrix) {
				var r = SkiaApi.sk_pathmeasure_get_matrix (Handle, distance, m, flags);
				GC.KeepAlive (this);
				return r;
			}
		}

		// GetSegment

		/// <summary>Copies a segment of the path contour, between the specified distances, into the specified path builder.</summary>
		/// <param name="start">The starting distance along the contour for the segment.</param>
		/// <param name="stop">The ending distance along the contour for the segment.</param>
		/// <param name="dst">The <see cref="T:SkiaSharp.SKPathBuilder" /> to receive the extracted path segment.</param>
		/// <param name="startWithMoveTo"><see langword="true" /> to start the segment with a move-to command; otherwise, <see langword="false" />.</param>
		/// <returns><see langword="true" /> if a valid segment was extracted; otherwise, <see langword="false" />.</returns>
		/// <remarks></remarks>
		public bool GetSegment (float start, float stop, SKPathBuilder dst, bool startWithMoveTo)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));
			var result = SkiaApi.sk_pathmeasure_get_segment (Handle, start, stop, dst.Handle, startWithMoveTo);
			GC.KeepAlive (dst);
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Extracts a contour segment of the path between two distances and appends it to the destination path.</summary>
		/// <param name="start">The distance along the path at which the segment begins.</param>
		/// <param name="stop">The distance along the path at which the segment ends.</param>
		/// <param name="dst">The <see cref="T:SkiaSharp.SKPath" /> to which the extracted segment contour is appended.</param>
		/// <param name="startWithMoveTo"><see langword="true" /> to prepend a move-to verb at the start position of the segment; otherwise, <see langword="false" />.</param>
		/// <returns><see langword="true" /> if the segment was successfully extracted; otherwise, <see langword="false" />.</returns>
		/// <remarks></remarks>
		[Obsolete ("Use the SKPathBuilder overload instead.")]
		public bool GetSegment (float start, float stop, SKPath dst, bool startWithMoveTo)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));

			using var builder = new SKPathBuilder ();
			if (!GetSegment (start, stop, builder, startWithMoveTo))
				return false;

			dst.ReplaceFromBuilder (builder);
			return true;
		}

		/// <summary>Returns a new path containing the intervening segment(s) between the start and stop distances.</summary>
		/// <param name="start">The starting offset of the segment.</param>
		/// <param name="stop">The end offset of the segment.</param>
		/// <param name="startWithMoveTo">If <see langword="true" />, begin the path segment with a <see cref="M:SkiaSharp.SKPath.MoveTo(SkiaSharp.SKPoint)" />.</param>
		/// <returns>A new path containing the segment, or <see langword="null" /> if the segment is zero-length.</returns>
		/// <remarks>The start and stop parameters are pinned to 0..<see cref="P:SkiaSharp.SKPathMeasure.Length" />.</remarks>
		public SKPath GetSegment (float start, float stop, bool startWithMoveTo)
		{
			using var dst = new SKPathBuilder ();
			if (!GetSegment (start, stop, dst, startWithMoveTo)) {
				return null;
			}
			return dst.Detach ();
		}

		// NextContour

		/// <summary>Move to the next contour in the path.</summary>
		/// <returns><see langword="true" /> if another one exists; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool NextContour ()
		{
			var r = SkiaApi.sk_pathmeasure_next_contour (Handle);
			GC.KeepAlive (this);
			return r;
		}
	}
}
