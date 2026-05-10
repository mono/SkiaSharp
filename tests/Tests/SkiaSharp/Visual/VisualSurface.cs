using System;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Per-test surface vended by a <see cref="VisualSetup"/>. Wraps a backend-
	/// specific offscreen render target with two operations the framework
	/// needs:
	///  • <see cref="Canvas"/> — the SKCanvas the test paints into.
	///  • <see cref="ReadPixels"/> — copies the rendered output to a CPU buffer
	///    in RGBA_8888/Premul (the canonical format for golden comparison).
	///
	/// Each backend's implementation handles the flush/submit dance internally
	/// — the test code never sees it.
	/// </summary>
	public abstract class VisualSurface : IDisposable
	{
		public abstract SKImageInfo ImageInfo { get; }
		public abstract SKCanvas    Canvas    { get; }

		/// <summary>Render and read back the pixels into a fresh
		/// RGBA_8888/Premul buffer. Returns the buffer; size is
		/// <c>Width * Height * 4</c> bytes.</summary>
		public abstract byte[] ReadPixels ();

		public abstract void Dispose ();
	}
}
