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
				Assert.Equal(value.ToString(), value.ToNative().ToString());
			}

			foreach (GRPixelConfigNative value in Enum.GetValues(typeof(GRPixelConfigNative)))
			{
				Assert.Equal(value.ToString(), value.FromNative().ToString());
			}
		}
	}
}
