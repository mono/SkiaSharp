using System;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Thrown by <see cref="IRenderer.RenderAsync"/> when a renderer determines at
	/// runtime that the current host genuinely cannot run it — for example a
	/// required driver extension is missing, no GPU device is attached, or a
	/// browser cannot create the requested context.
	///
	/// <para>
	/// This is distinct from a rendering failure so the matrix harness can convert
	/// it to a <c>Skip</c> without string-matching exception messages. Anything
	/// else thrown from <see cref="IRenderer.RenderAsync"/> is treated as a real
	/// test failure.
	/// </para>
	/// </summary>
	public sealed class RendererUnavailableException : Exception
	{
		public RendererUnavailableException(string message)
			: base(message)
		{
		}

		public RendererUnavailableException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
