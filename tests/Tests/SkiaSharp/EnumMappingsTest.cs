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

		[SkippableFact]
		public void SKColorTypeMappingsToNative()
		{
			foreach (SKColorType value in Enum.GetValues(typeof(SKColorType)))
			{
				var native = value.ToNative();

				if (value == SKColorType.RgbaF16Clamped)
					Assert.Equal(SKColorTypeNative.RgbaF16Norm, native);
				else if (value == SKColorType.Alpha16)
					Assert.Equal(SKColorTypeNative.A16Unorm, native);
				else if (value == SKColorType.Rgba16161616)
					Assert.Equal(SKColorTypeNative.R16g16b16a16Unorm, native);
				else if (value == SKColorType.Rg1616)
					Assert.Equal(SKColorTypeNative.R16g16Unorm, native);
				else if (value == SKColorType.Rg88)
					Assert.Equal(SKColorTypeNative.R8g8Unorm, native);
				else if (value == SKColorType.AlphaF16)
					Assert.Equal(SKColorTypeNative.A16Float, native);
				else if (value == SKColorType.RgF16)
					Assert.Equal(SKColorTypeNative.R16g16Float, native);
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
					Assert.Equal(SKColorType.Alpha16, managed);
				else if (value == SKColorTypeNative.R16g16b16a16Unorm)
					Assert.Equal(SKColorType.Rgba16161616, managed);
				else if (value == SKColorTypeNative.R16g16Unorm)
					Assert.Equal(SKColorType.Rg1616, managed);
				else if (value == SKColorTypeNative.R8g8Unorm)
					Assert.Equal(SKColorType.Rg88, managed);
				else if (value == SKColorTypeNative.A16Float)
					Assert.Equal(SKColorType.AlphaF16, managed);
				else if (value == SKColorTypeNative.R16g16Float)
					Assert.Equal(SKColorType.RgF16, managed);
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
