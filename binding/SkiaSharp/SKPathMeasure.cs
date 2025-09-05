#nullable disable

using System;

namespace SkiaSharp
{
	/// <summary>
	/// Represents a type that can be used to calculate the length of, and segments of, a path.
	/// </summary>
	public unsafe class SKPathMeasure : SKObject, ISKSkipObjectRegistration
	{
		internal SKPathMeasure (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		/// <summary>
		/// Create a new <see cref="T:SkiaSharp.SKPathMeasure" /> instance with a null path.
		/// </summary>
		public SKPathMeasure ()
			: this (SkiaApi.sk_pathmeasure_new (), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKPathMeasure instance.");
			}
		}

		/// <summary>
		/// Create a new <see cref="T:SkiaSharp.SKPathMeasure" /> instance with the specified path.
		/// </summary>
		/// <param name="path">The path to use, or null.</param>
		/// <param name="forceClosed">Controls whether or not the path is treated as closed.</param>
		/// <param name="resScale">Controls the precision of the measure. Values greater 1 increase the precision (and possibly slow down the computation).</param>
		/// <remarks>The path must remain valid for the lifetime of the measure object, or until <see cref="M:SkiaSharp.SKPathMeasure.SetPath(SkiaSharp.SKPath,System.Boolean)" /> is called with a different path (or null), since the measure object keeps a reference to the path object (does not copy its data).</remarks>
		public SKPathMeasure (SKPath path, bool forceClosed = false, float resScale = 1)
			: this (IntPtr.Zero, true)
		{
			if (path == null)
				throw new ArgumentNullException (nameof (path));

			Handle = SkiaApi.sk_pathmeasure_new_with_path (path.Handle, forceClosed, resScale);

			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKPathMeasure instance.");
			}
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeNative () =>
			SkiaApi.sk_pathmeasure_destroy (Handle);

		// properties

		/// <summary>
		/// Gets the total length of the current contour, or 0 if no path is associated.
		/// </summary>
		/// <value>The total length of the current contour.</value>
		public float Length {
			get {
				return SkiaApi.sk_pathmeasure_get_length (Handle);
			}
		}

		/// <summary>
		/// Gets a value indicating if the current contour is closed.
		/// </summary>
		/// <value>Returns true if the current contour is closed.</value>
		public bool IsClosed {
			get {
				return SkiaApi.sk_pathmeasure_is_closed (Handle);
			}
		}

		// SetPath

		/// <param name="path"></param>
		public void SetPath (SKPath path) =>
			SetPath (path, false);

		/// <summary>
		/// Reset the path measure with the specified path.
		/// </summary>
		/// <param name="path">The path to use, or null.</param>
		/// <param name="forceClosed">Controls whether or not the path is treated as closed.</param>
		/// <remarks>The path must remain valid for the lifetime of the measure object, or until <see cref="M:SkiaSharp.SKPathMeasure.SetPath(SkiaSharp.SKPath,System.Boolean)" /> is called with a different path (or null), since the measure object keeps a reference to the path object (does not copy its data).</remarks>
		public void SetPath (SKPath path, bool forceClosed)
		{
			SkiaApi.sk_pathmeasure_set_path (Handle, path == null ? IntPtr.Zero : path.Handle, forceClosed);
		}

		// GetPositionAndTangent

		/// <summary>
		/// Computes the corresponding position and tangent from the specified distance along the path.
		/// </summary>
		/// <param name="distance">The distance to use.</param>
		/// <param name="position">The position of a point along the current contour.</param>
		/// <param name="tangent">The tangent along the current contour.</param>
		/// <returns>Returns false if there is no path, or a zero-length path was specified, in which case position and tangent are unchanged.</returns>
		/// <remarks>Distance is pinned to 0 &lt;= distance &lt;= <see cref="P:SkiaSharp.SKPathMeasure.Length" />.</remarks>
		public bool GetPositionAndTangent (float distance, out SKPoint position, out SKPoint tangent)
		{
			fixed (SKPoint* p = &position)
			fixed (SKPoint* t = &tangent) {
				return SkiaApi.sk_pathmeasure_get_pos_tan (Handle, distance, p, t);
			}
		}

		// GetPosition

		/// <param name="distance"></param>
		public SKPoint GetPosition (float distance)
		{
			if (!GetPosition (distance, out var position))
				position = SKPoint.Empty;
			return position;
		}

		/// <summary>
		/// Computes the corresponding position from the specified distance along the path.
		/// </summary>
		/// <param name="distance">The distance to use.</param>
		/// <param name="position">The position of a point along the current contour.</param>
		/// <returns>Returns false if there is no path, or a zero-length path was specified, in which case position is unchanged.</returns>
		/// <remarks>Distance is pinned to 0 &lt;= distance &lt;= <see cref="P:SkiaSharp.SKPathMeasure.Length" />.</remarks>
		public bool GetPosition (float distance, out SKPoint position)
		{
			fixed (SKPoint* p = &position) {
				return SkiaApi.sk_pathmeasure_get_pos_tan (Handle, distance, p, null);
			}
		}

		// GetTangent

		/// <param name="distance"></param>
		public SKPoint GetTangent (float distance)
		{
			if (!GetTangent (distance, out var tangent))
				tangent = SKPoint.Empty;
			return tangent;
		}

		/// <summary>
		/// Computes the corresponding tangent from the specified distance along the path.
		/// </summary>
		/// <param name="distance">The distance to use.</param>
		/// <param name="tangent">The tangent along the current contour.</param>
		/// <returns>Returns false if there is no path, or a zero-length path was specified, in which case position and tangent are unchanged.</returns>
		/// <remarks>Distance is pinned to 0 &lt;= distance &lt;= <see cref="P:SkiaSharp.SKPathMeasure.Length" />.</remarks>
		public bool GetTangent (float distance, out SKPoint tangent)
		{
			fixed (SKPoint* t = &tangent) {
				return SkiaApi.sk_pathmeasure_get_pos_tan (Handle, distance, null, t);
			}
		}

		// GetMatrix

		/// <param name="distance"></param>
		/// <param name="flags"></param>
		public SKMatrix GetMatrix (float distance, SKPathMeasureMatrixFlags flags)
		{
			if (!GetMatrix (distance, out var matrix, flags))
				matrix = SKMatrix.Empty;
			return matrix;
		}

		/// <summary>
		/// Computes a <see cref="T:SkiaSharp.SKMatrix" /> from the specified distance along the path.
		/// </summary>
		/// <param name="distance">The distance to use.</param>
		/// <param name="matrix">The computed matrix.</param>
		/// <param name="flags">Flags to indicate how to compute the matrix.</param>
		/// <returns>Returns false if there is no path, or a zero-length path was specified, in which case matrix is unchanged.</returns>
		/// <remarks>Distance is pinned to 0 &lt;= distance &lt;= <see cref="P:SkiaSharp.SKPathMeasure.Length" />.</remarks>
		public bool GetMatrix (float distance, out SKMatrix matrix, SKPathMeasureMatrixFlags flags)
		{
			fixed (SKMatrix* m = &matrix) {
				return SkiaApi.sk_pathmeasure_get_matrix (Handle, distance, m, flags);
			}
		}

		// GetSegment

		/// <summary>
		/// Given a start and stop distance, update the destination path with the intervening segment(s).
		/// </summary>
		/// <param name="start">The starting offset of the segment.</param>
		/// <param name="stop">The end offset of the segment.</param>
		/// <param name="dst">The path to hold the new segment.</param>
		/// <param name="startWithMoveTo">If true, begin the path segment with a <see cref="M:SkiaSharp.SKPath.MoveTo(SkiaSharp.SKPoint)" />.</param>
		/// <returns>Returns false if the segment is zero-length, otherwise returns true.</returns>
		/// <remarks>The start and stop parameters are pinned to 0..<see cref="P:SkiaSharp.SKPathMeasure.Length" />.</remarks>
		public bool GetSegment (float start, float stop, SKPath dst, bool startWithMoveTo)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));
			return SkiaApi.sk_pathmeasure_get_segment (Handle, start, stop, dst.Handle, startWithMoveTo);
		}

		/// <param name="start"></param>
		/// <param name="stop"></param>
		/// <param name="startWithMoveTo"></param>
		public SKPath GetSegment (float start, float stop, bool startWithMoveTo)
		{
			var dst = new SKPath ();
			if (!GetSegment (start, stop, dst, startWithMoveTo)) {
				dst.Dispose ();
				dst = null;
			}
			return dst;
		}

		// NextContour

		/// <summary>
		/// Move to the next contour in the path.
		/// </summary>
		/// <returns>Returns true if another one exists, otherwise false.</returns>
		public bool NextContour ()
		{
			return SkiaApi.sk_pathmeasure_next_contour (Handle);
		}
	}
}
