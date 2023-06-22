﻿using System;
using System.ComponentModel;

namespace SkiaSharp
{
	public enum GRBackend
	{
		Metal = 0,
		OpenGL = 1,
		Vulkan = 2,
		Dawn = 3,
		Direct3D = 4,
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	[Obsolete ("Use SKColorType instead.")]
	public enum GRPixelConfig
	{
		Unknown = 0,
		Alpha8 = 1,
		Gray8 = 2,
		Rgb565 = 3,
		Rgba4444 = 4,
		Rgba8888 = 5,
		Rgb888 = 6,
		Bgra8888 = 7,
		Srgba8888 = 8,
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("The pixel configuration 'sBGRA 8888' is no longer supported in the native library.", true)]
		Sbgra8888 = 9,
		Rgba1010102 = 10,
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("The pixel configuration 'floating-point RGBA' is no longer supported in the native library.", true)]
		RgbaFloat = 11,
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("The pixel configuration 'floating-point RG' is no longer supported in the native library.", true)]
		RgFloat = 12,
		AlphaHalf = 13,
		RgbaHalf = 14,
		Alpha8AsAlpha = 15,
		Alpha8AsRed = 16,
		AlphaHalfAsLum = 17,
		AlphaHalfAsRed = 18,
		Gray8AsLum = 19,
		Gray8AsRed = 20,
		RgbaHalfClamped = 21,
		Alpha16 = 22,
		Rg1616 = 23,
		Rgba16161616 = 24,
		RgHalf = 25,
		Rg88 = 26,
		Rgb888x = 27,
		RgbEtc1 = 28,
	}

	public static partial class SkiaExtensions
	{
		internal static GRBackendNative ToNative (this GRBackend backend) =>
			backend switch
			{
				GRBackend.Metal => GRBackendNative.Metal,
				GRBackend.OpenGL => GRBackendNative.OpenGL,
				GRBackend.Vulkan => GRBackendNative.Vulkan,
				GRBackend.Dawn => GRBackendNative.Dawn,
				GRBackend.Direct3D => GRBackendNative.Direct3D,
				_ => throw new ArgumentOutOfRangeException (nameof (backend)),
			};

		internal static GRBackend FromNative (this GRBackendNative backend) =>
			backend switch
			{
				GRBackendNative.Metal => GRBackend.Metal,
				GRBackendNative.OpenGL => GRBackend.OpenGL,
				GRBackendNative.Vulkan => GRBackend.Vulkan,
				GRBackendNative.Dawn => GRBackend.Dawn,
				GRBackendNative.Direct3D => GRBackend.Direct3D,
				_ => throw new ArgumentOutOfRangeException (nameof (backend)),
			};

		internal static SKColorTypeNative ToNative (this SKColorType colorType) =>
			colorType switch
			{
				SKColorType.Unknown => SKColorTypeNative.Unknown,
				SKColorType.Alpha8 => SKColorTypeNative.Alpha8,
				SKColorType.Rgb565 => SKColorTypeNative.Rgb565,
				SKColorType.Argb4444 => SKColorTypeNative.Argb4444,
				SKColorType.Rgba8888 => SKColorTypeNative.Rgba8888,
				SKColorType.Rgb888x => SKColorTypeNative.Rgb888x,
				SKColorType.Bgra8888 => SKColorTypeNative.Bgra8888,
				SKColorType.Rgba1010102 => SKColorTypeNative.Rgba1010102,
				SKColorType.Rgb101010x => SKColorTypeNative.Rgb101010x,
				SKColorType.Gray8 => SKColorTypeNative.Gray8,
				SKColorType.RgbaF16Clamped => SKColorTypeNative.RgbaF16Norm,
				SKColorType.RgbaF16 => SKColorTypeNative.RgbaF16,
				SKColorType.RgbaF32 => SKColorTypeNative.RgbaF32,
				SKColorType.Rg88 => SKColorTypeNative.R8g8Unorm,
				SKColorType.AlphaF16 => SKColorTypeNative.A16Float,
				SKColorType.RgF16 => SKColorTypeNative.R16g16Float,
				SKColorType.Alpha16 => SKColorTypeNative.A16Unorm,
				SKColorType.Rg1616 => SKColorTypeNative.R16g16Unorm,
				SKColorType.Rgba16161616 => SKColorTypeNative.R16g16b16a16Unorm,
				SKColorType.Bgra1010102 => SKColorTypeNative.Bgra1010102,
				SKColorType.Bgr101010x => SKColorTypeNative.Bgr101010x,
				_ => throw new ArgumentOutOfRangeException (nameof (colorType)),
			};

		internal static SKColorType FromNative (this SKColorTypeNative colorType) =>
			colorType switch
			{
				SKColorTypeNative.Unknown => SKColorType.Unknown,
				SKColorTypeNative.Alpha8 => SKColorType.Alpha8,
				SKColorTypeNative.Rgb565 => SKColorType.Rgb565,
				SKColorTypeNative.Argb4444 => SKColorType.Argb4444,
				SKColorTypeNative.Rgba8888 => SKColorType.Rgba8888,
				SKColorTypeNative.Rgb888x => SKColorType.Rgb888x,
				SKColorTypeNative.Bgra8888 => SKColorType.Bgra8888,
				SKColorTypeNative.Rgba1010102 => SKColorType.Rgba1010102,
				SKColorTypeNative.Rgb101010x => SKColorType.Rgb101010x,
				SKColorTypeNative.Gray8 => SKColorType.Gray8,
				SKColorTypeNative.RgbaF16Norm => SKColorType.RgbaF16Clamped,
				SKColorTypeNative.RgbaF16 => SKColorType.RgbaF16,
				SKColorTypeNative.RgbaF32 => SKColorType.RgbaF32,
				SKColorTypeNative.R8g8Unorm => SKColorType.Rg88,
				SKColorTypeNative.A16Float => SKColorType.AlphaF16,
				SKColorTypeNative.R16g16Float => SKColorType.RgF16,
				SKColorTypeNative.A16Unorm => SKColorType.Alpha16,
				SKColorTypeNative.R16g16Unorm => SKColorType.Rg1616,
				SKColorTypeNative.R16g16b16a16Unorm => SKColorType.Rgba16161616,
				SKColorTypeNative.Bgra1010102 => SKColorType.Bgra1010102,
				SKColorTypeNative.Bgr101010x => SKColorType.Bgr101010x,
				_ => throw new ArgumentOutOfRangeException (nameof (colorType)),
			};
	}
}
