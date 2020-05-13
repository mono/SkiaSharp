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
		public void GRPixelConfigMappings()
		{
			foreach (GRPixelConfig value in Enum.GetValues(typeof(GRPixelConfig)))
			{
				if (IsEnumValueDeprected(value))
					Assert.Throws<ArgumentOutOfRangeException>(() => value.ToNative());
				else
					Assert.Equal(value.ToString(), value.ToNative().ToString());
			}

			foreach (GRPixelConfigNative value in Enum.GetValues(typeof(GRPixelConfigNative)))
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
