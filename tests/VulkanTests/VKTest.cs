using System;
using SkiaSharp.Tests;
using Xunit;

namespace SkiaSharp.Vulkan.Tests
{
	public class VKTest : SKTest
	{
		// The default Vulkan context for all backend-agnostic tests: a headless
		// Silk.NET bring-up that uses the OS-provided Vulkan loader, so it runs on
		// any host that has a Vulkan ICD (Windows/Linux desktop, and — with a
		// software rasterizer such as Mesa Lavapipe — inside a container).
		protected SilkVkContext CreateSilkVkContext()
		{
			try
			{
				return new SilkVkContext();
			}
			catch (Exception ex) when (ex is not EntryPointNotFoundException and not MissingMethodException)
			{
				Assert.Skip($"Unable to create a Silk.NET Vulkan context: {ex.Message}");
				throw;
			}
		}

		// Legacy SharpVk context, kept only for the SharpVk-specific tests. SharpVk
		// cannot create a surface on non-Windows here, so this is Windows-only.
		protected VkContext CreateSharpVkContext()
		{
			try
			{
				if (!IsWindows)
					throw new PlatformNotSupportedException();

				return new Win32VkContext();
			}
			catch (Exception ex)
			{
				Assert.Skip($"Unable to create a SharpVk Vulkan context: {ex.Message}");
				throw;
			}
		}
	}
}
