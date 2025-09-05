#nullable disable

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

using GRBackendObject = System.IntPtr;

namespace SkiaSharp
{
	/// <summary>
	/// Various flags for the <see cref="M:SkiaSharp.GRContext.ResetContext(SkiaSharp.GRGlBackendState)" /> method when using a <see cref="F:SkiaSharp.GRBackend.OpenGL" /> backend.
	/// </summary>
	/// <remarks>
	/// </remarks>
	[Flags]
	public enum GRGlBackendState : UInt32
	{
		/// <summary>
		/// Reset nothing.
		/// </summary>
		None = 0,
		/// <summary>
		/// Reset the render target state.
		/// </summary>
		RenderTarget = 1 << 0,
		/// <summary>
		/// Reset the texture binding state.
		/// </summary>
		TextureBinding = 1 << 1,
		/// <summary>
		/// Reset the view state.
		/// </summary>
		View = 1 << 2, // scissor and viewport
		/// <summary>
		/// Reset the blend state.
		/// </summary>
		Blend = 1 << 3,
		/// <summary>
		/// Reset the multisample state.
		/// </summary>
		MSAAEnable = 1 << 4,
		/// <summary>
		/// Reset the vertex state.
		/// </summary>
		Vertex = 1 << 5,
		/// <summary>
		/// Reset the stencil state.
		/// </summary>
		Stencil = 1 << 6,
		/// <summary>
		/// Reset the pixel store state.
		/// </summary>
		PixelStore = 1 << 7,
		/// <summary>
		/// Reset the program state.
		/// </summary>
		Program = 1 << 8,
		/// <summary>
		/// Reset the fixed function state.
		/// </summary>
		FixedFunction = 1 << 9,
		/// <summary>
		/// Reset the miscellaneous state.
		/// </summary>
		Misc = 1 << 10,
		/// <summary>
		/// Reset the path rendering state.
		/// </summary>
		PathRendering = 1 << 11,
		/// <summary>
		/// Reset all state.
		/// </summary>
		All = 0xffff
	}

	/// <summary>
	/// Various flags for the <see cref="M:SkiaSharp.GRContext.ResetContext(SkiaSharp.GRBackendState)" /> method.
	/// </summary>
	/// <remarks>
	/// </remarks>
	[Flags]
	public enum GRBackendState : UInt32
	{
		/// <summary>
		/// Reset nothing.
		/// </summary>
		None = 0,
		/// <summary>
		/// Reset all the context state for any backend.
		/// </summary>
		All = 0xffffffff,
	}

	public partial struct GRGlFramebufferInfo
	{
		public GRGlFramebufferInfo (uint fboId)
		{
			fProtected = default;

			fFBOID = fboId;
			fFormat = 0;
		}

		public GRGlFramebufferInfo (uint fboId, uint format)
		{
			fProtected = default;

			fFBOID = fboId;
			fFormat = format;
		}
	}

	public partial struct GRGlTextureInfo
	{
		public GRGlTextureInfo (uint target, uint id)
		{
			fProtected = default;

			fTarget = target;
			fID = id;
			fFormat = 0;
		}

		public GRGlTextureInfo (uint target, uint id, uint format)
		{
			fProtected = default;

			fTarget = target;
			fID = id;
			fFormat = format;
		}
	}

	public unsafe partial struct GRMtlTextureInfo
	{
		private IntPtr _textureHandle;

		public GRMtlTextureInfo (IntPtr textureHandle)
		{
			TextureHandle = textureHandle;
		}

		public IntPtr TextureHandle {
			readonly get => _textureHandle;
			set {
				_textureHandle = value;
#if __IOS__ || __MACOS__ || __TVOS__
				_texture = null;
#endif
			}
		}

#if __IOS__ || __MACOS__ || __TVOS__
		private Metal.IMTLTexture _texture;
		public GRMtlTextureInfo (Metal.IMTLTexture texture)
		{
			Texture = texture;
		}

		public Metal.IMTLTexture Texture {
			readonly get => _texture;
			set {
				_texture = value;
				_textureHandle = _texture.Handle;
			}
		}
#endif

		internal GRMtlTextureInfoNative ToNative () =>
			new GRMtlTextureInfoNative {
				fTexture = (void*)TextureHandle
			};

		public readonly bool Equals (GRMtlTextureInfo obj) =>
			TextureHandle == obj.TextureHandle;

		public readonly override bool Equals (object obj) =>
			obj is GRMtlTextureInfo f && Equals (f);

		public static bool operator == (GRMtlTextureInfo left, GRMtlTextureInfo right) =>
			left.Equals (right);

		public static bool operator != (GRMtlTextureInfo left, GRMtlTextureInfo right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (TextureHandle);
			return hash.ToHashCode ();
		}
	}

	public static partial class SkiaExtensions
	{
		public static uint ToGlSizedFormat (this SKColorType colorType) =>
			colorType switch {
				SKColorType.Unknown => 0,
				SKColorType.Alpha8 => GRGlSizedFormat.ALPHA8,
				SKColorType.Gray8 => GRGlSizedFormat.LUMINANCE8,
				SKColorType.Rgb565 => GRGlSizedFormat.RGB565,
				SKColorType.Argb4444 => GRGlSizedFormat.RGBA4,
				SKColorType.Rgba8888 => GRGlSizedFormat.RGBA8,
				SKColorType.Rgb888x => GRGlSizedFormat.RGB8,
				SKColorType.Bgra8888 => GRGlSizedFormat.BGRA8,
				SKColorType.Rgba1010102 => GRGlSizedFormat.RGB10_A2,
				SKColorType.AlphaF16 => GRGlSizedFormat.R16F,
				SKColorType.RgbaF16 => GRGlSizedFormat.RGBA16F,
				SKColorType.RgbaF16Clamped => GRGlSizedFormat.RGBA16F,
				SKColorType.Alpha16 => GRGlSizedFormat.R16,
				SKColorType.Rg1616 => GRGlSizedFormat.RG16,
				SKColorType.Rgba16161616 => GRGlSizedFormat.RGBA16,
				SKColorType.Rgba10x6 => 0,
				SKColorType.RgF16 => GRGlSizedFormat.RG16F,
				SKColorType.Rg88 => GRGlSizedFormat.RG8,
				SKColorType.Rgb101010x => 0,
				SKColorType.RgbaF32 => 0,
				SKColorType.Bgra1010102 => 0,
				SKColorType.Bgr101010x => 0,
				SKColorType.Bgr101010xXR => 0,
				SKColorType.Srgba8888 => GRGlSizedFormat.SRGB8_ALPHA8,
				SKColorType.R8Unorm => GRGlSizedFormat.R8,
				_ => throw new ArgumentOutOfRangeException (nameof (colorType), $"Unknown color type: '{colorType}'"),
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
		internal const uint LUMINANCE16F = 0x881E;

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
		internal const uint RG16F = 0x822F;

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
		internal const uint RGBA16 = 0x805B;

		// RGBA integer sized formats
		internal const uint RGBA8I = 0x8D8E;
		internal const uint RGBA8UI = 0x8D7C;
		internal const uint RGBA16I = 0x8D88;
		internal const uint RGBA16UI = 0x8D76;
		internal const uint RGBA32I = 0x8D82;
		internal const uint RGBA32UI = 0x8D70;

		// BGRA sized formats
		internal const uint BGRA8 = 0x93A1;

		// Compressed texture sized formats
		internal const uint COMPRESSED_ETC1_RGB8 = 0x8D64;
	}
}
