using System;
using Xunit;

namespace SkiaSharp.Tests
{
	public class EnumMappingsTest : SKTest
	{
		[SkippableFact]
		public void GRBackendMappings()
		{
			foreach (GRBackend value in Enum.GetValues(typeof(GRBackend)))
			{
				Assert.Equal(value.ToString(), value.ToNative().ToString());
			}

			foreach (GRBackendNative value in Enum.GetValues(typeof(GRBackendNative)))
			{
				Assert.Equal(value.ToString(), value.FromNative().ToString());
			}
		}

		[Obsolete]
		[SkippableFact]
		public void GRPixelConfigMappingsToSKColorType()
		{
			foreach (GRPixelConfig value in Enum.GetValues(typeof(GRPixelConfig)))
			{
				var colortype = value switch
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
					GRPixelConfig.Rgba1010102 => SKColorType.Rgba1010102,
					GRPixelConfig.AlphaHalf => SKColorType.A16Float,
					GRPixelConfig.RgbaHalf => SKColorType.RgbaF16,
					GRPixelConfig.Alpha8AsAlpha => SKColorType.Alpha8,
					GRPixelConfig.Alpha8AsRed => SKColorType.Alpha8,
					GRPixelConfig.AlphaHalfAsLum => SKColorType.A16Float,
					GRPixelConfig.AlphaHalfAsRed => SKColorType.A16Float,
					GRPixelConfig.Gray8AsLum => SKColorType.Gray8,
					GRPixelConfig.Gray8AsRed => SKColorType.Gray8,
					GRPixelConfig.RgbaHalfClamped => SKColorType.RgbaF16Clamped,
					GRPixelConfig.Alpha16 => SKColorType.A16,
					GRPixelConfig.Rg1616 => SKColorType.R16g16,
					GRPixelConfig.Rgba16161616 => SKColorType.R16g16b16a16,
					GRPixelConfig.RgHalf => SKColorType.R16g16Float,
					GRPixelConfig.Rg88 => SKColorType.R8g8,
					GRPixelConfig.Rgb888x => SKColorType.Rgb888x,
					GRPixelConfig.RgbEtc1 => SKColorType.Rgb888x,
					_ => SKColorType.Unknown,
				};

				if (IsEnumValueDeprected(value))
					Assert.Throws<ArgumentOutOfRangeException>(() => value.ToColorType());
				else
					Assert.Equal(colortype, value.ToColorType());
			}
		}

		[Obsolete]
		[SkippableFact]
		public void GRPixelConfigMappingsFromSKColorType()
		{
			foreach (SKColorType value in Enum.GetValues(typeof(SKColorType)))
			{
				var config = value switch
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
					SKColorType.A16Float => GRPixelConfig.AlphaHalf,
					SKColorType.RgbaF16 => GRPixelConfig.RgbaHalf,
					SKColorType.RgbaF16Clamped => GRPixelConfig.RgbaHalfClamped,
					SKColorType.A16 => GRPixelConfig.Alpha16,
					SKColorType.R16g16 => GRPixelConfig.Rg1616,
					SKColorType.R16g16b16a16 => GRPixelConfig.Rgba16161616,
					SKColorType.R16g16Float => GRPixelConfig.RgHalf,
					SKColorType.R8g8 => GRPixelConfig.Rg88,
					_ => GRPixelConfig.Unknown,
				};

				Assert.Equal(config, value.ToPixelConfig());
			}
		}

		[SkippableFact]
		public void SKColorTypeMappingsToNative()
		{
			foreach (SKColorType value in Enum.GetValues(typeof(SKColorType)))
			{
				var native = value.ToNative();

				if (value == SKColorType.RgbaF16Clamped)
					Assert.Equal(SKColorTypeNative.RgbaF16Norm, native);
				else if (value == SKColorType.A16)
					Assert.Equal(SKColorTypeNative.A16Unorm, native);
				else if (value == SKColorType.R16g16b16a16)
					Assert.Equal(SKColorTypeNative.R16g16b16a16Unorm, native);
				else if (value == SKColorType.R16g16)
					Assert.Equal(SKColorTypeNative.R16g16Unorm, native);
				else if (value == SKColorType.R8g8)
					Assert.Equal(SKColorTypeNative.R8g8Unorm, native);
				else
					Assert.Equal(value.ToString(), native.ToString());
			}
		}

		[SkippableFact]
		public void SKColorTypeMappingsFromNative()
		{
			foreach (SKColorTypeNative value in Enum.GetValues(typeof(SKColorTypeNative)))
			{
				var managed = value.FromNative();

				if (value == SKColorTypeNative.RgbaF16Norm)
					Assert.Equal(SKColorType.RgbaF16Clamped, managed);
				else if (value == SKColorTypeNative.A16Unorm)
					Assert.Equal(SKColorType.A16, managed);
				else if (value == SKColorTypeNative.R16g16b16a16Unorm)
					Assert.Equal(SKColorType.R16g16b16a16, managed);
				else if (value == SKColorTypeNative.R16g16Unorm)
					Assert.Equal(SKColorType.R16g16, managed);
				else if (value == SKColorTypeNative.R8g8Unorm)
					Assert.Equal(SKColorType.R8g8, managed);
				else
					Assert.Equal(value.ToString(), managed.ToString());
			}
		}

		[SkippableFact]
		public void GetAlphaTypeMappings()
		{
			foreach (SKColorType value in Enum.GetValues(typeof(SKColorType)))
			{
				var alphaType = value.GetAlphaType();

				if (value == SKColorType.Unknown)
					Assert.Equal(SKAlphaType.Unknown, alphaType);
				else
					Assert.NotEqual(SKAlphaType.Unknown, alphaType);
			}
		}
	}
}
