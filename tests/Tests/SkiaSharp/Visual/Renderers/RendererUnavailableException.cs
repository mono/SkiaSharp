using System;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Thrown when a renderer's <c>RenderAsync</c> determines, at runtime,
	/// that this host can't run the renderer at all — e.g. WGL ARB extension
	/// missing, no Android device attached, browser can't initialise WebGPU.
	///
	/// <para>
	/// Distinct from regular failures so <see cref="VisualTestBase"/> can
	/// convert it to <c>Skip</c> without string-matching exception messages.
	/// Anything else thrown from <c>RenderAsync</c> is treated as a real
	/// test failure.
	/// </para>
	/// </summary>
	public sealed class RendererUnavailableException : Exception
	{
		public RendererUnavailableException (string message)
			: base (message) { }

		public RendererUnavailableException (string message, Exception inner)
			: base (message, inner) { }
	}
}
