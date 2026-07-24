using System;
using System.IO;
using System.Runtime.InteropServices;
using Xunit;

[assembly: AssemblyFixture(typeof(SkiaSharp.Tests.VulkanIcdEnvironmentFixture))]

namespace SkiaSharp.Tests
{
	// Points the Vulkan loader at the software ICD (SwiftShader) that the
	// Silk.NET.Vulkan.SwiftShader.Native / Silk.NET.Vulkan.Loader.Native packages copy
	// next to this test host on Windows (see SkiaSharp.Tests.Console.csproj).
	//
	// The Windows Vulkan loader discovers ICDs through the registry by default, NOT the
	// application directory, so a headless CI agent with no registered driver enumerates
	// zero physical devices and every ganesh-vulkan cell skips. Setting VK_ICD_FILENAMES
	// (legacy) and VK_DRIVER_FILES (modern) to our copied manifest makes the loader use
	// SwiftShader, mirroring the Linux leg which sets the same variables to the Mesa
	// lavapipe manifest.
	//
	// This must run before the first Vk.GetApi() call; an xUnit assembly fixture is
	// constructed once, before any test in the assembly executes. It is best-effort and
	// skip-safe: it is a no-op unless we are on Windows with the manifest present and no
	// ICD already selected, and any failure is swallowed. Worst case the Vulkan cells
	// skip (see tests/VulkanTests/VKTest.cs); it can never turn the leg red.
	public sealed class VulkanIcdEnvironmentFixture
	{
		public VulkanIcdEnvironmentFixture()
		{
			try
			{
				if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
					return;

				// Respect a deliberately provided ICD; only fill in the default.
				if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VK_ICD_FILENAMES")) ||
					!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VK_DRIVER_FILES")))
					return;

				var manifest = Path.Combine(AppContext.BaseDirectory, "vk_swiftshader_icd.json");
				if (!File.Exists(manifest))
					return;

				Environment.SetEnvironmentVariable("VK_ICD_FILENAMES", manifest);
				Environment.SetEnvironmentVariable("VK_DRIVER_FILES", manifest);
			}
			catch
			{
				// Skip-safe: provisioning must never throw.
			}
		}
	}
}
