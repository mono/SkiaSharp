using System;
using System.Runtime.InteropServices;

using GRBackendObject = System.IntPtr;
using GRBackendContext = System.IntPtr;

namespace SkiaSharp
{
	public enum GRSurfaceOrigin
	{
		TopLeft = 1,
		BottomLeft,
	}

	public enum GRPixelConfig
	{
		Unknown,
		Alpha8,
		Gray8,
		Rgb565,
		Rgba4444,
		Rgba8888,
		Bgra8888,
		Srgba8888,
		Sbgra8888,
		Rgba8888SInt,
		RgbaFloat,
		RgFloat,
		AlphaHalf,
		RgbaHalf,
	}

	[StructLayout (LayoutKind.Sequential)]
	public struct GRBackendRenderTargetDesc
	{
		private int width;
		private int height;
		private GRPixelConfig config;
		private GRSurfaceOrigin origin;
		private int sampleCount;
		private int stencilBits;
		private GRBackendObject renderTargetHandle;

		public int Width {
			get { return width; }
			set { width = value; }
		}
		public int Height {
			get { return height; }
			set { height = value; }
		}
		public GRPixelConfig Config {
			get { return config; }
			set { config = value; }
		}
		public GRSurfaceOrigin Origin {
			get { return origin; }
			set { origin = value; }
		}
		public int SampleCount {
			get { return sampleCount; }
			set { sampleCount = value; }
		}
		public int StencilBits {
			get { return stencilBits; }
			set { stencilBits = value; }
		}
		public GRBackendObject RenderTargetHandle {
			get { return renderTargetHandle; }
			set { renderTargetHandle = value; }
		}
		public SKSizeI Size => new SKSizeI (width, height);
		public SKRectI Rect => new SKRectI (0, 0, width, height);
	}

	public enum GRBackend
	{
		OpenGL,
		Vulkan,
	}

	[Flags]
	public enum GRGlBackendState : UInt32
	{
		None = 0,
		RenderTarget = 1 << 0,
		TextureBinding = 1 << 1,
		View = 1 << 2, // scissor and viewport
		Blend = 1 << 3,
		MSAAEnable = 1 << 4,
		Vertex = 1 << 5,
		Stencil = 1 << 6,
		PixelStore = 1 << 7,
		Program = 1 << 8,
		FixedFunction = 1 << 9,
		Misc = 1 << 10,
		PathRendering = 1 << 11,
		All = 0xffff
	}

	[Flags]
	public enum GRBackendState : UInt32
	{
		None = 0,
		All = 0xffffffff,
	}

	[Flags]
	public enum GRBackendTextureDescFlags
	{
		None = 0,
		RenderTarget = 1,
	}

	[StructLayout (LayoutKind.Sequential)]
	public struct GRGlTextureInfo
	{
		private uint fTarget;
		private uint fID;

		public uint Target {
			get { return fTarget; }
			set { fTarget = value; }
		}
		public uint Id {
			get { return fID; }
			set { fID = value; }
		}
	};

	[StructLayout (LayoutKind.Sequential)]
	public struct GRBackendTextureDesc
	{
		private GRBackendTextureDescFlags flags;
		private GRSurfaceOrigin origin;
		private int width;
		private int height;
		private GRPixelConfig config;
		private int sampleCount;
		private GRBackendObject textureHandle;

		public GRBackendTextureDescFlags Flags {
			get { return flags; }
			set { flags = value; }
		}
		public GRSurfaceOrigin Origin {
			get { return origin; }
			set { origin = value; }
		}
		public int Width {
			get { return width; }
			set { width = value; }
		}
		public int Height {
			get { return height; }
			set { height = value; }
		}
		public GRPixelConfig Config {
			get { return config; }
			set { config = value; }
		}
		public int SampleCount {
			get { return sampleCount; }
			set { sampleCount = value; }
		}
		public GRBackendObject TextureHandle {
			get { return textureHandle; }
			set { textureHandle = value; }
		}
	}

	public struct GRGlBackendTextureDesc
	{
		public GRBackendTextureDescFlags Flags { get; set; }
		public GRSurfaceOrigin Origin { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public GRPixelConfig Config { get; set; }
		public int SampleCount { get; set; }
		public GRGlTextureInfo TextureHandle { get; set; }
	}

	public enum GRContextOptionsGpuPathRenderers
	{
		None = 0,
		DashLine = 1 << 0,
		StencilAndCover = 1 << 1,
		Msaa = 1 << 2,
		AaHairline = 1 << 3,
		AaConvex = 1 << 4,
		AaLinearizing = 1 << 5,
		Small = 1 << 6,
		Tessellating = 1 << 7,
		Default = 1 << 8,

		All = GRContextOptionsGpuPathRenderers.Default | (GRContextOptionsGpuPathRenderers.Default - 1)
	}

	[StructLayout (LayoutKind.Sequential)]
	public struct GRContextOptions
	{
		private byte fSuppressPrints;
		private int fMaxTextureSizeOverride;
		private int fMaxTileSizeOverride;
		private byte fSuppressDualSourceBlending;
		private int fBufferMapThreshold;
		private byte fUseDrawInsteadOfPartialRenderTargetWrite;
		private byte fImmediateMode;
		private byte fUseShaderSwizzling;
		private byte fDoManualMipmapping;
		private byte fEnableInstancedRendering;
		private byte fAllowPathMaskCaching;
		private byte fRequireDecodeDisableForSRGB;
		private byte fDisableGpuYUVConversion;
		private byte fSuppressPathRendering;
		private byte fWireframeMode;
		private GRContextOptionsGpuPathRenderers fGpuPathRenderers;
		private float fGlyphCacheTextureMaximumBytes;
		private byte fAvoidStencilBuffers;

		public bool SuppressPrints {
			get { return fSuppressPrints != 0; }
			set { fSuppressPrints = value ? (byte)1 : (byte)0; }
		}
		public int MaxTextureSizeOverride {
			get { return fMaxTextureSizeOverride; }
			set { fMaxTextureSizeOverride = value; }
		}
		public int MaxTileSizeOverride {
			get { return fMaxTileSizeOverride; }
			set { fMaxTileSizeOverride = value; }
		}
		public bool SuppressDualSourceBlending {
			get { return fSuppressDualSourceBlending != 0; }
			set { fSuppressDualSourceBlending = value ? (byte)1 : (byte)0; }
		}
		public int BufferMapThreshold {
			get { return fBufferMapThreshold; }
			set { fBufferMapThreshold = value; }
		}
		public bool UseDrawInsteadOfPartialRenderTargetWrite {
			get { return fUseDrawInsteadOfPartialRenderTargetWrite != 0; }
			set { fUseDrawInsteadOfPartialRenderTargetWrite = value ? (byte)1 : (byte)0; }
		}
		public bool ImmediateMode {
			get { return fImmediateMode != 0; }
			set { fImmediateMode = value ? (byte)1 : (byte)0; }
		}
		public bool UseShaderSwizzling {
			get { return fUseShaderSwizzling != 0; }
			set { fUseShaderSwizzling = value ? (byte)1 : (byte)0; }
		}
		public bool DoManualMipmapping {
			get { return fDoManualMipmapping != 0; }
			set { fDoManualMipmapping = value ? (byte)1 : (byte)0; }
		}
		public bool EnableInstancedRendering {
			get { return fEnableInstancedRendering != 0; }
			set { fEnableInstancedRendering = value ? (byte)1 : (byte)0; }
		}
		public bool AllowPathMaskCaching {
			get { return fAllowPathMaskCaching != 0; }
			set { fAllowPathMaskCaching = value ? (byte)1 : (byte)0; }
		}
		public bool RequireDecodeDisableForSrgb {
			get { return fRequireDecodeDisableForSRGB != 0; }
			set { fRequireDecodeDisableForSRGB = value ? (byte)1 : (byte)0; }
		}
		public bool DisableGpuYuvConversion {
			get { return fDisableGpuYUVConversion != 0; }
			set { fDisableGpuYUVConversion = value ? (byte)1 : (byte)0; }
		}
		public bool SuppressPathRendering {
			get { return fSuppressPathRendering != 0; }
			set { fSuppressPathRendering = value ? (byte)1 : (byte)0; }
		}
		public bool WireframeMode {
			get { return fWireframeMode != 0; }
			set { fWireframeMode = value ? (byte)1 : (byte)0; }
		}
		public GRContextOptionsGpuPathRenderers GpuPathRenderers {
			get { return fGpuPathRenderers; }
			set { fGpuPathRenderers = value; }
		}
		public float GlyphCacheTextureMaximumBytes {
			get { return fGlyphCacheTextureMaximumBytes; }
			set { fGlyphCacheTextureMaximumBytes = value; }
		}
		public bool AvoidStencilBuffers {
			get { return fAvoidStencilBuffers != 0; }
			set { fAvoidStencilBuffers = value ? (byte)1 : (byte)0; }
		}

		public static GRContextOptions Default {
			get {
				return new GRContextOptions {
					fSuppressPrints = 0,
					fMaxTextureSizeOverride = 0x7FFFFFFF,
					fMaxTileSizeOverride = 0,
					fSuppressDualSourceBlending = 0,
					fBufferMapThreshold = -1,
					fUseDrawInsteadOfPartialRenderTargetWrite = 0,
					fImmediateMode = 0,
					fUseShaderSwizzling = 0,
					fDoManualMipmapping = 0,
					fEnableInstancedRendering = 0,
					fAllowPathMaskCaching = 0,
					fRequireDecodeDisableForSRGB = 1,
					fDisableGpuYUVConversion = 0,
					fSuppressPathRendering = 0,
					fWireframeMode = 0,
					fGpuPathRenderers = GRContextOptionsGpuPathRenderers.All,
					fGlyphCacheTextureMaximumBytes = 2048 * 1024 * 4,
					fAvoidStencilBuffers = 0,
				};
			}
		}
	}
}
