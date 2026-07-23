using System;
using Silk.NET.Vulkan;

namespace SkiaSharp
{
	public static class GRVkExtensionsSilkNetExtensions
	{
		public static void Initialize(this GRVkExtensions extensions, GRSilkNetGetProcedureAddressDelegate getProc, Instance instance, PhysicalDevice physicalDevice) =>
			extensions.Initialize(getProc, instance, physicalDevice, null, null);

		public static void Initialize(this GRVkExtensions extensions, GRSilkNetGetProcedureAddressDelegate getProc, Instance instance, PhysicalDevice physicalDevice, string[] instanceExtensions, string[] deviceExtensions)
		{
			GRVkGetProcedureAddressDelegate proc = (name, inst, _) =>
			{
				if (inst != IntPtr.Zero && instance.Handle != inst)
					throw new InvalidOperationException("Incorrect object for VkInstance.");

				return getProc?.Invoke(name, inst != IntPtr.Zero ? instance : default, default) ?? IntPtr.Zero;
			};

			extensions.Initialize(proc, instance.Handle, physicalDevice.Handle, instanceExtensions, deviceExtensions);
		}
	}
}
