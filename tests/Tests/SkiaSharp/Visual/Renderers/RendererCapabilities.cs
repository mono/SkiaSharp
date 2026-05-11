using System;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// What a renderer can do. Combined with <see cref="SceneRequirements"/>
	/// to skip incompatible cells in the (renderer × scene) matrix.
	/// </summary>
	[Flags]
	public enum RendererCapabilities
	{
		None = 0,

		/// <summary>CPU rasterizer (e.g. <c>RasterRenderer</c>).</summary>
		Cpu = 1 << 0,

		/// <summary>GPU-backed renderer (Vulkan, Metal, GL, GLES, WebGPU, …).</summary>
		Gpu = 1 << 1,
	}

	internal static class CapabilityChecks
	{
		public static bool Satisfies (this RendererCapabilities caps, SceneRequirements requires)
		{
			if ((requires & SceneRequirements.Gpu) != 0 && (caps & RendererCapabilities.Gpu) == 0)
				return false;
			return true;
		}
	}
}
