using System;
using System.Runtime.InteropServices;

using GRBackendObject = System.IntPtr;

namespace SkiaSharp
{
	public enum GRSurfaceOrigin
	{
		TopLeft,
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
		Rgb888,
		Bgra8888,
		Srgba8888,
		Sbgra8888,
		Rgba1010102,
		RgbaFloat,
		RgFloat,
		AlphaHalf,
		RgbaHalf,
	}

	[Obsolete("Use GRBackendRenderTarget instead.")]
	public struct GRBackendRenderTargetDesc
	{
		public int Width { get; set; }
		public int Height { get; set; }
		public GRPixelConfig Config { get; set; }
		public GRSurfaceOrigin Origin { get; set; }
		public int SampleCount { get; set; }
		public int StencilBits { get; set; }
		public GRBackendObject RenderTargetHandle { get; set; }
		public SKSizeI Size => new SKSizeI (Width, Height);
		public SKRectI Rect => new SKRectI (0, 0, Width, Height);
	}

	public enum GRBackend
	{
		Metal,
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

	[StructLayout (LayoutKind.Sequential)]
	public struct GRGlFramebufferInfo
	{
		private uint fboId;
		private uint format;

		public GRGlFramebufferInfo (uint fboId)
		{
			this.fboId = fboId;
			this.format = 0;
		}

		public GRGlFramebufferInfo (uint fboId, uint format)
		{
			this.fboId = fboId;
			this.format = format;
		}

		public uint FramebufferObjectId {
			get => fboId;
			set => fboId = value;
		}
		public uint Format {
			get => format;
			set => format = value;
		}
	}

	[StructLayout (LayoutKind.Sequential)]
	public struct GRGlTextureInfo
	{
		private uint fTarget;
		private uint fID;
		private uint fFormat;

		public GRGlTextureInfo (uint target, uint id, uint format)
		{
			fTarget = target;
			fID = id;
			fFormat = format;
		}

		public uint Target {
			get { return fTarget; }
			set { fTarget = value; }
		}
		public uint Id {
			get { return fID; }
			set { fID = value; }
		}
		public uint Format {
			get { return fFormat; }
			set { fFormat = value; }
		}
	};

	[Obsolete("Use GRBackendTexture instead.")]
	public struct GRGlBackendTextureDesc
	{
		public GRSurfaceOrigin Origin { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public GRPixelConfig Config { get; set; }
		public int SampleCount { get; set; }
		public GRGlTextureInfo TextureHandle { get; set; }
	}

	public static partial class SkiaExtensions
	{
		public static SKColorType ToColorType (this GRPixelConfig config)
		{
			switch (config) {
				case GRPixelConfig.Unknown:
					return SKColorType.Unknown;
				case GRPixelConfig.Alpha8:
					return SKColorType.Alpha8;
				case GRPixelConfig.Gray8:
					return SKColorType.Gray8;
				case GRPixelConfig.Rgb565:
					return SKColorType.Rgb565;
				case GRPixelConfig.Rgba4444:
					return SKColorType.Argb4444;
				case GRPixelConfig.Rgba8888:
					return SKColorType.Rgba8888;
				case GRPixelConfig.Rgb888:
					return SKColorType.Rgb888x;
				case GRPixelConfig.Bgra8888:
					return SKColorType.Bgra8888;
				case GRPixelConfig.Srgba8888:
					return SKColorType.Rgba8888;
				case GRPixelConfig.Sbgra8888:
					return SKColorType.Bgra8888;
				case GRPixelConfig.Rgba1010102:
					return SKColorType.Rgba1010102;
				case GRPixelConfig.RgbaFloat:
					return SKColorType.Unknown;
				case GRPixelConfig.RgFloat:
					return SKColorType.Unknown;
				case GRPixelConfig.AlphaHalf:
					return SKColorType.Unknown;
				case GRPixelConfig.RgbaHalf:
					return SKColorType.RgbaF16;
				default:
					throw new ArgumentOutOfRangeException (nameof (config));
			}
		}
	}
}
