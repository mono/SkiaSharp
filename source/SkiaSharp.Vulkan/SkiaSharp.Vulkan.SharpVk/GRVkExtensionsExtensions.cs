using System;
using SharpVk;

namespace SkiaSharp.Vulkan.SharpVk
{
	public static class GRVkExtensionsExtensions
	{
		public static void Initialize(this GRVkExtensions extensions, GRVkGetProcDelegate getProc, Instance instance, PhysicalDevice physicalDevice) =>
			extensions.Initialize(getProc, instance, physicalDevice, null, null);

		public static void Initialize(this GRVkExtensions extensions, GRVkGetProcDelegate getProc, Instance instance, PhysicalDevice physicalDevice, string[] instanceExtensions, string[] deviceExtensions) =>
			extensions.Initialize(getProc, (IntPtr)instance?.RawHandle.ToUInt64(), (IntPtr)physicalDevice?.RawHandle.ToUInt64(), instanceExtensions, deviceExtensions);
	}
}
