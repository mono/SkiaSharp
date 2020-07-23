using System;
using SharpVk;

namespace SkiaSharp
{
	public static class GRVkExtensionsSharpVkExtensions
	{
		public static void Initialize(this GRVkExtensions extensions, GRSharpVkGetProcedureAddressDelegate getProc, Instance instance, PhysicalDevice physicalDevice) =>
			extensions.Initialize(getProc, instance, physicalDevice, null, null);

		public static void Initialize(this GRVkExtensions extensions, GRSharpVkGetProcedureAddressDelegate getProc, Instance instance, PhysicalDevice physicalDevice, string[] instanceExtensions, string[] deviceExtensions)
		{
			GRVkGetProcedureAddressDelegate proc = (name, inst, _) =>
			{
				if (inst != IntPtr.Zero && instance?.RawHandle.ToUInt64() != (ulong)inst.ToInt64())
					throw new InvalidOperationException("Incorrect object for VkInstance.");

				return getProc?.Invoke(name, inst != IntPtr.Zero ? instance : null, null) ?? IntPtr.Zero;
			};

			extensions.Initialize(proc, (IntPtr)instance?.RawHandle.ToUInt64(), (IntPtr)physicalDevice?.RawHandle.ToUInt64(), instanceExtensions, deviceExtensions);
		}
	}
}
