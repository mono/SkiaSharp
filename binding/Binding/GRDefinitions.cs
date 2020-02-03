﻿using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

using GRBackendObject = System.IntPtr;

namespace SkiaSharp
{
	[EditorBrowsable (EditorBrowsableState.Never)]
	[Obsolete ("Use GRBackendRenderTarget instead.")]
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

	public partial struct GRGlFramebufferInfo
	{
		public GRGlFramebufferInfo (uint fboId)
		{
			fFBOID = fboId;
			fFormat = 0;
		}

		public GRGlFramebufferInfo (uint fboId, uint format)
		{
			fFBOID = fboId;
			fFormat = format;
		}
	}

	public partial struct GRGlTextureInfo
	{
		public GRGlTextureInfo (uint target, uint id, uint format)
		{
			fTarget = target;
			fID = id;
			fFormat = format;
		}
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	[Flags]
	[Obsolete]
	public enum GRBackendTextureDescFlags
	{
		None = 0,
		RenderTarget = 1,
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	[Obsolete ("Use GRBackendTexture instead.")]
	[StructLayout (LayoutKind.Sequential)]
	public struct GRBackendTextureDesc
	{
		public GRBackendTextureDescFlags Flags { get; set; }
		public GRSurfaceOrigin Origin { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public GRPixelConfig Config { get; set; }
		public int SampleCount { get; set; }
		public GRBackendObject TextureHandle { get; set; }
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	[Obsolete ("Use GRBackendTexture instead.")]
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

	public static partial class SkiaExtensions
	{
		public static uint ToGlSizedFormat (this SKColorType colorType) =>
			colorType switch
			{
				SKColorType.Unknown => 0,
				SKColorType.Alpha8 => GRGlSizedFormat.ALPHA8,
				SKColorType.Rgb565 => GRGlSizedFormat.RGB565,
				SKColorType.Argb4444 => GRGlSizedFormat.RGBA4,
				SKColorType.Rgba8888 => GRGlSizedFormat.RGBA8,
				SKColorType.Rgb888x => GRGlSizedFormat.RGB8,
				SKColorType.Bgra8888 => GRGlSizedFormat.BGRA8,
				SKColorType.Rgba1010102 => GRGlSizedFormat.RGB10_A2,
				SKColorType.Rgb101010x => 0,
				SKColorType.Gray8 => GRGlSizedFormat.LUMINANCE8,
				SKColorType.RgbaF16 => GRGlSizedFormat.RGBA16F,
				_ => throw new ArgumentOutOfRangeException (nameof (colorType)),
			};

		public static uint ToGlSizedFormat (this GRPixelConfig config) =>
			config switch
			{
				GRPixelConfig.Alpha8 => GRGlSizedFormat.ALPHA8,
				GRPixelConfig.Gray8 => GRGlSizedFormat.LUMINANCE8,
				GRPixelConfig.Rgb565 => GRGlSizedFormat.RGB565,
				GRPixelConfig.Rgba4444 => GRGlSizedFormat.RGBA4,
				GRPixelConfig.Rgba8888 => GRGlSizedFormat.RGBA8,
				GRPixelConfig.Rgb888 => GRGlSizedFormat.RGB8,
				GRPixelConfig.Bgra8888 => GRGlSizedFormat.BGRA8,
				GRPixelConfig.Srgba8888 => GRGlSizedFormat.SRGB8_ALPHA8,
				GRPixelConfig.Sbgra8888 => GRGlSizedFormat.SRGB8_ALPHA8,
				GRPixelConfig.Rgba1010102 => GRGlSizedFormat.RGB10_A2,
				GRPixelConfig.RgbaFloat => GRGlSizedFormat.RGBA32F,
				GRPixelConfig.RgFloat => GRGlSizedFormat.RG32F,
				GRPixelConfig.AlphaHalf => GRGlSizedFormat.R16F,
				GRPixelConfig.RgbaHalf => GRGlSizedFormat.RGBA16F,
				GRPixelConfig.Unknown => 0,
				_ => throw new ArgumentOutOfRangeException (nameof (config)),
			};

		public static GRPixelConfig ToPixelConfig (this SKColorType colorType) =>
			colorType switch
			{
				SKColorType.Unknown => GRPixelConfig.Unknown,
				SKColorType.Alpha8 => GRPixelConfig.Alpha8,
				SKColorType.Gray8 => GRPixelConfig.Gray8,
				SKColorType.Rgb565 => GRPixelConfig.Rgb565,
				SKColorType.Argb4444 => GRPixelConfig.Rgba4444,
				SKColorType.Rgba8888 => GRPixelConfig.Rgba8888,
				SKColorType.Rgb888x => GRPixelConfig.Rgb888,
				SKColorType.Bgra8888 => GRPixelConfig.Bgra8888,
				SKColorType.Rgba1010102 => GRPixelConfig.Rgba1010102,
				SKColorType.RgbaF16 => GRPixelConfig.RgbaHalf,
				SKColorType.Rgb101010x => GRPixelConfig.Unknown,
				_ => throw new ArgumentOutOfRangeException (nameof (colorType)),
			};

		public static SKColorType ToColorType (this GRPixelConfig config) =>
			config switch
			{
				GRPixelConfig.Unknown => SKColorType.Unknown,
				GRPixelConfig.Alpha8 => SKColorType.Alpha8,
				GRPixelConfig.Gray8 => SKColorType.Gray8,
				GRPixelConfig.Rgb565 => SKColorType.Rgb565,
				GRPixelConfig.Rgba4444 => SKColorType.Argb4444,
				GRPixelConfig.Rgba8888 => SKColorType.Rgba8888,
				GRPixelConfig.Rgb888 => SKColorType.Rgb888x,
				GRPixelConfig.Bgra8888 => SKColorType.Bgra8888,
				GRPixelConfig.Srgba8888 => SKColorType.Rgba8888,
				GRPixelConfig.Sbgra8888 => SKColorType.Bgra8888,
				GRPixelConfig.Rgba1010102 => SKColorType.Rgba1010102,
				GRPixelConfig.RgbaFloat => SKColorType.Unknown,
				GRPixelConfig.RgFloat => SKColorType.Unknown,
				GRPixelConfig.AlphaHalf => SKColorType.Unknown,
				GRPixelConfig.RgbaHalf => SKColorType.RgbaF16,
				_ => throw new ArgumentOutOfRangeException (nameof (config)),
			};
	}

	internal static class GRGlSizedFormat
	{
		// Unsized formats
		internal const uint STENCIL_INDEX = 0x1901;
		internal const uint DEPTH_COMPONENT = 0x1902;
		internal const uint DEPTH_STENCIL = 0x84F9;
		internal const uint RED = 0x1903;
		internal const uint RED_INTEGER = 0x8D94;
		internal const uint GREEN = 0x1904;
		internal const uint BLUE = 0x1905;
		internal const uint ALPHA = 0x1906;
		internal const uint LUMINANCE = 0x1909;
		internal const uint LUMINANCE_ALPHA = 0x190A;
		internal const uint RG_INTEGER = 0x8228;
		internal const uint RGB = 0x1907;
		internal const uint RGB_INTEGER = 0x8D98;
		internal const uint SRGB = 0x8C40;
		internal const uint RGBA = 0x1908;
		internal const uint RG = 0x8227;
		internal const uint SRGB_ALPHA = 0x8C42;
		internal const uint RGBA_INTEGER = 0x8D99;
		internal const uint BGRA = 0x80E1;

		// Stencil index sized formats
		internal const uint STENCIL_INDEX4 = 0x8D47;
		internal const uint STENCIL_INDEX8 = 0x8D48;
		internal const uint STENCIL_INDEX16 = 0x8D49;

		// Depth component sized formats
		internal const uint DEPTH_COMPONENT16 = 0x81A5;

		// Depth stencil sized formats
		internal const uint DEPTH24_STENCIL8 = 0x88F0;

		// Red sized formats
		internal const uint R8 = 0x8229;
		internal const uint R16 = 0x822A;
		internal const uint R16F = 0x822D;
		internal const uint R32F = 0x822E;

		// Red integer sized formats
		internal const uint R8I = 0x8231;
		internal const uint R8UI = 0x8232;
		internal const uint R16I = 0x8233;
		internal const uint R16UI = 0x8234;
		internal const uint R32I = 0x8235;
		internal const uint R32UI = 0x8236;

		// Luminance sized formats
		internal const uint LUMINANCE8 = 0x8040;

		// Alpha sized formats
		internal const uint ALPHA8 = 0x803C;
		internal const uint ALPHA16 = 0x803E;
		internal const uint ALPHA16F = 0x881C;
		internal const uint ALPHA32F = 0x8816;

		// Alpha integer sized formats
		internal const uint ALPHA8I = 0x8D90;
		internal const uint ALPHA8UI = 0x8D7E;
		internal const uint ALPHA16I = 0x8D8A;
		internal const uint ALPHA16UI = 0x8D78;
		internal const uint ALPHA32I = 0x8D84;
		internal const uint ALPHA32UI = 0x8D72;

		// RG sized formats
		internal const uint RG8 = 0x822B;
		internal const uint RG16 = 0x822C;
		//internal const uint R16F = 0x822D;
		//internal const uint R32F = 0x822E;

		// RG sized integer formats
		internal const uint RG8I = 0x8237;
		internal const uint RG8UI = 0x8238;
		internal const uint RG16I = 0x8239;
		internal const uint RG16UI = 0x823A;
		internal const uint RG32I = 0x823B;
		internal const uint RG32UI = 0x823C;

		// RGB sized formats
		internal const uint RGB5 = 0x8050;
		internal const uint RGB565 = 0x8D62;
		internal const uint RGB8 = 0x8051;
		internal const uint SRGB8 = 0x8C41;

		// RGB integer sized formats
		internal const uint RGB8I = 0x8D8F;
		internal const uint RGB8UI = 0x8D7D;
		internal const uint RGB16I = 0x8D89;
		internal const uint RGB16UI = 0x8D77;
		internal const uint RGB32I = 0x8D83;
		internal const uint RGB32UI = 0x8D71;

		// RGBA sized formats
		internal const uint RGBA4 = 0x8056;
		internal const uint RGB5_A1 = 0x8057;
		internal const uint RGBA8 = 0x8058;
		internal const uint RGB10_A2 = 0x8059;
		internal const uint SRGB8_ALPHA8 = 0x8C43;
		internal const uint RGBA16F = 0x881A;
		internal const uint RGBA32F = 0x8814;
		internal const uint RG32F = 0x8230;

		// RGBA integer sized formats
		internal const uint RGBA8I = 0x8D8E;
		internal const uint RGBA8UI = 0x8D7C;
		internal const uint RGBA16I = 0x8D88;
		internal const uint RGBA16UI = 0x8D76;
		internal const uint RGBA32I = 0x8D82;
		internal const uint RGBA32UI = 0x8D70;

		// BGRA sized formats
		internal const uint BGRA8 = 0x93A1;
	}
}
