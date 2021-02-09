using System;

namespace SkiaSharp
{
	public unsafe class GRContextOptions
	{
		public bool AvoidStencilBuffers { get; set; } = false;

		public int RuntimeProgramCacheSize { get; set; } = 256;

		public int GlyphCacheTextureMaximumBytes { get; set; } = 2048 * 1024 * 4;

		public bool AllowPathMaskCaching { get; set; } = true;

		public bool DoManualMipmapping { get; set; } = false;

		public int BufferMapThreshold { get; set; } = -1;

		internal GRContextOptionsNative ToNative () =>
			new GRContextOptionsNative {
				fAllowPathMaskCaching = AllowPathMaskCaching ? (byte)1 : (byte)0,
				fAvoidStencilBuffers = AvoidStencilBuffers ? (byte)1 : (byte)0,
				fBufferMapThreshold = BufferMapThreshold,
				fDoManualMipmapping = DoManualMipmapping ? (byte)1 : (byte)0,
				fGlyphCacheTextureMaximumBytes = (IntPtr)GlyphCacheTextureMaximumBytes,
				fRuntimeProgramCacheSize = RuntimeProgramCacheSize,
			};
	}
}
