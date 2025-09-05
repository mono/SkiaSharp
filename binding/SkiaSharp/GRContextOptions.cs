#nullable disable

using System;

namespace SkiaSharp
{
	/// <summary>
	/// Represents the collection of options for the construction of a context.
	/// </summary>
	public unsafe class GRContextOptions
	{
		/// <summary>
		/// Gets or sets a value indicating whether to avoid allocating stencil buffers.
		/// </summary>
		/// <remarks>Bugs on certain drivers cause stencil buffers to leak. This flag causes Skia to avoid allocating stencil buffers and use alternate rasterization paths, avoiding the leak.</remarks>
		public bool AvoidStencilBuffers { get; set; } = false;

		public int RuntimeProgramCacheSize { get; set; } = 256;

		/// <summary>
		/// Gets or sets the maximum size of cache textures used for the SkiaSharp Glyph cache.
		/// </summary>
		/// <remarks>Default is 2048 * 1024 * 4.</remarks>
		public int GlyphCacheTextureMaximumBytes { get; set; } = 2048 * 1024 * 4;

		/// <summary>
		/// Gets or sets a value indicating whether to allow path mask textures to be cached.
		/// </summary>
		/// <remarks>This is only really useful if paths are commonly rendered at the same scale and fractional translation. Default is false.</remarks>
		public bool AllowPathMaskCaching { get; set; } = true;

		/// <summary>
		/// Gets or sets a value indicating whether to construct mipmaps manually, via repeated downsampling draw-calls.
		/// </summary>
		/// <remarks>This is used when the driver's implementation (glGenerateMipmap) contains bugs. This requires mipmap level and LOD control (for example, desktop or ES3). Default is false.</remarks>
		public bool DoManualMipmapping { get; set; } = false;

		/// <summary>
		/// Gets or sets the threshold, in bytes, above which a buffer mapping API will be used to map vertex and index buffers to CPU memory in order to update them.
		/// </summary>
		/// <remarks>A value of -1 means the context should deduce the optimal value for this platform. Default is -1.</remarks>
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
