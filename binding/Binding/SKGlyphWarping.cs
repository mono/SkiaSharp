namespace SkiaSharp
{
	/// <summary>
	/// How the geometry of glyphs should be warped when drawn on a path.
	/// </summary>
	public enum SKGlyphWarping
	{
		/// <summary>
		/// Indicates that the advance values are adjusted and the glyphs themselves stretched or compressed.
		/// </summary>
		SpacingAndGlyphs,

		/// <summary>
		/// Indicates that only the advance values are adjusted. The glyphs themselves are not stretched or compressed.
		/// </summary>
		SpacingOnly
	}
}
