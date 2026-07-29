using SkiaSharp.Tests;

namespace SkiaSharp.Vulkan.Tests
{
	public class VKTest : SKTest
	{
		// The default Vulkan context for all backend-agnostic tests: a headless
		// Silk.NET bring-up that uses the OS-provided Vulkan loader, so it runs on
		// any host that has a Vulkan ICD (Windows/Linux desktop, and — with a
		// software rasterizer such as Mesa Lavapipe — inside a container).
		//
		// No catch: GpuPolicy has already decided whether Vulkan is required here.
		// Where it is, CI provisions a software ICD (lavapipe on Linux, SwiftShader
		// on Windows) precisely so this succeeds, so a failure to create the context
		// is a red test — either the provisioning broke, or the agent needs an
		// explicit SKIASHARP_TEST_SKIP_GPU=ganesh-vulkan opt-out.
		protected SilkVkContext CreateSilkVkContext()
		{
			GpuPolicy.RequireOrSkip(GpuBackend.GaneshVulkan);

			return new SilkVkContext();
		}

		// Legacy SharpVk context, kept only for the SharpVk-specific tests. SharpVk
		// cannot create a surface on non-Windows, which is why the policy only marks
		// ganesh-vulkan-sharpvk as built on Windows.
		protected VkContext CreateSharpVkContext()
		{
			GpuPolicy.RequireOrSkip(GpuBackend.GaneshVulkanSharpVk);

			return new Win32VkContext();
		}
	}
}
