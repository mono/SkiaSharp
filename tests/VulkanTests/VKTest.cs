using SkiaSharp.Tests;

namespace SkiaSharp.Vulkan.Tests
{
	public class VKTest : SKTest
	{
		// A headless Silk.NET bring-up against the OS-provided Vulkan loader, so it
		// runs on any host with an ICD — including a software one such as Mesa
		// lavapipe or SwiftShader, which is what CI provisions.
		protected SilkVkContext CreateSilkVkContext()
		{
			GpuPolicy.RequireOrSkip(GpuBackend.GaneshVulkan);

			return new SilkVkContext();
		}

		// Legacy SharpVk context, kept only for the SharpVk-specific tests. It cannot
		// create a surface off Windows, hence the Windows-only BuiltOn in the policy.
		protected VkContext CreateSharpVkContext()
		{
			GpuPolicy.RequireOrSkip(GpuBackend.GaneshVulkanSharpVk);

			return new Win32VkContext();
		}
	}
}
