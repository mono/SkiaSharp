using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	public class SKTextBlob : SKObject
	{
		[Preserve]
		internal SKTextBlob (IntPtr x, bool owns)
			: base (x, owns)
		{
		}

		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.sk_textblob_unref (Handle);
			}

			base.Dispose (disposing);
		}

		public SKRect Bounds {
			get {
				SkiaApi.sk_textblob_get_bounds (Handle, out var bounds);
				return bounds;
			}
		}

		public uint UniqueId => SkiaApi.sk_textblob_get_unique_id (Handle);
	}

	public class SKTextBlobBuilder : SKObject
	{
		[Preserve]
		internal SKTextBlobBuilder (IntPtr x, bool owns)
			: base (x, owns)
		{
		}

		public SKTextBlobBuilder ()
			: this (SkiaApi.sk_textblob_builder_new (), true)
		{
		}

		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.sk_textblob_builder_delete (Handle);
			}

			base.Dispose (disposing);
		}

		// Build

		public SKTextBlob Build () =>
			GetObject<SKTextBlob> (SkiaApi.sk_textblob_builder_make (Handle));

		// AddRun

		public void AddRun (SKPaint font, float x, float y, ushort[] glyphs) =>
			AddRunInternal (font, x, y, glyphs, null, null, null, false);

		public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, SKRect bounds) =>
			AddRunInternal (font, x, y, glyphs, null, null, bounds, false);

		public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, string text, uint[] clusters)
		{
			var utf8Text = StringUtilities.GetEncodedText (text, SKEncoding.Utf8);
			AddRunInternal (font, x, y, glyphs, utf8Text, clusters, null, true);
		}

		public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, string text, uint[] clusters, SKRect bounds)
		{
			var utf8Text = StringUtilities.GetEncodedText (text, SKEncoding.Utf8);
			AddRunInternal (font, x, y, glyphs, utf8Text, clusters, bounds, true);
		}

		public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, byte[] utf8Text, uint[] clusters) =>
			AddRunInternal (font, x, y, glyphs, utf8Text, clusters, null, true);

		public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, byte[] utf8Text, uint[] clusters, SKRect bounds) =>
			AddRunInternal (font, x, y, glyphs, utf8Text, clusters, bounds, true);

		private void AddRunInternal (SKPaint font, float x, float y, ushort[] glyphs, byte[] utf8Text, uint[] clusters, SKRect? bounds, bool hasText)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));
			if (glyphs == null)
				throw new ArgumentNullException (nameof (glyphs));

			if (hasText) {
				if (utf8Text == null)
					throw new ArgumentNullException (nameof (utf8Text));
				if (clusters == null)
					throw new ArgumentNullException (nameof (clusters));
				if (glyphs.Length != clusters.Length)
					throw new ArgumentException ("The number of glyphs and clusters must be the same.");
			}

			var run = AllocateRun (font, glyphs.Length, x, y, hasText ? utf8Text.Length : 0, bounds);
			run.SetGlyphs (glyphs);

			if (hasText) {
				run.SetText (utf8Text);
				run.SetClusters (clusters);
			}
		}

		// AddHorizontalRun

		public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions) =>
			AddHorizontalRunInternal (font, y, glyphs, positions, null, null, null, false);

		public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, SKRect bounds) =>
			AddHorizontalRunInternal (font, y, glyphs, positions, null, null, bounds, false);

		public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, string text, uint[] clusters)
		{
			var utf8Text = StringUtilities.GetEncodedText (text, SKEncoding.Utf8);
			AddHorizontalRunInternal (font, y, glyphs, positions, utf8Text, clusters, null, true);
		}

		public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, string text, uint[] clusters, SKRect bounds)
		{
			var utf8Text = StringUtilities.GetEncodedText (text, SKEncoding.Utf8);
			AddHorizontalRunInternal (font, y, glyphs, positions, utf8Text, clusters, bounds, true);
		}

		public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, byte[] utf8Text, uint[] clusters) =>
			AddHorizontalRunInternal (font, y, glyphs, positions, utf8Text, clusters, null, true);

		public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, byte[] utf8Text, uint[] clusters, SKRect bounds) =>
			AddHorizontalRunInternal (font, y, glyphs, positions, utf8Text, clusters, bounds, true);

		private void AddHorizontalRunInternal (SKPaint font, float y, ushort[] glyphs, float[] positions, byte[] utf8Text, uint[] clusters, SKRect? bounds, bool hasText)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));
			if (glyphs == null)
				throw new ArgumentNullException (nameof (glyphs));
			if (positions == null)
				throw new ArgumentNullException (nameof (positions));
			if (glyphs.Length != positions.Length)
				throw new ArgumentException ("The number of glyphs and positions must be the same.");

			if (hasText) {
				if (utf8Text == null)
					throw new ArgumentNullException (nameof (utf8Text));
				if (clusters == null)
					throw new ArgumentNullException (nameof (clusters));
				if (glyphs.Length != clusters.Length)
					throw new ArgumentException ("The number of glyphs and clusters must be the same.");
			}

			var run = AllocateRunHorizontal (font, glyphs.Length, y, hasText ? utf8Text.Length : 0, bounds);
			run.SetGlyphs (glyphs);
			run.SetPositions (positions);

			if (hasText) {
				run.SetText (utf8Text);
				run.SetClusters (clusters);
			}
		}

		// AddPositionedRun

		public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions) =>
			AddPositionedRunInternal (font, glyphs, positions, null, null, null, false);

		public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, SKRect bounds) =>
			AddPositionedRunInternal (font, glyphs, positions, null, null, bounds, false);

		public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, string text, uint[] clusters)
		{
			var utf8Text = StringUtilities.GetEncodedText (text, SKEncoding.Utf8);
			AddPositionedRunInternal (font, glyphs, positions, utf8Text, clusters, null, true);
		}

		public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, string text, uint[] clusters, SKRect bounds)
		{
			var utf8Text = StringUtilities.GetEncodedText (text, SKEncoding.Utf8);
			AddPositionedRunInternal (font, glyphs, positions, utf8Text, clusters, bounds, true);
		}

		public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, byte[] utf8Text, uint[] clusters) =>
			AddPositionedRunInternal (font, glyphs, positions, utf8Text, clusters, null, true);

		public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, byte[] utf8Text, uint[] clusters, SKRect bounds) =>
			AddPositionedRunInternal (font, glyphs, positions, utf8Text, clusters, bounds, true);

		private void AddPositionedRunInternal (SKPaint font, ushort[] glyphs, SKPoint[] positions, byte[] utf8Text, uint[] clusters, SKRect? bounds, bool hasText)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));
			if (glyphs == null)
				throw new ArgumentNullException (nameof (glyphs));
			if (positions == null)
				throw new ArgumentNullException (nameof (positions));
			if (glyphs.Length != positions.Length)
				throw new ArgumentException ("The number of glyphs and positions must be the same.");

			if (hasText) {
				if (utf8Text == null)
					throw new ArgumentNullException (nameof (utf8Text));
				if (clusters == null)
					throw new ArgumentNullException (nameof (clusters));
				if (glyphs.Length != clusters.Length)
					throw new ArgumentException ("The number of glyphs and clusters must be the same.");
			}

			var run = AllocateRunPositioned (font, glyphs.Length, hasText ? utf8Text.Length : 0, bounds);
			run.SetGlyphs (glyphs);
			run.SetPositions (positions);

			if (hasText) {
				run.SetText (utf8Text);
				run.SetClusters (clusters);
			}
		}

		// Allocate

		public SKRunBuffer AllocateRun (SKPaint font, int count, float x, float y, int textByteCount, SKRect? bounds)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			using (var lang = new SKString ()) {
				unsafe {
					SKRunBufferInternal runbuffer;
					if (bounds is SKRect b) {
						SkiaApi.sk_textblob_builder_alloc_run_text (Handle, font.Handle, count, x, y, textByteCount, lang.Handle, &b, out runbuffer);
					} else {
						SkiaApi.sk_textblob_builder_alloc_run_text (Handle, font.Handle, count, x, y, textByteCount, lang.Handle, (SKRect*)IntPtr.Zero, out runbuffer);
					}

					return new SKRunBuffer (runbuffer, count, textByteCount);
				}
			}
		}

		public SKHorizontalRunBuffer AllocateRunHorizontal (SKPaint font, int count, float y, int textByteCount, SKRect? bounds)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			using (var lang = new SKString ()) {
				unsafe {
					SKRunBufferInternal runbuffer;
					if (bounds is SKRect b) {
						SkiaApi.sk_textblob_builder_alloc_run_text_pos_h (Handle, font.Handle, count, y, textByteCount, lang.Handle, &b, out runbuffer);
					} else {
						SkiaApi.sk_textblob_builder_alloc_run_text_pos_h (Handle, font.Handle, count, y, textByteCount, lang.Handle, (SKRect*)IntPtr.Zero, out runbuffer);
					}

					return new SKHorizontalRunBuffer (runbuffer, count, textByteCount);
				}
			}
		}

		public SKPositionedRunBuffer AllocateRunPositioned (SKPaint font, int count, int textByteCount, SKRect? bounds)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			using (var lang = new SKString ()) {
				unsafe {
					SKRunBufferInternal runbuffer;
					if (bounds is SKRect b) {
						SkiaApi.sk_textblob_builder_alloc_run_text_pos (Handle, font.Handle, count, textByteCount, lang.Handle, &b, out runbuffer);
					} else {
						SkiaApi.sk_textblob_builder_alloc_run_text_pos (Handle, font.Handle, count, textByteCount, lang.Handle, (SKRect*)IntPtr.Zero, out runbuffer);
					}

					return new SKPositionedRunBuffer (runbuffer, count, textByteCount);
				}
			}
		}
	}

	public class SKRunBuffer
	{
		internal readonly SKRunBufferInternal internalBuffer;

		internal SKRunBuffer (SKRunBufferInternal buffer, int size, int textSize)
		{
			internalBuffer = buffer;
			Size = size;
			TextSize = textSize;
		}

		public int Size { get; }

		public int TextSize { get; }

		public void SetGlyphs (IReadOnlyList<ushort> glyphs)
		{
			if (glyphs.Count > Size)
				throw new ArgumentOutOfRangeException (nameof (glyphs));

			var glyphsSpan = GetGlyphsSpan ();

			for (var i = 0; i < Size; i++) {
				glyphsSpan[i] = glyphs[i];
			}
		}

		public Span<ushort> GetGlyphsSpan ()
		{
			unsafe {
				return new Span<ushort> (internalBuffer.Glyphs, Size);
			}
		}

		public void SetText (IReadOnlyList<byte> text)
		{
			if (text.Count > TextSize)
				throw new ArgumentOutOfRangeException (nameof (text));

			var textSpan = GetTextSpan ();

			for (var i = 0; i < TextSize; i++) {
				textSpan[i] = text[i];
			}
		}

		public Span<byte> GetTextSpan ()
		{
			unsafe {
				return new Span<byte> (internalBuffer.Utf8text, TextSize);
			}
		}

		public void SetClusters (IReadOnlyList<uint> clusters)
		{
			if (clusters.Count > Size)
				throw new ArgumentOutOfRangeException (nameof (clusters));

			var clustersSpan = GetClustersSpan ();

			for (var i = 0; i < Size; i++) {
				clustersSpan[i] = clusters[i];
			}
		}

		public Span<uint> GetClustersSpan ()
		{
			unsafe {
				return new Span<uint> (internalBuffer.Clusters, Size);
			}
		}
	}

	public sealed class SKHorizontalRunBuffer : SKRunBuffer
	{
		internal SKHorizontalRunBuffer (SKRunBufferInternal buffer, int count, int textSize)
			: base (buffer, count, textSize)
		{
		}

		public void SetPositions (IReadOnlyList<float> positions)
		{
			if (positions.Count > Size)
				throw new ArgumentOutOfRangeException (nameof (positions));

			var positionsSpan = GetPositionsSpan ();

			for (var i = 0; i < TextSize; i++) {
				positionsSpan[i] = positions[i];
			}
		}

		public Span<float> GetPositionsSpan ()
		{
			unsafe {
				return new Span<float> (internalBuffer.Positions, Size);
			}
		}
	}

	public sealed class SKPositionedRunBuffer : SKRunBuffer
	{
		internal SKPositionedRunBuffer (SKRunBufferInternal buffer, int count, int textSize)
			: base (buffer, count, textSize)
		{
		}

		public void SetPositions (IReadOnlyList<SKPoint> positions)
		{
			if (positions.Count > Size)
				throw new ArgumentOutOfRangeException (nameof (positions));

			var positionsSpan = GetPositionsSpan ();

			for (var i = 0; i < TextSize; i++) {
				positionsSpan[i] = positions[i];
			}
		}

		public Span<SKPoint> GetPositionsSpan ()
		{
			unsafe {
				return new Span<SKPoint> (internalBuffer.Positions, Size);
			}
		}
	}

	[StructLayout (LayoutKind.Sequential)]
	internal unsafe struct SKRunBufferInternal
	{
		public void* Glyphs;
		public void* Positions;
		public void* Utf8text;
		public void* Clusters;
	}
}
