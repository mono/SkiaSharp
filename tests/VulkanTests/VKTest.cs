using System;
using SkiaSharp.Tests;
using Xunit;

namespace SkiaSharp.Vulkan.Tests
{
	public class VKTest : SKTest
	{
		protected VkContext CreateVkContext()
		{
			try
			{
				if (!IsWindows)
					throw new PlatformNotSupportedException();

				return new Win32VkContext();
			}
			catch (Exception ex)
			{
				throw new SkipException($"Unable to create Vulkan context: {ex.Message}");
			}
		}
	}
}
