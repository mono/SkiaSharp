using System;
using Silk.NET.Vulkan;

namespace SkiaSharp
{
	public static class GRVkExtensionsSilkNetExtensions
	{
		public static void Initialize(this GRVkExtensions extensions, GRSilkNetGetProcedureAddressDelegate getProc, Instance instance, PhysicalDevice physicalDevice) =>
			extensions.Initialize(getProc, instance, physicalDevice, null, null);

		public static void Initialize(this GRVkExtensions extensions, GRSilkNetGetProcedureAddressDelegate getProc, Instance instance, PhysicalDevice physicalDevice, string[] instanceExtensions, string[] deviceExtensions) =>
			extensions.Initialize(ToBaseProc(getProc, instance), instance.Handle, physicalDevice.Handle, instanceExtensions, deviceExtensions);

		private static GRVkGetProcedureAddressDelegate ToBaseProc(GRSilkNetGetProcedureAddressDelegate getProc, Instance instance) =>
			(name, inst, _) =>
			{
				if (inst != IntPtr.Zero && instance.Handle != inst)
					throw new InvalidOperationException("Incorrect object for VkInstance.");

				return getProc?.Invoke(name, inst != IntPtr.Zero ? instance : default, default) ?? IntPtr.Zero;
			};
	}
}
