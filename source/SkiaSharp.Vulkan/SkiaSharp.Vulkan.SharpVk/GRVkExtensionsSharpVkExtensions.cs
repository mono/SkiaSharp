using System;
using SharpVk;

namespace SkiaSharp
{
	/// <summary>Provides extension methods for <see cref="T:SkiaSharp.GRVkExtensions" /> to support SharpVk types.</summary>
	/// <remarks />
	public static class GRVkExtensionsSharpVkExtensions
	{
		/// <summary>Initializes the extensions object with the specified SharpVk Vulkan instance and physical device.</summary>
		/// <param name="extensions">The <see cref="T:SkiaSharp.GRVkExtensions" /> instance to initialize.</param>
		/// <param name="getProc">The delegate used to retrieve Vulkan procedure addresses.</param>
		/// <param name="instance">The SharpVk Vulkan instance.</param>
		/// <param name="physicalDevice">The SharpVk Vulkan physical device.</param>
		/// <remarks />
		public static void Initialize(this GRVkExtensions extensions, GRSharpVkGetProcedureAddressDelegate getProc, Instance instance, PhysicalDevice physicalDevice) =>
			extensions.Initialize(getProc, instance, physicalDevice, null, null);

		/// <summary>Initializes the extensions object with the specified SharpVk Vulkan objects and extensions.</summary>
		/// <param name="extensions">The <see cref="T:SkiaSharp.GRVkExtensions" /> instance to initialize.</param>
		/// <param name="getProc">The delegate used to retrieve Vulkan procedure addresses.</param>
		/// <param name="instance">The SharpVk Vulkan instance.</param>
		/// <param name="physicalDevice">The SharpVk Vulkan physical device.</param>
		/// <param name="instanceExtensions">The array of enabled Vulkan instance extension names.</param>
		/// <param name="deviceExtensions">The array of enabled Vulkan device extension names.</param>
		/// <remarks />
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
