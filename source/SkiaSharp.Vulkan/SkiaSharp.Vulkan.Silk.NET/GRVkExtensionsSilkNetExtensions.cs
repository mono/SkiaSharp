using System;
using Silk.NET.Vulkan;

namespace SkiaSharp
{
	/// <summary>Provides Silk.NET-specific extensions for <see cref="T:SkiaSharp.GRVkExtensions" />.</summary>
	/// <remarks />
	public static class GRVkExtensionsSilkNetExtensions
	{
		/// <summary>Initializes Vulkan extensions using Silk.NET handles.</summary>
		/// <param name="extensions">The Vulkan extensions to initialize.</param>
		/// <param name="getProc">The callback that resolves Vulkan procedure addresses.</param>
		/// <param name="instance">The Vulkan instance.</param>
		/// <param name="physicalDevice">The Vulkan physical device.</param>
		/// <remarks />
		public static void Initialize(this GRVkExtensions extensions, GRSilkNetGetProcedureAddressDelegate getProc, Instance instance, PhysicalDevice physicalDevice) =>
			extensions.Initialize(getProc, instance, physicalDevice, null, null);

		/// <summary>Initializes Vulkan extensions using Silk.NET handles and extension names.</summary>
		/// <param name="extensions">The Vulkan extensions to initialize.</param>
		/// <param name="getProc">The callback that resolves Vulkan procedure addresses.</param>
		/// <param name="instance">The Vulkan instance.</param>
		/// <param name="physicalDevice">The Vulkan physical device.</param>
		/// <param name="instanceExtensions">The enabled Vulkan instance extensions.</param>
		/// <param name="deviceExtensions">The enabled Vulkan device extensions.</param>
		/// <remarks />
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
