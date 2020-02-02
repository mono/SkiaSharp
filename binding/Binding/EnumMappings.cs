using System;

namespace SkiaSharp
{
	public enum GRBackend
	{
		Metal = 0,
		OpenGL = 1,
		Vulkan = 2,
		Dawn = 3,
	}

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
		Rgba1010102 = 10,
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
				_ => throw new ArgumentOutOfRangeException (nameof (backend)),
			};

		internal static GRBackend FromNative (this GRBackendNative backend) =>
			backend switch
			{
				GRBackendNative.Metal => GRBackend.Metal,
				GRBackendNative.OpenGL => GRBackend.OpenGL,
				GRBackendNative.Vulkan => GRBackend.Vulkan,
				GRBackendNative.Dawn => GRBackend.Dawn,
				_ => throw new ArgumentOutOfRangeException (nameof (backend)),
			};

		internal static GRPixelConfigNative ToNative (this GRPixelConfig config) =>
			config switch
			{
				GRPixelConfig.Unknown => GRPixelConfigNative.Unknown,
				GRPixelConfig.Alpha8 => GRPixelConfigNative.Alpha8,
				GRPixelConfig.Gray8 => GRPixelConfigNative.Gray8,
				GRPixelConfig.Rgb565 => GRPixelConfigNative.Rgb565,
				GRPixelConfig.Rgba4444 => GRPixelConfigNative.Rgba4444,
				GRPixelConfig.Rgba8888 => GRPixelConfigNative.Rgba8888,
				GRPixelConfig.Rgb888 => GRPixelConfigNative.Rgb888,
				GRPixelConfig.Bgra8888 => GRPixelConfigNative.Bgra8888,
				GRPixelConfig.Srgba8888 => GRPixelConfigNative.Srgba8888,
				GRPixelConfig.Rgba1010102 => GRPixelConfigNative.Rgba1010102,
				GRPixelConfig.AlphaHalf => GRPixelConfigNative.AlphaHalf,
				GRPixelConfig.RgbaHalf => GRPixelConfigNative.RgbaHalf,
				GRPixelConfig.Alpha8AsAlpha => GRPixelConfigNative.Alpha8AsAlpha,
				GRPixelConfig.Alpha8AsRed => GRPixelConfigNative.Alpha8AsRed,
				GRPixelConfig.AlphaHalfAsLum => GRPixelConfigNative.AlphaHalfAsLum,
				GRPixelConfig.AlphaHalfAsRed => GRPixelConfigNative.AlphaHalfAsRed,
				GRPixelConfig.Gray8AsLum => GRPixelConfigNative.Gray8AsLum,
				GRPixelConfig.Gray8AsRed => GRPixelConfigNative.Gray8AsRed,
				GRPixelConfig.RgbaHalfClamped => GRPixelConfigNative.RgbaHalfClamped,
				GRPixelConfig.Alpha16 => GRPixelConfigNative.Alpha16,
				GRPixelConfig.Rg1616 => GRPixelConfigNative.Rg1616,
				GRPixelConfig.Rgba16161616 => GRPixelConfigNative.Rgba16161616,
				GRPixelConfig.RgHalf => GRPixelConfigNative.RgHalf,
				GRPixelConfig.Rg88 => GRPixelConfigNative.Rg88,
				GRPixelConfig.Rgb888x => GRPixelConfigNative.Rgb888x,
				GRPixelConfig.RgbEtc1 => GRPixelConfigNative.RgbEtc1,
				_ => throw new ArgumentOutOfRangeException (nameof (config)),
			};

		internal static GRPixelConfig FromNative (this GRPixelConfigNative config) =>
			config switch
			{
				GRPixelConfigNative.Unknown => GRPixelConfig.Unknown,
				GRPixelConfigNative.Alpha8 => GRPixelConfig.Alpha8,
				GRPixelConfigNative.Gray8 => GRPixelConfig.Gray8,
				GRPixelConfigNative.Rgb565 => GRPixelConfig.Rgb565,
				GRPixelConfigNative.Rgba4444 => GRPixelConfig.Rgba4444,
				GRPixelConfigNative.Rgba8888 => GRPixelConfig.Rgba8888,
				GRPixelConfigNative.Rgb888 => GRPixelConfig.Rgb888,
				GRPixelConfigNative.Bgra8888 => GRPixelConfig.Bgra8888,
				GRPixelConfigNative.Srgba8888 => GRPixelConfig.Srgba8888,
				GRPixelConfigNative.Rgba1010102 => GRPixelConfig.Rgba1010102,
				GRPixelConfigNative.AlphaHalf => GRPixelConfig.AlphaHalf,
				GRPixelConfigNative.RgbaHalf => GRPixelConfig.RgbaHalf,
				GRPixelConfigNative.Alpha8AsAlpha => GRPixelConfig.Alpha8AsAlpha,
				GRPixelConfigNative.Alpha8AsRed => GRPixelConfig.Alpha8AsRed,
				GRPixelConfigNative.AlphaHalfAsLum => GRPixelConfig.AlphaHalfAsLum,
				GRPixelConfigNative.AlphaHalfAsRed => GRPixelConfig.AlphaHalfAsRed,
				GRPixelConfigNative.Gray8AsLum => GRPixelConfig.Gray8AsLum,
				GRPixelConfigNative.Gray8AsRed => GRPixelConfig.Gray8AsRed,
				GRPixelConfigNative.RgbaHalfClamped => GRPixelConfig.RgbaHalfClamped,
				GRPixelConfigNative.Alpha16 => GRPixelConfig.Alpha16,
				GRPixelConfigNative.Rg1616 => GRPixelConfig.Rg1616,
				GRPixelConfigNative.Rgba16161616 => GRPixelConfig.Rgba16161616,
				GRPixelConfigNative.RgHalf => GRPixelConfig.RgHalf,
				GRPixelConfigNative.Rg88 => GRPixelConfig.Rg88,
				GRPixelConfigNative.Rgb888x => GRPixelConfig.Rgb888x,
				GRPixelConfigNative.RgbEtc1 => GRPixelConfig.RgbEtc1,
				_ => throw new ArgumentOutOfRangeException (nameof (config)),
			};
	}
}
