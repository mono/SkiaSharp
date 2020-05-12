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

		//[SkippableFact]
		//public void GRPixelConfigMappings()
		//{
		//	foreach (GRPixelConfig value in Enum.GetValues(typeof(GRPixelConfig)))
		//	{
		//		Assert.Equal(value.ToString(), value.ToNative().ToString());
		//	}
		//
		//	foreach (GRPixelConfigNative value in Enum.GetValues(typeof(GRPixelConfigNative)))
		//	{
		//		Assert.Equal(value.ToString(), value.FromNative().ToString());
		//	}
		//}

		[SkippableFact]
		public void SKColorTypeMappings()
		{
			foreach (SKColorType value in Enum.GetValues(typeof(SKColorType)))
			{
				Assert.Equal(value.ToString(), value.ToNative().ToString());
			}

			foreach (SKColorTypeNative value in Enum.GetValues(typeof(SKColorTypeNative)))
			{
				Assert.Equal(value.ToString(), value.FromNative().ToString());
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
