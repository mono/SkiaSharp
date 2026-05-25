using System;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// A reproducible draw operation, identified by name. Renderers ask scenes
	/// to draw against an SKCanvas they own and then read back pixels. Scenes
	/// must be deterministic: the same Draw() must produce the same bytes
	/// every time on a given backend.
	/// </summary>
	public interface ISkiaScene
	{
		/// <summary>
		/// Stable identifier — used as the golden-file basename
		/// (Goldens/{renderer-or-_shared}/{Name}.png).
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Default surface size and pixel format. Renderers allocate a surface
		/// of exactly this shape and pass <see cref="Draw"/> its canvas.
		/// </summary>
		SKImageInfo SuggestedInfo { get; }

		void Draw (SKCanvas canvas);
	}
}
