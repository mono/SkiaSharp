using System;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// A reproducible drawing identified by a stable name. Renderers ask a scene
	/// to draw against an <see cref="SKCanvas"/> they own, then read back the
	/// resulting pixels for comparison against a committed golden image.
	///
	/// <para>
	/// Scenes MUST be deterministic: the same <see cref="Draw"/> must produce the
	/// same pixels every time on a given backend. Avoid system fonts, the current
	/// time, random values, or any other host-dependent state. Where text is
	/// needed, load a font bundled under <c>tests/Content/fonts</c> so the result
	/// does not depend on the host's installed fonts.
	/// </para>
	/// </summary>
	public interface ISkiaScene
	{
		/// <summary>
		/// Stable identifier, used as the golden-file basename
		/// (<c>Content/Goldens/{renderer-or-_shared}/{Name}.png</c>).
		/// </summary>
		string Name { get; }

		/// <summary>
		/// The surface size and pixel format the scene expects. Renderers allocate
		/// a surface of exactly this shape and pass <see cref="Draw"/> its canvas.
		/// </summary>
		SKImageInfo Info { get; }

		/// <summary>
		/// <see langword="true"/> when the scene's output legitimately differs
		/// across platforms even on the deterministic CPU raster backend — text is
		/// the canonical case, because the font scaler (CoreText / FreeType /
		/// DirectWrite) is platform-specific. Such scenes never share the portable
		/// <c>_shared</c> baseline; their goldens are recorded per platform so a
		/// genuine regression on one platform cannot be masked by another
		/// platform's reference. Geometry, gradients, and blends are portable and
		/// return <see langword="false"/>.
		/// </summary>
		bool IsPlatformDependent { get; }

		/// <summary>Draws the scene onto the supplied canvas.</summary>
		void Draw(SKCanvas canvas);
	}
}
