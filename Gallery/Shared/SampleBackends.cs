using System;

namespace SkiaSharpSample
{
	[Flags]
	public enum SampleBackends
	{
		Memory = 1 << 0,
		OpenGL = 1 << 1,
		// Vulkan = 1 << 2,
		// Metal  = 1 << 3,
		// Dawn   = 1 << 4,

		All = Memory | OpenGL // | Vulkan | Metal | Dawn,
	}
}
