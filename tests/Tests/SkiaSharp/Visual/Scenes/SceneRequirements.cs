using System;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// What a scene needs from a renderer. Used by the matrix-discovery code
	/// to skip (renderer, scene) cells that can't usefully run together.
	/// </summary>
	[Flags]
	public enum SceneRequirements
	{
		/// <summary>Runs on any backend (raster or GPU).</summary>
		None = 0,

		/// <summary>Requires a GPU backend; raster renderers are skipped.</summary>
		Gpu = 1 << 0,
	}
}
